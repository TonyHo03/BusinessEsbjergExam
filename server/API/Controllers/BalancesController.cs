using API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers;

[ApiController]
[Route("api/employees/{employeeId:guid}/balances")]
[Authorize]
public class BalancesController(HolidayService holidays) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BalancesResponse>> Get(Guid employeeId, [FromQuery] DateOnly? asOf)
    {
        if (!User.CanAccess(employeeId))
            return Forbid();

        var date = asOf ?? DateOnly.FromDateTime(DateTime.Today);
        var result = await holidays.GetBalancesAsync(employeeId, date);

        return result is null ? NotFound() : Ok(result);
    }
}