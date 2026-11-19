using System.Globalization;
using Infra;

namespace Service;

public enum ParameterKind { Decimal, Integer, Boolean }

/// <summary>De regelparametre, der findes, hvad de betyder, og hvilken slags værdi de tager.</summary>
public record ParameterDefinition(string Key, string Description, ParameterKind Kind);

public static class ParameterRules
{
    public static readonly IReadOnlyList<ParameterDefinition> Definitions =
    [
        new(ParameterKeys.Weeks1To5DaysPerMonth, "Uge 1–5: dage optjent pr. måned", ParameterKind.Decimal),
        new(ParameterKeys.Week6HoursPerMonth, "6. ferieuge: timer pr. måned (fuldtid)", ParameterKind.Decimal),
        new(ParameterKeys.MainHolidayDays, "Hovedferie: antal sammenhængende dage", ParameterKind.Integer),
        new(ParameterKeys.NoticeMainHolidayMonths, "Varsel for hovedferie (måneder)", ParameterKind.Integer),
        new(ParameterKeys.NoticeOtherMonths, "Varsel for øvrig ferie (måneder)", ParameterKind.Integer),
        new(ParameterKeys.TransferEnabled, "Overførsel af ferie er slået til", ParameterKind.Boolean),
        new(ParameterKeys.TransferMaxDays, "Overførsel: højst antal dage", ParameterKind.Decimal)
    ];

    /// <summary>
    /// Tjekker en ny værdi. En ændring må aldrig påvirke fortiden, så den skal gælde fra i dag eller senere.
    /// Den gamle værdi bliver stående og gælder stadig for datoer før.
    /// </summary>
    public static IReadOnlyList<string> Validate(
        string key, string value, DateOnly effectiveFrom, DateOnly today, IEnumerable<RuleParameter> existing)
    {
        var definition = Definitions.FirstOrDefault(d => d.Key == key);
        if (definition is null)
            return [$"Ukendt parameter '{key}'."];

        var errors = new List<string>();

        if (!IsValidValue(definition.Kind, value))
            errors.Add(definition.Kind switch
            {
                ParameterKind.Boolean => "Værdien skal være true eller false.",
                ParameterKind.Integer => "Værdien skal være et helt tal, der ikke er negativt.",
                _ => "Værdien skal være et tal, der ikke er negativt (brug punktum som decimaltegn)."
            });

        if (effectiveFrom < today)
            errors.Add("En ændring kan ikke gælde bagud i tid. Vælg i dag eller en senere dato.");

        if (existing.Any(p => p.Key == key && p.EffectiveFrom == effectiveFrom))
            errors.Add($"Der findes allerede en værdi, der gælder fra {effectiveFrom:dd.MM.yyyy}.");

        return errors;
    }

    /// <summary>Kun værdier, der ikke er trådt i kraft endnu, kan slettes – ellers ville det ændre fortiden.</summary>
    public static string? CanDelete(RuleParameter parameter, DateOnly today) =>
        parameter.EffectiveFrom > today ? null : "Værdien er allerede trådt i kraft og kan ikke slettes.";

    private static bool IsValidValue(ParameterKind kind, string value) => kind switch
    {
        ParameterKind.Boolean => value is "true" or "false",
        ParameterKind.Integer => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out _),
        _ => decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _)
    };
}