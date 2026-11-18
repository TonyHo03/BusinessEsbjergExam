using System.Security.Claims;

namespace Service.Security;

public static class AccountingRoles
{
    public const string Employee = "Employee";
    public const string Accountant = "Accountant";
    public const string Admin = "Admin";

    public static readonly string[] Names = [Accountant, Admin];

    public static readonly string[] AllNames = [Employee, Accountant, Admin];

    public static bool Has(String? role) => role is not null && Names.Contains(role, StringComparer.OrdinalIgnoreCase);

    public static bool Has(ClaimsPrincipal user) => Names.Any(user.IsInRole);

    public static bool IsEmployee(ClaimsPrincipal user) => user.IsInRole(Employee);
}