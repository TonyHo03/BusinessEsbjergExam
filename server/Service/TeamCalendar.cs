using Infra;

namespace Service;

/// <summary>Én medarbejders godkendte ferie. Kun datoer – konto og kommentar er private.</summary>
public record TeamAbsence(DateOnly StartDate, DateOnly EndDate);

public record TeamMember(Guid EmployeeId, string Name, IReadOnlyList<TeamAbsence> Absences);

/// <summary>Hvor mange der er væk og til stede på en dag. På weekender og fridage er begge 0.</summary>
public record TeamDay(DateOnly Date, bool IsWorkingDay, int Absent, int Present);

public record TeamCalendarResponse(
    DateOnly From,
    DateOnly To,
    IReadOnlyList<TeamMember> Members,
    IReadOnlyList<TeamDay> Days);

/// <summary>
/// Bygger holdkalenderen: alles godkendte ferie i en periode og antal væk/til stede pr. dag.
/// Alle medarbejdere må se alles planlagte ferie.
/// </summary>
public static class TeamCalendar
{
    public static TeamCalendarResponse Build(
        IEnumerable<Employee> employees,
        IEnumerable<HolidayRequest> requests,
        IReadOnlySet<DateOnly> daysOff,
        DateOnly from,
        DateOnly to)
    {
        if (to < from)
            throw new ArgumentException("Slutdatoen kan ikke ligge før startdatoen.");

        // Kun medarbejdere, der er ansat i en del af perioden
        var employed = employees
            .Where(e => e.StartDate <= to && (e.EndDate is null || e.EndDate >= from))
            .OrderBy(e => e.Name)
            .ToList();

        // Ferie med en åben annulleringsanmodning er stadig godkendt, indtil bogholderiet har svaret
        var approved = requests
            .Where(r => (r.Status == RequestStatus.Approved || r.Status == RequestStatus.CancellationRequested)
                        && r.StartDate <= to && r.EndDate >= from)
            .ToList();

        var members = employed
            .Select(e => new TeamMember(
                e.Id,
                e.Name,
                approved
                    .Where(r => r.EmployeeId == e.Id)
                    .OrderBy(r => r.StartDate)
                    .Select(r => new TeamAbsence(r.StartDate, r.EndDate))
                    .ToList()))
            .ToList();

        var days = new List<TeamDay>();

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var isWorkingDay = date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)
                               && !daysOff.Contains(date);

            if (!isWorkingDay)
            {
                days.Add(new TeamDay(date, false, 0, 0));
                continue;
            }

            var day = date;
            var employedThatDay = employed
                .Where(e => e.StartDate <= day && (e.EndDate is null || e.EndDate >= day))
                .Select(e => e.Id)
                .ToHashSet();

            var absent = approved
                .Where(r => employedThatDay.Contains(r.EmployeeId) && r.StartDate <= day && r.EndDate >= day)
                .Select(r => r.EmployeeId)
                .Distinct()
                .Count();

            days.Add(new TeamDay(date, true, absent, employedThatDay.Count - absent));
        }

        return new TeamCalendarResponse(from, to, members, days);
    }
}