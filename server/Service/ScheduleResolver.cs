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
    
    /// <summary>
    /// Timer pr. feriedag på datoen: ugentlige timer / 5.
    /// En uges ferie er altid 5 feriedage, også for deltidsansatte, der arbejder færre dage.
    /// </summary>
    public static decimal DayLength(IEnumerable<WorkSchedule> schedules, DateOnly date)
    {
        var dayLength = For(schedules, date).WeeklyHours / 5m;

        return dayLength > 0
            ? dayLength
            : throw new InvalidOperationException($"Arbejdsskemaet den {date:yyyy-MM-dd} har ingen arbejdstimer.");
    }
}