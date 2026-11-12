using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers;

[ApiController]
[Route("employees/{employeeId:guid}/balances")]
public class BalancesController(HolidayService holidays) : ControllerBase
{
    /// <summary>Saldi pr. konto og optjeningsperiode med udløbsdatoer.</summary>
    [HttpGet]
    public async Task<ActionResult<BalancesResponse>> Get(Guid employeeId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var result = await holidays.GetBalancesAsync(employeeId, today);

        return result is null ? NotFound() : Ok(result);
    }
}