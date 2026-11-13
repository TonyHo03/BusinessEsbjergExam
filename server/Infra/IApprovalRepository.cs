namespace Infra;

public record HolidayRequestSnapshot(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    RequestStatus Status,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    decimal Hours,
    HolidayAccount Account,
    DateTimeOffset SubmittedAt,
    string? Note);

public interface IApprovalRepository
{
    Task<IReadOnlyList<HolidayRequestSnapshot>> ListPendingAsync(CancellationToken ct);
    
    Task<HolidayRequestSnapshot?> GetAsync(Guid id, CancellationToken ct);
    
    Task<bool> TrySaveDecisionAsync(Guid requestId, RequestStatus expected, RequestStatus next, ApprovalAudit audit, CancellationToken ct);
}