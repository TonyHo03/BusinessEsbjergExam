namespace Service.Accounting;

public sealed record AccountPosition(decimal QualifyingSalary, decimal Earned, decimal Taken);

public sealed record LiabilityParameters(decimal Weeks1To5Rate, decimal Week6Rate, bool Week6Enabled = true);
public sealed record LiabilityInput(AccountPosition Weeks1To5, AccountPosition Week6, decimal UnpaidSupplement);
public sealed record LiabilityResult(decimal Weeks1To5, decimal Week6, decimal Supplement, decimal Total);

public class LiabilityCalculator
{
    public static LiabilityResult Calculate(LiabilityInput input, LiabilityParameters parameters)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(input.UnpaidSupplement);
        
        var weeks1To5 = AccountValue(input.Weeks1To5, parameters.Weeks1To5Rate);
        var week6 = parameters.Week6Enabled ? AccountValue(input.Week6, parameters.Week6Rate) : 0m;
        var supplement = Round(input.UnpaidSupplement);

        return new LiabilityResult(weeks1To5, week6, supplement, weeks1To5 + week6 + supplement);
    }

    private static decimal AccountValue(AccountPosition account, decimal rate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(account.QualifyingSalary);
        ArgumentOutOfRangeException.ThrowIfNegative(account.Earned);
        ArgumentOutOfRangeException.ThrowIfNegative(account.Taken);
        ArgumentOutOfRangeException.ThrowIfNegative(rate);

        if (account.Earned == 0m || account.QualifyingSalary == 0m)
            return 0m;

        var remainingShare = Math.Clamp((account.Earned - account.Taken) / account.Earned, 0m, 1m);
        return Round(account.QualifyingSalary * rate * remainingShare);
    }
    
    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}