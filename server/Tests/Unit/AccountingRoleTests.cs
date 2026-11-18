using System.Security.Claims;
using Service.Security;

namespace Tests.Unit;

public class AccountingRolesTests
{
    [Theory]
    [InlineData("Accountant")]
    [InlineData("Admin")]
    [InlineData("admin")]
    [InlineData("ACCOUNTANT")]
    public void AccountingAndAdmin_HaveAccountingRights(string role) =>
        Assert.True(AccountingRoles.Has(role));

    [Theory]
    [InlineData("Employee")]
    [InlineData("")]
    [InlineData(null)]
    public void Everyone_Else_DoesNot(string? role) =>
        Assert.False(AccountingRoles.Has(role));
    
    [Theory]
    [InlineData("Accountant", true)]
    [InlineData("Admin", true)]
    [InlineData("Employee", false)]
    public void SignedInUser_HasRightsOnlyThroughTheRoleClaim(string role, bool expected)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "test"));

        Assert.Equal(expected, AccountingRoles.Has(user));
    }
}