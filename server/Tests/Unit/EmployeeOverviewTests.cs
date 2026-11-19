using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class EmployeeOverviewTests
{
    private static readonly DateOnly Today = new(2026, 11, 10);

    private static readonly Employee Mette = new()
    {
        Id = Guid.NewGuid(), Name = "Mette Jensen", Email = "mette@businessesbjerg.dk",
        Role = Roles.Employee, StartDate = new DateOnly(2025, 9, 1)
    };

    private static readonly ParameterResolver Parameters = new(
    [
        new RuleParameter { Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.08", EffectiveFrom = new DateOnly(2020, 1, 1) },
        new RuleParameter { Key = ParameterKeys.Week6HoursPerMonth, Value = "3.083", EffectiveFrom = new DateOnly(2020, 1, 1) }
    ]);

    private static readonly LedgerEntry[] SummerHoliday =
    [
        new() { Account = HolidayAccount.Weeks1To5, EarningYear = 2025, Type = LedgerEntryType.Usage, Days = -15m, Hours = -111m }
    ];

    private static EmployeeOverviewRow Row(WorkSchedule[] schedules, decimal pending = 0m) =>
        EmployeeOverview.Build(Mette, schedules, Parameters, SummerHoliday, pending, 0m, Today);

    [Fact]
    public void Shows_the_same_hours_as_the_balances_page()
    {
        var row = Row([TestSchedules.FullTime()]);

        Assert.True(row.HasSchedule);
        Assert.Equal(7.4m, row.DayLength);
        Assert.Equal(104.49m, row.AvailableWeeks1To5);
        Assert.Equal(12.33m, row.AvailableWeek6);
    }

    [Fact]
    public void Pending_requests_are_subtracted_and_shown()
    {
        var row = Row([TestSchedules.FullTime()], pending: 37m);

        Assert.Equal(67.49m, row.AvailableWeeks1To5);
        Assert.Equal(37m, row.PendingHours);
    }

    [Fact]
    public void Holiday_expiring_31_december_is_flagged()
    {
        var row = Row([TestSchedules.FullTime()]);

        Assert.Equal(73.70m, row.ExpiringHours); // 9,96 dage tilbage fra 2025/26
        Assert.Equal(new DateOnly(2026, 12, 31), row.ExpiresOn);
    }

    [Fact]
    public void Nothing_is_flagged_when_nothing_expires_soon()
    {
        var row = EmployeeOverview.Build(Mette, [TestSchedules.FullTime()], Parameters, SummerHoliday, 0m, 0m, new DateOnly(2026, 9, 15));

        Assert.Equal(0m, row.ExpiringHours);
        Assert.Null(row.ExpiresOn);
    }

    [Fact]
    public void An_employee_without_a_schedule_is_listed_without_hours()
    {
        var row = Row([]);

        Assert.False(row.HasSchedule);
        Assert.Null(row.AvailableWeeks1To5);
        Assert.Equal("Mette Jensen", row.Name);
    }
}