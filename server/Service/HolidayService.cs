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

public record BalancesResponse(
    DateOnly AsOf,
    decimal AvailableWeeks1To5,
    decimal AvailableWeek6,
    IReadOnlyList<BalanceDto> Periods);

/// <summary>Henter data fra databasen og lader de rene klasser regne.</summary>
public class HolidayService(AppDb db)
{
    public async Task<BalancesResponse?> GetBalancesAsync(Guid employeeId, DateOnly today)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee is null)
            return null;

        var schedules = await db.WorkSchedules.Where(s => s.EmployeeId == employeeId).ToListAsync();
        var entries = await db.LedgerEntries.Where(e => e.EmployeeId == employeeId).ToListAsync();
        var parameters = new ParameterResolver(await db.RuleParameters.ToListAsync());

        var balances = Balances.For(employee, schedules, parameters, entries, today);

        var periods = balances
            .Select(b => new BalanceDto(
                b.Account,
                b.EarningYear,
                Math.Round(b.Accrued, 2),
                Math.Round(-b.Movements, 2),
                Math.Round(b.Remaining, 2),
                b.UsableFrom,
                b.ExpiresOn,
                b.StatusOn(today),
                b.NeedsAction(today)))
            .ToList();

        return new BalancesResponse(
            today,
            Math.Round(Balances.AvailableOn(balances, HolidayAccount.Weeks1To5, today), 2),
            Math.Round(Balances.AvailableOn(balances, HolidayAccount.Week6, today), 2),
            periods);
    }
}