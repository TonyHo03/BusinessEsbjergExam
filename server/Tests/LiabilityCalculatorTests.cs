using Service.Accounting;
using Xunit;

namespace HolidayPlanner.UnitTests.Accounting;

public class LiabilityCalculatorTests
{
    private static readonly LiabilityParameters Rates = new(0.125m, 0.025m);
    private static readonly AccountPosition Empty = new(0m, 0m, 0m);

    private static LiabilityInput Weeks1To5Only(decimal salary, decimal earned, decimal taken, decimal supplement = 0m) =>
        new(new AccountPosition(salary, earned, taken), Empty, supplement);

    private static LiabilityInput Week6Only(decimal salary, decimal earned, decimal taken) =>
        new(Empty, new AccountPosition(salary, earned, taken), 0m);

    [Fact]
    public void Weeks1To5_NothingTaken_IsFullRateOfSalary()
    {
        var result = LiabilityCalculator.Calculate(Weeks1To5Only(300_000m, 25m, 0m), Rates);

        Assert.Equal(37_500.00m, result.Weeks1To5);
        Assert.Equal(37_500.00m, result.Total);
    }

    [Fact]
    public void Weeks1To5_HalfTaken_IsHalfTheLiability()
    {
        var result = LiabilityCalculator.Calculate(Weeks1To5Only(300_000m, 25m, 12.5m), Rates);

        Assert.Equal(18_750.00m, result.Weeks1To5);
    }

    [Fact]
    public void Week6_UsesItsOwnRateAndShareOfHours()
    {
        // 360,000 x 2.5% = 9,000. Remaining share = 15 / 37. 9,000 x 15 / 37 = 3,648.65 (rounded)
        var result = LiabilityCalculator.Calculate(Week6Only(360_000m, 37m, 22m), Rates);

        Assert.Equal(3_648.65m, result.Week6);
    }

    [Fact]
    public void FullyTaken_OwesNothing()
    {
        var result = LiabilityCalculator.Calculate(Weeks1To5Only(300_000m, 25m, 25m), Rates);

        Assert.Equal(0m, result.Total);
    }

    [Fact]
    public void Overdrawn_ClampsToZeroInsteadOfNegative()
    {
        var result = LiabilityCalculator.Calculate(Weeks1To5Only(300_000m, 25m, 30m), Rates);

        Assert.Equal(0m, result.Weeks1To5);
    }

    [Fact]
    public void NothingEarned_OwesNothingAndDoesNotDivideByZero()
    {
        var result = LiabilityCalculator.Calculate(Weeks1To5Only(300_000m, 0m, 0m), Rates);

        Assert.Equal(0m, result.Total);
    }

    [Fact]
    public void UnpaidSupplement_IsAddedToTheTotal()
    {
        var result = LiabilityCalculator.Calculate(Weeks1To5Only(300_000m, 25m, 0m, supplement: 1_447.60m), Rates);

        Assert.Equal(1_447.60m, result.Supplement);
        Assert.Equal(38_947.60m, result.Total);
    }

    [Fact]
    public void Total_IsSumOfBothAccountsAndSupplement()
    {
        var input = new LiabilityInput(
            new AccountPosition(300_000m, 25m, 0m),   // 37,500.00
            new AccountPosition(360_000m, 37m, 0m),   //  9,000.00
            100m);

        var result = LiabilityCalculator.Calculate(input, Rates);

        Assert.Equal(46_600.00m, result.Total);
    }

    [Fact]
    public void RatesComeFromParameters_NotHardCoded()
    {
        var raised = new LiabilityParameters(0.15m, 0.025m);

        var result = LiabilityCalculator.Calculate(Weeks1To5Only(200_000m, 25m, 0m), raised);

        Assert.Equal(30_000.00m, result.Weeks1To5);
    }

    [Fact]
    public void Week6Disabled_OwesNothingForWeek6()
    {
        var input = new LiabilityInput(
            new AccountPosition(300_000m, 25m, 0m),   // 37,500.00
            new AccountPosition(360_000m, 37m, 0m),   // would be 9,000.00
            0m);

        var result = LiabilityCalculator.Calculate(input, new LiabilityParameters(0.125m, 0.025m, Week6Enabled: false));

        Assert.Equal(0m, result.Week6);
        Assert.Equal(37_500.00m, result.Total);
    }

    [Fact]
    public void NegativeInput_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            LiabilityCalculator.Calculate(Weeks1To5Only(-1m, 25m, 0m), Rates));
    }
}