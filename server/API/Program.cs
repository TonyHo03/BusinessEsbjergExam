using Infra;
using LinqToDB;
using LinqToDB.AspNet;
using System.Text.Json.Serialization;
using Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));builder.Services.AddOpenApiDocument();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(builder.Configuration["ClientUrl"] ?? "http://localhost:3000")
        .AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddLinqToDBContext<AppDb>((sp, options) =>
    options.UsePostgreSQL(builder.Configuration.GetConnectionString("Db")!));
builder.Services.AddScoped<HolidayService>();

var app = builder.Build();

app.UseCors();
app.UseOpenApi();
app.UseSwaggerUi();
app.MapControllers();

app.Run();

public partial class Program { } // så integrationstests kan starte API'et