using System.Security.Claims;
using Service.Security;

namespace API.Security;

/// <summary>
/// Hvem må se og ændre en medarbejders ferie: medarbejderen selv og bogholderiet (Accountant og Admin).
/// </summary>
public static class EmployeeAccess
{
    public static bool CanAccess(this ClaimsPrincipal user, Guid employeeId) =>
        AccountingRoles.Has(user) || user.FindFirstValue("sub") == employeeId.ToString();
}