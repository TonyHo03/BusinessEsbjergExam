using System.ComponentModel.DataAnnotations;
using Infra;

namespace Service.DTOs;

public static class ApprovalLimits
{
    public const int MaxReasonLength = 500;
}

public record PendingRequestDto(
    Guid RequestId,
    Guid EmployeeId,
    string EmployeeName,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    decimal Hours,
    HolidayAccount Account,
    DateTimeOffset SubmittedAt,
    string? Note,
    bool IsOwnRequest);
    
public record RejectRequestDto([param: Required, StringLength(500)] string Reason);

public record DecisionResponseDto(Guid RequestId, RequestStatus Status);