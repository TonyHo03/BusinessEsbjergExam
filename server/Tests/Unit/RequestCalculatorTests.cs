using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class RequestCalculatorTests
{
    // Business Esbjergs standardskema: Man–Tor 7,5 t, Fre 7 t
    private static readonly WorkSchedule FullTime = new()
    {
        EffectiveFrom = new DateOnly(2020, 1, 1),
        Mon = 7.5m, Tue = 7.5m, Wed = 7.5m, Thu = 7.5m, Fri = 7m
    };

    private static readonly IReadOnlySet<DateOnly> NoDaysOff = new HashSet<DateOnly>();

    private static RequestCalculation Calc(DateOnly start, DateOnly end,
        IReadOnlySet<DateOnly>? daysOff = null, params WorkSchedule[] schedules) =>
        RequestCalculator.Calculate(start, end,
            schedules.Length > 0 ? schedules : [FullTime],
            daysOff ?? NoDaysOff);

    [Fact]
    public void A_friday_costs_7_hours()
    {
        var result = Calc(new DateOnly(2026, 11, 13), new DateOnly(2026, 11, 13));

        Assert.Equal(7m, result.TotalHours);
        Assert.Equal(1, result.WorkingDays);
    }

    [Fact]
    public void A_tuesday_costs_7_5_hours()
    {
        var result = Calc(new DateOnly(2026, 11, 17), new DateOnly(2026, 11, 17));

        Assert.Equal(7.5m, result.TotalHours);
    }

    [Fact]
    public void A_full_week_costs_37_hours()
    {
        var result = Calc(new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 20));

        Assert.Equal(37m, result.TotalHours);
        Assert.Equal(5, result.WorkingDays);
    }

    [Fact]
    public void A_weekend_costs_nothing()
    {
        var result = Calc(new DateOnly(2026, 11, 14), new DateOnly(2026, 11, 15));

        Assert.Equal(0m, result.TotalHours);
        Assert.Equal(0, result.WorkingDays);
        Assert.All(result.Days, d => Assert.Equal(DayKind.NonWorkingDay, d.Kind));
    }

    [Fact]
    public void A_company_day_off_costs_nothing()
    {
        var christmasEve = new DateOnly(2026, 12, 24);

        var result = Calc(christmasEve, christmasEve, new HashSet<DateOnly> { christmasEve });

        Assert.Equal(0m, result.TotalHours);
        Assert.Equal(DayKind.DayOff, result.Days.Single().Kind);
    }

    [Fact]
    public void Christmas_example_from_the_sketch_costs_52_hours()
    {
        // 15.12–23.12.2026: Tir–Fre + Man–Ons = 7 arbejdsdage
        var result = Calc(new DateOnly(2026, 12, 15), new DateOnly(2026, 12, 23));

        Assert.Equal(52m, result.TotalHours);
        Assert.Equal(7, result.WorkingDays);
        Assert.Equal(9, result.Days.Count);
    }

    [Fact]
    public void Part_time_employee_uses_own_schedule()
    {
        var partTime = new WorkSchedule
        {
            EffectiveFrom = new DateOnly(2020, 1, 1),
            Mon = 6m, Tue = 6m, Wed = 0m, Thu = 6m, Fri = 0m
        };

        var result = Calc(new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 20), null, partTime);

        Assert.Equal(18m, result.TotalHours);
        Assert.Equal(3, result.WorkingDays);
    }

    [Fact]
    public void Uses_the_schedule_that_applies_on_each_day()
    {
        var newSchedule = new WorkSchedule
        {
            EffectiveFrom = new DateOnly(2026, 11, 19), // fra torsdag: kun 5 t om dagen
            Mon = 5m, Tue = 5m, Wed = 5m, Thu = 5m, Fri = 5m
        };

        // Man–Ons på gammelt skema (3 × 7,5) + Tor–Fre på nyt (2 × 5)
        var result = Calc(new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 20), null, FullTime, newSchedule);

        Assert.Equal(32.5m, result.TotalHours);
    }

    [Fact]
    public void End_before_start_is_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            Calc(new DateOnly(2026, 12, 23), new DateOnly(2026, 12, 15)));
    }

    [Fact]
    public void Missing_schedule_is_rejected()
    {
        var future = new WorkSchedule { EffectiveFrom = new DateOnly(2030, 1, 1), Mon = 7.5m };

        Assert.Throws<InvalidOperationException>(() =>
            Calc(new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 16), null, future));
    }
}