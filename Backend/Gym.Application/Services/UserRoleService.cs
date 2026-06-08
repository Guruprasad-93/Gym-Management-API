using Gym.Application.DTOs.Authorization;
using Gym.Application.Interfaces;
using Gym.Domain.Entities;

namespace Gym.Application.Services;

public class UserRoleService : IUserRoleService
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public UserRoleService(
        IUserRoleRepository userRoleRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        _userRoleRepository = userRoleRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task AssignAsync(AssignUserRoleDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        _ = await _roleRepository.GetByIdAsync(dto.RoleId, cancellationToken)
            ?? throw new KeyNotFoundException("Role not found.");

        if (await _userRoleRepository.GetAsync(dto.UserId, dto.RoleId, cancellationToken) is not null)
            throw new InvalidOperationException("Role is already assigned to this user.");

        await _userRoleRepository.AddAsync(UserRole.Create(user.Id, dto.RoleId), cancellationToken);
    }

    public async Task RemoveAsync(Guid userId, int roleId, CancellationToken cancellationToken = default)
    {
        _ = await _userRoleRepository.GetAsync(userId, roleId, cancellationToken)
            ?? throw new KeyNotFoundException("User role assignment not found.");

        await _userRoleRepository.RemoveAsync(userId, roleId, cancellationToken);
    }

    public async Task<IReadOnlyList<UserRoleDto>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _ = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        var assignments = await _userRoleRepository.GetByUserIdAsync(userId, cancellationToken);

        return assignments.Select(ur => new UserRoleDto
        {
            Id = ur.Id,
            UserId = ur.UserId,
            UserName = ur.User.Name,
            UserEmail = ur.User.Email,
            RoleId = ur.RoleId,
            RoleName = ur.Role.RoleName
        }).ToList();
    }
}
