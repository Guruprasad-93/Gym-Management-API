using Dapper;
using Gym.Application.Interfaces;
using Gym.Domain.Entities;
using Gym.Infrastructure.Persistence;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class RolePrivilegeRepository : IRolePrivilegeRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public RolePrivilegeRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<IReadOnlyList<RolePrivilege>> GetByRoleIdAsync(
        int roleId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<RolePrivilegeRow>(
            StoredProcedureNames.GetPrivilegesByRoleId,
            new { RoleId = roleId },
            cancellationToken);

        return rows.Select(EntityMapper.ToRolePrivilegeWithDetails).ToList();
    }

    public async Task<RolePrivilege?> GetAsync(
        int roleId,
        int privilegeId,
        CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<RolePrivilegeRow>(
            StoredProcedureNames.GetRolePrivilege,
            new { RoleId = roleId, PrivilegeId = privilegeId },
            cancellationToken);

        return row is null ? null : EntityMapper.ToRolePrivilege(row);
    }

    public async Task<RolePrivilege> AddAsync(
        RolePrivilege rolePrivilege,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@RoleId", rolePrivilege.RoleId);
        parameters.Add("@PrivilegeId", rolePrivilege.PrivilegeId);
        parameters.Add("@RolePrivilegeId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        var id = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.AssignPrivilegeToRole,
            parameters,
            "@RolePrivilegeId",
            cancellationToken);

        return RolePrivilege.Hydrate(id, rolePrivilege.RoleId, rolePrivilege.PrivilegeId, DateTime.UtcNow, null);
    }

    public async Task RemoveAsync(int roleId, int privilegeId, CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(
            StoredProcedureNames.RemovePrivilegeFromRole,
            new { RoleId = roleId, PrivilegeId = privilegeId },
            cancellationToken);
    }

    public async Task<IReadOnlyList<RolePrivilege>> GetAllWithDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<RolePrivilegeRow>(
            StoredProcedureNames.GetAllRolePrivileges,
            cancellationToken: cancellationToken);

        return rows.Select(EntityMapper.ToRolePrivilege).ToList();
    }
}
