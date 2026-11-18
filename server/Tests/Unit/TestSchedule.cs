using Infra;

namespace Tests.Unit;

/// <summary>Hjælpere til at lave arbejdsskemaer i tests, fx "08:00-15:30".</summary>
public static class TestSchedules
{
    public static readonly DateOnly LongAgo = new(2020, 1, 1);

    /// <summary>Business Esbjergs standardskema: Man–Tor 08:00–15:30 (7,5 t), Fre 08:00–15:00 (7 t) = 37 t/uge.</summary>
    public static WorkSchedule FullTime(DateOnly? from = null) =>
        Create(from ?? LongAgo, "08:00-15:30", "08:00-15:30", "08:00-15:30", "08:00-15:30", "08:00-15:00");

    /// <summary>Et skema med samme tider alle fem hverdage.</summary>
    public static WorkSchedule SameEveryDay(string times, DateOnly? from = null) =>
        Create(from ?? LongAgo, times, times, times, times, times);

    /// <summary>Opretter et skema. null betyder, at medarbejderen ikke arbejder den dag.</summary>
    public static WorkSchedule Create(DateOnly from, string? mon, string? tue, string? wed, string? thu, string? fri,
        DateOnly? to = null)
    {
        var (monStart, monEnd) = Parse(mon);
        var (tueStart, tueEnd) = Parse(tue);
        var (wedStart, wedEnd) = Parse(wed);
        var (thuStart, thuEnd) = Parse(thu);
        var (friStart, friEnd) = Parse(fri);

        return new WorkSchedule
        {
            Id = Guid.NewGuid(),
            EffectiveFrom = from,
            EffectiveTo = to,
            MonStart = monStart, MonEnd = monEnd,
            TueStart = tueStart, TueEnd = tueEnd,
            WedStart = wedStart, WedEnd = wedEnd,
            ThuStart = thuStart, ThuEnd = thuEnd,
            FriStart = friStart, FriEnd = friEnd
        };
    }

    private static (TimeOnly?, TimeOnly?) Parse(string? times)
    {
        if (times is null) return (null, null);
        var parts = times.Split('-');
        return (TimeOnly.Parse(parts[0]), TimeOnly.Parse(parts[1]));
    }
}