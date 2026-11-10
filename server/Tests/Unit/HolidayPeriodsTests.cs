using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class HolidayPeriodsTests
{
    [Fact]
    public void Weeks1To5_earning_year_starts_1_september()
    {
        Assert.Equal(2025, HolidayPeriods.EarningYearFor(HolidayAccount.Weeks1To5, new DateOnly(2026, 8, 31)));
        Assert.Equal(2026, HolidayPeriods.EarningYearFor(HolidayAccount.Weeks1To5, new DateOnly(2026, 9, 1)));
    }

    [Fact]
    public void Week6_earning_year_is_the_calendar_year()
    {
        Assert.Equal(2026, HolidayPeriods.EarningYearFor(HolidayAccount.Week6, new DateOnly(2026, 1, 1)));
        Assert.Equal(2026, HolidayPeriods.EarningYearFor(HolidayAccount.Week6, new DateOnly(2026, 12, 31)));
    }

    [Fact]
    public void Weeks1To5_can_be_used_from_earning_start_until_31_december_the_following_year()
    {
        Assert.Equal(new DateOnly(2025, 9, 1), HolidayPeriods.UsageStart(HolidayAccount.Weeks1To5, 2025));
        Assert.Equal(new DateOnly(2026, 12, 31), HolidayPeriods.UsageEnd(HolidayAccount.Weeks1To5, 2025));
    }

    [Fact]
    public void Week6_earned_in_2026_can_be_used_from_1_may_2027_to_30_april_2028()
    {
        // Eksemplet fra casen
        Assert.Equal(new DateOnly(2027, 5, 1), HolidayPeriods.UsageStart(HolidayAccount.Week6, 2026));
        Assert.Equal(new DateOnly(2028, 4, 30), HolidayPeriods.UsageEnd(HolidayAccount.Week6, 2026));
    }

    [Fact]
    public void Week6_is_not_usable_before_1_may_the_following_year()
    {
        Assert.False(HolidayPeriods.IsUsable(HolidayAccount.Week6, 2026, new DateOnly(2027, 4, 30)));
        Assert.True(HolidayPeriods.IsUsable(HolidayAccount.Week6, 2026, new DateOnly(2027, 5, 1)));
    }

    [Fact]
    public void Weeks1To5_is_not_usable_after_expiry()
    {
        Assert.True(HolidayPeriods.IsUsable(HolidayAccount.Weeks1To5, 2025, new DateOnly(2026, 12, 31)));
        Assert.False(HolidayPeriods.IsUsable(HolidayAccount.Weeks1To5, 2025, new DateOnly(2027, 1, 1)));
    }
}