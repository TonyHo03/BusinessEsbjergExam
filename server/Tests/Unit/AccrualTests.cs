using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class AccrualTests
{
    private static readonly WorkSchedule FullTime = new()
    {
        EffectiveFrom = new DateOnly(2020, 1, 1),
        Mon = 7.5m, Tue = 7.5m, Wed = 7.5m, Thu = 7.5m, Fri = 7m // 37 t/uge
    };

    private static readonly ParameterResolver Parameters = new(
    [
        new RuleParameter { Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.08", EffectiveFrom = new DateOnly(2020, 1, 1) },
        new RuleParameter { Key = ParameterKeys.Week6HoursPerMonth, Value = "3.083", EffectiveFrom = new DateOnly(2020, 1, 1) }
    ]);

    private static Employee Employee(DateOnly start, DateOnly? end = null) =>
        new() { Id = Guid.NewGuid(), StartDate = start, EndDate = end };

    private static decimal Accrued(HolidayAccount account, int year, Employee employee, DateOnly asOf,
        WorkSchedule? schedule = null, ParameterResolver? parameters = null) =>
        Math.Round(Accrual.Accrued(account, year, employee, [schedule ?? FullTime], parameters ?? Parameters, asOf), 2);

    private static readonly Employee LongTimeEmployee = Employee(new DateOnly(2020, 1, 1));

    [Fact]
    public void Full_period_on_weeks1to5_gives_12_months()
    {
        // 12 × 2,08 dage × 7,4 t = 184,70 t
        Assert.Equal(184.70m, Accrued(HolidayAccount.Weeks1To5, 2025, LongTimeEmployee, new DateOnly(2026, 9, 30)));
    }

    [Fact]
    public void Full_year_on_week6_gives_12_months()
    {
        // 12 × 3,083 t = 37,00 t
        Assert.Equal(37.00m, Accrued(HolidayAccount.Week6, 2025, LongTimeEmployee, new DateOnly(2026, 6, 1)));
    }

    [Fact]
    public void Only_finished_months_count()
    {
        // 10.11.2026: september og oktober er slut, november er ikke
        Assert.Equal(30.78m, Accrued(HolidayAccount.Weeks1To5, 2026, LongTimeEmployee, new DateOnly(2026, 11, 10)));
    }

    [Fact]
    public void A_month_counts_on_its_last_day()
    {
        Assert.Equal(15.39m, Accrued(HolidayAccount.Weeks1To5, 2026, LongTimeEmployee, new DateOnly(2026, 9, 30)));
        Assert.Equal(0m, Accrued(HolidayAccount.Weeks1To5, 2026, LongTimeEmployee, new DateOnly(2026, 9, 29)));
    }

    [Fact]
    public void Starting_mid_month_earns_for_the_days_employed()
    {
        // Start 16.9: 15/30 af september + hele oktober
        var employee = Employee(new DateOnly(2026, 9, 16));

        Assert.Equal(23.09m, Accrued(HolidayAccount.Weeks1To5, 2026, employee, new DateOnly(2026, 10, 31)));
    }

    [Fact]
    public void Leaving_mid_month_earns_for_the_days_employed()
    {
        // Stop 15.10: hele september + 15/31 af oktober
        var employee = Employee(new DateOnly(2020, 1, 1), new DateOnly(2026, 10, 15));

        Assert.Equal(22.84m, Accrued(HolidayAccount.Weeks1To5, 2026, employee, new DateOnly(2026, 11, 10)));
    }

    [Fact]
    public void Part_time_earns_in_proportion_to_hours()
    {
        var partTime = new WorkSchedule
        {
            EffectiveFrom = new DateOnly(2020, 1, 1),
            Mon = 6m, Tue = 6m, Wed = 6m, Thu = 6m, Fri = 6m // 30 t/uge
        };

        // Uge 1–5: 2,08 × 6 t = 12,48 t. 6. ferieuge: 3,083 × 30/37 = 2,50 t
        Assert.Equal(12.48m, Accrued(HolidayAccount.Weeks1To5, 2026, LongTimeEmployee, new DateOnly(2026, 9, 30), partTime));
        Assert.Equal(2.50m, Accrued(HolidayAccount.Week6, 2026, LongTimeEmployee, new DateOnly(2026, 1, 31), partTime));
    }

    [Fact]
    public void Nothing_is_earned_before_employment_starts()
    {
        var employee = Employee(new DateOnly(2027, 1, 1));

        Assert.Equal(0m, Accrued(HolidayAccount.Week6, 2026, employee, new DateOnly(2027, 6, 1)));
    }

    [Fact]
    public void A_parameter_change_only_affects_months_after_it()
    {
        var parameters = new ParameterResolver(
        [
            new RuleParameter { Key = ParameterKeys.Week6HoursPerMonth, Value = "3.083", EffectiveFrom = new DateOnly(2020, 1, 1) },
            new RuleParameter { Key = ParameterKeys.Week6HoursPerMonth, Value = "3.5", EffectiveFrom = new DateOnly(2026, 7, 1) }
        ]);

        // 6 × 3,083 + 6 × 3,5 = 39,50 t
        Assert.Equal(39.50m, Accrued(HolidayAccount.Week6, 2026, LongTimeEmployee, new DateOnly(2026, 12, 31), null, parameters));
    }
}