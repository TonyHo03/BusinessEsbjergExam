using Infra;
using LinqToDB;
using LinqToDB.Async;

namespace Service;

public record WorkScheduleDto(
    Guid Id,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    DayTimes Monday,
    DayTimes Tuesday,
    DayTimes Wednesday,
    DayTimes Thursday,
    DayTimes Friday,
    decimal WeeklyHours,
    bool IsCurrent);

public record WorkSchedulesResponse(DateOnly EmployeeStartDate, IReadOnlyList<WorkScheduleDto> Schedules);

public record WorkScheduleResult(IReadOnlyList<string> Errors, WorkScheduleDto? Created);

public class WorkScheduleService(AppDb db)
{
    public async Task<WorkSchedulesResponse?> GetAsync(Guid employeeId, DateOnly today)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee is null)
            return null;

        var schedules = await db.WorkSchedules
            .Where(s => s.EmployeeId == employeeId)
            .OrderByDescending(s => s.EffectiveFrom)
            .ToListAsync();

        return new WorkSchedulesResponse(employee.StartDate, schedules.Select(s => ToDto(s, today)).ToList());
    }

    public async Task<WorkScheduleResult?> AddAsync(Guid employeeId, WorkScheduleInput input, DateOnly today)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee is null)
            return null;

        var existing = await db.WorkSchedules.Where(s => s.EmployeeId == employeeId).ToListAsync();

        var (schedule, errors) = WorkScheduleRules.Build(employee, input, existing, today);
        if (schedule is null)
            return new WorkScheduleResult(errors, null);

        var toClose = WorkScheduleRules.ScheduleToClose(existing, schedule.EffectiveFrom);

        await using var transaction = db.BeginTransaction();

        if (toClose is not null)
            await db.WorkSchedules
                .Where(s => s.Id == toClose.Id)
                .Set(s => s.EffectiveTo, schedule.EffectiveFrom.AddDays(-1))
                .UpdateAsync();

        await db.InsertAsync(schedule);

        transaction.Commit();

        return new WorkScheduleResult([], ToDto(schedule, today));
    }
    
    public async Task<string?> DeleteAsync(Guid employeeId, Guid scheduleId, DateOnly today)
    {
        var existing = await db.WorkSchedules.Where(s => s.EmployeeId == employeeId).ToListAsync();
        var schedule = existing.FirstOrDefault(s => s.Id == scheduleId) ?? throw new KeyNotFoundException();

        var error = WorkScheduleRules.CanDelete(schedule, existing, today);
        if (error is not null)
            return error;

        var toReopen = WorkScheduleRules.ScheduleToReopen(existing, schedule);

        await using var transaction = db.BeginTransaction();

        await db.WorkSchedules.Where(s => s.Id == scheduleId).DeleteAsync();

        if (toReopen is not null)
            await db.WorkSchedules
                .Where(s => s.Id == toReopen.Id)
                .Set(s => s.EffectiveTo, (DateOnly?)null)
                .UpdateAsync();

        transaction.Commit();
        return null;
    }

    private static WorkScheduleDto ToDto(WorkSchedule s, DateOnly today) => new(
        s.Id,
        s.EffectiveFrom,
        s.EffectiveTo,
        new DayTimes(WorkScheduleRules.Format(s.MonStart), WorkScheduleRules.Format(s.MonEnd)),
        new DayTimes(WorkScheduleRules.Format(s.TueStart), WorkScheduleRules.Format(s.TueEnd)),
        new DayTimes(WorkScheduleRules.Format(s.WedStart), WorkScheduleRules.Format(s.WedEnd)),
        new DayTimes(WorkScheduleRules.Format(s.ThuStart), WorkScheduleRules.Format(s.ThuEnd)),
        new DayTimes(WorkScheduleRules.Format(s.FriStart), WorkScheduleRules.Format(s.FriEnd)),
        s.WeeklyHours,
        s.Covers(today));
}