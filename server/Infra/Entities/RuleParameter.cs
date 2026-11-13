using LinqToDB.Mapping;

namespace Infra;

[Table("rule_parameter")]
public class RuleParameter
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("key"), NotNull] public string Key { get; set; } = "";
    [Column("value"), NotNull] public string Value { get; set; } = "";
    [Column("effective_from")] public DateOnly EffectiveFrom { get; set; }
}