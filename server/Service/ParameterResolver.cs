using System.Globalization;
using Infra;

namespace Service;

/// <summary>
/// Slår regelparametre op på en given dato.
/// Den nyeste værdi med EffectiveFrom på eller før datoen gælder,
/// så en ny værdi aldrig ændrer beregninger bagud i tid.
/// </summary>
public class ParameterResolver(IEnumerable<RuleParameter> parameters)
{
    private readonly List<RuleParameter> _parameters = parameters.ToList();

    public string Get(string key, DateOnly date)
    {
        var match = _parameters
            .Where(p => p.Key == key && p.EffectiveFrom <= date)
            .OrderByDescending(p => p.EffectiveFrom)
            .FirstOrDefault();

        return match?.Value
               ?? throw new KeyNotFoundException($"Parameteren '{key}' har ingen værdi den {date:yyyy-MM-dd}.");
    }

    public decimal GetDecimal(string key, DateOnly date) =>
        decimal.Parse(Get(key, date), CultureInfo.InvariantCulture);

    public int GetInt(string key, DateOnly date) =>
        int.Parse(Get(key, date), CultureInfo.InvariantCulture);
}