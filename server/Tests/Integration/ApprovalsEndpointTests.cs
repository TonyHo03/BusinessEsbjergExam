using System.Net;
using System.Net.Http.Json;
using Infra;
using Service;
using Service.DTOs;

namespace Tests.Integration;

public class ApprovalsEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly string Mette = ApiFactory.MetteId.ToString();
    // approval_audit.actor_id skal pege på en rigtig medarbejder, så "bogholderen" er Mette med Accountant-rollen
    private static readonly string Accountant = ApiFactory.MetteId.ToString();

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    private static DateOnly MondayAfter(int days)
    {
        var date = Today.AddDays(days);
        while (date.DayOfWeek != DayOfWeek.Monday)
            date = date.AddDays(1);
        return date;
    }

    private HttpClient As(string role, string user)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        client.DefaultRequestHeaders.Add("X-Test-User", user);
        return client;
    }

    private async Task<Guid> SubmitDay(DateOnly monday)
    {
        var input = new RequestInput(ApiFactory.MetteId, HolidayAccount.Weeks1To5, monday, monday, "Integrationstest");
        var response = await As("Employee", Mette).PostAsJsonAsync("/api/requests", input, ApiFactory.Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<RequestDto>(ApiFactory.Json))!.Id;
    }

    private async Task<decimal> AvailableOn(DateOnly date)
    {
        var balances = await As("Employee", Mette).GetFromJsonAsync<BalancesResponse>(
            $"/api/employees/{Mette}/balances?asOf={date:yyyy-MM-dd}", ApiFactory.Json);
        return balances!.AvailableWeeks1To5;
    }

    private async Task<RequestDto> Request(Guid id)
    {
        var requests = await As("Employee", Mette).GetFromJsonAsync<List<RequestDto>>(
            $"/employees/{Mette}/requests", ApiFactory.Json);
        return requests!.Single(r => r.Id == id);
    }

    [Fact]
    public async Task Approved_holiday_is_subtracted_and_an_approved_cancellation_gives_it_back()
    {
        var monday = MondayAfter(42);
        var afterHoliday = monday.AddDays(7);
        var before = await AvailableOn(afterHoliday);

        var id = await SubmitDay(monday);
        var approve = await As("Accountant", Accountant).PostAsync($"/api/approvals/{id}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approve.StatusCode);
        Assert.Equal(7.5m, before - await AvailableOn(afterHoliday)); // en mandag

        var ask = await As("Employee", Mette).PostAsync($"/api/approvals/{id}/request-cancellation", null);
        Assert.Equal(HttpStatusCode.OK, ask.StatusCode);
        Assert.Equal(RequestStatus.CancellationRequested, (await Request(id)).Status);

        var cancel = await As("Accountant", Accountant).PostAsync($"/api/approvals/{id}/approve", null);
        Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);
        Assert.Equal(RequestStatus.Cancelled, (await Request(id)).Status);
        Assert.Equal(before, await AvailableOn(afterHoliday));
    }

    [Fact]
    public async Task A_rejected_cancellation_keeps_the_holiday_and_shows_the_reason()
    {
        var monday = MondayAfter(70);
        var id = await SubmitDay(monday);
        await As("Accountant", Accountant).PostAsync($"/api/approvals/{id}/approve", null);
        var afterApproval = await AvailableOn(monday.AddDays(7));

        await As("Employee", Mette).PostAsync($"/api/approvals/{id}/request-cancellation", null);
        var reject = await As("Accountant", Accountant).PostAsJsonAsync(
            $"/api/approvals/{id}/reject", new RejectRequestDto("We have planned around it."), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.OK, reject.StatusCode);
        var request = await Request(id);
        Assert.Equal(RequestStatus.Approved, request.Status);
        Assert.Equal("We have planned around it.", request.RejectionReason);
        Assert.Equal(afterApproval, await AvailableOn(monday.AddDays(7))); // ferien er ikke trukket fra to gange
    }

    [Fact]
    public async Task An_employee_cannot_approve_requests()
    {
        var id = await SubmitDay(MondayAfter(98));

        var response = await As("Employee", Mette).PostAsync($"/api/approvals/{id}/approve", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(RequestStatus.Pending, (await Request(id)).Status);
    }
}