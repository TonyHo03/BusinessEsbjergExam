using Infra;

namespace Service;

/// <summary>
/// Saldoen for én konto i én optjeningsperiode. Alle tal er i dage.
/// Omregn til timer med daglængden på den dato, saldoen vises eller bruges.
/// </summary>
public record PeriodBalance(HolidayAccount Account, int EarningYear, decimal Accrued, decimal Movements)
{
    public decimal Remaining => Accrued + Movements;
    public DateOnly UsableFrom => HolidayPeriods.UsageStart(Account, EarningYear);
    public DateOnly ExpiresOn => HolidayPeriods.UsageEnd(Account, EarningYear);

    public ExpiryStatus StatusOn(DateOnly date, int warningDays = 60)
    {
        if (date < UsableFrom) return ExpiryStatus.NotYetUsable;
        if (date > ExpiresOn) return ExpiryStatus.Expired;
        if (ExpiresOn.DayNumber - date.DayNumber <= warningDays) return ExpiryStatus.ExpiringSoon;
        return ExpiryStatus.Active;
    }

    /// <summary>Der er timer tilbage, og de udløber snart eller er udløbet: skal afholdes, overføres eller udbetales.</summary>
    public bool NeedsAction(DateOnly date, int warningDays = 60) =>
        Remaining > 0 && StatusOn(date, warningDays) is ExpiryStatus.ExpiringSoon or ExpiryStatus.Expired;
}

/// <summary>
/// Regler for de to ferie-konti. Kontiene blandes aldrig:
/// alle metoder arbejder kun på den konto, de bliver bedt om.
/// </summary>
public static class Ledger
{
    /// <summary>Summen af alle bevægelser (i dage) på én konto i én optjeningsperiode.</summary>
    public static decimal Movements(IEnumerable<LedgerEntry> entries, HolidayAccount account, int earningYear) =>
        entries
            .Where(e => e.Account == account && e.EarningYear == earningYear)
            .Sum(e => e.Days);

    /// <summary>
    /// Fordeler et forbrug af timer på de perioder, der kan bruges på datoen.
    /// Timerne omregnes til dage med daglængden på datoen (fx 7,4 t for fuld tid).
    /// Den ældste periode bruges først, så dage med kortest levetid forsvinder først.
    /// Er der ikke dage nok, lånes resten af den igangværende optjeningsperiode (feriegæld),
    /// så længe den samlede saldo ikke kommer under -debtLimitDays.
    /// </summary>
    public static IReadOnlyList<LedgerEntry> AllocateUsage(
        Guid employeeId,
        HolidayAccount account,
        decimal hours,
        decimal dayLength,
        DateOnly date,
        IEnumerable<PeriodBalance> balances,
        Guid? requestId = null,
        decimal debtLimitDays = 0m)
    {
        if (hours <= 0)
            throw new ArgumentException("Antal timer skal være større end 0.");
        if (dayLength <= 0)
            throw new ArgumentException("Daglængden skal være større end 0.");

        var days = hours / dayLength;

        var usable = balances
            .Where(b => b.Account == account && HolidayPeriods.IsUsable(account, b.EarningYear, date))
            .OrderBy(b => b.EarningYear)
            .ToList();

        if (usable.Sum(b => b.Remaining) - days < -debtLimitDays)
            throw new InvalidOperationException("Der er ikke dage nok på kontoen.");

        // Dage pr. optjeningsår: ældste først, resten er feriegæld i det igangværende år
        var portions = new List<(int Year, decimal Days)>();
        var missing = days;

        foreach (var balance in usable.Where(b => b.Remaining > 0))
        {
            if (missing == 0) break;

            var take = Math.Min(missing, balance.Remaining);
            portions.Add((balance.EarningYear, take));
            missing -= take;
        }

        if (missing > 0)
            portions.Add((HolidayPeriods.EarningYearFor(account, date), missing));

        // Timerne fordeles i samme forhold. Den sidste del får resten, så summen altid passer
        var entries = new List<LedgerEntry>();
        var hoursLeft = hours;

        for (var i = 0; i < portions.Count; i++)
        {
            var (year, portionDays) = portions[i];
            var portionHours = i == portions.Count - 1 ? hoursLeft : Math.Round(hours * portionDays / days, 2);
            hoursLeft -= portionHours;

            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                Account = account,
                EarningYear = year,
                Type = LedgerEntryType.Usage,
                Days = -portionDays,
                Hours = -portionHours,
                EntryDate = date,
                RequestId = requestId
            });
        }

        return entries;
    }
}