using System.Globalization;
using Infra;

namespace Service;

/// <summary>Fra- og til-tid på én dag som "HH:mm". Begge tomme betyder, at det ikke er en arbejdsdag.</summary>
public record DayTimes(string? Start, string? End);

/// <param name="EffectiveFrom">Datoen skemaet gælder fra. Det tidligere skema gælder til og med dagen før.</param>
public record WorkScheduleInput(
    DateOnly EffectiveFrom,
    DayTimes Monday,
    DayTimes Tuesday,
    DayTimes Wednesday,
    DayTimes Thursday,
    DayTimes Friday);

/// <summary>
/// Regler for et nyt arbejdsskema. Et skema ændrer aldrig fortiden:
/// det nye skema skal starte efter det nuværende og tidligst i dag.
/// Har medarbejderen intet skema endnu, må det starte allerede på ansættelsesdatoen.
/// </summary>
public static class WorkScheduleRules
{
    public static (WorkSchedule? Schedule, IReadOnlyList<string> Errors) Build(
        Employee employee, WorkScheduleInput input, IReadOnlyList<WorkSchedule> existing, DateOnly today)
    {
        var errors = new List<string>();

        var mon = ParseDay("Monday", input.Monday, errors);
        var tue = ParseDay("Tuesday", input.Tuesday, errors);
        var wed = ParseDay("Wednesday", input.Wednesday, errors);
        var thu = ParseDay("Thursday", input.Thursday, errors);
        var fri = ParseDay("Friday", input.Friday, errors);

        if (errors.Count == 0 && new[] { mon, tue, wed, thu, fri }.All(d => d.Start is null))
            errors.Add("The schedule must have at least one working day.");

        if (input.EffectiveFrom < employee.StartDate)
            errors.Add($"The schedule cannot start before the employee started ({employee.StartDate:dd.MM.yyyy}).");

        var latest = existing.OrderByDescending(s => s.EffectiveFrom).FirstOrDefault();
        if (latest is not null)
        {
            if (input.EffectiveFrom <= latest.EffectiveFrom)
                errors.Add($"The schedule must start after the current one ({latest.EffectiveFrom:dd.MM.yyyy}).");
            else if (input.EffectiveFrom < today)
                errors.Add("A schedule change cannot apply to the past. Choose today or a later date.");
        }

        if (errors.Count > 0)
            return (null, errors);

        return (new WorkSchedule
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            EffectiveFrom = input.EffectiveFrom,
            MonStart = mon.Start, MonEnd = mon.End,
            TueStart = tue.Start, TueEnd = tue.End,
            WedStart = wed.Start, WedEnd = wed.End,
            ThuStart = thu.Start, ThuEnd = thu.End,
            FriStart = fri.Start, FriEnd = fri.End
        }, errors);
    }

    /// <summary>Det skema, der gælder nu og ingen slutdato har, skal slutte dagen før det nye starter.</summary>
    public static WorkSchedule? ScheduleToClose(IReadOnlyList<WorkSchedule> existing, DateOnly newFrom) =>
        existing
            .Where(s => s.EffectiveFrom < newFrom && (s.EffectiveTo is null || s.EffectiveTo >= newFrom))
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefault();
    
    /// <summary>
    /// Kun det seneste skema kan slettes, og kun hvis det ikke er trådt i kraft endnu – ellers ville det ændre fortiden.
    /// Null betyder, at det må slettes.
    /// </summary>
    public static string? CanDelete(WorkSchedule schedule, IReadOnlyList<WorkSchedule> existing, DateOnly today)
    {
        if (schedule.EffectiveFrom < today)
            return "This schedule started before today and cannot be deleted.";

        if (existing.Any(s => s.EffectiveFrom > schedule.EffectiveFrom))
            return "Only the latest schedule can be deleted.";

        return null;
    }

    /// <summary>Det skema, der sluttede dagen før det slettede startede. Det skal gælde videre uden slutdato.</summary>
    public static WorkSchedule? ScheduleToReopen(IReadOnlyList<WorkSchedule> existing, WorkSchedule deleted) =>
        existing.FirstOrDefault(s => s.EffectiveTo == deleted.EffectiveFrom.AddDays(-1));

    public static string? Format(TimeOnly? time) => time?.ToString("HH:mm", CultureInfo.InvariantCulture);

    private static (TimeOnly? Start, TimeOnly? End) ParseDay(string day, DayTimes? times, List<string> errors)
    {
        var start = times?.Start?.Trim();
        var end = times?.End?.Trim();

        if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            return (null, null);

        if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
        {
            errors.Add($"{day}: enter both a start and an end time, or leave both empty.");
            return (null, null);
        }

        if (!TimeOnly.TryParseExact(start, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var s) ||
            !TimeOnly.TryParseExact(end, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var e))
        {
            errors.Add($"{day}: times must be written as HH:mm, e.g. 08:00.");
            return (null, null);
        }

        if (e <= s)
        {
            errors.Add($"{day}: the end time must be after the start time.");
            return (null, null);
        }

        return (s, e);
    }
}