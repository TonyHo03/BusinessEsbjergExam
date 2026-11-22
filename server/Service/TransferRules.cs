using Infra;

namespace Service;

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
            return ["Transferring holiday is turned off."];

        var errors = new List<string>();
        var maxDays = parameters.GetDecimal(ParameterKeys.TransferMaxDays, today);

        if (days <= 0)
            errors.Add("The number of days must be greater than 0.");

        var period = balances.FirstOrDefault(b => b.Account == HolidayAccount.Weeks1To5 && b.EarningYear == fromEarningYear);

        if (period is null || period.ExpiresOn.Year != today.Year || today > period.ExpiresOn)
        {
            errors.Add($"Only holiday that expires on 31.12.{today.Year} can be transferred.");
            return errors;
        }

        if (days > period.Remaining)
            errors.Add($"There are only {Math.Round(period.Remaining, 2)} days left in the period.");

        // Kun ferie ud over 4 uger (20 dage) kan overføres, og højst maxDays
        var limit = Math.Min(maxDays, Math.Max(period.Accrued - 20m, 0m));
        if (alreadyTransferred + days > limit)
            errors.Add($"At most {Math.Round(limit, 2)} days can be transferred (holiday above 4 weeks). {Math.Round(alreadyTransferred, 2)} are already transferred or requested.");

        return errors;
    }

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