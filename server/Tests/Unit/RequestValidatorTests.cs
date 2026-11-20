using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class RequestValidatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 10);

    private static readonly WorkSchedule[] FullTime = [TestSchedules.FullTime()];

    private static readonly ParameterResolver Parameters = new(
    [
        new RuleParameter { Key = ParameterKeys.MainHolidayDays, Value = "15", EffectiveFrom = new DateOnly(2020, 1, 1) },
        new RuleParameter { Key = ParameterKeys.NoticeMainHolidayMonths, Value = "3", EffectiveFrom = new DateOnly(2020, 1, 1) },
        new RuleParameter { Key = ParameterKeys.NoticeOtherMonths, Value = "1", EffectiveFrom = new DateOnly(2020, 1, 1) }
    ]);

    private static BalanceOutlook Outlook(decimal available = 104.49m, decimal atYearEnd = 300m, decimal debtLimit = 37m) =>
        new(available, debtLimit, new DateOnly(2027, 8, 31), atYearEnd);

    private static RequestCalculation Calc(DateOnly start, DateOnly end) =>
        RequestCalculator.Calculate(start, end, FullTime, new HashSet<DateOnly>());

    private static RequestValidation Validate(HolidayAccount account, DateOnly start, DateOnly end,
        BalanceOutlook? outlook = null, DateOnly? today = null) =>
        RequestValidator.Validate(account, Calc(start, end), outlook ?? Outlook(), Parameters, today ?? Today);

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
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2027, 6, 7), new DateOnly(2027, 6, 25),
            Outlook(available: 185m), today: new DateOnly(2027, 4, 1));

        Assert.True(v.IsMainHoliday);
        Assert.True(Has(v, IssueCode.NotEnoughNotice));
    }

    [Fact]
    public void Main_holiday_with_three_months_notice_is_valid()
    {
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2027, 6, 7), new DateOnly(2027, 6, 25),
            Outlook(available: 185m), today: new DateOnly(2027, 3, 1));

        Assert.True(v.IsMainHoliday);
        Assert.True(v.IsValid);
    }

    [Fact]
    public void Holiday_debt_up_to_one_week_is_allowed_with_a_warning()
    {
        // 3 uger = 111 t. Har 80 t og må låne 37 t
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 14), new DateOnly(2027, 1, 1),
            Outlook(available: 80m));

        Assert.True(v.IsValid);
        Assert.True(Has(v, IssueCode.UsesHolidayDebt));
        Assert.False(Has(v, IssueCode.InsufficientBalance));
    }

    [Fact]
    public void No_debt_warning_when_the_balance_covers_the_request()
    {
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 15), new DateOnly(2026, 12, 23),
            Outlook(available: 52m)); // præcis nok

        Assert.False(Has(v, IssueCode.UsesHolidayDebt));
    }

    [Fact]
    public void Balance_must_not_be_negative_on_31_august()
    {
        // Juleferien koster 52 t, men den 31.8.2027 er der kun 40 t
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 15), new DateOnly(2026, 12, 23),
            Outlook(atYearEnd: 40m));

        Assert.False(v.IsValid);
        Assert.True(Has(v, IssueCode.NegativeOn31August));
    }

    [Fact]
    public void Balance_of_exactly_zero_on_31_august_is_allowed()
    {
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 15), new DateOnly(2026, 12, 23),
            Outlook(atYearEnd: 52m));

        Assert.False(Has(v, IssueCode.NegativeOn31August));
    }

    [Fact]
    public void Week6_has_no_holiday_debt()
    {
        // 2 dage = 15 t på 6. ferieuge, men der er kun 12,33 t og intet lån
        var v = Validate(HolidayAccount.Week6, new DateOnly(2026, 12, 14), new DateOnly(2026, 12, 15),
            new BalanceOutlook(12.33m, 0m, new DateOnly(2026, 12, 14), 12.33m));

        Assert.True(Has(v, IssueCode.InsufficientBalance));
        Assert.False(Has(v, IssueCode.NegativeOn31August));
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
        // For kort varsel og mere end én uges feriegæld (3 uger = 111 t, har 60 + 37)
        var v = Validate(HolidayAccount.Weeks1To5, new DateOnly(2026, 11, 16), new DateOnly(2026, 12, 4),
            Outlook(available: 60m));

        Assert.False(v.IsValid);
        Assert.True(Has(v, IssueCode.NotEnoughNotice));
        Assert.True(Has(v, IssueCode.InsufficientBalance));
    }
}