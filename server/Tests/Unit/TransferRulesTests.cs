using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class TransferRulesTests
{
    private static readonly DateOnly Today = new(2026, 11, 18);

    // 2025-perioden udløber 31.12.2026: 25 dage optjent, 10 tilbage. 2026-perioden udløber først 31.12.2027
    private static readonly PeriodBalance[] Balances =
    [
        new(HolidayAccount.Weeks1To5, 2025, 25m, -15m),
        new(HolidayAccount.Weeks1To5, 2026, 4.16m, 0m)
    ];

    private static ParameterResolver Parameters(string enabled = "true") => new(
    [
        new RuleParameter { Key = ParameterKeys.TransferEnabled, Value = enabled, EffectiveFrom = new DateOnly(2020, 1, 1) },
        new RuleParameter { Key = ParameterKeys.TransferMaxDays, Value = "5", EffectiveFrom = new DateOnly(2020, 1, 1) }
    ]);

    private static IReadOnlyList<string> Validate(int year, decimal days, decimal already = 0m, string enabled = "true", DateOnly? today = null) =>
        TransferRules.Validate(Balances, year, days, already, Parameters(enabled), today ?? Today);

    [Fact]
    public void Up_to_five_days_from_the_expiring_period_is_allowed()
    {
        Assert.Empty(Validate(2025, 5m));
    }

    [Fact]
    public void More_than_five_days_is_rejected()
    {
        Assert.Single(Validate(2025, 6m));
    }

    [Fact]
    public void Already_transferred_days_count_against_the_limit()
    {
        Assert.Empty(Validate(2025, 2m, already: 3m));
        Assert.Single(Validate(2025, 3m, already: 3m));
    }

    [Fact]
    public void Cannot_transfer_more_than_is_left()
    {
        var balances = new[] { new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 25m, -22m) }; // 3 dage

        Assert.Single(TransferRules.Validate(balances, 2025, 4m, 0m, Parameters(), Today));
    }

    [Fact]
    public void Only_holiday_above_four_weeks_can_be_transferred()
    {
        // 24,96 dage optjent: kun 4,96 ligger ud over 4 uger
        var balances = new[] { new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 24.96m, -15m) };

        Assert.Empty(TransferRules.Validate(balances, 2025, 4.96m, 0m, Parameters(), Today));
        Assert.Single(TransferRules.Validate(balances, 2025, 5m, 0m, Parameters(), Today));
    }

    [Fact]
    public void Only_the_period_expiring_this_year_can_be_transferred()
    {
        Assert.Single(Validate(2026, 1m)); // udløber først 31.12.2027
    }

    [Fact]
    public void Cannot_transfer_after_the_period_has_expired()
    {
        Assert.Single(Validate(2025, 1m, today: new DateOnly(2027, 1, 4)));
    }

    [Fact]
    public void Zero_days_is_rejected()
    {
        Assert.Single(Validate(2025, 0m));
    }

    [Fact]
    public void Nothing_is_allowed_when_the_feature_is_turned_off()
    {
        var errors = Validate(2025, 1m, enabled: "false");

        Assert.Equal("Transferring holiday is turned off.", Assert.Single(errors));
    }

    [Fact]
    public void Approved_transfer_moves_the_days_to_the_next_earning_year()
    {
        var entries = TransferRules.Entries(Guid.NewGuid(), Guid.NewGuid(), 2025, 5m, 7.4m, Today);

        Assert.Equal(2, entries.Count);
        Assert.Equal((2025, -5m, -37m), (entries[0].EarningYear, entries[0].Days, entries[0].Hours));
        Assert.Equal((2026, 5m, 37m), (entries[1].EarningYear, entries[1].Days, entries[1].Hours));
        Assert.All(entries, e => Assert.Equal(LedgerEntryType.Transfer, e.Type));
    }

    [Fact]
    public void Transferred_days_can_be_used_a_year_longer()
    {
        var mette = new Employee { Id = Guid.NewGuid(), StartDate = new DateOnly(2025, 9, 1), Role = Roles.Employee };
        var parameters = new ParameterResolver(
        [
            new RuleParameter { Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.08", EffectiveFrom = new DateOnly(2020, 1, 1) },
            new RuleParameter { Key = ParameterKeys.Week6HoursPerMonth, Value = "3.083", EffectiveFrom = new DateOnly(2020, 1, 1) }
        ]);
        var entries = new List<LedgerEntry>
        {
            new() { Account = HolidayAccount.Weeks1To5, EarningYear = 2025, Type = LedgerEntryType.Usage, Days = -15m, Hours = -111m }
        };
        entries.AddRange(TransferRules.Entries(mette.Id, Guid.NewGuid(), 2025, 5m, 7.4m, Today));

        // 1.1.2027: 2025-perioden er udløbet, men de 5 overførte dage ligger nu i 2026
        var balances = Service.Balances.For(mette, parameters, entries, new DateOnly(2027, 1, 1));
        var period2026 = balances.Single(b => b.Account == HolidayAccount.Weeks1To5 && b.EarningYear == 2026);

        Assert.Equal(5m, period2026.Movements);
        Assert.Equal(new DateOnly(2027, 12, 31), period2026.ExpiresOn);
    }
}