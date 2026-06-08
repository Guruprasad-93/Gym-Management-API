using Gym.Application.Authorization;
using Gym.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Gym.Infrastructure.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissions = context.User
            .FindAll(AuthClaimTypes.Permission)
            .Select(c => c.Value);

        if (permissions.Contains(requirement.PermissionName, StringComparer.OrdinalIgnoreCase))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
