using Infra;

namespace Service;

/// <summary>
/// Hovedferie: retten til 15 sammenhængende arbejdsdage mellem 1. maj og 30. september.
/// Weekender og helligdage inde i perioden bryder ikke sammenhængen.
/// </summary>
public static class MainHolidayRule
{
    public static DateOnly WindowStart(int year) => new(year, 5, 1);
    public static DateOnly WindowEnd(int year) => new(year, 9, 30);

    /// <summary>
    /// En ansøgning er hovedferie, hvis den er på uge 1–5, ligger helt inden for
    /// 1. maj – 30. september og har mindst det antal arbejdsdage, parameteren siger (15).
    /// </summary>
    public static bool IsMainHoliday(HolidayAccount account, RequestCalculation calc, ParameterResolver parameters)
    {
        if (account != HolidayAccount.Weeks1To5 || calc.Days.Count == 0)
            return false;

        var start = calc.Days[0].Date;
        var end = calc.Days[^1].Date;

        return start >= WindowStart(start.Year)
               && end <= WindowEnd(start.Year)
               && calc.WorkingDays >= parameters.GetInt(ParameterKeys.MainHolidayDays, start);
    }

    /// <summary>Har medarbejderen en godkendt hovedferie i det givne år?</summary>
    public static bool IsFulfilled(IEnumerable<RequestCalculation> approvedWeeks1To5, int year, ParameterResolver parameters) =>
        approvedWeeks1To5.Any(calc =>
            calc.Days.Count > 0
            && calc.Days[0].Date.Year == year
            && IsMainHoliday(HolidayAccount.Weeks1To5, calc, parameters));
}