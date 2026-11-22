using API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers;

[ApiController]
[Route("api/days-off")]
[Authorize]
public class DaysOffController(DayOffService daysOff) : ControllerBase
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    // Alle må se helligdage og firmafridage
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DayOffDto>>> Get([FromQuery] int? year) =>
        Ok(await daysOff.GetAsync(year ?? Today.Year));

    [HttpPost, Authorize(Policy = AccessPolicies.AdminRights)]
    [ProducesResponseType<DayOffDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DayOffDto>> Add(DayOffInput input)
    {
        var result = await daysOff.AddAsync(input, Today);

        return result.Created is null
            ? BadRequest(result.Errors)
            : Created($"/api/days-off/{result.Created.Id}", result.Created);
    }

    [HttpDelete("{id:guid}"), Authorize(Policy = AccessPolicies.AdminRights)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var error = await daysOff.DeleteAsync(id, Today);
            return error is null ? NoContent() : Problem(error, statusCode: StatusCodes.Status409Conflict);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}