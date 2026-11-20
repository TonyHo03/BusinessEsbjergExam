using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class WorkScheduleRulesTests
{
    private static readonly DateOnly Today = new(2026, 11, 20);

    private static readonly Employee Mette = new()
        { Id = Guid.NewGuid(), Name = "Mette", Role = Roles.Employee, StartDate = new DateOnly(2025, 9, 1) };

    private static readonly DayTimes Normal = new("08:00", "15:30");
    private static readonly DayTimes Off = new(null, null);

    private static WorkScheduleInput Input(DateOnly from, DayTimes? mon = null, DayTimes? fri = null) =>
        new(from, mon ?? Normal, Normal, Normal, Normal, fri ?? new DayTimes("08:00", "15:00"));

    private static (WorkSchedule? Schedule, IReadOnlyList<string> Errors) Build(WorkScheduleInput input, params WorkSchedule[] existing) =>
        WorkScheduleRules.Build(Mette, input, existing, Today);

    [Fact]
    public void A_first_schedule_can_start_on_the_employment_date()
    {
        var (schedule, errors) = Build(Input(Mette.StartDate));

        Assert.Empty(errors);
        Assert.Equal(37m, schedule!.WeeklyHours);
        Assert.Null(schedule.EffectiveTo);
    }

    [Fact]
    public void A_schedule_cannot_start_before_the_employee_started()
    {
        Assert.Single(Build(Input(Mette.StartDate.AddDays(-1))).Errors);
    }

    [Fact]
    public void A_new_schedule_cannot_change_the_past()
    {
        var current = TestSchedules.FullTime(Mette.StartDate);

        Assert.Single(Build(Input(Today.AddDays(-1)), current).Errors);
        Assert.Empty(Build(Input(Today), current).Errors);
    }

    [Fact]
    public void A_new_schedule_must_start_after_the_latest_one()
    {
        var upcoming = TestSchedules.FullTime(new DateOnly(2027, 1, 1));

        Assert.Single(Build(Input(new DateOnly(2026, 12, 1)), upcoming).Errors);
    }

    [Fact]
    public void Days_off_are_left_empty()
    {
        var (schedule, errors) = Build(Input(Mette.StartDate, mon: Off, fri: Off));

        Assert.Empty(errors);
        Assert.Equal(22.5m, schedule!.WeeklyHours); // tir–tor 3 × 7,5 t
        Assert.Equal(0m, schedule.HoursFor(DayOfWeek.Monday));
    }

    [Fact]
    public void At_least_one_working_day_is_required()
    {
        var input = new WorkScheduleInput(Mette.StartDate, Off, Off, Off, Off, Off);

        Assert.Single(Build(input).Errors);
    }

    [Theory]
    [InlineData("08:00", null)]    // kun starttid
    [InlineData("8", "15:30")]     // forkert format
    [InlineData("15:30", "08:00")] // slut før start
    public void Invalid_times_are_rejected(string? start, string? end)
    {
        Assert.Single(Build(Input(Mette.StartDate, mon: new DayTimes(start, end))).Errors);
    }
    
    [Fact]
    public void Only_the_latest_upcoming_schedule_can_be_deleted()
    {
        var current = TestSchedules.FullTime(Mette.StartDate);
        var upcoming = TestSchedules.SameEveryDay("09:00-15:00", new DateOnly(2027, 1, 1));
        var later = TestSchedules.FullTime(new DateOnly(2027, 6, 1));

        Assert.Null(WorkScheduleRules.CanDelete(upcoming, [current, upcoming], Today));
        Assert.NotNull(WorkScheduleRules.CanDelete(current, [current, upcoming], Today));           // gælder allerede
        Assert.NotNull(WorkScheduleRules.CanDelete(upcoming, [current, upcoming, later], Today));   // ikke det seneste
    }

    [Fact]
    public void Deleting_reopens_the_schedule_before_it()
    {
        var current = TestSchedules.Create(Mette.StartDate, "08:00-15:30", "08:00-15:30", "08:00-15:30", "08:00-15:30", "08:00-15:00",
            to: new DateOnly(2026, 12, 31));
        var upcoming = TestSchedules.SameEveryDay("09:00-15:00", new DateOnly(2027, 1, 1));

        Assert.Equal(current, WorkScheduleRules.ScheduleToReopen([current, upcoming], upcoming));
    }

    [Fact]
    public void The_current_open_schedule_is_closed_the_day_before()
    {
        var current = TestSchedules.FullTime(Mette.StartDate);

        Assert.Equal(current, WorkScheduleRules.ScheduleToClose([current], new DateOnly(2027, 1, 1)));
    }
}