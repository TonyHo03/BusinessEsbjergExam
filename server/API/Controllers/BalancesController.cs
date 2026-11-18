using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers;

[ApiController]
[Route("employees/{employeeId:guid}/balances")]
public class BalancesController(HolidayService holidays) : ControllerBase
{
    /// <summary>
    /// Saldi pr. konto og optjeningsperiode med udløbsdatoer.
    /// Angiv asOf for at se saldoen på en anden dato, fx "hvor meget har jeg i december?".
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<BalancesResponse>> Get(Guid employeeId, [FromQuery] DateOnly? asOf)
    {
        var date = asOf ?? DateOnly.FromDateTime(DateTime.Today);
        var result = await holidays.GetBalancesAsync(employeeId, date);

        return result is null ? NotFound() : Ok(result);
    }
}