using System.Text;
using System.Text.Json.Serialization;
using API;
using Infra;
using LinqToDB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using Service;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

const string CookieName = "access_token";
 
var connectionString = config.GetConnectionString("Default")
                       ?? throw new InvalidOperationException("ConnectionStrings:Default is missing");
var jwtKey = config["Jwt:Key"];
if (jwtKey is null || jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters");
var issuer = config["Jwt:Issuer"]!;
var audience = config["Jwt:Audience"]!;
builder.Services.AddScoped(_ => new AppDb(new DataOptions().UsePostgreSQL(connectionString)));

builder.Services.AddScoped<UserService>();
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
            RoleClaimType = "role"
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies[CookieName];
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();


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
