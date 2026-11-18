using LinqToDB.Mapping;

namespace Infra;

/// <summary>
/// Anmodning om at overføre uge 1–5-dage, der ellers udløber 31.12, til næste ferieår.
/// Godkendes eller afvises af bogholderiet. Først ved godkendelse flyttes dagene i ledgeren.
/// </summary>
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