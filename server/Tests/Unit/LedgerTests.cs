using Infra;
using Service;
using Xunit;

namespace Tests.Unit;

public class LedgerTests
{
    private static readonly Guid EmployeeId = Guid.NewGuid();

    private static LedgerEntry Entry(HolidayAccount account, int year, decimal hours) => new()
    {
        Id = Guid.NewGuid(),
        EmployeeId = EmployeeId,
        Account = account,
        EarningYear = year,
        Type = LedgerEntryType.Usage,
        Hours = hours,
        EntryDate = new DateOnly(2026, 7, 6)
    };

    [Fact]
    public void Movements_only_count_the_requested_account_and_year()
    {
        var entries = new[]
        {
            Entry(HolidayAccount.Weeks1To5, 2025, -111m),
            Entry(HolidayAccount.Weeks1To5, 2026, -7.5m),
            Entry(HolidayAccount.Week6, 2025, -15m)
        };

        Assert.Equal(-111m, Ledger.Movements(entries, HolidayAccount.Weeks1To5, 2025));
        Assert.Equal(-15m, Ledger.Movements(entries, HolidayAccount.Week6, 2025));
    }

    [Fact]
    public void Remaining_is_accrued_plus_movements()
    {
        var balance = new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 185m, -111m);

        Assert.Equal(74m, balance.Remaining);
    }

    [Fact]
    public void Usage_is_taken_from_one_period_when_it_has_enough()
    {
        var balances = new[]
        {
            new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 185m, -111m), // 74 t tilbage
            new PeriodBalance(HolidayAccount.Weeks1To5, 2026, 30.83m, 0m)
        };

        var entries = Ledger.AllocateUsage(EmployeeId, HolidayAccount.Weeks1To5, 52m, new DateOnly(2026, 12, 15), balances);

        var entry = Assert.Single(entries);
        Assert.Equal(2025, entry.EarningYear);
        Assert.Equal(-52m, entry.Hours);
        Assert.Equal(LedgerEntryType.Usage, entry.Type);
    }

    [Fact]
    public void Usage_takes_the_oldest_period_first_and_splits_the_rest()
    {
        var balances = new[]
        {
            new PeriodBalance(HolidayAccount.Weeks1To5, 2026, 30.83m, 0m),
            new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 185m, -111m) // 74 t, udløber først
        };

        var entries = Ledger.AllocateUsage(EmployeeId, HolidayAccount.Weeks1To5, 80m, new DateOnly(2026, 12, 1), balances);

        Assert.Equal(2, entries.Count);
        Assert.Equal((2025, -74m), (entries[0].EarningYear, entries[0].Hours));
        Assert.Equal((2026, -6m), (entries[1].EarningYear, entries[1].Hours));
    }

    [Fact]
    public void The_two_accounts_are_never_mixed()
    {
        var balances = new[]
        {
            new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 185m, 0m),
            new PeriodBalance(HolidayAccount.Week6, 2025, 37m, -30m) // kun 7 t på 6. ferieuge
        };

        Assert.Throws<InvalidOperationException>(() =>
            Ledger.AllocateUsage(EmployeeId, HolidayAccount.Week6, 15m, new DateOnly(2026, 11, 16), balances));
    }

    [Fact]
    public void Week6_hours_cannot_be_used_before_their_usage_period()
    {
        // 6. ferieuge optjent i 2026 kan først bruges fra 1.5.2027
        var balances = new[] { new PeriodBalance(HolidayAccount.Week6, 2026, 30.83m, 0m) };

        Assert.Throws<InvalidOperationException>(() =>
            Ledger.AllocateUsage(EmployeeId, HolidayAccount.Week6, 7.5m, new DateOnly(2026, 11, 16), balances));
    }

    [Fact]
    public void Expired_periods_are_not_used()
    {
        var balances = new[]
        {
            new PeriodBalance(HolidayAccount.Weeks1To5, 2024, 185m, -150m), // udløb 31.12.2025
            new PeriodBalance(HolidayAccount.Weeks1To5, 2025, 185m, 0m)
        };

        var entries = Ledger.AllocateUsage(EmployeeId, HolidayAccount.Weeks1To5, 7.5m, new DateOnly(2026, 2, 2), balances);

        Assert.Equal(2025, Assert.Single(entries).EarningYear);
    }

    [Fact]
    public void Zero_or_negative_hours_are_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            Ledger.AllocateUsage(EmployeeId, HolidayAccount.Weeks1To5, 0m, new DateOnly(2026, 11, 16), []));
    }
}