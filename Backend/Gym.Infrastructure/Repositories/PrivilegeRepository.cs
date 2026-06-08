using Dapper;
using Gym.Application.Interfaces;
using Gym.Domain.Entities;
using Gym.Infrastructure.Persistence;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class PrivilegeRepository : IPrivilegeRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public PrivilegeRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<Privilege?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<PrivilegeRow>(
            StoredProcedureNames.GetPrivilegeById,
            new { PrivilegeId = id },
            cancellationToken);

        return row is null ? null : EntityMapper.ToPrivilege(row);
    }

    public async Task<IReadOnlyList<Privilege>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<PrivilegeRow>(
            StoredProcedureNames.GetAllPrivileges,
            cancellationToken: cancellationToken);

        return rows.Select(EntityMapper.ToPrivilege).ToList();
    }

    public async Task<Privilege> AddAsync(Privilege privilege, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@PrivilegeName", privilege.PrivilegeName);
        parameters.Add("@Description", privilege.Description);
        parameters.Add("@Category", privilege.Category);
        parameters.Add("@PrivilegeId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        var privilegeId = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CreatePrivilege,
            parameters,
            "@PrivilegeId",
            cancellationToken);

        return Privilege.Hydrate(
            privilegeId,
            privilege.PrivilegeName,
            privilege.Description,
            privilege.Category,
            privilege.CreatedDate,
            privilege.CreatedAt,
            privilege.UpdatedAt);
    }

    public async Task UpdateAsync(Privilege privilege, CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(
            StoredProcedureNames.UpdatePrivilege,
            new
            {
                PrivilegeId = privilege.Id,
                privilege.PrivilegeName,
                privilege.Description,
                privilege.Category
            },
            cancellationToken);
    }

    public async Task RemoveAsync(int id, CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(
            StoredProcedureNames.DeletePrivilege,
            new { PrivilegeId = id },
            cancellationToken);
    }

    public async Task<Privilege?> GetByNameAsync(string privilegeName, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<PrivilegeRow>(
            StoredProcedureNames.GetPrivilegeByName,
            new { PrivilegeName = privilegeName.Trim() },
            cancellationToken);

        return row is null ? null : EntityMapper.ToPrivilege(row);
    }

    public async Task<bool> IsAssignedToRolesAsync(int privilegeId, CancellationToken cancellationToken = default)
    {
        var assigned = await _sp.QuerySingleOrDefaultAsync<bool>(
            StoredProcedureNames.PrivilegeIsAssignedToRoles,
            new { PrivilegeId = privilegeId },
            cancellationToken);

        return assigned;
    }
}
