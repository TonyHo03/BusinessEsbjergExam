using System.Net;
using System.Net.Http.Json;
using Service;

namespace Tests.Integration;

public class TeamHolidaysEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Shows_mettes_approved_summer_holiday()
    {
        var calendar = await _client.GetFromJsonAsync<TeamCalendarResponse>(
            "/team-holidays?from=2026-07-01&to=2026-07-31", ApiFactory.Json);

        var mette = Assert.Single(calendar!.Members);
        Assert.Equal(ApiFactory.MetteId, mette.EmployeeId);
        Assert.Equal(new TeamAbsence(new DateOnly(2026, 7, 6), new DateOnly(2026, 7, 24)), Assert.Single(mette.Absences));
    }

    [Fact]
    public async Task Counts_absent_and_present_per_day()
    {
        var calendar = await _client.GetFromJsonAsync<TeamCalendarResponse>(
            "/team-holidays?from=2026-07-01&to=2026-07-31", ApiFactory.Json);

        var onHoliday = calendar!.Days.Single(d => d.Date == new DateOnly(2026, 7, 6));
        var atWork = calendar.Days.Single(d => d.Date == new DateOnly(2026, 7, 27));
        var saturday = calendar.Days.Single(d => d.Date == new DateOnly(2026, 7, 4));

        Assert.Equal((1, 0), (onHoliday.Absent, onHoliday.Present));
        Assert.Equal((0, 1), (atWork.Absent, atWork.Present));
        Assert.False(saturday.IsWorkingDay);
    }

    [Fact]
    public async Task End_before_start_gives_400()
    {
        var response = await _client.GetAsync("/team-holidays?from=2026-07-31&to=2026-07-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task More_than_a_year_gives_400()
    {
        var response = await _client.GetAsync("/team-holidays?from=2026-01-01&to=2027-06-30");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}