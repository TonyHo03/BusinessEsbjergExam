using Infra;
using LinqToDB;
using LinqToDB.Async;

namespace Service;

public enum ParameterVersionStatus { Past, Current, Upcoming }

public record ParameterVersionDto(Guid Id, string Value, DateOnly EffectiveFrom, ParameterVersionStatus Status);

/// <param name="CurrentValue">Værdien, der gælder på AsOf. Null, hvis ingen værdi gælder endnu.</param>
/// <param name="Versions">Alle værdier, nyeste først.</param>
public record ParameterDto(
    string Key,
    string Description,
    ParameterKind Kind,
    string? CurrentValue,
    DateOnly? CurrentFrom,
    IReadOnlyList<ParameterVersionDto> Versions);

public record ParameterInput(string Key, string Value, DateOnly EffectiveFrom);

public record ParameterResult(IReadOnlyList<string> Errors, ParameterVersionDto? Created);

/// <summary>Regelparametre med historik. Ændringer tilføjes som nye værdier med en startdato og overskriver aldrig de gamle.</summary>
public class ParameterService(AppDb db)
{
    public async Task<IReadOnlyList<ParameterDto>> GetAsync(DateOnly asOf)
    {
        var all = await db.RuleParameters.ToListAsync();

        return ParameterRules.Definitions
            .Select(d =>
            {
                var versions = all.Where(p => p.Key == d.Key).OrderByDescending(p => p.EffectiveFrom).ToList();
                var current = versions.FirstOrDefault(p => p.EffectiveFrom <= asOf);

                return new ParameterDto(
                    d.Key,
                    d.Description,
                    d.Kind,
                    current?.Value,
                    current?.EffectiveFrom,
                    versions.Select(p => new ParameterVersionDto(
                            p.Id,
                            p.Value,
                            p.EffectiveFrom,
                            p.EffectiveFrom > asOf ? ParameterVersionStatus.Upcoming
                            : p == current ? ParameterVersionStatus.Current
                            : ParameterVersionStatus.Past))
                        .ToList());
            })
            .ToList();
    }

    public async Task<ParameterResult> AddAsync(ParameterInput input, DateOnly today)
    {
        var value = input.Value.Trim();
        var existing = await db.RuleParameters.Where(p => p.Key == input.Key).ToListAsync();

        var errors = ParameterRules.Validate(input.Key, value, input.EffectiveFrom, today, existing);
        if (errors.Count > 0)
            return new ParameterResult(errors, null);

        var parameter = new RuleParameter
        {
            Id = Guid.NewGuid(),
            Key = input.Key,
            Value = value,
            EffectiveFrom = input.EffectiveFrom
        };
        await db.InsertAsync(parameter);

        var status = parameter.EffectiveFrom > today ? ParameterVersionStatus.Upcoming : ParameterVersionStatus.Current;
        return new ParameterResult([], new ParameterVersionDto(parameter.Id, parameter.Value, parameter.EffectiveFrom, status));
    }

    /// <returns>Null hvis slettet, ellers en fejlbesked. KeyNotFoundException hvis den ikke findes.</returns>
    public async Task<string?> DeleteAsync(Guid id, DateOnly today)
    {
        var parameter = await db.RuleParameters.FirstOrDefaultAsync(p => p.Id == id)
                        ?? throw new KeyNotFoundException();

        var error = ParameterRules.CanDelete(parameter, today);
        if (error is null)
            await db.RuleParameters.Where(p => p.Id == id).DeleteAsync();

        return error;
    }
}