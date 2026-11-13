using Infra;

namespace Service;

/// <summary>
/// Beregner hvor mange timer en medarbejder har optjent i én optjeningsperiode.
/// Optjening gemmes ikke, den beregnes ud fra ansættelse, arbejdsskema og parametre.
///
/// Regler:
/// - En måned tæller først, når den er slut (eller ansættelsen er slut i den måned).
/// - Uge 1–5: dage pr. måned × (ugentlige timer / 5).
/// - 6. ferieuge: timer pr. måned × (ugentlige timer / 37).
/// - Starter eller stopper man midt i en måned, optjener man for de kalenderdage, man var ansat.
/// - Skema og parametre slås op på den måned, timerne optjenes i.
/// </summary>
public static class Accrual
{
    private const decimal FullTimeWeeklyHours = 37m;

    public static decimal Accrued(
        HolidayAccount account,
        int earningYear,
        Employee employee,
        IReadOnlyList<WorkSchedule> schedules,
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

            var weeklyHours = WeeklyHours(ScheduleResolver.For(schedules, firstDay));
            total += MonthlyHours(account, weeklyHours, parameters, monthStart) * share;
        }

        return total;
    }

    private static decimal MonthlyHours(HolidayAccount account, decimal weeklyHours,
        ParameterResolver parameters, DateOnly month) => account switch
    {
        HolidayAccount.Weeks1To5 =>
            parameters.GetDecimal(ParameterKeys.Weeks1To5DaysPerMonth, month) * weeklyHours / 5m,
        HolidayAccount.Week6 =>
            parameters.GetDecimal(ParameterKeys.Week6HoursPerMonth, month) * weeklyHours / FullTimeWeeklyHours,
        _ => throw new ArgumentOutOfRangeException(nameof(account))
    };

    private static decimal WeeklyHours(WorkSchedule s) =>
        s.Mon + s.Tue + s.Wed + s.Thu + s.Fri + s.Sat + s.Sun;

    private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;
    private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;
}