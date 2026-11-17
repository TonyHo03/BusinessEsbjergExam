using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class ParameterResolverTests
{
    // Eksemplet fra afklaringen: optjeningen ændres fra 2,08 til 2,16 dage pr. måned
    private static readonly ParameterResolver Resolver = new(
    [
        new RuleParameter { Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.08", EffectiveFrom = new DateOnly(2020, 9, 1) },
        new RuleParameter { Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.16", EffectiveFrom = new DateOnly(2027, 9, 1) },
        new RuleParameter { Key = ParameterKeys.NoticeOtherMonths, Value = "1", EffectiveFrom = new DateOnly(2020, 9, 1) }
    ]);

    [Fact]
    public void Uses_old_value_the_day_before_a_change()
    {
        Assert.Equal(2.08m, Resolver.GetDecimal(ParameterKeys.Weeks1To5DaysPerMonth, new DateOnly(2027, 8, 31)));
    }

    [Fact]
    public void Uses_new_value_from_its_effective_date()
    {
        Assert.Equal(2.16m, Resolver.GetDecimal(ParameterKeys.Weeks1To5DaysPerMonth, new DateOnly(2027, 9, 1)));
    }

    [Fact]
    public void Reads_integer_values()
    {
        Assert.Equal(1, Resolver.GetInt(ParameterKeys.NoticeOtherMonths, new DateOnly(2026, 11, 10)));
    }

    [Fact]
    public void Throws_when_no_value_existed_yet()
    {
        Assert.Throws<KeyNotFoundException>(() =>
            Resolver.Get(ParameterKeys.Weeks1To5DaysPerMonth, new DateOnly(2019, 1, 1)));
    }

    [Fact]
    public void Throws_for_unknown_key()
    {
        Assert.Throws<KeyNotFoundException>(() => Resolver.Get("does.not.exist", new DateOnly(2026, 1, 1)));
    }
}