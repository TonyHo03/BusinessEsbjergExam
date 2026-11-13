using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class RequestValidatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 10);

    private static readonly WorkSchedule[] FullTime =
    [
        new() { EffectiveFrom = new DateOnly(2020, 1, 1), Mon = 7.5m, Tue = 7.5m, Wed = 7.5m, Thu = 7.5m, Fri = 7m }
    ];

    private static readonly ParameterResolver Parameters = new(
    [
        new RuleParameter { Key = ParameterKeys.MainHolidayDays, Value = "15", EffectiveFrom = new DateOnly(2020, 1, 1) },
        new RuleParameter { Key = ParameterKeys.NoticeMainHolidayMonths, Value = "3", EffectiveFrom = new DateOnly(2020, 1, 1) },
        new RuleParameter { Key = ParameterKeys.NoticeOtherMonths, Value = "1", EffectiveFrom = new DateOnly(2020, 1, 1) }
    ]);

    // Mettes saldi 10.11.2026 (se BalancesTests)
    private static readonly PeriodBalance[] MetteBalances =
    [
        new(HolidayAccount.Weeks1To5, 2025, 184.70m, -111m), // 73,70 t, udløber 31.12.2026
        new(HolidayAccount.Weeks1To5, 2026, 30.78m, 0m),     // 30,78 t
        new(HolidayAccount.Week6, 2025, 12.33m, 0m),         // 12,33 t, kan bruges nu
        new(HolidayAccount.Week6, 2026, 30.83m, 0m)          // kan først bruges fra 1.5.2027
    ];

    private static RequestCalculation Calc(DateOnly start, DateOnly end) =>
        RequestCalculator.Calculate(start, end, FullTime, new HashSet<DateOnly>());

    private static RequestValidation Validate(HolidayAccount account, DateOnly start, DateOnly end,
        IReadOnlyList<PeriodBalance>? balances = null, decimal reserved = 0m, DateOnly? today = null) =>
        RequestValidator.Validate(account, Calc(start, end), balances ?? MetteBalances, reserved, Parameters, today ?? Today);

    private static bool Has(RequestValidation v, IssueCode code) => v.Issues.Any(i => i.Code == code);

    [Fact]
    public void Christmas_request_from_the_sketch_is_valid()
    {
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 15), new DateOnly(2026, 12, 23));

        Assert.True(v.IsValid);
        Assert.Empty(v.Issues);
    }

    [Fact]
    public void Less_than_one_month_notice_is_rejected()
    {
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 11, 23), new DateOnly(2026, 11, 27));

        Assert.False(v.IsValid);
        Assert.True(Has(v, IssueCode.NotEnoughNotice));
    }

    [Fact]
    public void Exactly_one_month_notice_is_enough()
    {
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 10), new DateOnly(2026, 12, 16));

        Assert.False(Has(v, IssueCode.NotEnoughNotice));
    }

    [Fact]
    public void Main_holiday_needs_three_months_notice()
    {
        // 15 arbejdsdage i juni 2027, ansøgt 1.4.2027: under 3 måneders varsel
        var balances = new[] { new PeriodBalance(HolidayAccount.Weeks1To5, 2026, 185m, 0m) };

        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2027, 6, 7), new DateOnly(2027, 6, 25),
            balances, today: new DateOnly(2027, 4, 1));

        Assert.True(v.IsMainHoliday);
        Assert.True(Has(v, IssueCode.NotEnoughNotice));
    }

    [Fact]
    public void Main_holiday_with_three_months_notice_is_valid()
    {
        var balances = new[] { new PeriodBalance(HolidayAccount.Weeks1To5, 2026, 185m, 0m) };

        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2027, 6, 7), new DateOnly(2027, 6, 25),
            balances, today: new DateOnly(2027, 3, 1));

        Assert.True(v.IsMainHoliday);
        Assert.True(v.IsValid);
    }

    [Fact]
    public void Not_enough_hours_is_rejected()
    {
        // 3 uger = 111 t, men der er kun 104,49 t
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 14), new DateOnly(2027, 1, 1));

        Assert.True(Has(v, IssueCode.InsufficientBalance));
    }

    [Fact]
    public void Hours_in_pending_requests_are_reserved()
    {
        // 52 t ønskes, 104,49 t findes, men 60 t er allerede reserveret
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 15), new DateOnly(2026, 12, 23), reserved: 60m);

        Assert.True(Has(v, IssueCode.InsufficientBalance));
    }

    [Fact]
    public void Week6_only_uses_week6_hours()
    {
        // 2 dage = 15 t på 6. ferieuge, men der er kun 12,33 t (uge 1–5 tæller ikke med)
        var v = Validate(HolidayAccount.Week6, new DateOnly(2026, 12, 14), new DateOnly(2026, 12, 15));

        Assert.True(Has(v, IssueCode.InsufficientBalance));
    }

    [Fact]
    public void Week6_hours_count_once_they_become_usable()
    {
        // I maj 2027 kan 6. ferieuge fra 2026 bruges (30,83 t). 2 dage = 15 t
        var v = Validate(HolidayAccount.Week6, new DateOnly(2027, 5, 10), new DateOnly(2027, 5, 11));

        Assert.False(Has(v, IssueCode.InsufficientBalance));
    }

    [Fact]
    public void A_weekend_only_request_is_rejected()
    {
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 19), new DateOnly(2026, 12, 20));

        Assert.True(Has(v, IssueCode.NoWorkingDays));
        Assert.Single(v.Issues);
    }

    [Fact]
    public void Start_in_the_past_is_rejected()
    {
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 6));

        Assert.True(Has(v, IssueCode.StartInPast));
    }

    [Fact]
    public void Short_blocks_give_a_warning_but_are_still_valid()
    {
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 14), new DateOnly(2026, 12, 15));

        Assert.True(Has(v, IssueCode.ShortBlock));
        Assert.True(v.IsValid);
    }

    [Fact]
    public void All_problems_are_reported_at_once()
    {
        // For kort varsel og for mange timer
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 11, 16), new DateOnly(2026, 12, 4));

        Assert.True(Has(v, IssueCode.NotEnoughNotice));
        Assert.True(Has(v, IssueCode.InsufficientBalance));
    }
}