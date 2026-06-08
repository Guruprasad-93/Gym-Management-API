using Gym.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.Authorization;

public class DynamicAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public DynamicAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (string.IsNullOrWhiteSpace(policyName))
            return _fallback.GetPolicyAsync(policyName);

        var builder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();

        if (policyName.Contains('|', StringComparison.Ordinal))
        {
            var permissions = policyName.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            builder.AddRequirements(new AnyPermissionRequirement(permissions));
        }
        else
        {
            builder.AddRequirements(new PermissionRequirement(policyName));
        }

        return Task.FromResult<AuthorizationPolicy?>(builder.Build());
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallback.GetFallbackPolicyAsync();
}
