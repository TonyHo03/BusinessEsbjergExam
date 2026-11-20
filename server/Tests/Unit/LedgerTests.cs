using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class LedgerTests
{
    private static readonly Guid EmployeeId = Guid.NewGuid();

    private const decimal FullTimeDay = 7.4m; // 37 t / 5

    private static LedgerEntry Entry(HolidayAccount account, int year, decimal days) => new()
    {
        Id = Guid.NewGuid(),
        EmployeeId = EmployeeId,
        Account = account,
        EarningYear = year,
        Type = LedgerEntryType.Usage,
        Days = days,
        Hours = days * FullTimeDay,
        EntryDate = new DateOnly(2026, 7, 6)
    };

    private static IReadOnlyList<LedgerEntry> Allocate(HolidayAccount account, decimal hours, DateOnly date,
        PeriodBalance[] balances, decimal debtLimitDays = 0m, decimal dayLength = FullTimeDay) =>
        Ledger.AllocateUsage(EmployeeId, account, hours, dayLength, date, balances, debtLimitDays: debtLimitDays);

    [Fact]
    public void Movements_only_count_the_requested_account_and_year()
    {
        var entries = new[]
        {
            Entry(HolidayAccount.Weeks1To5, 2025, -15m),
            Entry(HolidayAccount.Weeks1To5, 2026, -1m),
            Entry(HolidayAccount.Week6, 2025, -2m)
        };

        Assert.Equal(-15m, Ledger.Movements(entries, HolidayAccount.Weeks1To5, 2025));
        Assert.Equal(-2m, Ledger.Movements(entries, HolidayAccount.Week6, 2025));
    }

    [Fact]
    public void Remaining_is_accrued_plus_movements()
    {
        var balance = new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 24.96m, -15m);

        Assert.Equal(9.96m, balance.Remaining);
    }

    [Fact]
    public void Usage_is_taken_from_one_period_when_it_has_enough()
    {
        var balances = new[]
        {
            new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 25m, -15m), // 10 dage
            new PeriodBalance(HolidayAccount.Weeks1To5, 2026, 4m, 0m)
        };

        // 37 t = 5 dage
        var entry = Assert.Single(Allocate(HolidayAccount.Weeks1To5, 37m, new DateOnly(2026, 12, 15), balances));

        Assert.Equal(2025, entry.EarningYear);
        Assert.Equal(-5m, entry.Days);
        Assert.Equal(-37m, entry.Hours);
        Assert.Equal(LedgerEntryType.Usage, entry.Type);
    }

    [Fact]
    public void Usage_takes_the_oldest_period_first_and_splits_the_rest()
    {
        var balances = new[]
        {
            new PeriodBalance(HolidayAccount.Weeks1To5, 2026, 4m, 0m),
            new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 3m, 0m) // udløber først
        };

        // 37 t = 5 dage: 3 fra 2025 og 2 fra 2026
        var entries = Allocate(HolidayAccount.Weeks1To5, 37m, new DateOnly(2026, 12, 1), balances);

        Assert.Equal(2, entries.Count);
        Assert.Equal((2025, -3m, -22.2m), (entries[0].EarningYear, entries[0].Days, entries[0].Hours));
        Assert.Equal((2026, -2m, -14.8m), (entries[1].EarningYear, entries[1].Days, entries[1].Hours));
    }

    [Fact]
    public void Split_hours_always_add_up_to_the_request()
    {
        var balances = new[]
        {
            new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 1.3m, 0m),
            new PeriodBalance(HolidayAccount.Weeks1To5, 2026, 10m, 0m)
        };

        var entries = Allocate(HolidayAccount.Weeks1To5, 52m, new DateOnly(2026, 12, 15), balances);

        Assert.Equal(-52m, entries.Sum(e => e.Hours));
    }

    [Fact]
    public void The_two_accounts_are_never_mixed()
    {
        var balances = new[]
        {
            new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 25m, 0m),
            new PeriodBalance(HolidayAccount.Week6, 2025, 5m, -4m) // kun 1 dag på 6. ferieuge
        };

        Assert.Throws<InvalidOperationException>(() =>
            Allocate(HolidayAccount.Week6, 14.8m, new DateOnly(2026, 11, 16), balances)); // 2 dage
    }

    [Fact]
    public void Week6_hours_cannot_be_used_before_their_usage_period()
    {
        // 6. ferieuge optjent i 2026 kan først bruges fra 1.5.2027
        var balances = new[] { new PeriodBalance(HolidayAccount.Week6, 2026, 4m, 0m) };

        Assert.Throws<InvalidOperationException>(() =>
            Allocate(HolidayAccount.Week6, 7.4m, new DateOnly(2026, 11, 16), balances));
    }

    [Fact]
    public void Expired_periods_are_not_used()
    {
        var balances = new[]
        {
            new PeriodBalance(HolidayAccount.Weeks1To5, 2024, 25m, -20m), // udløb 31.12.2025
            new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 25m, 0m)
        };

        var entries = Allocate(HolidayAccount.Weeks1To5, 7.4m, new DateOnly(2026, 2, 2), balances);

        Assert.Equal(2025, Assert.Single(entries).EarningYear);
    }

    [Fact]
    public void Zero_or_negative_hours_are_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            Allocate(HolidayAccount.Weeks1To5, 0m, new DateOnly(2026, 11, 16), []));
    }

    [Fact]
    public void Holiday_debt_is_borrowed_from_the_current_earning_year()
    {
        // 1 dag tilbage i 2025. Ferien er 3 dage, så 2 dage lånes af 2026/27
        var balances = new[] { new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 25m, -24m) };

        var entries = Allocate(HolidayAccount.Weeks1To5, 22.2m, new DateOnly(2026, 12, 1), balances, debtLimitDays: 5m);

        Assert.Equal(2, entries.Count);
        Assert.Equal((2025, -1m), (entries[0].EarningYear, entries[0].Days));
        Assert.Equal((2026, -2m), (entries[1].EarningYear, entries[1].Days));
    }

    [Fact]
    public void More_debt_than_the_limit_is_rejected()
    {
        var balances = new[] { new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 25m, -24m) }; // 1 dag

        // 7 dage: 1 dag + 5 dages lån er ikke nok
        Assert.Throws<InvalidOperationException>(() =>
            Allocate(HolidayAccount.Weeks1To5, 51.8m, new DateOnly(2026, 12, 1), balances, debtLimitDays: 5m));
    }

    [Fact]
    public void Existing_debt_counts_against_the_limit()
    {
        // Allerede 4 dage i minus. 2 dage mere ville give -6
        var balances = new[] { new PeriodBalance(HolidayAccount.Weeks1To5, 2026, 0m, -4m) };

        Assert.Throws<InvalidOperationException>(() =>
            Allocate(HolidayAccount.Weeks1To5, 14.8m, new DateOnly(2026, 12, 1), balances, debtLimitDays: 5m));
    }
}