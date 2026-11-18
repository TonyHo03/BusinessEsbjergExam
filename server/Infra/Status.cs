using LinqToDB.Mapping;

namespace Infra;

public enum Status
{
    [MapValue("Active")] Active,
    [MapValue("Inactive")] Inactive
}