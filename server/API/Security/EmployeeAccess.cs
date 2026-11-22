using System.Security.Claims;
using Service.Security;

namespace API.Security;

public static class EmployeeAccess
{
    public static bool CanAccess(this ClaimsPrincipal user, Guid employeeId) =>
        AccountingRoles.Has(user) || user.FindFirstValue("sub") == employeeId.ToString();
}