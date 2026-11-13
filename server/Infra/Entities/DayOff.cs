using LinqToDB.Mapping;

namespace Infra;

[Table("day_off")]
public class DayOff
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("date")] public DateOnly Date { get; set; }
    [Column("name"), NotNull] public string Name { get; set; } = "";
    [Column("is_company")] public bool IsCompany { get; set; }
}