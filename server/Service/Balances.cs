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
/// Saldoen set fra en ansøgnings første feriedag, i timer efter daglængden den dag.
/// AvailableAtStart: timer der kan bruges den dag (minus ventende ansøgninger).
/// DebtLimit: hvor mange timer saldoen må gå i minus (1 uge efter medarbejderens skema, 0 for 6. ferieuge).
/// YearEnd / AvailableAtYearEnd: det, der dækker ansøgningen set fra den 31. august (ældre perioders rest
/// plus hele den igangværende periode). Efter ansøgningen må det ikke være negativt.
/// </summary>
public record BalanceOutlook(decimal AvailableAtStart, decimal DebtLimit, DateOnly YearEnd, decimal AvailableAtYearEnd);

/// <summary>
/// Samler optjening og bevægelser til saldi pr. konto og optjeningsperiode.
/// Saldi føres i dage og omregnes til timer med daglængden på datoen.
/// </summary>
public static class Balances
{
    /// <summary>Saldi i dage pr. konto og optjeningsperiode på datoen.</summary>
    public static IReadOnlyList<PeriodBalance> For(
        Employee employee,
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
                    Accrual.Accrued(account, year, employee, parameters, asOf),
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

    /// <summary>Dage der kan bruges på datoen på én konto.</summary>
    public static decimal AvailableOn(IEnumerable<PeriodBalance> balances, HolidayAccount account, DateOnly date) =>
        balances
            .Where(b => b.Account == account && HolidayPeriods.IsUsable(account, b.EarningYear, date))
            .Sum(b => b.Remaining);

    /// <summary>
    /// Beregner saldoen på ansøgningens startdato og den efterfølgende 31. august, i timer.
    /// Optjening frem til datoen tæller med, så en ferie længere ude kan bruge flere timer.
    /// Dagene omregnes med daglængden på startdatoen, så et skemaskift før ferien er taget med.
    /// </summary>
    public static BalanceOutlook Outlook(
        Employee employee,
        IReadOnlyList<WorkSchedule> schedules,
        ParameterResolver parameters,
        IReadOnlyList<LedgerEntry> entries,
        HolidayAccount account,
        DateOnly start,
        decimal reservedHours)
    {
        var dayLength = ScheduleResolver.DayLength(schedules, start);

        var balancesAtStart = For(employee, parameters, entries, start);
        var atStart = AvailableOn(balancesAtStart, account, start) * dayLength - reservedHours;

        // Feriegæld gælder kun uge 1–5. 6. ferieuge kan ikke bruges på forskud
        if (account == HolidayAccount.Week6)
            return new BalanceOutlook(atStart, 0m, start, atStart);

        var debtLimit = 5m * dayLength; // 1 uge
        var currentYear = HolidayPeriods.EarningYearFor(account, start);
        var yearEnd = HolidayPeriods.EarningEnd(account, currentYear); // 31. august

        // Ældre perioder bruges først. Resten lånes af den igangværende periode,
        // som skal være 0 eller positiv den 31. august, når hele året er optjent
        var olderAtStart = balancesAtStart
            .Where(b => b.Account == account && b.EarningYear < currentYear && HolidayPeriods.IsUsable(account, b.EarningYear, start))
            .Sum(b => b.Remaining);
        var currentAtYearEnd = For(employee, parameters, entries, yearEnd)
            .Where(b => b.Account == account && b.EarningYear == currentYear)
            .Sum(b => b.Remaining);
        var atYearEnd = (olderAtStart + currentAtYearEnd) * dayLength - reservedHours;

        return new BalanceOutlook(atStart, debtLimit, yearEnd, atYearEnd);
    }
}