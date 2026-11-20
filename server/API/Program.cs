using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using API;
using API.Security;
using Infra;
using LinqToDB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Service;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

const string CookieName = "access_token";

var connectionString = config.GetConnectionString("Db")
                       ?? throw new InvalidOperationException("ConnectionStrings:Db is missing");
var jwtKey = config["Jwt:Key"];
if (jwtKey is null || jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters");
var issuer = config["Jwt:Issuer"]!;
var audience = config["Jwt:Audience"]!;
builder.Services.AddScoped(_ => new AppDb(new DataOptions().UsePostgreSQL(connectionString)));

builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<RequestService>();
builder.Services.AddScoped<HolidayService>();
builder.Services.AddScoped<TeamHolidayService>();
builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
builder.Services.AddScoped<ApprovalService>();
builder.Services.AddScoped<TransferService>();
builder.Services.AddScoped<EmployeeOverviewService>();
builder.Services.AddScoped<ParameterService>();
builder.Services.AddScoped<WorkScheduleService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton(new JwtTokenService(issuer: issuer, audience: audience, key: jwtKey));
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApiDocument();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<MyExceptionHandler>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            NameClaimType = "sub",
            RoleClaimType = "role" // ellers virker RequireRole ikke, fordi rollen ligger i claimet "role"
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies[CookieName];
                return Task.CompletedTask;
            },
            OnTokenValidated = async ctx =>
            {
                var employeeService = ctx.HttpContext.RequestServices.GetRequiredService<EmployeeService>();

                if (!Guid.TryParse(ctx.Principal?.FindFirst("sub")?.Value, out var id))
                {
                    ctx.Fail("Invalid token");
                    return;
                }

                Employee employee;
                try
                {
                    employee = await employeeService.GetById(id);
                }
                catch (KeyNotFoundException)
                {
                    // Brugeren findes ikke længere (fx efter nulstillet database)
                    ctx.Fail("User no longer exists");
                    return;
                }

                var identity = (ClaimsIdentity) ctx.Principal.Identity!;
                
                foreach (var c in identity.FindAll("role").ToList())
                    identity.RemoveClaim(c);
                
                foreach (var c in identity.FindAll("name").ToList())
                    identity.RemoveClaim(c);
                
                foreach (var c in identity.FindAll("email").ToList())
                    identity.RemoveClaim(c);
                
                identity.AddClaim(new Claim("role", employee.Role.ToString()));
                identity.AddClaim(new Claim("name", employee.Name));
                identity.AddClaim(new Claim("email", employee.Email));
            }
        };
    });

builder.Services.AddAuthorization(AccessPolicies.Register);

var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:3000"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(origins).AllowCredentials().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseStaticFiles();
app.UseOpenApi();
app.UseSwaggerUi();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().Seed();
}

app.MapControllers();

app.Run();

public partial class Program { }