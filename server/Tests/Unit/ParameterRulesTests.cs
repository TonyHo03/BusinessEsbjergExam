using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class ParameterRulesTests
{
    private static readonly DateOnly Today = new(2026, 11, 19);

    private static readonly RuleParameter[] Existing =
    [
        new() { Id = Guid.NewGuid(), Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.08", EffectiveFrom = new DateOnly(2020, 9, 1) }
    ];

    private static IReadOnlyList<string> Validate(string key, string value, DateOnly from) =>
        ParameterRules.Validate(key, value, from, Today, Existing);

    [Fact]
    public void A_new_value_from_a_future_date_is_allowed()
    {
        // Eksemplet fra afklaringen: 2,08 bliver til 2,16 fra 1.3.2027
        Assert.Empty(Validate(ParameterKeys.Weeks1To5DaysPerMonth, "2.16", new DateOnly(2027, 3, 1)));
    }

    [Fact]
    public void A_new_value_from_today_is_allowed()
    {
        Assert.Empty(Validate(ParameterKeys.Weeks1To5DaysPerMonth, "2.16", Today));
    }

    [Fact]
    public void A_change_cannot_apply_to_the_past()
    {
        Assert.Single(Validate(ParameterKeys.Weeks1To5DaysPerMonth, "2.16", Today.AddDays(-1)));
    }

    [Fact]
    public void Two_values_from_the_same_date_are_rejected()
    {
        var existing = Existing.Append(new RuleParameter
            { Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.16", EffectiveFrom = new DateOnly(2027, 3, 1) });

        Assert.Single(ParameterRules.Validate(ParameterKeys.Weeks1To5DaysPerMonth, "2.20", new DateOnly(2027, 3, 1), Today, existing));
    }

    [Fact]
    public void Unknown_keys_are_rejected()
    {
        Assert.Single(Validate("salary.per_hour", "200", Today));
    }

    [Theory]
    [InlineData(ParameterKeys.Weeks1To5DaysPerMonth, "2,16")] // komma er ikke decimaltegn
    [InlineData(ParameterKeys.Weeks1To5DaysPerMonth, "-1")]
    [InlineData(ParameterKeys.NoticeOtherMonths, "1.5")]
    [InlineData(ParameterKeys.TransferEnabled, "yes")]
    public void Values_of_the_wrong_kind_are_rejected(string key, string value)
    {
        Assert.Single(Validate(key, value, Today));
    }

    [Theory]
    [InlineData(ParameterKeys.TransferEnabled, "false")]
    [InlineData(ParameterKeys.NoticeOtherMonths, "2")]
    public void Values_of_the_right_kind_are_accepted(string key, string value)
    {
        Assert.Empty(Validate(key, value, Today));
    }

    [Fact]
    public void Only_values_not_yet_in_effect_can_be_deleted()
    {
        var upcoming = new RuleParameter { Key = ParameterKeys.TransferMaxDays, Value = "3", EffectiveFrom = Today.AddDays(1) };
        var inEffect = new RuleParameter { Key = ParameterKeys.TransferMaxDays, Value = "5", EffectiveFrom = Today };

        Assert.Null(ParameterRules.CanDelete(upcoming, Today));
        Assert.NotNull(ParameterRules.CanDelete(inEffect, Today));
    }

    [Fact]
    public void Every_key_in_ParameterKeys_has_a_definition()
    {
        var keys = typeof(ParameterKeys).GetFields().Select(f => (string)f.GetValue(null)!);

        Assert.All(keys, k => Assert.Contains(ParameterRules.Definitions, d => d.Key == k));
    }
}