using Microsoft.AspNetCore.Authorization;

namespace Gym.Application.Authorization;

/// <summary>
/// Declares which privilege name (from the database) is required for this endpoint.
/// Authorization is evaluated dynamically from JWT permission claims loaded at login.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permissionName)
    {
        Policy = permissionName;
    }
}
