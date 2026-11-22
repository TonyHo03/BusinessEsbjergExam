using Infra;

namespace Service;

// Regler for helligdage og firmafridage. En ændring må aldrig ændre fortiden,
// så kun dage fra i dag og frem kan tilføjes eller slettes.
public static class DayOffRules
{
    public const int MaxNameLength = 100;

    public static IReadOnlyList<string> Validate(DateOnly date, string name, DateOnly today, IEnumerable<DayOff> existing)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add("Give the day a name, for example \"Christmas Eve\".");
        else if (name.Trim().Length > MaxNameLength)
            errors.Add($"The name can be at most {MaxNameLength} characters.");

        if (date < today)
            errors.Add("A day off cannot be added in the past.");

        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            errors.Add("Weekends are never working days, so they do not need to be added.");

        if (existing.Any(d => d.Date == date))
            errors.Add($"{date:dd.MM.yyyy} is already a day off.");

        return errors;
    }

    public static string? CanDelete(DayOff dayOff, DateOnly today) =>
        dayOff.Date < today ? "A day off in the past cannot be deleted." : null;
}