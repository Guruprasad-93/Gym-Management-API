using Gym.Application.Authorization;
using Gym.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Gym.Infrastructure.Authorization;

public class AnyPermissionAuthorizationHandler : AuthorizationHandler<AnyPermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyPermissionRequirement requirement)
    {
        var userPermissions = context.User
            .FindAll(AuthClaimTypes.Permission)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (requirement.PermissionNames.Any(p => userPermissions.Contains(p)))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
