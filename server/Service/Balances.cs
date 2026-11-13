using System.Text.Json.Serialization;
using Infra;

namespace Service;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExpiryStatus
{
    NotYetUsable, // 6. ferieuge før 1. maj året efter
    Active,
    ExpiringSoon, // udløber inden for advarselsperioden
    Expired
}

/// <summary>
/// Samler optjening og bevægelser til saldi pr. konto og optjeningsperiode.
/// </summary>
public static class Balances
{
    public static IReadOnlyList<PeriodBalance> For(
        Employee employee,
        IReadOnlyList<WorkSchedule> schedules,
        ParameterResolver parameters,
        IReadOnlyList<LedgerEntry> entries,
        DateOnly asOf)
    {
        var result = new List<PeriodBalance>();

        foreach (var account in Enum.GetValues<HolidayAccount>())
        {
            var firstYear = HolidayPeriods.EarningYearFor(account, employee.StartDate);
            var lastYear = HolidayPeriods.EarningYearFor(account, asOf);

            for (var year = firstYear; year <= lastYear; year++)
            {
                var balance = new PeriodBalance(
                    account,
                    year,
                    Accrual.Accrued(account, year, employee, schedules, parameters, asOf),
                    Ledger.Movements(entries, account, year));

                if (balance.Accrued == 0 && balance.Movements == 0)
                    continue; // intet at vise

                if (balance.StatusOn(asOf) == ExpiryStatus.Expired && balance.Remaining <= 0)
                    continue; // afsluttet periode

                result.Add(balance);
            }
        }

        return result;
    }

    /// <summary>Timer der kan bruges på datoen på én konto.</summary>
    public static decimal AvailableOn(IEnumerable<PeriodBalance> balances, HolidayAccount account, DateOnly date) =>
        balances
            .Where(b => b.Account == account && HolidayPeriods.IsUsable(account, b.EarningYear, date))
            .Sum(b => b.Remaining);
}