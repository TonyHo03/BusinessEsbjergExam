using System.Net;
using System.Net.Http.Json;
using Service;

namespace Tests.Integration;

public class BalancesEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<BalancesResponse> GetBalances(string asOf)
    {
        var response = await _client.GetAsync($"/employees/{ApiFactory.MetteId}/balances?asOf={asOf}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<BalancesResponse>(ApiFactory.Json))!;
    }

    [Fact]
    public async Task Returns_mettes_balances_from_the_database()
    {
        var balances = await GetBalances("2026-11-17");

        Assert.Equal(104.49m, balances.AvailableWeeks1To5);
        Assert.Equal(12.33m, balances.AvailableWeek6);
        Assert.Equal(4, balances.Periods.Count);
    }

    [Fact]
    public async Task Sommerferien_is_subtracted_from_the_2025_period()
    {
        var balances = await GetBalances("2026-11-17");

        var period2025 = balances.Periods.Single(p => p.Account == Infra.HolidayAccount.Weeks1To5 && p.EarningYear == 2025);
        Assert.Equal(111m, period2025.Used);
        Assert.Equal(73.70m, period2025.Remaining);
        Assert.True(period2025.NeedsAction); // udløber 31.12.2026
    }

    [Fact]
    public async Task A_future_date_includes_the_hours_earned_until_then()
    {
        var balances = await GetBalances("2026-12-31");

        Assert.Equal(new DateOnly(2026, 12, 31), balances.AsOf);
        Assert.Equal(135.27m, balances.AvailableWeeks1To5);
    }

    [Fact]
    public async Task Unknown_employee_gives_404()
    {
        var response = await _client.GetAsync($"/employees/{Guid.NewGuid()}/balances");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}