using System.ComponentModel.DataAnnotations;
using Infra;

namespace Service.DTOs;

public static class ApprovalLimits
{
    public const int MaxReasonLength = 300;
}

public record PendingRequestDto(
    Guid RequestId,
    Guid EmployeeId,
    string EmployeeName,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    decimal WorkingHours,
    HolidayAccount Account,
    DateTimeOffset SubmittedAt,
    string? Note,
    bool IsOwnRequest);
    
public record RejectRequestDto([property: Required, StringLength(ApprovalLimits.MaxReasonLength)] string Reason);

public record DecisionResponseDto(Guid RequestId, RequestStatus Status);