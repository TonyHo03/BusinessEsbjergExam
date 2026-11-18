using LinqToDB.Mapping;

namespace Infra;


[Table("approval_audit")]
public class ApprovalAudit
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("request_id")] public Guid RequestId { get; set; }
    [Column("actor_id")] public Guid ActorId { get; set; }
    [Column("action")] public RequestAction Action { get; set; }
    [Column("from_status")] public RequestStatus FromStatus { get; set; }
    [Column("to_status")] public RequestStatus ToStatus { get; set; }
    [Column("reason")] public string? Reason { get; set; }
    [Column("occurred_at")] public DateTimeOffset OccurredAt { get; set; }
}