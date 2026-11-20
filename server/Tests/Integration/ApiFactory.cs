using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static readonly Guid MetteId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("holiday")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        // Opret tabeller og testdata med det samme script som docker-compose bruger
        var script = await File.ReadAllTextAsync(FindInitScript());
        await using var connection = new NpgsqlConnection(_database.GetConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(script, connection);
        await command.ExecuteNonQueryAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" gør, at appsettings.Development.json ikke indlæses
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Db", _database.GetConnectionString());
        builder.UseSetting("Jwt:Key", "test-key-der-kun-bruges-i-integrationstestene-1234");
        builder.UseSetting("Jwt:Issuer", "Test");
        builder.UseSetting("Jwt:Audience", "Test");

        // Testene logger ind via TestAuthHandler i stedet for en JWT-cookie
        builder.ConfigureTestServices(services =>
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultForbidScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { }));
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    private static string FindInitScript()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var path = Path.Combine(dir.FullName, "db", "init.sql");
            if (File.Exists(path))
                return path;
        }

        throw new FileNotFoundException("Kunne ikke finde db/init.sql.");
    }
}