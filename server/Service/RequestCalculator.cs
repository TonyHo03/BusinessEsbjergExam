using System.Text.Json.Serialization;
using Infra;

namespace Service;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DayKind
{
    Workday,
    NonWorkingDay, // weekend eller en dag uden timer i skemaet
    DayOff         // helligdag eller firmafridag
}

public record RequestDay(DateOnly Date, DayKind Kind, decimal Hours);

public record RequestCalculation(IReadOnlyList<RequestDay> Days)
{
    public decimal TotalHours => Days.Sum(d => d.Hours);
    public int WorkingDays => Days.Count(d => d.Kind == DayKind.Workday);
}

/// <summary>
/// Omregner en periode i hele dage til timer ud fra medarbejderens arbejdsskema.
/// Helligdage, firmafridage og dage uden timer i skemaet koster 0 timer.
/// </summary>
public static class RequestCalculator
{
    public static RequestCalculation Calculate(
        DateOnly start,
        DateOnly end,
        IReadOnlyList<WorkSchedule> schedules,
        IReadOnlySet<DateOnly> daysOff)
    {
        if (end < start)
            throw new ArgumentException("Sidste feriedag kan ikke ligge før første feriedag.");

        var days = new List<RequestDay>();

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (daysOff.Contains(date))
            {
                days.Add(new RequestDay(date, DayKind.DayOff, 0));
                continue;
            }

            var hours = ScheduleResolver.For(schedules, date).HoursFor(date.DayOfWeek);
            var kind = hours > 0 ? DayKind.Workday : DayKind.NonWorkingDay;
            days.Add(new RequestDay(date, kind, hours));
        }

        return new RequestCalculation(days);
    }
}