using Gym.Application.Interfaces;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Security;

public class PermissionResolver : IPermissionResolver
{
    private readonly IStoredProcedureExecutor _sp;

    public PermissionResolver(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<IReadOnlyList<string>> GetPermissionsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var permissions = await _sp.QueryAsync<string>(
            StoredProcedureNames.GetUserPermissions,
            new { UserId = userId },
            cancellationToken);

        return permissions;
    }

    public async Task<IReadOnlyList<string>> GetRolesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var roles = await _sp.QueryAsync<string>(
            StoredProcedureNames.GetUserRoles,
            new { UserId = userId },
            cancellationToken);

        return roles;
    }
}
