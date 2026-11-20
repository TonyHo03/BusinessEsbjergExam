using System.Globalization;
using Infra;

namespace Service;

public enum ParameterKind { Decimal, Integer, Boolean }

public record ParameterDefinition(string Key, string Description, ParameterKind Kind);

public static class ParameterRules
{
    public static readonly IReadOnlyList<ParameterDefinition> Definitions =
    [
        new(ParameterKeys.Weeks1To5DaysPerMonth, "Weeks 1–5: days earned per month", ParameterKind.Decimal),
        new(ParameterKeys.Week6HoursPerMonth, "6th week: hours earned per month (full time)", ParameterKind.Decimal),
        new(ParameterKeys.MainHolidayDays, "Main holiday: consecutive days", ParameterKind.Integer),
        new(ParameterKeys.NoticeMainHolidayMonths, "Notice for main holiday (months)", ParameterKind.Integer),
        new(ParameterKeys.NoticeOtherMonths, "Notice for other holiday (months)", ParameterKind.Integer),
        new(ParameterKeys.TransferEnabled, "Holiday transfers enabled", ParameterKind.Boolean),
        new(ParameterKeys.TransferMaxDays, "Transfers: maximum days", ParameterKind.Decimal)
    ];

    public static IReadOnlyList<string> Validate(
        string key, string value, DateOnly effectiveFrom, DateOnly today, IEnumerable<RuleParameter> existing)
    {
        var definition = Definitions.FirstOrDefault(d => d.Key == key);
        if (definition is null)
            return [$"Unknown parameter '{key}'."];

        var errors = new List<string>();

        if (!IsValidValue(definition.Kind, value))
            errors.Add(definition.Kind switch
            {
                ParameterKind.Boolean => "The value must be true or false.",
                ParameterKind.Integer => "The value must be a whole number that is not negative.",
                _ => "The value must be a number that is not negative (use a dot as decimal separator)."
            });

        if (effectiveFrom < today)
            errors.Add("A change cannot apply to the past. Choose today or a later date.");

        if (existing.Any(p => p.Key == key && p.EffectiveFrom == effectiveFrom))
            errors.Add($"A value already applies from {effectiveFrom:dd.MM.yyyy}.");

        return errors;
    }

    public static string? CanDelete(RuleParameter parameter, DateOnly today) =>
        parameter.EffectiveFrom > today ? null : "This value is already in effect and cannot be deleted.";

    private static bool IsValidValue(ParameterKind kind, string value) => kind switch
    {
        ParameterKind.Boolean => value is "true" or "false",
        ParameterKind.Integer => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out _),
        _ => decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _)
    };
}