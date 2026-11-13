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
[Authorize(Policy = AccessPolicies.AccountingRights)]
public class ApprovalsController(
    ApprovalService service,
    IEmployeeDirectory employeeDirectory) : ControllerBase
{
    [HttpGet("api/approvals/pending")]
    public async Task<ActionResult<IReadOnlyList<PendingRequestDto>>> GetPending(
        CancellationToken ct)
    {
        if (!TryGetIdentity(out var actorUserId, out var identitySubject))
            return Unauthorized();

        var employeeId =
            await employeeDirectory.FindEmployeeIdAsync(identitySubject, ct);

        var requests = await service.ListPendingAsync(employeeId, ct);
        return Ok(requests);
    }

    [HttpPost("{requestId:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid requestId,
        CancellationToken ct)
    {
        if (!TryGetIdentity(out var actorUserId, out var identitySubject))
            return Unauthorized();

        var employeeId =
            await employeeDirectory.FindEmployeeIdAsync(identitySubject, ct);

        var result = await service.DecideAsync(
            requestId,
            actorUserId,
            RequestAction.Approve,
            reason: null,
            actorEmployeeId: employeeId,
            actorHasAccountingRights: AccountingRoles.Has(User),
            ct);

        return ToActionResult(requestId, result);
    }

    [HttpPost("{requestId:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid requestId,
        [FromBody] RejectRequestDto body,
        CancellationToken ct)
    {
        if (!TryGetIdentity(out var actorUserId, out var identitySubject))
            return Unauthorized();

        var employeeId =
            await employeeDirectory.FindEmployeeIdAsync(identitySubject, ct);

        var result = await service.DecideAsync(
            requestId,
            actorUserId,
            RequestAction.Reject,
            body.Reason,
            employeeId,
            AccountingRoles.Has(User),
            ct);

        return ToActionResult(requestId, result);
    }

    private bool TryGetIdentity(
        out Guid actorUserId,
        out string identitySubject)
    {
        identitySubject = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        return Guid.TryParse(identitySubject, out actorUserId)
            && !string.IsNullOrWhiteSpace(identitySubject);
    }

    private IActionResult ToActionResult(Guid requestId, DecisionResult result)
    {
        return result.Outcome switch
        {
            DecisionOutcome.Done when result.NewStatus is { } status =>
                Ok(new DecisionResponseDto(requestId, status)),

            DecisionOutcome.NotFound =>
                NotFound(),

            DecisionOutcome.NotAllowed =>
                Conflict(new { error = result.Error ?? "This decision is not allowed." }),

            DecisionOutcome.Conflict =>
                Conflict(new { error = result.Error ?? "The request changed. Refresh and try again." }),

            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
    [HttpPost("{requestId:guid}/cancel")]
    [Authorize(Roles = "Employee,Accountant,Admin")]
    public async Task<IActionResult> Cancel(
        Guid requestId,
        CancellationToken ct)
    {
        if (!TryGetIdentity(out var actorUserId, out var identitySubject))
            return Unauthorized();

        var employeeId =
            await employeeDirectory.FindEmployeeIdAsync(identitySubject, ct);

        var result = await service.DecideAsync(
            requestId,
            actorUserId,
            RequestAction.Cancel,
            reason: null,
            actorEmployeeId: employeeId,
            actorHasAccountingRights: AccountingRoles.Has(User),
            ct);

        return ToActionResult(requestId, result);
    }
}