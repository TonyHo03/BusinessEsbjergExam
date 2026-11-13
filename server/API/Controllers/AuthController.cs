using System.Security.Claims;
using Infra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.DTOs;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(UserService userService, JwtTokenService jwt, IConfiguration config) : ControllerBase
{
    const string CookieName = "access_token";
    readonly bool _cookieSecure = config.GetValue("Jwt:CookieSecure", true);

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrEmpty(req.Password))
            return BadRequest(new { error = "Username and password are required" });

        var user = userService.Login(req.Email, req.Password);
        if (user is null) return Unauthorized();

        var (token, expires) = jwt.Create(user);
        HttpContext.Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = _cookieSecure,
            SameSite = SameSiteMode.Strict,
            Expires = expires,
            Path = "/"
        });
        return Ok(user);
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        HttpContext.Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true, Secure = _cookieSecure, SameSite = SameSiteMode.Strict, Path = "/"
        });
        
        return NoContent();
    }
    
    [HttpGet("me")]
    public ActionResult<CurrentLoggedUser> Me()
    {

        var id = User.FindFirst("sub")?.Value;
        var role = User.FindFirst("role")?.Value;

        bool result = Enum.TryParse(role, out Roles newRole);

        if (!result) return BadRequest();
        
        return Ok(new CurrentLoggedUser(new() {Id = Guid.Parse(id!), Role = newRole}));
    }
}