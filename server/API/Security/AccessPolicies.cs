using Microsoft.AspNetCore.Authorization;
using Service.Security;

namespace API.Security;

[Authorize]
public static class AccessPolicies
{
    public const string AccountingRights = nameof(AccountingRights);
    public const string AdminRights = nameof(AdminRights);

    public static void Register(AuthorizationOptions options)
    {
        options.AddPolicy(AccountingRights, policy => policy.RequireRole(AccountingRoles.Names));
        options.AddPolicy(AdminRights, policy => policy.RequireRole(AccountingRoles.Admin));
    }
}