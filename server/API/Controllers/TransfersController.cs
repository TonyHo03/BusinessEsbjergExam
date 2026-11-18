using System.Security.Claims;
using API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.DTOs;

namespace API.Controllers;

[ApiController]
public class TransfersController(TransferService transfers) : ControllerBase
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    /// <summary>Anmod om at overføre uge 1–5-dage, der udløber 31.12, til næste ferieår.</summary>
    [HttpPost("transfers")]
    [ProducesResponseType<TransferDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransferDto>> Create(TransferInput input)
    {
        var result = await transfers.CreateAsync(input, Today);

        if (result is null)
            return NotFound();

        if (result.Created is null)
            return BadRequest(result.Errors);

        return Created($"/transfers/{result.Created.Id}", result.Created);
    }

    /// <summary>En medarbejders overførsler, nyeste først.</summary>
    [HttpGet("employees/{employeeId:guid}/transfers")]
    public async Task<ActionResult<IReadOnlyList<TransferDto>>> GetForEmployee(Guid employeeId) =>
        Ok(await transfers.GetForEmployeeAsync(employeeId));

    [HttpGet("api/transfers/pending"), Authorize(Policy = AccessPolicies.AccountingRights)]
    public async Task<ActionResult<IReadOnlyList<TransferDto>>> GetPending() =>
        Ok(await transfers.ListPendingAsync());

    [HttpPost("api/transfers/{id:guid}/approve"), Authorize(Policy = AccessPolicies.AccountingRights)]
    public Task<ActionResult<TransferDto>> Approve(Guid id) => Decide(id, true, null);

    [HttpPost("api/transfers/{id:guid}/reject"), Authorize(Policy = AccessPolicies.AccountingRights)]
    public Task<ActionResult<TransferDto>> Reject(Guid id, RejectRequestDto body) => Decide(id, false, body.Reason);

    private async Task<ActionResult<TransferDto>> Decide(Guid id, bool approve, string? reason)
    {
        var actorId = Guid.Parse(User.FindFirstValue("sub")!);
        var result = await transfers.DecideAsync(id, approve, reason, actorId, Today);

        return result.Outcome switch
        {
            DecisionOutcome.Done => Ok(result.Transfer),
            DecisionOutcome.NotFound => NotFound(),
            _ => Problem(result.Error, statusCode: StatusCodes.Status409Conflict)
        };
    }
}