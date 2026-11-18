using Infra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.DTOs;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(EmployeeService service)
{
    
    [HttpGet("users")]
    public async Task<List<EmployeeDto>> GetAllUsers() => await service.GetAll();
    
    [HttpGet("users/{email}")]
    public async Task<EmployeeDto> GetUserByEmail(string email) => new (await service.GetByEmail(email));

    [Authorize]
    [HttpPost]
    public async Task CreateUser([FromBody] CreateEmployeeRequest request) => await service.Create(request);

    [Authorize]
    [HttpPut("{id}")]
    public async Task UpdateUser(Guid id, [FromBody] UpdateEmployeeRequest request) =>
        await service.Update(id, request);
    
    [Authorize]
    [HttpPost("activate/{id}")]
    public async Task ActivateUser(Guid id) => await service.SetActive(id, Status.Active);

    [Authorize]
    [HttpPost("deactivate/{id}")]
    public async Task DeactivateUser(Guid id) => await service.SetActive(id, Status.Inactive);

}