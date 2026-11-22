using System.Net;
using System.Net.Http.Json;
using Service;

namespace Tests.Integration;

public class TransfersEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private HttpClient As(string role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        client.DefaultRequestHeaders.Add("X-Test-User", ApiFactory.MetteId.ToString());
        return client;
    }

    [Fact]
    public async Task Transfers_are_turned_off_by_default()
    {
        // Business Esbjerg tillader ikke overførsel i dag, men funktionen kan slås til som parameter
        var input = new TransferInput(ApiFactory.MetteId, DateTime.Today.Year - 1, 1m, "Integrationstest");

        var response = await As("Employee").PostAsJsonAsync("/transfers", input, ApiFactory.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await response.Content.ReadFromJsonAsync<List<string>>(ApiFactory.Json);
        Assert.Contains(errors!, e => e.Contains("turned off"));
    }

    [Fact]
    public async Task Only_accounting_can_see_and_decide_on_transfers()
    {
        var employee = As("Employee");

        Assert.Equal(HttpStatusCode.Forbidden, (await employee.GetAsync("/api/transfers/pending")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await employee.PostAsync($"/api/transfers/{Guid.NewGuid()}/approve", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await As("Accountant").GetAsync("/api/transfers/pending")).StatusCode);
    }
}