using LinqToDB;
using LinqToDB.Mapping;

namespace Infra;

[Table("employees")]
public class Employee
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("name"), NotNull] public string Name { get; set; } = "";
    [Column("email"), NotNull] public string Email { get; set; } = "";
    [Column("password_hash"), NotNull] public string PasswordHash { get; set; } = "";
    [Column("start_date")] public DateOnly StartDate { get; set; }
    [Column("end_date")] public DateOnly? EndDate { get; set; }
    [Column("created_at")] public DateTimeOffset CreatedAt { get; set; }
    [Column("role", DataType = DataType.NVarChar, Length = 255), NotNull] public required Roles Role { get; set; }
}