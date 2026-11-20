using API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers;

[ApiController]
[Route("api/employees/overview")]
[Authorize(Policy = AccessPolicies.AccountingRights)]
public class EmployeeOverviewController(EmployeeOverviewService overview) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmployeeOverviewRow>>> Get([FromQuery] DateOnly? asOf) =>
        Ok(await overview.GetAsync(asOf ?? DateOnly.FromDateTime(DateTime.Today)));
}