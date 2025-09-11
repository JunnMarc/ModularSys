using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ModularSys.Core.Interfaces;
using ModularSys.Data.Common.Db;
using Microsoft.EntityFrameworkCore;

public class DynamicAuthPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;

    public DynamicAuthPolicyProvider(IOptions<AuthorizationOptions> options,
                                     IDbContextFactory<ModularSysDbContext> contextFactory)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        _contextFactory = contextFactory;
    }

    public Task<AuthorizationPolicy?> GetDefaultPolicyAsync()
        => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Create a fresh DbContext for this policy check
        await using var db = _contextFactory.CreateDbContext();

        // Check if the permission exists in the DB
        var exists = await db.Permissions
            .AnyAsync(p => p.PermissionName == policyName);

        if (!exists)
            return null;

        // Build a policy that requires the Permission claim
        var policy = new AuthorizationPolicyBuilder()
            .RequireClaim("Permission", policyName)
            .Build();

        return policy;
    }
}
