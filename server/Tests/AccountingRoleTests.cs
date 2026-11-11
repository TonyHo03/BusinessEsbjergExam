using Service.Security;
using Xunit;

namespace HolidayPlanner.UnitTests.Approvals;

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
}