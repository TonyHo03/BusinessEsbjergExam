using LinqToDB;
using LinqToDB.Async;

namespace Infra;

public class ApprovalRepository(AppDb db) : IApprovalRepository
{
    public async Task<IReadOnlyList<HolidayRequestSnapshot>> ListPendingAsync(CancellationToken ct)
    {
        var query = from request in db.Requests
            join employee in db.Employees on request.EmployeeId equals employee.Id
            where request.Status == RequestStatus.Pending
            orderby request.SubmittedAt
            select new HolidayRequestSnapshot(
                request.Id,
                request.EmployeeId, employee.Name,
                request.Status,
                request.StartDate,
                request.EndDate,
                request.WorkingDays,
                request.Hours,
                request.Account,
                request.SubmittedAt,
                request.Note
            );
        return await query.ToListAsync(ct);
    }


public async Task<HolidayRequestSnapshot?> GetAsync(Guid id, CancellationToken ct)
{
    var query = from request in db.Requests
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
            request.SubmittedAt,
            request.Note
        );
    return await  query.FirstOrDefaultAsync(ct);
}

    public async Task<bool> TrySaveDecisionAsync(Guid requestId, RequestStatus expected, RequestStatus next, ApprovalAudit audit,
        CancellationToken ct)
    {
        await using var transaction = db.BeginTransaction();

        var updated = await db.Requests.Where(request => request.Id == requestId && request.Status == expected)
            .Set(request => request.Status, next).UpdateAsync(ct);

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