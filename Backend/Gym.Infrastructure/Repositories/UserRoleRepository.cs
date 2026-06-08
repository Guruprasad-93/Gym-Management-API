using Dapper;
using Gym.Application.Interfaces;
using Gym.Domain.Entities;
using Gym.Infrastructure.Persistence;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public UserRoleRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<IReadOnlyList<UserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<UserRoleRow>(
            StoredProcedureNames.GetUserRolesByUserId,
            new { UserId = userId },
            cancellationToken);

        return rows.Select(EntityMapper.ToUserRole).ToList();
    }

    public async Task<UserRole?> GetAsync(Guid userId, int roleId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<UserRoleRow>(
            StoredProcedureNames.GetUserRole,
            new { UserId = userId, RoleId = roleId },
            cancellationToken);

        return row is null ? null : EntityMapper.ToUserRole(row);
    }

    public async Task<UserRole> AddAsync(UserRole userRole, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userRole.UserId);
        parameters.Add("@RoleId", userRole.RoleId);
        parameters.Add("@UserRoleId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        var id = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.AssignUserRole,
            parameters,
            "@UserRoleId",
            cancellationToken);

        return UserRole.Hydrate(id, userRole.UserId, userRole.RoleId, DateTime.UtcNow, null);
    }

    public async Task RemoveAsync(Guid userId, int roleId, CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(
            StoredProcedureNames.RemoveUserRole,
            new { UserId = userId, RoleId = roleId },
            cancellationToken);
    }
}
