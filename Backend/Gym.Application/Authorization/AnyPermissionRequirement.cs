using Microsoft.AspNetCore.Authorization;

namespace Gym.Application.Authorization;

public class AnyPermissionRequirement : IAuthorizationRequirement
{
    public AnyPermissionRequirement(params string[] permissionNames)
    {
        PermissionNames = permissionNames;
    }

    public IReadOnlyList<string> PermissionNames { get; }
}
