using Gym.Application.Interfaces;
using Gym.Domain.Entities;
using Gym.Infrastructure.Persistence;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public UserRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var exists = await _sp.QuerySingleOrDefaultAsync<bool>(
            StoredProcedureNames.UserExistsByEmail,
            new { Email = email },
            cancellationToken);

        return exists;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<UserRow>(
            StoredProcedureNames.GetUserByEmail,
            new { Email = email },
            cancellationToken);

        return row is null ? null : EntityMapper.ToUser(row);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<UserRow>(
            StoredProcedureNames.GetUserById,
            new { Id = id },
            cancellationToken);

        return row is null ? null : EntityMapper.ToUser(row);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(
            StoredProcedureNames.CreateUser,
            new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Password,
                user.GymId
            },
            cancellationToken);

        return user;
    }

    public async Task<bool> AnyExistsAsync(CancellationToken cancellationToken = default)
    {
        var exists = await _sp.QuerySingleOrDefaultAsync<bool>(
            StoredProcedureNames.UserAnyExists,
            cancellationToken: cancellationToken);

        return exists;
    }
}
