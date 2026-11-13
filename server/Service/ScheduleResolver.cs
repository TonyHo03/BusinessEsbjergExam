using Infra;

namespace Service;

public static class ScheduleResolver
{
    /// <summary>Finder det arbejdsskema, der gælder på datoen (nyeste EffectiveFrom på eller før datoen).</summary>
    public static WorkSchedule For(IEnumerable<WorkSchedule> schedules, DateOnly date) =>
        schedules
            .Where(s => s.EffectiveFrom <= date)
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefault()
        ?? throw new InvalidOperationException($"Der findes intet arbejdsskema den {date:yyyy-MM-dd}.");
}