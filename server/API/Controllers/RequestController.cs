using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers;

[ApiController]
[Route("requests")]
public class RequestsController(RequestService requests) : ControllerBase
{
    private const int MaxDays = 366;

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    /// <summary>Beregner timer og validerer en ansøgning uden at gemme den.</summary>
    [HttpPost("preview")]
    [ProducesResponseType<RequestPreview>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequestPreview>> Preview(RequestInput input)
    {
        if (CheckPeriod(input) is { } problem)
            return problem;

        var preview = await requests.PreviewAsync(input, Today);
        return preview is null ? NotFound() : Ok(preview);
    }

    /// <summary>Sender en ansøgning. Gemmes som Pending, hvis den er gyldig.</summary>
    [HttpPost]
    [ProducesResponseType<RequestDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<RequestPreview>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequestDto>> Submit(RequestInput input)
    {
        if (CheckPeriod(input) is { } problem)
            return problem;

        var result = await requests.SubmitAsync(input, Today);

        if (result is null)
            return NotFound();

        if (result.Created is null)
            return BadRequest(result.Preview); // valideringsfejlene sendes med tilbage

        return Created($"/requests/{result.Created.Id}", result.Created);
    }

    /// <summary>En medarbejders ansøgninger, nyeste først.</summary>
    [HttpGet("/employees/{employeeId:guid}/requests")]
    public async Task<ActionResult<IReadOnlyList<RequestDto>>> GetForEmployee(Guid employeeId) =>
        Ok(await requests.GetForEmployeeAsync(employeeId));

    private ActionResult? CheckPeriod(RequestInput input)
    {
        if (input.EndDate < input.StartDate)
            return BadRequest("Sidste feriedag kan ikke ligge før første feriedag.");

        if (input.EndDate.DayNumber - input.StartDate.DayNumber >= MaxDays)
            return BadRequest($"En ansøgning kan højst dække {MaxDays} dage.");

        return null;
    }
}