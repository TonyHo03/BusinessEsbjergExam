using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Service;

namespace Tests.Integration;

public class RoleAccountsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static object NewUser(string role) => new
    {
        name = $"{role} account",
        email = $"{role.ToLower()}{Guid.NewGuid():N}@businessesbjerg.com",
        password = "Password123",
        role,
        startDate = "2025-09-01"
    };

    [Fact]
    public async Task There_is_only_one_accountant_account_and_it_is_not_in_the_overview()
    {
        var admin = factory.CreateClient();

        var first = await admin.PostAsJsonAsync("/api/User", NewUser("Accountant"));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var accountantId = (await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var second = await admin.PostAsJsonAsync("/api/User", NewUser("Accountant"));
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);

        var overview = await admin.GetFromJsonAsync<List<EmployeeOverviewRow>>("/api/employees/overview", ApiFactory.Json);
        Assert.DoesNotContain(overview!, r => r.EmployeeId == accountantId);
    }
}