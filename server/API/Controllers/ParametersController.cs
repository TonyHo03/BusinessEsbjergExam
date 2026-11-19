using API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers;

[ApiController]
[Route("api/parameters")]
[Authorize(Policy = AccessPolicies.AdminRights)]
public class ParametersController(ParameterService parameters) : ControllerBase
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    /// <summary>Alle regelparametre med den værdi, der gælder på asOf, og hele historikken.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ParameterDto>>> Get([FromQuery] DateOnly? asOf) =>
        Ok(await parameters.GetAsync(asOf ?? Today));

    /// <summary>Tilføjer en ny værdi med en startdato. Den gamle værdi gælder stadig for datoer før.</summary>
    [HttpPost]
    [ProducesResponseType<ParameterVersionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ParameterVersionDto>> Add(ParameterInput input)
    {
        var result = await parameters.AddAsync(input, Today);

        return result.Created is null
            ? BadRequest(result.Errors)
            : Created($"/api/parameters/{result.Created.Id}", result.Created);
    }

    /// <summary>Sletter en værdi, der ikke er trådt i kraft endnu (fx en tastefejl).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var error = await parameters.DeleteAsync(id, Today);
        return error is null ? NoContent() : Problem(error, statusCode: StatusCodes.Status409Conflict);
    }
}