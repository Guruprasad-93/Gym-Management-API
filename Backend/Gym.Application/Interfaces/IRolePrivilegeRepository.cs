using Gym.Domain.Entities;

namespace Gym.Application.Interfaces;

public interface IRolePrivilegeRepository
{
    Task<IReadOnlyList<RolePrivilege>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<RolePrivilege?> GetAsync(int roleId, int privilegeId, CancellationToken cancellationToken = default);
    Task<RolePrivilege> AddAsync(RolePrivilege rolePrivilege, CancellationToken cancellationToken = default);
    Task RemoveAsync(int roleId, int privilegeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolePrivilege>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
}
