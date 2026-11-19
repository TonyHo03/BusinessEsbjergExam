using Infra;

namespace Service;

/// <summary>
/// Én række i bogholderiets medarbejderoversigt. Timer er omregnet med daglængden på AsOf.
/// Har medarbejderen intet arbejdsskema på datoen, kan timerne ikke regnes ud, og felterne er null.
/// </summary>
/// <param name="ExpiringHours">Timer i perioder, der snart udløber eller er udløbet (skal afholdes, overføres eller udbetales).</param>
/// <param name="ExpiresOn">Den tidligste udløbsdato blandt de perioder.</param>
public record EmployeeOverviewRow(
    Guid EmployeeId,
    string Name,
    string Email,
    Roles Role,
    Status Status,
    bool HasSchedule,
    decimal? DayLength,
    decimal? AvailableWeeks1To5,
    decimal? AvailableWeek6,
    decimal PendingHours,
    decimal? ExpiringHours,
    DateOnly? ExpiresOn);

public static class EmployeeOverview
{
    public static EmployeeOverviewRow Build(
        Employee employee,
        IReadOnlyList<WorkSchedule> schedules,
        ParameterResolver parameters,
        IReadOnlyList<LedgerEntry> entries,
        decimal pendingWeeks1To5,
        decimal pendingWeek6,
        DateOnly asOf)
    {
        var pendingHours = Math.Round(pendingWeeks1To5 + pendingWeek6, 2);

        if (!schedules.Any(s => s.Covers(asOf)))
            return new EmployeeOverviewRow(employee.Id, employee.Name, employee.Email, employee.Role, employee.Status,
                false, null, null, null, pendingHours, null, null);

        var dayLength = ScheduleResolver.DayLength(schedules, asOf);
        var balances = Balances.For(employee, parameters, entries, asOf);
        var expiring = balances.Where(b => b.NeedsAction(asOf)).ToList();

        return new EmployeeOverviewRow(
            employee.Id,
            employee.Name,
            employee.Email,
            employee.Role,
            employee.Status,
            true,
            Math.Round(dayLength, 2),
            Math.Round(Balances.AvailableOn(balances, HolidayAccount.Weeks1To5, asOf) * dayLength - pendingWeeks1To5, 2),
            Math.Round(Balances.AvailableOn(balances, HolidayAccount.Week6, asOf) * dayLength - pendingWeek6, 2),
            pendingHours,
            Math.Round(expiring.Sum(b => b.Remaining) * dayLength, 2),
            expiring.Count > 0 ? expiring.Min(b => b.ExpiresOn) : null);
    }
}