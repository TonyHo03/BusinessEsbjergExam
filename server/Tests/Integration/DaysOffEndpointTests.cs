using System.Net;
using System.Net.Http.Json;
using Service;

namespace Tests.Integration;

public class DaysOffEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private HttpClient As(string role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        client.DefaultRequestHeaders.Add("X-Test-User", ApiFactory.MetteId.ToString());
        return client;
    }

    // En hverdag langt ude i fremtiden, hvor init.sql ikke har nogen fridage
    private static DateOnly FutureWeekday()
    {
        var date = new DateOnly(DateTime.Today.Year + 3, 3, 2);
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            date = date.AddDays(1);
        return date;
    }

    [Fact]
    public async Task Admin_adds_a_company_day_off_and_everyone_can_see_it()
    {
        var date = FutureWeekday();

        var created = await As("Admin").PostAsJsonAsync("/api/days-off", new DayOffInput(date, "Firmadag", true), ApiFactory.Json);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var days = await As("Employee").GetFromJsonAsync<List<DayOffDto>>($"/api/days-off?year={date.Year}", ApiFactory.Json);
        Assert.Contains(days!, d => d.Date == date && d.Name == "Firmadag" && d.IsCompany);
    }

    [Fact]
    public async Task Only_admin_can_add_days_off()
    {
        var response = await As("Accountant").PostAsJsonAsync("/api/days-off", new DayOffInput(FutureWeekday().AddDays(7), "X", false), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task A_day_off_in_the_past_cannot_be_deleted()
    {
        var days = await As("Admin").GetFromJsonAsync<List<DayOffDto>>("/api/days-off?year=2026", ApiFactory.Json);
        var newYear = days!.Single(d => d.Date == new DateOnly(2026, 1, 1));

        var response = await As("Admin").DeleteAsync($"/api/days-off/{newYear.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}