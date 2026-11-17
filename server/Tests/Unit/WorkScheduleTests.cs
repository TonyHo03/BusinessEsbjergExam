using Service;
using Xunit;

namespace Tests.Unit;

public class WorkScheduleTests
{
    [Fact]
    public void Hours_are_calculated_from_start_and_end_time()
    {
        var schedule = TestSchedules.FullTime();

        Assert.Equal(7.5m, schedule.HoursFor(DayOfWeek.Monday));  // 08:00–15:30
        Assert.Equal(7m, schedule.HoursFor(DayOfWeek.Friday));    // 08:00–15:00
    }

    [Fact]
    public void Full_time_week_is_37_hours()
    {
        Assert.Equal(37m, TestSchedules.FullTime().WeeklyHours);
    }

    [Fact]
    public void A_day_without_times_is_not_a_working_day()
    {
        var schedule = TestSchedules.Create(TestSchedules.LongAgo, "09:00-15:00", null, "09:00-15:00", null, null);

        Assert.Equal(0m, schedule.HoursFor(DayOfWeek.Tuesday));
        Assert.Equal(12m, schedule.WeeklyHours);
    }

    [Fact]
    public void Weekends_are_never_working_days()
    {
        var schedule = TestSchedules.FullTime();

        Assert.Equal(0m, schedule.HoursFor(DayOfWeek.Saturday));
        Assert.Equal(0m, schedule.HoursFor(DayOfWeek.Sunday));
    }

    [Fact]
    public void Minutes_give_hours_with_two_decimals()
    {
        var schedule = TestSchedules.SameEveryDay("08:00-14:20"); // 6 t 20 min

        Assert.Equal(6.33m, schedule.HoursFor(DayOfWeek.Monday));
    }

    [Fact]
    public void A_schedule_covers_its_period_including_both_ends()
    {
        var schedule = TestSchedules.Create(new DateOnly(2026, 1, 5), "08:00-16:00", null, null, null, null,
            to: new DateOnly(2026, 3, 29));

        Assert.False(schedule.Covers(new DateOnly(2026, 1, 4)));
        Assert.True(schedule.Covers(new DateOnly(2026, 1, 5)));
        Assert.True(schedule.Covers(new DateOnly(2026, 3, 29)));
        Assert.False(schedule.Covers(new DateOnly(2026, 3, 30)));
    }

    [Fact]
    public void A_schedule_without_end_date_runs_until_further_notice()
    {
        Assert.True(TestSchedules.FullTime().Covers(new DateOnly(2099, 1, 1)));
    }
}

public class ScheduleResolverTests
{
    // Fuldtid til og med 31.3.2026, derefter deltid 6 t om dagen
    private static readonly Infra.WorkSchedule FullTimeUntilMarch =
        TestSchedules.Create(TestSchedules.LongAgo, "08:00-15:30", "08:00-15:30", "08:00-15:30", "08:00-15:30", "08:00-15:00",
            to: new DateOnly(2026, 3, 31));

    private static readonly Infra.WorkSchedule PartTimeFromApril =
        TestSchedules.SameEveryDay("09:00-15:00", new DateOnly(2026, 4, 1));

    [Fact]
    public void Picks_the_schedule_whose_period_contains_the_date()
    {
        Infra.WorkSchedule[] history = [FullTimeUntilMarch, PartTimeFromApril];

        Assert.Equal(37m, ScheduleResolver.For(history, new DateOnly(2026, 3, 31)).WeeklyHours);
        Assert.Equal(30m, ScheduleResolver.For(history, new DateOnly(2026, 4, 1)).WeeklyHours);
    }

    [Fact]
    public void A_gap_in_the_history_is_an_error()
    {
        // Ingen skema efter 31.3.2026
        Assert.Throws<InvalidOperationException>(() =>
            ScheduleResolver.For([FullTimeUntilMarch], new DateOnly(2026, 4, 1)));
    }

    [Fact]
    public void When_periods_overlap_the_newest_wins()
    {
        var older = TestSchedules.FullTime();
        var newer = TestSchedules.SameEveryDay("09:00-15:00", new DateOnly(2026, 4, 1));

        Assert.Equal(30m, ScheduleResolver.For([older, newer], new DateOnly(2026, 6, 1)).WeeklyHours);
    }
}