using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

/// <summary>Saldoen set fra en ansøgnings startdato, beregnet ud fra Mettes data.</summary>
public class BalanceOutlookTests
{
    private static readonly Employee Mette = new()
    {
        Id = Guid.NewGuid(),
        StartDate = new DateOnly(2025,
            9,
            1),
        Role = Roles.Employee
    };

    private static readonly WorkSchedule[] FullTime = [TestSchedules.FullTime()];

    private static readonly ParameterResolver Parameters = new(
    [
        new RuleParameter { Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.08", EffectiveFrom = new DateOnly(2020, 1, 1) },
        new RuleParameter { Key = ParameterKeys.Week6HoursPerMonth, Value = "3.083", EffectiveFrom = new DateOnly(2020, 1, 1) }
    ]);

    // Sommerferie 2026: 111 t fra uge 1–5, optjeningsår 2025
    private static readonly LedgerEntry[] SummerHoliday =
    [
        new() { Account = HolidayAccount.Weeks1To5, EarningYear = 2025, Type = LedgerEntryType.Usage, Days = -15m, Hours = -111m }
    ];

    private static BalanceOutlook Outlook(HolidayAccount account, DateOnly start, decimal reserved = 0m,
        WorkSchedule[]? schedules = null) =>
        Balances.Outlook(Mette, schedules ?? FullTime, Parameters, SummerHoliday, account, start, reserved);

    [Fact]
    public void Hours_earned_before_the_holiday_starts_count()
    {
        // 15.12.2026: 73,70 t fra 2025 + sep, okt og nov 2026 (3 × 15,39 t)
        var outlook = Outlook(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 15));

        Assert.Equal(119.88m, Math.Round(outlook.AvailableAtStart, 2));
    }

    [Fact]
    public void Waiting_a_month_gives_one_more_month_of_hours()
    {
        var november = Outlook(HolidayAccount.Weeks1To5, new DateOnly(2026, 11, 23));
        var december = Outlook(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 21));

        Assert.Equal(15.39m, Math.Round(december.AvailableAtStart - november.AvailableAtStart, 2));
    }

    [Fact]
    public void Pending_requests_are_subtracted()
    {
        var withoutPending = Outlook(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 15));
        var withPending = Outlook(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 15), reserved: 45m);

        Assert.Equal(45m, withoutPending.AvailableAtStart - withPending.AvailableAtStart);
        Assert.Equal(45m, withoutPending.AvailableAtYearEnd - withPending.AvailableAtYearEnd);
    }

    [Fact]
    public void Debt_limit_is_one_week_of_the_employees_own_schedule()
    {
        var fullTime = Outlook(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 15));
        var partTime = Outlook(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 15),
            schedules: [TestSchedules.SameEveryDay("09:00-15:00")]); // 30 t/uge

        Assert.Equal(37m, fullTime.DebtLimit);
        Assert.Equal(30m, partTime.DebtLimit);
    }

    [Fact]
    public void Week6_has_no_debt_limit()
    {
        Assert.Equal(0m, Outlook(HolidayAccount.Week6, new DateOnly(2026, 12, 15)).DebtLimit);
    }

    [Fact]
    public void Year_end_is_the_31_august_after_the_start_date()
    {
        Assert.Equal(new DateOnly(2027, 8, 31), Outlook(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 15)).YearEnd);
        Assert.Equal(new DateOnly(2027, 8, 31), Outlook(HolidayAccount.Weeks1To5, new DateOnly(2027, 8, 31)).YearEnd);
        Assert.Equal(new DateOnly(2028, 8, 31), Outlook(HolidayAccount.Weeks1To5, new DateOnly(2027, 9, 1)).YearEnd);
    }

    [Fact]
    public void On_31_august_the_whole_earning_year_is_earned()
    {
        // 73,704 t tilbage fra 2025 + hele 2026/27 (184,704 t)
        var outlook = Outlook(HolidayAccount.Weeks1To5, new DateOnly(2026, 12, 15));

        Assert.Equal(258.41m, Math.Round(outlook.AvailableAtYearEnd, 2));
    }
}