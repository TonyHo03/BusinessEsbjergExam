using System.Net;

namespace Tests.Integration;

/// <summary>
/// Adgang: man skal være logget ind, medarbejdere må kun se deres egne data,
/// bogholderiet må se alles, og kun admin må administrere brugere.
/// </summary>
public class AuthorizationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly string OtherEmployee = Guid.NewGuid().ToString();

    private async Task<HttpStatusCode> Get(string url, string? role = null, string? user = null, bool anonymous = false)
    {
        var client = factory.CreateClient();
        if (anonymous) client.DefaultRequestHeaders.Add("X-Test-Anonymous", "true");
        if (role is not null) client.DefaultRequestHeaders.Add("X-Test-Role", role);
        if (user is not null) client.DefaultRequestHeaders.Add("X-Test-User", user);

        return (await client.GetAsync(url)).StatusCode;
    }

    private static string Balances => $"/api/employees/{ApiFactory.MetteId}/balances";
    private static string Requests => $"/employees/{ApiFactory.MetteId}/requests";

    [Fact]
    public async Task Not_logged_in_gives_401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, await Get(Balances, anonymous: true));
        Assert.Equal(HttpStatusCode.Unauthorized, await Get("/team-holidays?from=2026-12-01&to=2026-12-31", anonymous: true));
    }

    [Fact]
    public async Task An_employee_can_see_their_own_balance_and_requests()
    {
        var mette = ApiFactory.MetteId.ToString();

        Assert.Equal(HttpStatusCode.OK, await Get(Balances, "Employee", mette));
        Assert.Equal(HttpStatusCode.OK, await Get(Requests, "Employee", mette));
    }

    [Fact]
    public async Task An_employee_cannot_see_a_colleagues_balance_or_requests()
    {
        Assert.Equal(HttpStatusCode.Forbidden, await Get(Balances, "Employee", OtherEmployee));
        Assert.Equal(HttpStatusCode.Forbidden, await Get(Requests, "Employee", OtherEmployee));
    }

    [Fact]
    public async Task Accounting_can_see_everyones_balance()
    {
        Assert.Equal(HttpStatusCode.OK, await Get(Balances, "Accountant", OtherEmployee));
    }

    [Fact]
    public async Task Only_admin_can_manage_users()
    {
        Assert.Equal(HttpStatusCode.Forbidden, await Get("/api/User/users", "Employee"));
        Assert.Equal(HttpStatusCode.Forbidden, await Get("/api/User/users", "Accountant"));
        Assert.Equal(HttpStatusCode.OK, await Get("/api/User/users", "Admin"));
    }

    [Fact]
    public async Task Only_accounting_can_see_pending_approvals_and_the_employee_overview()
    {
        Assert.Equal(HttpStatusCode.Forbidden, await Get("/api/approvals/pending", "Employee"));
        Assert.Equal(HttpStatusCode.Forbidden, await Get("/api/employees/overview", "Employee"));
        Assert.Equal(HttpStatusCode.OK, await Get("/api/employees/overview", "Accountant"));
    }
}