using Infra;

namespace Service;

public static class MainHolidayRule
{
    public static DateOnly WindowStart(int year) => new(year, 5, 1);
    public static DateOnly WindowEnd(int year) => new(year, 9, 30);

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

    public static bool IsFulfilled(IEnumerable<RequestCalculation> approvedWeeks1To5, int year, ParameterResolver parameters) =>
        approvedWeeks1To5.Any(calc =>
            calc.Days.Count > 0
            && calc.Days[0].Date.Year == year
            && IsMainHoliday(HolidayAccount.Weeks1To5, calc, parameters));
}