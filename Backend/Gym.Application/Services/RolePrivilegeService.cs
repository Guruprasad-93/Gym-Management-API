using Gym.Application.DTOs.Authorization;
using Gym.Application.Interfaces;
using Gym.Domain.Entities;

namespace Gym.Application.Services;

public class RolePrivilegeService : IRolePrivilegeService
{
    private readonly IRolePrivilegeRepository _rolePrivilegeRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPrivilegeRepository _privilegeRepository;

    public RolePrivilegeService(
        IRolePrivilegeRepository rolePrivilegeRepository,
        IRoleRepository roleRepository,
        IPrivilegeRepository privilegeRepository)
    {
        _rolePrivilegeRepository = rolePrivilegeRepository;
        _roleRepository = roleRepository;
        _privilegeRepository = privilegeRepository;
    }

    public async Task AssignAsync(AssignRolePrivilegeDto dto, CancellationToken cancellationToken = default)
    {
        _ = await _roleRepository.GetByIdAsync(dto.RoleId, cancellationToken)
            ?? throw new KeyNotFoundException("Role not found.");

        _ = await _privilegeRepository.GetByIdAsync(dto.PrivilegeId, cancellationToken)
            ?? throw new KeyNotFoundException("Privilege not found.");

        if (await _rolePrivilegeRepository.GetAsync(dto.RoleId, dto.PrivilegeId, cancellationToken) is not null)
            throw new InvalidOperationException("Privilege is already assigned to this role.");

        await _rolePrivilegeRepository.AddAsync(
            RolePrivilege.Create(dto.RoleId, dto.PrivilegeId),
            cancellationToken);
    }

    public async Task RemoveAsync(int roleId, int privilegeId, CancellationToken cancellationToken = default)
    {
        _ = await _rolePrivilegeRepository.GetAsync(roleId, privilegeId, cancellationToken)
            ?? throw new KeyNotFoundException("Role privilege assignment not found.");

        await _rolePrivilegeRepository.RemoveAsync(roleId, privilegeId, cancellationToken);
    }

    public async Task<IReadOnlyList<RolePrivilegeDto>> GetByRoleIdAsync(
        int roleId,
        CancellationToken cancellationToken = default)
    {
        _ = await _roleRepository.GetByIdAsync(roleId, cancellationToken)
            ?? throw new KeyNotFoundException("Role not found.");

        var assignments = await _rolePrivilegeRepository.GetByRoleIdAsync(roleId, cancellationToken);

        return assignments.Select(rp => new RolePrivilegeDto
        {
            Id = rp.Id,
            RoleId = rp.RoleId,
            RoleName = rp.Role.RoleName,
            PrivilegeId = rp.PrivilegeId,
            PrivilegeName = rp.Privilege.PrivilegeName,
            Category = rp.Privilege.Category
        }).ToList();
    }

    public async Task<RolePermissionMatrixDto> GetMatrixAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        var privileges = await _privilegeRepository.GetAllAsync(cancellationToken);
        var assignments = await _rolePrivilegeRepository.GetAllWithDetailsAsync(cancellationToken);

        var assignmentSet = assignments
            .Select(a => (a.RoleId, a.PrivilegeId))
            .ToHashSet();

        return new RolePermissionMatrixDto
        {
            Roles = roles
                .OrderBy(r => r.RoleName)
                .Select(r => new RoleMatrixColumnDto { RoleId = r.Id, RoleName = r.RoleName })
                .ToList(),
            Privileges = privileges
                .OrderBy(p => p.Category)
                .ThenBy(p => p.PrivilegeName)
                .Select(p => new PrivilegeMatrixRowDto
                {
                    PrivilegeId = p.Id,
                    PrivilegeName = p.PrivilegeName,
                    Category = p.Category
                })
                .ToList(),
            Assignments = roles
                .SelectMany(role => privileges.Select(privilege => new MatrixAssignmentDto
                {
                    RoleId = role.Id,
                    PrivilegeId = privilege.Id,
                    Assigned = assignmentSet.Contains((role.Id, privilege.Id))
                }))
                .ToList()
        };
    }
}
