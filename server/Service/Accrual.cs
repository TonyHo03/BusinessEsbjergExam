using Infra;

namespace Service;

/// <summary>
/// Beregner hvor mange feriedage en medarbejder har optjent i én optjeningsperiode.
/// Optjening gemmes ikke, den beregnes ud fra ansættelse og parametre.
///
/// Saldoen føres i dage, fordi ferie er en ret til dage. Timerne regnes først ud, når
/// saldoen vises eller bruges – med daglængden fra det skema, der gælder på datoen.
/// Skifter en medarbejder skema, er antallet af dage derfor det samme, og timerne følger med.
///
/// Regler:
/// - En måned tæller først, når den er slut (eller ansættelsen er slut i den måned).
/// - Uge 1–5: dage pr. måned (2,08), uanset om man er på fuld tid eller deltid.
/// - 6. ferieuge: timer pr. måned for en fuldtidsansat (3,083) omregnet til dage á 7,4 t.
/// - Starter eller stopper man midt i en måned, optjener man for de kalenderdage, man var ansat.
/// - Parametre slås op på den måned, dagene optjenes i.
/// </summary>
public static class Accrual
{
    /// <summary>En fuldtidsansat feriedag: 37 t / 5.</summary>
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