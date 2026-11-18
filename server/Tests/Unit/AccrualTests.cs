using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

/// <summary>
/// Optjening i dage. Alle optjener 2,08 dage pr. måned på uge 1–5, uanset fuld tid eller deltid –
/// hvor mange timer en dag er værd, afgøres først, når saldoen omregnes (se ScheduleChangeTests).
/// </summary>
public class AccrualTests
{
    private static readonly ParameterResolver Parameters = new(
    [
        new RuleParameter { Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.08", EffectiveFrom = new DateOnly(2020, 1, 1) },
        new RuleParameter { Key = ParameterKeys.Week6HoursPerMonth, Value = "3.083", EffectiveFrom = new DateOnly(2020, 1, 1) }
    ]);

    private static Employee Employee(DateOnly start, DateOnly? end = null) =>
        new() { Id = Guid.NewGuid(), StartDate = start, EndDate = end };

    private static decimal Accrued(HolidayAccount account, int year, Employee employee, DateOnly asOf,
        ParameterResolver? parameters = null) =>
        Math.Round(Accrual.Accrued(account, year, employee, parameters ?? Parameters, asOf), 2);

    private static readonly Employee LongTimeEmployee = Employee(new DateOnly(2020, 1, 1));

    [Fact]
    public void Full_period_on_weeks1to5_gives_12_months()
    {
        // 12 × 2,08 dage
        Assert.Equal(24.96m, Accrued(HolidayAccount.Weeks1To5, 2025, LongTimeEmployee, new DateOnly(2026, 9, 30)));
    }

    [Fact]
    public void Full_year_on_week6_gives_five_days()
    {
        // 12 × 3,083 t = 37 t = 5 dage á 7,4 t
        Assert.Equal(5.00m, Accrued(HolidayAccount.Week6, 2025, LongTimeEmployee, new DateOnly(2026, 6, 1)));
    }

    [Fact]
    public void Only_finished_months_count()
    {
        // 10.11.2026: september og oktober er slut, november er ikke
        Assert.Equal(4.16m, Accrued(HolidayAccount.Weeks1To5, 2026, LongTimeEmployee, new DateOnly(2026, 11, 10)));
    }

    [Fact]
    public void A_month_counts_on_its_last_day()
    {
        Assert.Equal(2.08m, Accrued(HolidayAccount.Weeks1To5, 2026, LongTimeEmployee, new DateOnly(2026, 9, 30)));
        Assert.Equal(0m, Accrued(HolidayAccount.Weeks1To5, 2026, LongTimeEmployee, new DateOnly(2026, 9, 29)));
    }

    [Fact]
    public void Starting_mid_month_earns_for_the_days_employed()
    {
        // Start 16.9: 15/30 af september + hele oktober
        var employee = Employee(new DateOnly(2026, 9, 16));

        Assert.Equal(3.12m, Accrued(HolidayAccount.Weeks1To5, 2026, employee, new DateOnly(2026, 10, 31)));
    }

    [Fact]
    public void Leaving_mid_month_earns_for_the_days_employed()
    {
        // Stop 15.10: hele september + 15/31 af oktober
        var employee = Employee(new DateOnly(2020, 1, 1), new DateOnly(2026, 10, 15));

        Assert.Equal(3.09m, Accrued(HolidayAccount.Weeks1To5, 2026, employee, new DateOnly(2026, 11, 10)));
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
        // Eksemplet fra afklaringen: optjeningen ændres fra 2,08 til 2,16 dage pr. måned 1.3.2027
        var parameters = new ParameterResolver(
        [
            new RuleParameter { Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.08", EffectiveFrom = new DateOnly(2020, 1, 1) },
            new RuleParameter { Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.16", EffectiveFrom = new DateOnly(2027, 3, 1) }
        ]);

        // sep–feb: 6 × 2,08 + mar–aug: 6 × 2,16
        Assert.Equal(25.44m, Accrued(HolidayAccount.Weeks1To5, 2026, LongTimeEmployee, new DateOnly(2027, 8, 31), parameters));
    }
}