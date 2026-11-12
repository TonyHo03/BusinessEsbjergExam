using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class MainHolidayRuleTests
{
    private static readonly WorkSchedule[] FullTime =
    [
        new() { EffectiveFrom = new DateOnly(2020, 1, 1), Mon = 7.5m, Tue = 7.5m, Wed = 7.5m, Thu = 7.5m, Fri = 7m }
    ];

    private static readonly ParameterResolver Parameters = new(
    [
        new RuleParameter { Key = ParameterKeys.MainHolidayDays, Value = "15", EffectiveFrom = new DateOnly(2020, 1, 1) }
    ]);

    private static RequestCalculation Calc(DateOnly start, DateOnly end) =>
        RequestCalculator.Calculate(start, end, FullTime, new HashSet<DateOnly>());

    [Fact]
    public void Three_weeks_in_july_is_main_holiday()
    {
        // 6.7–24.7.2026 = 15 arbejdsdage
        Assert.True(MainHolidayRule.IsMainHoliday(HolidayAccount.Weeks1To5,
            Calc(new DateOnly(2026, 7, 6), new DateOnly(2026, 7, 24)), Parameters));
    }

    [Fact]
    public void Two_weeks_is_not_main_holiday()
    {
        Assert.False(MainHolidayRule.IsMainHoliday(HolidayAccount.Weeks1To5,
            Calc(new DateOnly(2026, 7, 6), new DateOnly(2026, 7, 17)), Parameters));
    }

    [Fact]
    public void Holiday_outside_1_may_to_30_september_is_not_main_holiday()
    {
        Assert.False(MainHolidayRule.IsMainHoliday(HolidayAccount.Weeks1To5,
            Calc(new DateOnly(2026, 9, 21), new DateOnly(2026, 10, 9)), Parameters));
    }

    [Fact]
    public void Week6_is_never_main_holiday()
    {
        Assert.False(MainHolidayRule.IsMainHoliday(HolidayAccount.Week6,
            Calc(new DateOnly(2026, 7, 6), new DateOnly(2026, 7, 24)), Parameters));
    }

    [Fact]
    public void Main_holiday_is_fulfilled_when_an_approved_request_qualifies()
    {
        var approved = new[]
        {
            Calc(new DateOnly(2026, 3, 2), new DateOnly(2026, 3, 3)),
            Calc(new DateOnly(2026, 7, 6), new DateOnly(2026, 7, 24))
        };

        Assert.True(MainHolidayRule.IsFulfilled(approved, 2026, Parameters));
        Assert.False(MainHolidayRule.IsFulfilled(approved, 2027, Parameters));
    }

    [Fact]
    public void Main_holiday_is_not_fulfilled_by_short_holidays()
    {
        var approved = new[] { Calc(new DateOnly(2026, 7, 6), new DateOnly(2026, 7, 10)) };

        Assert.False(MainHolidayRule.IsFulfilled(approved, 2026, Parameters));
    }
}