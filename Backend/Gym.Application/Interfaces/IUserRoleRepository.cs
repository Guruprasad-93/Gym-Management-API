using Gym.Domain.Entities;

namespace Gym.Application.Interfaces;

public interface IUserRoleRepository
{
    Task<IReadOnlyList<UserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserRole?> GetAsync(Guid userId, int roleId, CancellationToken cancellationToken = default);
    Task<UserRole> AddAsync(UserRole userRole, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid userId, int roleId, CancellationToken cancellationToken = default);
}
