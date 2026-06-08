using Gym.Application.DTOs.Roles;
using Gym.Application.Interfaces;
using Gym.Domain.Entities;

namespace Gym.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        if (await _roleRepository.GetByNameAsync(dto.RoleName, cancellationToken) is not null)
            throw new InvalidOperationException("A role with this name already exists.");

        var role = Role.Create(dto.RoleName, dto.Description);
        role = await _roleRepository.AddAsync(role, cancellationToken);

        return MapToDto(role);
    }

    public async Task<RoleDto> UpdateAsync(int id, UpdateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Role not found.");

        if (role.IsSystemRole)
            throw new InvalidOperationException("System roles cannot be modified.");

        var existing = await _roleRepository.GetByNameAsync(dto.RoleName, cancellationToken);
        if (existing is not null && existing.Id != id)
            throw new InvalidOperationException("A role with this name already exists.");

        role.Update(dto.RoleName, dto.Description);
        await _roleRepository.UpdateAsync(role, cancellationToken);

        return MapToDto(role);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Role not found.");

        if (role.IsSystemRole)
            throw new InvalidOperationException("System roles cannot be deleted.");

        if (await _roleRepository.IsAssignedToUsersAsync(id, cancellationToken))
            throw new InvalidOperationException("Cannot delete a role assigned to users.");

        await _roleRepository.RemoveAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        return roles.Select(MapToDto).ToList();
    }

    public async Task<RoleDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Role not found.");
        return MapToDto(role);
    }

    private static RoleDto MapToDto(Role role) =>
        new()
        {
            Id = role.Id,
            RoleName = role.RoleName,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            CreatedDate = role.CreatedDate
        };
}
