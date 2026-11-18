using Facet;
using Infra;

namespace Service.DTOs;

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

[Facet(typeof(Employee), Include = [nameof(Employee.Id), nameof(Employee.Role), nameof(Employee.Name), nameof(Employee.Email)])]
public partial class CurrentLoggedUser {}