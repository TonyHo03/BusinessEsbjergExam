using Infra;

namespace Service;

/// <summary>
/// Optjenings- og afholdelsesperioder for de to konti.
/// En periode identificeres af sit optjeningsår (EarningYear):
///   Uge 1–5, år 2025:   optjent 1.9.2025–31.8.2026, kan bruges 1.9.2025–31.12.2026
///   6. ferieuge, år 2025: optjent 1.1.2025–31.12.2025, kan bruges 1.5.2026–30.4.2027
/// </summary>
public static class HolidayPeriods
{
    public static int EarningYearFor(HolidayAccount account, DateOnly date) => account switch
    {
        HolidayAccount.Weeks1To5 => date.Month >= 9 ? date.Year : date.Year - 1,
        HolidayAccount.Week6 => date.Year,
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };

    public static DateOnly EarningStart(HolidayAccount account, int earningYear) => account switch
    {
        HolidayAccount.Weeks1To5 => new DateOnly(earningYear, 9, 1),
        HolidayAccount.Week6 => new DateOnly(earningYear, 1, 1),
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };

    public static DateOnly EarningEnd(HolidayAccount account, int earningYear) => account switch
    {
        HolidayAccount.Weeks1To5 => new DateOnly(earningYear + 1, 8, 31),
        HolidayAccount.Week6 => new DateOnly(earningYear, 12, 31),
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };

    public static DateOnly UsageStart(HolidayAccount account, int earningYear) => account switch
    {
        HolidayAccount.Weeks1To5 => new DateOnly(earningYear, 9, 1),     // samtidighedsferie
        HolidayAccount.Week6 => new DateOnly(earningYear + 1, 5, 1),
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };

    /// <summary>Sidste dag timerne kan bruges (udløbsdatoen).</summary>
    public static DateOnly UsageEnd(HolidayAccount account, int earningYear) => account switch
    {
        HolidayAccount.Weeks1To5 => new DateOnly(earningYear + 1, 12, 31),
        HolidayAccount.Week6 => new DateOnly(earningYear + 2, 4, 30),
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };

    public static bool IsUsable(HolidayAccount account, int earningYear, DateOnly date) =>
        date >= UsageStart(account, earningYear) && date <= UsageEnd(account, earningYear);
}