using Infra;

namespace Service;

public static class ScheduleResolver
{
    /// <summary>
    /// Finder det arbejdsskema, der gælder på datoen.
    /// Overlapper to perioder, vinder den nyeste.
    /// </summary>
    public static WorkSchedule For(IEnumerable<WorkSchedule> schedules, DateOnly date) =>
        schedules
            .Where(s => s.Covers(date))
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefault()
        ?? throw new InvalidOperationException($"Der findes intet arbejdsskema den {date:yyyy-MM-dd}.");
}