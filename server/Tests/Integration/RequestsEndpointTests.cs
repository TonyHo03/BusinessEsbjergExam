using System.Net;
using System.Net.Http.Json;
using Infra;
using Service;

namespace Tests.Integration;

public class RequestsEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Første mandag mindst `days` dage ude. Datoerne regnes fra i dag,
    /// så testene virker, uanset hvornår de køres.
    /// </summary>
    private static DateOnly MondayAfter(int days)
    {
        var date = Today.AddDays(days);
        while (date.DayOfWeek != DayOfWeek.Monday)
            date = date.AddDays(1);
        return date;
    }

    private static RequestInput Week(DateOnly monday, Guid? employeeId = null) =>
        new(employeeId ?? ApiFactory.MetteId, HolidayAccount.Weeks1To5, monday, monday.AddDays(4), "Integrationstest");

    [Fact]
    public async Task Preview_calculates_hours_without_saving()
    {
        var input = Week(MondayAfter(42)); // 6 uger ude: varslet er overholdt

        var response = await _client.PostAsJsonAsync("/requests/preview", input, ApiFactory.Json);
        response.EnsureSuccessStatusCode();
        var preview = (await response.Content.ReadFromJsonAsync<RequestPreview>(ApiFactory.Json))!;

        Assert.True(preview.IsValid);
        Assert.Equal(5, preview.Days.Count);
        Assert.Equal(preview.Days.Sum(d => d.Hours), preview.TotalHours);

        // Intet er gemt
        var requests = await _client.GetFromJsonAsync<List<RequestDto>>($"/employees/{ApiFactory.MetteId}/requests", ApiFactory.Json);
        Assert.DoesNotContain(requests!, r => r.StartDate == input.StartDate);
    }

    [Fact]
    public async Task Preview_reports_too_short_notice()
    {
        var input = Week(MondayAfter(3)); // under 1 måneds varsel

        var response = await _client.PostAsJsonAsync("/requests/preview", input, ApiFactory.Json);
        var preview = (await response.Content.ReadFromJsonAsync<RequestPreview>(ApiFactory.Json))!;

        Assert.False(preview.IsValid);
        Assert.Contains(preview.Issues, i => i.Code == IssueCode.NotEnoughNotice);
    }

    [Fact]
    public async Task Submit_saves_a_valid_request_as_pending()
    {
        var input = Week(MondayAfter(56));

        var response = await _client.PostAsJsonAsync("/requests", input, ApiFactory.Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<RequestDto>(ApiFactory.Json))!;
        Assert.Equal(RequestStatus.Pending, created.Status);

        // Den kan nu ses i medarbejderens liste sammen med sommerferien fra testdataene
        var requests = await _client.GetFromJsonAsync<List<RequestDto>>($"/employees/{ApiFactory.MetteId}/requests", ApiFactory.Json);
        Assert.Contains(requests!, r => r.Id == created.Id && r.Status == RequestStatus.Pending);
        Assert.Contains(requests!, r => r.Comment == "Sommerferie" && r.Status == RequestStatus.Approved);
    }

    [Fact]
    public async Task Submit_rejects_an_invalid_request_with_the_reasons()
    {
        var input = Week(MondayAfter(3));

        var response = await _client.PostAsJsonAsync("/requests", input, ApiFactory.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var preview = (await response.Content.ReadFromJsonAsync<RequestPreview>(ApiFactory.Json))!;
        Assert.Contains(preview.Issues, i => i.Code == IssueCode.NotEnoughNotice);
    }

    [Fact]
    public async Task End_before_start_gives_400()
    {
        var monday = MondayAfter(42);
        var input = new RequestInput(ApiFactory.MetteId, HolidayAccount.Weeks1To5, monday, monday.AddDays(-1), null);

        var response = await _client.PostAsJsonAsync("/requests/preview", input, ApiFactory.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_employee_gives_404()
    {
        var response = await _client.PostAsJsonAsync("/requests/preview", Week(MondayAfter(42), Guid.NewGuid()), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}