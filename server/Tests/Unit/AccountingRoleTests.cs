using System.Security.Claims;
using Service.Security;

namespace Tests.Unit;

public class AccountingRolesTests
{
    [Theory]
    [InlineData("Accountant", true)]
    [InlineData("Admin", true)]
    [InlineData("Employee", false)]
    public void Only_accountant_and_admin_have_accounting_rights(string role, bool expected) =>
        Assert.Equal(expected, AccountingRoles.Has(role));

    [Fact]
    public void SignedInUser_HasRightsThroughTheRoleClaim()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Accountant")], "test"));

        Assert.True(AccountingRoles.Has(user));
    }
}