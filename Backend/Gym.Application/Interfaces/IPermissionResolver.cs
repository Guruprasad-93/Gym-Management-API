namespace Gym.Application.Interfaces;

public interface IPermissionResolver
{
    Task<IReadOnlyList<string>> GetPermissionsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetRolesForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
