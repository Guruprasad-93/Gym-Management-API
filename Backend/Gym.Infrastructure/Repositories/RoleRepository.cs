using Dapper;
using Gym.Application.Interfaces;
using Gym.Domain.Entities;
using Gym.Infrastructure.Persistence;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public RoleRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<RoleRow>(
            StoredProcedureNames.GetRoleById,
            new { RoleId = id },
            cancellationToken);

        return row is null ? null : EntityMapper.ToRole(row);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<RoleRow>(
            StoredProcedureNames.GetAllRoles,
            cancellationToken: cancellationToken);

        return rows.Select(EntityMapper.ToRole).ToList();
    }

    public async Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@RoleName", role.RoleName);
        parameters.Add("@Description", role.Description);
        parameters.Add("@IsSystemRole", role.IsSystemRole);
        parameters.Add("@RoleId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        var roleId = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CreateRole,
            parameters,
            "@RoleId",
            cancellationToken);

        return Role.Hydrate(
            roleId,
            role.RoleName,
            role.Description,
            role.IsSystemRole,
            role.CreatedDate,
            role.CreatedAt,
            role.UpdatedAt);
    }

    public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(
            StoredProcedureNames.UpdateRole,
            new
            {
                RoleId = role.Id,
                role.RoleName,
                role.Description
            },
            cancellationToken);
    }

    public async Task RemoveAsync(int id, CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(
            StoredProcedureNames.DeleteRole,
            new { RoleId = id },
            cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<RoleRow>(
            StoredProcedureNames.GetRoleByName,
            new { RoleName = roleName.Trim() },
            cancellationToken);

        return row is null ? null : EntityMapper.ToRole(row);
    }

    public async Task<bool> IsAssignedToUsersAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var assigned = await _sp.QuerySingleOrDefaultAsync<bool>(
            StoredProcedureNames.RoleIsAssignedToUsers,
            new { RoleId = roleId },
            cancellationToken);

        return assigned;
    }

    public async Task<bool> AnyExistsAsync(CancellationToken cancellationToken = default)
    {
        var exists = await _sp.QuerySingleOrDefaultAsync<bool>(
            StoredProcedureNames.RoleAnyExists,
            cancellationToken: cancellationToken);

        return exists;
    }
}
