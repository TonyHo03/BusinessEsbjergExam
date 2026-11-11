using Infra;

namespace Service;

/// <summary>Saldoen for én konto i én optjeningsperiode.</summary>
public record PeriodBalance(HolidayAccount Account, int EarningYear, decimal Accrued, decimal Movements)
{
    public decimal Remaining => Accrued + Movements;
}

/// <summary>
/// Regler for de to ferie-konti. Kontiene blandes aldrig:
/// alle metoder arbejder kun på den konto, de bliver bedt om.
/// </summary>
public static class Ledger
{
    /// <summary>Summen af alle bevægelser på én konto i én optjeningsperiode.</summary>
    public static decimal Movements(IEnumerable<LedgerEntry> entries, HolidayAccount account, int earningYear) =>
        entries
            .Where(e => e.Account == account && e.EarningYear == earningYear)
            .Sum(e => e.Hours);

    /// <summary>
    /// Fordeler et forbrug af timer på de perioder, der kan bruges på datoen.
    /// Den ældste periode bruges først, så timer med kortest levetid forsvinder først.
    /// </summary>
    public static IReadOnlyList<LedgerEntry> AllocateUsage(
        Guid employeeId,
        HolidayAccount account,
        decimal hours,
        DateOnly date,
        IEnumerable<PeriodBalance> balances,
        Guid? requestId = null)
    {
        if (hours <= 0)
            throw new ArgumentException("Antal timer skal være større end 0.");

        var usable = balances
            .Where(b => b.Account == account
                        && b.Remaining > 0
                        && HolidayPeriods.IsUsable(account, b.EarningYear, date))
            .OrderBy(b => b.EarningYear)
            .ToList();

        if (usable.Sum(b => b.Remaining) < hours)
            throw new InvalidOperationException("Der er ikke timer nok på kontoen.");

        var entries = new List<LedgerEntry>();
        var missing = hours;

        foreach (var balance in usable)
        {
            if (missing == 0) break;

            var take = Math.Min(missing, balance.Remaining);
            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                Account = account,
                EarningYear = balance.EarningYear,
                Type = LedgerEntryType.Usage,
                Hours = -take,
                EntryDate = date,
                RequestId = requestId
            });
            missing -= take;
        }

        return entries;
    }
}