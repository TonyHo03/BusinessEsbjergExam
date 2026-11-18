using LinqToDB.Mapping;

namespace Infra;

/// <summary>
/// En medarbejders arbejdstider mandag–fredag i en periode.
/// En dag uden tider (null) er ikke en arbejdsdag.
/// Perioden løber fra EffectiveFrom til og med EffectiveTo (null = indtil videre).
/// </summary>
[Table("work_schedule")]
public class WorkSchedule
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("employee_id")] public Guid EmployeeId { get; set; }
    [Column("effective_from")] public DateOnly EffectiveFrom { get; set; }
    [Column("effective_to")] public DateOnly? EffectiveTo { get; set; }

    [Column("mon_start")] public TimeOnly? MonStart { get; set; }
    [Column("mon_end")] public TimeOnly? MonEnd { get; set; }
    [Column("tue_start")] public TimeOnly? TueStart { get; set; }
    [Column("tue_end")] public TimeOnly? TueEnd { get; set; }
    [Column("wed_start")] public TimeOnly? WedStart { get; set; }
    [Column("wed_end")] public TimeOnly? WedEnd { get; set; }
    [Column("thu_start")] public TimeOnly? ThuStart { get; set; }
    [Column("thu_end")] public TimeOnly? ThuEnd { get; set; }
    [Column("fri_start")] public TimeOnly? FriStart { get; set; }
    [Column("fri_end")] public TimeOnly? FriEnd { get; set; }

    /// <summary>Timer på en ugedag, beregnet ud fra fra- og til-tid. Weekender er altid 0.</summary>
    public decimal HoursFor(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => Hours(MonStart, MonEnd),
        DayOfWeek.Tuesday => Hours(TueStart, TueEnd),
        DayOfWeek.Wednesday => Hours(WedStart, WedEnd),
        DayOfWeek.Thursday => Hours(ThuStart, ThuEnd),
        DayOfWeek.Friday => Hours(FriStart, FriEnd),
        _ => 0m
    };

    public decimal WeeklyHours =>
        HoursFor(DayOfWeek.Monday) + HoursFor(DayOfWeek.Tuesday) + HoursFor(DayOfWeek.Wednesday) +
        HoursFor(DayOfWeek.Thursday) + HoursFor(DayOfWeek.Friday);

    public bool Covers(DateOnly date) =>
        date >= EffectiveFrom && (EffectiveTo is null || date <= EffectiveTo);

    private static decimal Hours(TimeOnly? start, TimeOnly? end) =>
        start is { } s && end is { } e && e > s
            ? Math.Round((decimal)(e - s).TotalHours, 2)
            : 0m;
}