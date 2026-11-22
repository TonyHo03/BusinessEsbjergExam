using System.ComponentModel.DataAnnotations;
using Infra;
using LinqToDB;
using LinqToDB.Async;

namespace Service;

public record TransferInput(
    Guid EmployeeId,
    int FromEarningYear,
    decimal Days,
    [MaxLength(500)] string? Comment);

public record TransferDto(
    Guid Id,
    Guid EmployeeId,
    int FromEarningYear,
    decimal Days,
    RequestStatus Status,
    string? Comment,
    string? RejectionReason,
    DateTimeOffset CreatedAt);

// En overførsel, der venter på bogholderiet, med medarbejderens navn
public record PendingTransferDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    int FromEarningYear,
    decimal Days,
    string? Comment,
    DateTimeOffset CreatedAt);

public record TransferResult(IReadOnlyList<string> Errors, TransferDto? Created);

public record TransferDecision(DecisionOutcome Outcome, TransferDto? Transfer = null, string? Error = null);

public class TransferService(AppDb db)
{
    public async Task<TransferResult?> CreateAsync(TransferInput input, DateOnly today)
    {
        var errors = await ValidateAsync(input.EmployeeId, input.FromEarningYear, input.Days, today, excludeId: null);
        if (errors is null)
            return null;

        if (errors.Count > 0)
            return new TransferResult(errors, null);

        var transfer = new TransferRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = input.EmployeeId,
            FromEarningYear = input.FromEarningYear,
            Days = input.Days,
            Status = RequestStatus.Pending,
            Comment = input.Comment,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await db.InsertAsync(transfer);

        return new TransferResult([], ToDto(transfer));
    }

    public async Task<IReadOnlyList<TransferDto>> GetForEmployeeAsync(Guid employeeId) =>
        (await db.TransferRequests
            .Where(t => t.EmployeeId == employeeId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync())
        .Select(ToDto)
        .ToList();

    public async Task<IReadOnlyList<PendingTransferDto>> ListPendingAsync() =>
        await (from t in db.TransferRequests
                join e in db.Employees on t.EmployeeId equals e.Id
                where t.Status == RequestStatus.Pending
                orderby t.CreatedAt
                select new PendingTransferDto(t.Id, t.EmployeeId, e.Name, t.FromEarningYear, t.Days, t.Comment, t.CreatedAt))
            .ToListAsync();

    public async Task<TransferDecision> DecideAsync(Guid id, bool approve, string? reason, Guid actorId, DateOnly today)
    {
        var transfer = await db.TransferRequests.FirstOrDefaultAsync(t => t.Id == id);
        if (transfer is null)
            return new TransferDecision(DecisionOutcome.NotFound);

        if (transfer.Status != RequestStatus.Pending)
            return new TransferDecision(DecisionOutcome.NotAllowed, Error: "This transfer has already been decided.");

        if (!approve && string.IsNullOrWhiteSpace(reason))
            return new TransferDecision(DecisionOutcome.NotAllowed, Error: "Add a reason for rejecting.");

        var entries = new List<LedgerEntry>();

        if (approve)
        {
            var errors = await ValidateAsync(transfer.EmployeeId, transfer.FromEarningYear, transfer.Days, today, excludeId: id);
            if (errors is { Count: > 0 })
                return new TransferDecision(DecisionOutcome.NotAllowed, Error: string.Join(" ", errors));

            var schedules = await db.WorkSchedules.Where(s => s.EmployeeId == transfer.EmployeeId).ToListAsync();
            var dayLength = ScheduleResolver.DayLength(schedules, today);
            entries.AddRange(TransferRules.Entries(transfer.EmployeeId, id, transfer.FromEarningYear, transfer.Days, dayLength, today));
        }

        var newStatus = approve ? RequestStatus.Approved : RequestStatus.Rejected;
        var decidedAt = DateTimeOffset.UtcNow;

        await using var transaction = db.BeginTransaction();

        var updated = await db.TransferRequests
            .Where(t => t.Id == id && t.Status == RequestStatus.Pending)
            .Set(t => t.Status, newStatus)
            .Set(t => t.RejectionReason, approve ? null : reason!.Trim())
            .Set(t => t.DecidedBy, actorId)
            .Set(t => t.DecidedAt, decidedAt)
            .UpdateAsync();

        if (updated != 1)
        {
            transaction.Rollback();
            return new TransferDecision(DecisionOutcome.Conflict, Error: "This transfer was changed by someone else. Refresh and try again.");
        }

        foreach (var entry in entries)
            await db.InsertAsync(entry);

        transaction.Commit();

        transfer.Status = newStatus;
        transfer.RejectionReason = approve ? null : reason!.Trim();
        return new TransferDecision(DecisionOutcome.Done, ToDto(transfer));
    }

    private async Task<IReadOnlyList<string>?> ValidateAsync(
        Guid employeeId, int fromEarningYear, decimal days, DateOnly today, Guid? excludeId)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee is null)
            return null;

        var entries = await db.LedgerEntries.Where(e => e.EmployeeId == employeeId).ToListAsync();
        var parameters = new ParameterResolver(await db.RuleParameters.ToListAsync());

        // Allerede overført fra perioden plus ventende anmodninger om det samme
        var transferred = -entries
            .Where(e => e.Type == LedgerEntryType.Transfer && e.Account == HolidayAccount.Weeks1To5
                        && e.EarningYear == fromEarningYear && e.Days < 0)
            .Sum(e => e.Days);
        var pending = await db.TransferRequests
            .Where(t => t.EmployeeId == employeeId && t.FromEarningYear == fromEarningYear
                        && t.Status == RequestStatus.Pending && t.Id != excludeId)
            .SumAsync(t => (decimal?)t.Days) ?? 0m; // SUM giver NULL, når der ingen rækker er

        var balances = Balances.For(employee, parameters, entries, today);

        return TransferRules.Validate(balances, fromEarningYear, days, transferred + pending, parameters, today);
    }

    private static TransferDto ToDto(TransferRequest t) =>
        new(t.Id, t.EmployeeId, t.FromEarningYear, t.Days, t.Status, t.Comment, t.RejectionReason, t.CreatedAt);
}