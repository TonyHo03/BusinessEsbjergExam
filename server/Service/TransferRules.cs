using Infra;

namespace Service;

/// <summary>
/// Regler for overførsel af ferie (uge 1–5):
/// - Funktionen kan slås til og fra med parameteren transfer.enabled.
/// - Der kan kun overføres fra den periode, der udløber 31.12 i år, og kun indtil den dato.
/// - Kun ferie ud over 4 uger (20 dage) kan overføres, og højst transfer.max_days dage pr. periode (5),
///   inkl. det der allerede er overført eller anmodet om.
/// - Man kan ikke overføre flere dage, end der er tilbage.
/// Dagene flyttes til næste optjeningsår og kan derfor bruges et år længere.
/// </summary>
public static class TransferRules
{
    public static IReadOnlyList<string> Validate(
        IEnumerable<PeriodBalance> balances,
        int fromEarningYear,
        decimal days,
        decimal alreadyTransferred,
        ParameterResolver parameters,
        DateOnly today)
    {
        if (!string.Equals(parameters.Get(ParameterKeys.TransferEnabled, today), "true", StringComparison.OrdinalIgnoreCase))
            return ["Overførsel af ferie er slået fra."];

        var errors = new List<string>();
        var maxDays = parameters.GetDecimal(ParameterKeys.TransferMaxDays, today);

        if (days <= 0)
            errors.Add("Antal dage skal være større end 0.");

        var period = balances.FirstOrDefault(b => b.Account == HolidayAccount.Weeks1To5 && b.EarningYear == fromEarningYear);

        if (period is null || period.ExpiresOn.Year != today.Year || today > period.ExpiresOn)
        {
            errors.Add($"Der kan kun overføres fra den ferie, der udløber 31.12.{today.Year}.");
            return errors;
        }

        if (days > period.Remaining)
            errors.Add($"Der er kun {Math.Round(period.Remaining, 2)} dage tilbage i perioden.");

        // Kun ferie ud over 4 uger (20 dage) kan overføres, og højst maxDays
        var limit = Math.Min(maxDays, Math.Max(period.Accrued - 20m, 0m));
        if (alreadyTransferred + days > limit)
            errors.Add($"Der kan højst overføres {Math.Round(limit, 2)} dage (ferie ud over 4 uger). {Math.Round(alreadyTransferred, 2)} er allerede overført eller anmodet om.");

        return errors;
    }

    /// <summary>To poster: dagene trækkes fra perioden og lægges til næste optjeningsår.</summary>
    public static IReadOnlyList<LedgerEntry> Entries(
        Guid employeeId, Guid transferId, int fromEarningYear, decimal days, decimal dayLength, DateOnly date)
    {
        var hours = Math.Round(days * dayLength, 2);

        return
        [
            Entry(employeeId, transferId, fromEarningYear, -days, -hours, date),
            Entry(employeeId, transferId, fromEarningYear + 1, days, hours, date)
        ];
    }

    private static LedgerEntry Entry(Guid employeeId, Guid transferId, int year, decimal days, decimal hours, DateOnly date) => new()
    {
        Id = Guid.NewGuid(),
        EmployeeId = employeeId,
        Account = HolidayAccount.Weeks1To5,
        EarningYear = year,
        Type = LedgerEntryType.Transfer,
        Days = days,
        Hours = hours,
        EntryDate = date,
        TransferId = transferId
    };
}