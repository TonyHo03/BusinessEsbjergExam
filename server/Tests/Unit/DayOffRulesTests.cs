using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class DayOffRulesTests
{
    private static readonly DateOnly Today = new(2026, 11, 22);

    private static readonly DayOff[] Existing =
    [
        new() { Id = Guid.NewGuid(), Date = new DateOnly(2026, 12, 24), Name = "Juleaftensdag", IsCompany = true }
    ];

    [Fact]
    public void A_future_weekday_with_a_name_is_allowed()
    {
        Assert.Empty(DayOffRules.Validate(new DateOnly(2026, 12, 23), "Lillejuleaften", Today, Existing));
    }

    [Fact]
    public void A_day_off_cannot_change_the_past()
    {
        Assert.Single(DayOffRules.Validate(new DateOnly(2026, 11, 20), "Fredag", Today, Existing));
        Assert.NotNull(DayOffRules.CanDelete(new DayOff { Date = new DateOnly(2026, 1, 1) }, Today));
        Assert.Null(DayOffRules.CanDelete(new DayOff { Date = new DateOnly(2026, 12, 24) }, Today));
    }

    [Fact]
    public void The_same_date_cannot_be_added_twice()
    {
        Assert.Single(DayOffRules.Validate(new DateOnly(2026, 12, 24), "Juleaften", Today, Existing));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void A_name_is_required(string name)
    {
        Assert.Single(DayOffRules.Validate(new DateOnly(2026, 12, 23), name, Today, Existing));
    }

    [Fact]
    public void Weekends_are_not_added()
    {
        Assert.Single(DayOffRules.Validate(new DateOnly(2026, 12, 26), "Lørdag", Today, Existing));
    }
}