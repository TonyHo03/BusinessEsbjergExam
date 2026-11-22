using Infra;

namespace Service;

public static class ScheduleResolver
{
    public static WorkSchedule For(IEnumerable<WorkSchedule> schedules, DateOnly date) =>
        schedules
            .Where(s => s.Covers(date))
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefault()
        ?? throw new InvalidOperationException($"Der findes intet arbejdsskema den {date:yyyy-MM-dd}.");
    
    public static decimal DayLength(IEnumerable<WorkSchedule> schedules, DateOnly date)
    {
        var dayLength = For(schedules, date).WeeklyHours / 5m;

        return dayLength > 0
            ? dayLength
            : throw new InvalidOperationException($"Arbejdsskemaet den {date:yyyy-MM-dd} har ingen arbejdstimer.");
    }
}