using System.Security.Claims;
using API.Security;
using Infra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.DTOs;
using Service.Security;

namespace API.Controllers;

[ApiController]
[Route("api/approvals")]
[Authorize]
public class ApprovalsController(ApprovalService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue("sub")!);

    [HttpGet("pending"), Authorize(Policy = AccessPolicies.AccountingRights)]
    public async Task<IReadOnlyList<PendingRequestDto>> GetPending(CancellationToken ct) =>
        await service.ListPendingAsync(UserId, ct);

    [HttpPost("{id:guid}/approve"), Authorize(Policy = AccessPolicies.AccountingRights)]
    public Task<ActionResult<DecisionResponseDto>> Approve(Guid id, CancellationToken ct) =>
        Decide(id, RequestAction.Approve, null, ct);

    [HttpPost("{id:guid}/reject"), Authorize(Policy = AccessPolicies.AccountingRights)]
    public Task<ActionResult<DecisionResponseDto>> Reject(Guid id, RejectRequestDto body, CancellationToken ct) =>
        Decide(id, RequestAction.Reject, body.Reason, ct);

    [HttpPost("{id:guid}/cancel")]
    public Task<ActionResult<DecisionResponseDto>> Cancel(Guid id, CancellationToken ct) =>
        Decide(id, RequestAction.Cancel, null, ct);

    [HttpPost("{id:guid}/request-cancellation")]
    public Task<ActionResult<DecisionResponseDto>> RequestCancellation(Guid id, CancellationToken ct) =>
        Decide(id, RequestAction.RequestCancellation, null, ct);

    private async Task<ActionResult<DecisionResponseDto>> Decide(
        Guid id, RequestAction action, string? reason, CancellationToken ct)
    {
        var result = await service.DecideAsync(id, action, reason, UserId, AccountingRoles.Has(User), ct);
        return result.Outcome switch
        {
            DecisionOutcome.Done => new DecisionResponseDto(id, result.NewStatus!.Value),
            DecisionOutcome.NotFound => NotFound(),
            _ => Problem(result.Error, statusCode: StatusCodes.Status409Conflict)
        };
    }
}