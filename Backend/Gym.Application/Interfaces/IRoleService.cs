using Gym.Application.DTOs.Roles;

namespace Gym.Application.Interfaces;

public interface IRoleService
{
    Task<RoleDto> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default);
    Task<RoleDto> UpdateAsync(int id, UpdateRoleDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
