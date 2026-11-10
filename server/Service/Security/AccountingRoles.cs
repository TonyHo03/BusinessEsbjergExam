namespace Service.Security;

public static class AccountingRoles
{
    public const string Accountant = "Accountant";
    public const string Admin = "Admin";

    public static readonly string[] Names = [Accountant, Admin];
    
    public static bool Has(string? role) => role is not null && Names.Contains(role, StringComparer.OrdinalIgnoreCase);
}