using Microsoft.AspNetCore.Mvc;
using Service;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[ApiController]
[Route("team-holidays")]
[Authorize]
public class TeamHolidaysController(TeamHolidayService teamHolidays) : ControllerBase
{
    private const int MaxDays = 366;

    /// <summary>
    /// Alles godkendte ferie i en periode og hvor mange der er væk/til stede pr. dag.
    /// Bruges til holdkalenderen.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<TeamCalendarResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamCalendarResponse>> Get([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        if (to < from)
            return BadRequest("Slutdatoen kan ikke ligge før startdatoen.");

        if (to.DayNumber - from.DayNumber >= MaxDays)
            return BadRequest($"Perioden kan højst være {MaxDays} dage.");

        return Ok(await teamHolidays.GetAsync(from, to));
    }
}