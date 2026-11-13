using Microsoft.AspNetCore.Authorization;
using Service.Security;

namespace API.Security;

public static class AccessPolicies
{
    public const string AccountingRights = nameof(AccountingRights);
    
    public static void Register(AuthorizationOptions options) =>    
        options.AddPolicy(AccountingRights, policy => policy.RequireRole(AccountingRoles.Names));
    
}