using System.Security.Claims;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace Gym.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(AuthClaimTypes.UserId), out var id) ? id : null;

    public Guid? GymId =>
        Guid.TryParse(User?.FindFirstValue(AuthClaimTypes.GymId), out var id) ? id : null;

    public bool HasPermission(string permission) =>
        User?.FindAll(AuthClaimTypes.Permission)
            .Any(c => c.Value.Equals(permission, StringComparison.OrdinalIgnoreCase)) == true;

    public bool HasRole(string role) =>
        User?.FindAll(AuthClaimTypes.Role)
            .Any(c => c.Value.Equals(role, StringComparison.OrdinalIgnoreCase)) == true;

    public IReadOnlyList<string> Permissions =>
        User is null
            ? Array.Empty<string>()
            : User.FindAll(AuthClaimTypes.Permission).Select(c => c.Value).ToList();

    public IReadOnlyList<string> Roles =>
        User is null
            ? Array.Empty<string>()
            : User.FindAll(AuthClaimTypes.Role).Select(c => c.Value).ToList();

    public Guid RequireGymId() =>
        GymId ?? throw new UnauthorizedAccessException("Gym context is required for this operation.");
}
