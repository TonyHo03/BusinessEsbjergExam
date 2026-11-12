using LinqToDB.Mapping;

namespace Infra;

public enum RequestStatus
{
    [MapValue("Pending")] Pending,
    [MapValue("Approved")] Approved,
    [MapValue("Rejected")] Rejected,
    [MapValue("Cancelled")] Cancelled
}

[Table("holiday_request")]
public class HolidayRequest
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    [Column("employee_id")] public Guid EmployeeId { get; set; }
    [Column("account")] public HolidayAccount Account { get; set; }
    [Column("start_date")] public DateOnly StartDate { get; set; }
    [Column("end_date")] public DateOnly EndDate { get; set; }
    [Column("hours")] public decimal Hours { get; set; }
    [Column("status")] public RequestStatus Status { get; set; }
    [Column("comment")] public string? Comment { get; set; }
    [Column("rejection_reason")] public string? RejectionReason { get; set; }
    [Column("decided_by")] public Guid? DecidedBy { get; set; }
    [Column("decided_at")] public DateTimeOffset? DecidedAt { get; set; }
    [Column("created_at")] public DateTimeOffset CreatedAt { get; set; }
}