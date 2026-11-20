using System.Net;
using System.Net.Http.Json;
using Service;

namespace Tests.Integration;

public class WorkSchedulesEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly string Url = $"/api/employees/{ApiFactory.MetteId}/schedules";

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    private static readonly DayTimes PartTimeDay = new("09:00", "15:00"); // 6 t

    private static WorkScheduleInput PartTimeFrom(DateOnly from) =>
        new(from, PartTimeDay, PartTimeDay, PartTimeDay, PartTimeDay, PartTimeDay);

    private HttpClient As(string role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        client.DefaultRequestHeaders.Add("X-Test-User", ApiFactory.MetteId.ToString());
        return client;
    }

    [Fact]
    public async Task A_new_schedule_closes_the_current_one_and_deleting_it_reopens_it()
    {
        var admin = As("Admin");
        var from = Today.AddDays(14);

        var created = await admin.PostAsJsonAsync(Url, PartTimeFrom(from), ApiFactory.Json);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var newSchedule = (await created.Content.ReadFromJsonAsync<WorkScheduleDto>(ApiFactory.Json))!;
        Assert.Equal(30m, newSchedule.WeeklyHours);

        var history = (await admin.GetFromJsonAsync<WorkSchedulesResponse>(Url, ApiFactory.Json))!.Schedules;
        Assert.Equal(newSchedule.Id, history[0].Id);           // nyeste først
        Assert.Equal(from.AddDays(-1), history[1].EffectiveTo); // det gamle slutter dagen før

        var deleted = await admin.DeleteAsync($"{Url}/{newSchedule.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var after = (await admin.GetFromJsonAsync<WorkSchedulesResponse>(Url, ApiFactory.Json))!.Schedules;
        var reopened = Assert.Single(after);
        Assert.Null(reopened.EffectiveTo);
    }

    [Fact]
    public async Task An_employee_can_see_but_not_change_their_schedule()
    {
        var employee = As("Employee");

        var get = await employee.GetAsync(Url);
        var post = await employee.PostAsJsonAsync(Url, PartTimeFrom(Today.AddDays(30)), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, post.StatusCode);
    }
}