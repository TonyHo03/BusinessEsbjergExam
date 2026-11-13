using Facet;
using Infra;
using Infra.Entities;

namespace Service.DTOs;

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

[Facet(typeof(User), Include = [nameof(User.Id), nameof(User.Role)])]
public partial class CurrentLoggedUser {}