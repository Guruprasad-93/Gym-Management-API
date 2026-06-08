using Microsoft.AspNetCore.Authorization;

namespace Gym.Application.Authorization;

/// <summary>
/// Requires the user to have at least one of the listed permission claims.
/// Policy name is pipe-separated permission names (e.g. VIEW_MEMBERS|VIEW_MEMBER_DETAILS).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireAnyPermissionAttribute : AuthorizeAttribute
{
    public RequireAnyPermissionAttribute(params string[] permissionNames)
    {
        Policy = string.Join('|', permissionNames);
    }
}
