using Infra;
using LinqToDB;
using LinqToDB.Async;

namespace Service;

public record BalanceDto(
    HolidayAccount Account,
    int EarningYear,
    decimal Accrued,
    decimal Used,
    decimal Remaining,
    DateOnly UsableFrom,
    DateOnly ExpiresOn,
    ExpiryStatus Status,
    bool NeedsAction);

/// <param name="AsOf">Datoen saldoen er beregnet for. Kan ligge i fremtiden.</param>
/// <param name="AvailableWeeks1To5">Timer der kan bruges på datoen, minus ventende ansøgninger.</param>
/// <param name="PendingWeeks1To5">Timer reserveret af ansøgninger, der venter på godkendelse.</param>
public record BalancesResponse(
    DateOnly AsOf,
    decimal AvailableWeeks1To5,
    decimal AvailableWeek6,
    decimal PendingWeeks1To5,
    decimal PendingWeek6,
    IReadOnlyList<BalanceDto> Periods);

/// <summary>Henter data fra databasen og lader de rene klasser regne.</summary>
public class HolidayService(AppDb db)
{
    /// <summary>
    /// Saldi på en vilkårlig dato. En fremtidig dato giver en prognose:
    /// optjening frem til datoen tæller med, og godkendt og ventende ferie er trukket fra.
    /// </summary>
    public async Task<BalancesResponse?> GetBalancesAsync(Guid employeeId, DateOnly asOf)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee is null)
            return null;

        var schedules = await db.WorkSchedules.Where(s => s.EmployeeId == employeeId).ToListAsync();
        var entries = await db.LedgerEntries.Where(e => e.EmployeeId == employeeId).ToListAsync();
        var parameters = new ParameterResolver(await db.RuleParameters.ToListAsync());

        var pending = await db.HolidayRequests
            .Where(r => r.EmployeeId == employeeId && r.Status == RequestStatus.Pending)
            .ToListAsync();
        var pendingWeeks1To5 = pending.Where(r => r.Account == HolidayAccount.Weeks1To5).Sum(r => r.Hours);
        var pendingWeek6 = pending.Where(r => r.Account == HolidayAccount.Week6).Sum(r => r.Hours);

        var balances = Balances.For(employee, schedules, parameters, entries, asOf);

        var periods = balances
            .Select(b => new BalanceDto(
                b.Account,
                b.EarningYear,
                Math.Round(b.Accrued, 2),
                Math.Round(-b.Movements, 2),
                Math.Round(b.Remaining, 2),
                b.UsableFrom,
                b.ExpiresOn,
                b.StatusOn(asOf),
                b.NeedsAction(asOf)))
            .ToList();

        return new BalancesResponse(
            asOf,
            Math.Round(Balances.AvailableOn(balances, HolidayAccount.Weeks1To5, asOf) - pendingWeeks1To5, 2),
            Math.Round(Balances.AvailableOn(balances, HolidayAccount.Week6, asOf) - pendingWeek6, 2),
            Math.Round(pendingWeeks1To5, 2),
            Math.Round(pendingWeek6, 2),
            periods);
    }
}