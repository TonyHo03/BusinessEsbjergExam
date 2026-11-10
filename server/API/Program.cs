var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApiDocument();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(builder.Configuration["ClientUrl"] ?? "http://localhost:3000")
        .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.UseOpenApi();
app.UseSwaggerUi();
app.MapControllers();

app.Run();

public partial class Program { } // så integrationstests kan starte API'et