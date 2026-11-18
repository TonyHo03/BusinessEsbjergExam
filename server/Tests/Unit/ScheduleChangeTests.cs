using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

/// <summary>
/// Omregning ved skemaskift (#68): ferie er en ret til dage. Når skemaet ændres, er antallet
/// af dage det samme, og timerne regnes om med den nye daglængde (ugentlige timer / 5).
/// </summary>
public class ScheduleChangeTests
{
    private static readonly Employee Mette = new() { Id = Guid.NewGuid(), StartDate = new DateOnly(2025, 9, 1) };

    private static readonly ParameterResolver Parameters = new(
    [
        new RuleParameter { Key = ParameterKeys.Weeks1To5DaysPerMonth, Value = "2.08", EffectiveFrom = new DateOnly(2020, 1, 1) },
        new RuleParameter { Key = ParameterKeys.Week6HoursPerMonth, Value = "3.083", EffectiveFrom = new DateOnly(2020, 1, 1) }
    ]);

    // Sommerferie 2026: 15 dage (111 t) fra uge 1–5, optjeningsår 2025
    private static readonly LedgerEntry[] SummerHoliday =
    [
        new() { Account = HolidayAccount.Weeks1To5, EarningYear = 2025, Type = LedgerEntryType.Usage, Days = -15m, Hours = -111m }
    ];

    private static readonly WorkSchedule[] AlwaysFullTime = [TestSchedules.FullTime()];

    // Fuldtid (37 t) til og med 31.12.2026, derefter deltid 6 t om dagen (30 t)
    private static readonly WorkSchedule[] PartTimeFrom2027 =
    [
        TestSchedules.Create(TestSchedules.LongAgo, "08:00-15:30", "08:00-15:30", "08:00-15:30", "08:00-15:30", "08:00-15:00",
            to: new DateOnly(2026, 12, 31)),
        TestSchedules.SameEveryDay("09:00-15:00", new DateOnly(2027, 1, 1))
    ];

    private static BalanceOutlook Outlook(WorkSchedule[] schedules, DateOnly start) =>
        Balances.Outlook(Mette, schedules, Parameters, SummerHoliday, HolidayAccount.Weeks1To5, start, 0m);

    [Fact]
    public void A_holiday_day_is_a_fifth_of_the_working_week()
    {
        Assert.Equal(7.4m, ScheduleResolver.DayLength(AlwaysFullTime, new DateOnly(2026, 11, 16)));
        Assert.Equal(6m, ScheduleResolver.DayLength(PartTimeFrom2027, new DateOnly(2027, 1, 4)));
    }

    [Fact]
    public void Hours_are_converted_with_the_new_day_length()
    {
        // 15.2.2027: optjent sep–jan = 5 × 2,08 = 10,4 dage
        var fullTime = Outlook(AlwaysFullTime, new DateOnly(2027, 2, 15));
        var partTime = Outlook(PartTimeFrom2027, new DateOnly(2027, 2, 15));

        Assert.Equal(76.96m, Math.Round(fullTime.AvailableAtStart, 2)); // 10,4 × 7,4 t
        Assert.Equal(62.40m, Math.Round(partTime.AvailableAtStart, 2)); // 10,4 × 6 t
    }

    [Fact]
    public void The_number_of_days_stays_the_same()
    {
        var date = new DateOnly(2027, 2, 15);
        var fullTime = Outlook(AlwaysFullTime, date);
        var partTime = Outlook(PartTimeFrom2027, date);

        Assert.Equal(
            Math.Round(fullTime.AvailableAtStart / 7.4m, 4),
            Math.Round(partTime.AvailableAtStart / 6m, 4));
    }

    [Fact]
    public void The_schedule_before_the_change_is_used_for_dates_before_it()
    {
        // 15.12.2026 er stadig fuld tid, så timerne er de samme som hvis skemaet aldrig ændres
        var date = new DateOnly(2026, 12, 15);

        Assert.Equal(Outlook(AlwaysFullTime, date).AvailableAtStart, Outlook(PartTimeFrom2027, date).AvailableAtStart);
    }

    [Fact]
    public void Holiday_debt_limit_follows_the_new_schedule()
    {
        Assert.Equal(30m, Outlook(PartTimeFrom2027, new DateOnly(2027, 2, 15)).DebtLimit); // 1 uge á 30 t
    }

    [Fact]
    public void Taken_holiday_keeps_its_days_after_the_change()
    {
        // Sommerferien var 15 dage. Den er stadig 15 dage, også efter skiftet til deltid
        var balances = Balances.For(Mette, Parameters, SummerHoliday, new DateOnly(2026, 11, 16));

        Assert.Equal(-15m, balances.Single(b => b.Account == HolidayAccount.Weeks1To5 && b.EarningYear == 2025).Movements);
    }

    [Fact]
    public void A_week_off_costs_five_days_also_for_part_time_working_three_days()
    {
        // Arbejder man, tir, tor 09:00–15:00 = 18 t/uge, daglængde 3,6 t
        WorkSchedule[] threeDays = [TestSchedules.Create(TestSchedules.LongAgo, "09:00-15:00", "09:00-15:00", null, "09:00-15:00", null)];
        var week = RequestCalculator.Calculate(new DateOnly(2027, 3, 1), new DateOnly(2027, 3, 5), threeDays, new HashSet<DateOnly>());
        var dayLength = ScheduleResolver.DayLength(threeDays, new DateOnly(2027, 3, 1));

        var entries = Ledger.AllocateUsage(Mette.Id, HolidayAccount.Weeks1To5, week.TotalHours, dayLength,
            new DateOnly(2027, 3, 1), [new PeriodBalance(HolidayAccount.Weeks1To5, 2026, 20m, 0m)]);

        Assert.Equal(18m, week.TotalHours);
        Assert.Equal(-5m, Assert.Single(entries).Days);
    }
}