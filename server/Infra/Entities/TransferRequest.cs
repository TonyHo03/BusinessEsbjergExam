using LinqToDB.Mapping;

namespace Infra;

[Table("transfer_request")]
public class TransferRequest
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("employee_id")] public Guid EmployeeId { get; set; }
    [Column("from_earning_year")] public int FromEarningYear { get; set; }
    [Column("days")] public decimal Days { get; set; }
    [Column("status")] public RequestStatus Status { get; set; }
    [Column("comment")] public string? Comment { get; set; }
    [Column("rejection_reason")] public string? RejectionReason { get; set; }
    [Column("decided_by")] public Guid? DecidedBy { get; set; }
    [Column("decided_at")] public DateTimeOffset? DecidedAt { get; set; }
    [Column("created_at")] public DateTimeOffset CreatedAt { get; set; }
}