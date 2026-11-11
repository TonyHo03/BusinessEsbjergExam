using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class BalancesTests
{
    private static readonly DateOnly Today = new(2026, 11, 10);

    private static readonly Employee Mette = new() { Id = Guid.NewGuid(), StartDate = new DateOnly(2025, 9, 1) };

    private static readonly WorkSchedule[] FullTime =
    [
        new() { EffectiveFrom = new DateOnly(2020, 1, 1), Mon = 7.5m, Tue = 7.5m, Wed = 7.5m, Thu = 7.5m, Fri = 7m }
    ];

    private static readonly ParameterResolver Parameters = new(
    [
        new RuleParameter { Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.08", EffectiveFrom = new DateOnly(2020, 1, 1) },
        new RuleParameter { Key = ParameterKeys.Week6HoursPerMonth, Value = "3.083", EffectiveFrom = new DateOnly(2020, 1, 1) }
    ]);

    // Sommerferie 2026: 111 t fra uge 1–5, optjeningsår 2025
    private static readonly LedgerEntry[] SummerHoliday =
    [
        new() { Account = HolidayAccount.Weeks1To5, EarningYear = 2025, Type = LedgerEntryType.Usage, Hours = -111m }
    ];

    private static IReadOnlyList<PeriodBalance> MetteBalances() =>
        Balances.For(Mette, FullTime, Parameters, SummerHoliday, Today);

    private static PeriodBalance Find(IEnumerable<PeriodBalance> balances, HolidayAccount account, int year) =>
        balances.Single(b => b.Account == account && b.EarningYear == year);

    [Fact]
    public void Lists_every_open_period_on_both_accounts()
    {
        var balances = MetteBalances();

        Assert.Equal(4, balances.Count);
    }

    [Fact]
    public void Previous_weeks1to5_period_expires_31_december_and_needs_action()
    {
        var b = Find(MetteBalances(), HolidayAccount.Weeks1To5, 2025);

        Assert.Equal(73.70m, Math.Round(b.Remaining, 2)); // 184,70 − 111
        Assert.Equal(new DateOnly(2026, 12, 31), b.ExpiresOn);
        Assert.Equal(ExpiryStatus.ExpiringSoon, b.StatusOn(Today));
        Assert.True(b.NeedsAction(Today));
    }

    [Fact]
    public void Current_weeks1to5_period_is_active()
    {
        var b = Find(MetteBalances(), HolidayAccount.Weeks1To5, 2026);

        Assert.Equal(30.78m, Math.Round(b.Remaining, 2));
        Assert.Equal(new DateOnly(2027, 12, 31), b.ExpiresOn);
        Assert.Equal(ExpiryStatus.Active, b.StatusOn(Today));
    }

    [Fact]
    public void Week6_from_2025_is_usable_until_30_april_2027()
    {
        var b = Find(MetteBalances(), HolidayAccount.Week6, 2025);

        Assert.Equal(12.33m, Math.Round(b.Remaining, 2)); // ansat sep–dec 2025: 4 × 3,083
        Assert.Equal(new DateOnly(2027, 4, 30), b.ExpiresOn);
        Assert.Equal(ExpiryStatus.Active, b.StatusOn(Today));
    }

    [Fact]
    public void Week6_from_2026_is_not_usable_yet()
    {
        var b = Find(MetteBalances(), HolidayAccount.Week6, 2026);

        Assert.Equal(30.83m, Math.Round(b.Remaining, 2)); // jan–okt: 10 × 3,083
        Assert.Equal(new DateOnly(2027, 5, 1), b.UsableFrom);
        Assert.Equal(ExpiryStatus.NotYetUsable, b.StatusOn(Today));
    }

    [Fact]
    public void Available_hours_only_count_usable_periods_on_the_account()
    {
        var balances = MetteBalances();

        Assert.Equal(104.49m, Math.Round(Balances.AvailableOn(balances, HolidayAccount.Weeks1To5, Today), 2)); // 73,70 + 30,78
        Assert.Equal(12.33m, Math.Round(Balances.AvailableOn(balances, HolidayAccount.Week6, Today), 2));
    }

    [Fact]
    public void Expired_period_with_hours_left_is_shown_and_needs_action()
    {
        var oldEmployee = new Employee { Id = Guid.NewGuid(), StartDate = new DateOnly(2024, 9, 1) };

        var balances = Balances.For(oldEmployee, FullTime, Parameters, [], Today);
        var b = Find(balances, HolidayAccount.Weeks1To5, 2024); // udløb 31.12.2025

        Assert.Equal(ExpiryStatus.Expired, b.StatusOn(Today));
        Assert.True(b.NeedsAction(Today));
    }

    [Fact]
    public void Expired_period_with_nothing_left_is_hidden()
    {
        var oldEmployee = new Employee { Id = Guid.NewGuid(), StartDate = new DateOnly(2024, 9, 1) };
        LedgerEntry[] allUsed =
        [
            new() { Account = HolidayAccount.Weeks1To5, EarningYear = 2024, Type = LedgerEntryType.Usage, Hours = -184.704m }
        ];

        var balances = Balances.For(oldEmployee, FullTime, Parameters, allUsed, Today);

        Assert.DoesNotContain(balances, b => b.Account == HolidayAccount.Weeks1To5 && b.EarningYear == 2024);
    }

    [Fact]
    public void No_action_needed_when_nothing_is_left()
    {
        var b = new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 185m, -185m);

        Assert.Equal(ExpiryStatus.ExpiringSoon, b.StatusOn(Today));
        Assert.False(b.NeedsAction(Today));
    }
}