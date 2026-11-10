using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class ParameterResolverTests
{
    private static readonly ParameterResolver Resolver = new(
    [
        new RuleParameter { Key = ParameterKeys.SupplementRaisedPercent, Value = "0.95", EffectiveFrom = new DateOnly(2020, 9, 1) },
        new RuleParameter { Key = ParameterKeys.SupplementRaisedPercent, Value = "1.48", EffectiveFrom = new DateOnly(2024, 5, 1) },
        new RuleParameter { Key = ParameterKeys.NoticeOtherMonths, Value = "1", EffectiveFrom = new DateOnly(2020, 9, 1) }
    ]);

    [Fact]
    public void Uses_old_value_the_day_before_a_change()
    {
        Assert.Equal(0.95m, Resolver.GetDecimal(ParameterKeys.SupplementRaisedPercent, new DateOnly(2024, 4, 30)));
    }

    [Fact]
    public void Uses_new_value_from_its_effective_date()
    {
        Assert.Equal(1.48m, Resolver.GetDecimal(ParameterKeys.SupplementRaisedPercent, new DateOnly(2024, 5, 1)));
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
            Resolver.Get(ParameterKeys.SupplementRaisedPercent, new DateOnly(2019, 1, 1)));
    }

    [Fact]
    public void Throws_for_unknown_key()
    {
        Assert.Throws<KeyNotFoundException>(() => Resolver.Get("does.not.exist", new DateOnly(2026, 1, 1)));
    }
}