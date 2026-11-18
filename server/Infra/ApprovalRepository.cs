using LinqToDB;
using LinqToDB.Async;

namespace Infra;

public class ApprovalRepository(AppDb db) : IApprovalRepository
{
    public async Task<IReadOnlyList<HolidayRequestSnapshot>> ListPendingAsync(CancellationToken ct)
    {
        var query = from request in db.HolidayRequests
            join employee in db.Employees on request.EmployeeId equals employee.Id
            where request.Status == RequestStatus.Pending
            orderby request.CreatedAt
            select new HolidayRequestSnapshot(
                request.Id,
                request.EmployeeId, employee.Name,
                request.Status,
                request.StartDate,
                request.EndDate,
                request.WorkingDays,
                request.Hours,
                request.Account,
                request.CreatedAt,
                request.Comment
            );
        return await query.ToListAsync(ct);
    }


public async Task<HolidayRequestSnapshot?> GetAsync(Guid id, CancellationToken ct)
{
    var query = from request in db.HolidayRequests
        join employee in db.Employees on request.EmployeeId equals employee.Id
        where request.Id == id
        select new HolidayRequestSnapshot(
            request.Id,
            request.EmployeeId, employee.Name,
            request.Status,
            request.StartDate,
            request.EndDate,
            request.WorkingDays,
            request.Hours,
            request.Account,
            request.CreatedAt,
            request.Comment
        );
    return await  query.FirstOrDefaultAsync(ct);
}

    public async Task<bool> TrySaveDecisionAsync(Guid requestId, RequestStatus expected, RequestStatus next, ApprovalAudit audit,
        CancellationToken ct)
    {
        await using var transaction = db.BeginTransaction();

        var updated = await db.HolidayRequests.Where(request => request.Id == requestId && request.Status == expected)
            .Set(request => request.Status, next).Set(request => request.RejectionReason,
                next == RequestStatus.Rejected ? audit.Reason : null)
            .Set(request => request.DecidedBy, audit.ActorId)
            .Set(request => request.DecidedAt, audit.OccurredAt).UpdateAsync(ct);

        if (updated != 1)
        {
            transaction.Rollback();
            return false;
        }

        await db.InsertAsync(audit, token: ct);
        
        transaction.Commit();
        return true;
    }
}