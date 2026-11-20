using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tests.Integration;

/// <summary>
/// Logger testene ind uden JWT. Som standard er man Admin med Mettes id, så testene kan ramme alle endpoints.
/// En test kan skifte bruger med headers:
///   X-Test-Role: Employee / Accountant / Admin
///   X-Test-User: medarbejderens id
///   X-Test-Anonymous: true  (ikke logget ind)
/// </summary>
public class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.ContainsKey("X-Test-Anonymous"))
            return Task.FromResult(AuthenticateResult.NoResult());

        var role = Request.Headers["X-Test-Role"].FirstOrDefault() ?? "Admin";
        var userId = Request.Headers["X-Test-User"].FirstOrDefault() ?? ApiFactory.MetteId.ToString();

        var identity = new ClaimsIdentity(
            [new Claim("sub", userId), new Claim("role", role)],
            SchemeName, nameType: "sub", roleType: "role");

        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}