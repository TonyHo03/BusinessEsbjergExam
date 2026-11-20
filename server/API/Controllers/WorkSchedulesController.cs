using API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers;

[ApiController]
[Route("api/employees/{employeeId:guid}/schedules")]
[Authorize]
public class WorkSchedulesController(WorkScheduleService schedules) : ControllerBase
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    /// <summary>En medarbejders arbejdsskemaer, nyeste først. Medarbejderen selv og bogholderiet har adgang.</summary>
    [HttpGet]
    public async Task<ActionResult<WorkSchedulesResponse>> Get(Guid employeeId)
    {
        if (!User.CanAccess(employeeId))
            return Forbid();

        var result = await schedules.GetAsync(employeeId, Today);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Tilføjer et nyt skema (kun admin). Det nuværende skema slutter dagen før.</summary>
    [HttpPost, Authorize(Policy = AccessPolicies.AdminRights)]
    [ProducesResponseType<WorkScheduleDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkScheduleDto>> Add(Guid employeeId, WorkScheduleInput input)
    {
        var result = await schedules.AddAsync(employeeId, input, Today);

        if (result is null)
            return NotFound();

        if (result.Created is null)
            return BadRequest(result.Errors);

        return Created($"/api/employees/{employeeId}/schedules", result.Created);
    }

    /// <summary>Sletter et kommende skema (kun admin). Det forrige skema gælder så videre.</summary>
    [HttpDelete("{id:guid}"), Authorize(Policy = AccessPolicies.AdminRights)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid employeeId, Guid id)
    {
        try
        {
            var error = await schedules.DeleteAsync(employeeId, id, Today);
            return error is null ? NoContent() : Conflict(error);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}