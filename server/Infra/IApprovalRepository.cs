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

public record LedgerData(
    Employee Employee,
    IReadOnlyList<WorkSchedule> Schedules,
    IReadOnlyList<LedgerEntry> Entries,
    IReadOnlyList<RuleParameter> Parameters);

public interface IApprovalRepository
{
    Task<IReadOnlyList<HolidayRequestSnapshot>> ListPendingAsync(CancellationToken ct);
    
    Task<HolidayRequestSnapshot?> GetAsync(Guid id, CancellationToken ct);
    
    Task<LedgerData> GetLedgerDataAsync(Guid employeeId, CancellationToken ct);

    Task<bool> TrySaveDecisionAsync(Guid requestId, RequestStatus expected, RequestStatus next, ApprovalAudit audit,
        IReadOnlyList<LedgerEntry> ledgerEntries, CancellationToken ct);
}