using LinqToDB.Mapping;

namespace Infra;

public enum Roles
{
    [MapValue("Employee")] Employee,
    [MapValue("Accountant")] Accountant,
    [MapValue("Admin")] Admin
}