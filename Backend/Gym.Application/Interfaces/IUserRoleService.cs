using Gym.Application.DTOs.Authorization;

namespace Gym.Application.Interfaces;

public interface IUserRoleService
{
    Task AssignAsync(AssignUserRoleDto dto, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid userId, int roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserRoleDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
