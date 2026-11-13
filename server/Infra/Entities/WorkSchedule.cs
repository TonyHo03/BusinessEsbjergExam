using LinqToDB.Mapping;

namespace Infra;

[Table("work_schedule")]
public class WorkSchedule
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("employee_id")] public Guid EmployeeId { get; set; }
    [Column("effective_from")] public DateOnly EffectiveFrom { get; set; }
    [Column("mon")] public decimal Mon { get; set; }
    [Column("tue")] public decimal Tue { get; set; }
    [Column("wed")] public decimal Wed { get; set; }
    [Column("thu")] public decimal Thu { get; set; }
    [Column("fri")] public decimal Fri { get; set; }
    [Column("sat")] public decimal Sat { get; set; }
    [Column("sun")] public decimal Sun { get; set; }

    public decimal HoursFor(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => Mon,
        DayOfWeek.Tuesday => Tue,
        DayOfWeek.Wednesday => Wed,
        DayOfWeek.Thursday => Thu,
        DayOfWeek.Friday => Fri,
        DayOfWeek.Saturday => Sat,
        DayOfWeek.Sunday => Sun,
        _ => 0
    };
}