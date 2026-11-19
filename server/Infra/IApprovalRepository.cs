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

/// <summary>Det, der skal til for at regne en medarbejders saldo ud: ansættelse, skemaer, ledger og parametre.</summary>
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

    /// <summary>Gemmer status, audit og ledger-poster i én transaktion. False hvis status er ændret af en anden.</summary>
    Task<bool> TrySaveDecisionAsync(Guid requestId, RequestStatus expected, RequestStatus next, ApprovalAudit audit,
        IReadOnlyList<LedgerEntry> ledgerEntries, CancellationToken ct);
}