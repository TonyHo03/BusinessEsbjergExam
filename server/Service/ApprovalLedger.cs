using Infra;

namespace Service;

/// <summary>
/// De ledger-poster, en beslutning giver. Ved godkendelse trækkes ferien fra saldoen,
/// ældste periode først og med feriegæld (højst 1 uge, kun uge 1–5).
/// Ved annullering af godkendt ferie modposteres de samme poster, så dagene kommer tilbage.
/// </summary>
public static class ApprovalLedger
{
    private const decimal DebtLimitDays = 5m; // 1 uge

    public static IReadOnlyList<LedgerEntry> ForApproval(HolidayRequestSnapshot request, LedgerData data)
    {
        var dayLength = ScheduleResolver.DayLength(data.Schedules, request.StartDate);
        var balances = Balances.For(data.Employee, new ParameterResolver(data.Parameters), data.Entries, request.StartDate);
        var debtLimit = request.Account == HolidayAccount.Weeks1To5 ? DebtLimitDays : 0m;

        return Ledger.AllocateUsage(request.EmployeeId, request.Account, request.Hours, dayLength,
            request.StartDate, balances, request.Id, debtLimit);
    }

    public static IReadOnlyList<LedgerEntry> ForCancellation(Guid requestId, IEnumerable<LedgerEntry> entries, DateOnly today) =>
        entries
            .Where(e => e.RequestId == requestId)
            .Select(e => new LedgerEntry
            {
                Id = Guid.NewGuid(),
                EmployeeId = e.EmployeeId,
                Account = e.Account,
                EarningYear = e.EarningYear,
                Type = LedgerEntryType.Adjustment,
                Days = -e.Days,
                Hours = -e.Hours,
                EntryDate = today,
                RequestId = requestId
            })
            .ToList();
}