using Gym.Application.DTOs.Privileges;
using Gym.Application.Interfaces;
using Gym.Domain.Entities;

namespace Gym.Application.Services;

public class PrivilegeService : IPrivilegeService
{
    private readonly IPrivilegeRepository _privilegeRepository;

    public PrivilegeService(IPrivilegeRepository privilegeRepository)
    {
        _privilegeRepository = privilegeRepository;
    }

    public async Task<PrivilegeDto> CreateAsync(CreatePrivilegeDto dto, CancellationToken cancellationToken = default)
    {
        if (await _privilegeRepository.GetByNameAsync(dto.PrivilegeName, cancellationToken) is not null)
            throw new InvalidOperationException("A privilege with this name already exists.");

        var privilege = Privilege.Create(dto.PrivilegeName, dto.Description, dto.Category);
        privilege = await _privilegeRepository.AddAsync(privilege, cancellationToken);

        return MapToDto(privilege);
    }

    public async Task<PrivilegeDto> UpdateAsync(int id, UpdatePrivilegeDto dto, CancellationToken cancellationToken = default)
    {
        var privilege = await _privilegeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Privilege not found.");

        var existing = await _privilegeRepository.GetByNameAsync(dto.PrivilegeName, cancellationToken);
        if (existing is not null && existing.Id != id)
            throw new InvalidOperationException("A privilege with this name already exists.");

        privilege.Update(dto.PrivilegeName, dto.Description, dto.Category);
        await _privilegeRepository.UpdateAsync(privilege, cancellationToken);

        return MapToDto(privilege);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _ = await _privilegeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Privilege not found.");

        if (await _privilegeRepository.IsAssignedToRolesAsync(id, cancellationToken))
            throw new InvalidOperationException("Cannot delete a privilege assigned to roles. Remove assignments first.");

        await _privilegeRepository.RemoveAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<PrivilegeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var privileges = await _privilegeRepository.GetAllAsync(cancellationToken);
        return privileges.Select(MapToDto).ToList();
    }

    public async Task<PrivilegeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var privilege = await _privilegeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Privilege not found.");
        return MapToDto(privilege);
    }

    private static PrivilegeDto MapToDto(Privilege privilege) =>
        new()
        {
            Id = privilege.Id,
            PrivilegeName = privilege.PrivilegeName,
            Description = privilege.Description,
            Category = privilege.Category,
            CreatedDate = privilege.CreatedDate
        };
}
