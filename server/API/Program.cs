using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


var connectionString = config.GetConnectionString("Default")
                       ?? throw new InvalidOperationException("ConnectionStrings:Default is missing");
var jwtKey = config["Jwt:Key"];
if (jwtKey is null || jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters");
var issuer = config["Jwt:Issuer"]!;
var audience = config["Jwt:Audience"]!;
var cookieSecure = config.GetValue("Jwt:CookieSecure", true);

builder.Services.AddScoped(_ => new AppDb(new DataOption().UsePostgreSQL(connectionString)));

builder.Services.AddScoped<>();
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApiDocument();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<MyExceptionHandler>();

var app = builder.Build();



app.Run();
