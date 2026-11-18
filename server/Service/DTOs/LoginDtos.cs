using Facet;
using Infra;

namespace Service.DTOs;

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

[Facet(typeof(Employee), Include = [nameof(Employee.Id), nameof(Employee.Role)])]
public partial class CurrentLoggedUser {}