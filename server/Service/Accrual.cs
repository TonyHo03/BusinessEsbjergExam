using Infra;

namespace Service;

public static class Accrual
{
    private const decimal FullTimeDayLength = 37m / 5m;

    public static decimal Accrued(
        HolidayAccount account,
        int earningYear,
        Employee employee,
        ParameterResolver parameters,
        DateOnly asOf)
    {
        var periodStart = HolidayPeriods.EarningStart(account, earningYear);
        var periodEnd = HolidayPeriods.EarningEnd(account, earningYear);

        var employedFrom = Max(periodStart, employee.StartDate);
        var employedTo = employee.EndDate is { } end ? Min(periodEnd, end) : periodEnd;

        if (employedFrom > employedTo)
            return 0m;

        var total = 0m;

        for (var monthStart = new DateOnly(employedFrom.Year, employedFrom.Month, 1);
             monthStart <= employedTo;
             monthStart = monthStart.AddMonths(1))
        {
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var firstDay = Max(monthStart, employedFrom);
            var lastDay = Min(monthEnd, employedTo);

            // Måneden er ikke slut endnu, så den er ikke optjent
            if (lastDay > asOf)
                break;

            var daysEmployed = lastDay.DayNumber - firstDay.DayNumber + 1;
            var daysInMonth = monthEnd.Day;
            var share = (decimal)daysEmployed / daysInMonth;

            total += MonthlyDays(account, parameters, monthStart) * share;
        }

        return total;
    }

    private static decimal MonthlyDays(HolidayAccount account, ParameterResolver parameters, DateOnly month) => account switch
    {
        HolidayAccount.Weeks1To5 =>
            parameters.GetDecimal(ParameterKeys.Weeks1To5DaysPerMonth, month),
        HolidayAccount.Week6 =>
            parameters.GetDecimal(ParameterKeys.Week6HoursPerMonth, month) / FullTimeDayLength,
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };

    private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;
    private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;
}