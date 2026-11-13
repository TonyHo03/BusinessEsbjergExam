using LinqToDB.Mapping;

namespace Infra.Entities;


[System.ComponentModel.DataAnnotations.Schema.Table("request")]
public class Request
{
    [PrimaryKey, Column("id")] public Guid Id { get; set; }
    
    [Column("employee_id")] public Guid EmployeeId { get; set; }
    
    [Column("status")] public RequestStatus Status { get; set; }
    
    [Column("start_date")] public DateOnly StartDate { get; set; }
    
    [Column("end_date")] public DateOnly EndDate { get; set; }
    
    [Column("working_days")] public int WorkingDays { get; set; }
    
    [Column("hours")] public decimal Hours { get; set; }
    
    [Column("account_id")] public HolidayAccount Account { get; set; }
    
    [Column("submitted_at")] public DateTimeOffset SubmittedAt { get; set; }
    
    [Column("note")] public string? Note { get; set; }
}