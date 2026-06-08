using Gym.Application.DTOs.Authorization;

namespace Gym.Application.Interfaces;

public interface IRolePrivilegeService
{
    Task AssignAsync(AssignRolePrivilegeDto dto, CancellationToken cancellationToken = default);
    Task RemoveAsync(int roleId, int privilegeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolePrivilegeDto>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<RolePermissionMatrixDto> GetMatrixAsync(CancellationToken cancellationToken = default);
}
