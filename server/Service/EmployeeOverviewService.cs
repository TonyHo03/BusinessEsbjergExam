using Infra;
using LinqToDB;
using LinqToDB.Async;

namespace Service;

public class EmployeeOverviewService(AppDb db)
{
    public async Task<IReadOnlyList<EmployeeOverviewRow>> GetAsync(DateOnly asOf)
    {
        // Accountant- og system owner-kontiene har ingen ferie, så de er ikke med i overblikket
        var employees = await db.Employees.Where(e => e.Role == Roles.Employee).OrderBy(e => e.Name).ToListAsync();
        var schedules = (await db.WorkSchedules.ToListAsync()).ToLookup(s => s.EmployeeId);
        var entries = (await db.LedgerEntries.ToListAsync()).ToLookup(e => e.EmployeeId);
        var pending = (await db.HolidayRequests.Where(r => r.Status == RequestStatus.Pending).ToListAsync())
            .ToLookup(r => r.EmployeeId);
        var parameters = new ParameterResolver(await db.RuleParameters.ToListAsync());

        return employees
            .Select(e => EmployeeOverview.Build(
                e,
                schedules[e.Id].ToList(),
                parameters,
                entries[e.Id].ToList(),
                pending[e.Id].Where(r => r.Account == HolidayAccount.Weeks1To5).Sum(r => r.Hours),
                pending[e.Id].Where(r => r.Account == HolidayAccount.Week6).Sum(r => r.Hours),
                asOf))
            .ToList();
    }
}