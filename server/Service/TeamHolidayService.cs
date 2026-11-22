using Infra;
using LinqToDB;
using LinqToDB.Async;

namespace Service;

public class TeamHolidayService(AppDb db)
{
    public async Task<TeamCalendarResponse> GetAsync(DateOnly from, DateOnly to)
    {
        var employees = await db.Employees
            .Where(e => e.StartDate <= to && (e.EndDate == null || e.EndDate >= from))
            .ToListAsync();

        // Ferie med en åben annulleringsanmodning er stadig godkendt, indtil bogholderiet har svaret
        var requests = await db.HolidayRequests
            .Where(r => (r.Status == RequestStatus.Approved || r.Status == RequestStatus.CancellationRequested)
                        && r.StartDate <= to && r.EndDate >= from)
            .ToListAsync();

        var daysOff = await db.DaysOff
            .Where(d => d.Date >= from && d.Date <= to)
            .Select(d => d.Date)
            .ToListAsync();

        return TeamCalendar.Build(employees, requests, daysOff.ToHashSet(), from, to);
    }
}