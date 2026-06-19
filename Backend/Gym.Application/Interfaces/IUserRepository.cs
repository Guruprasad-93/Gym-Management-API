using Gym.Domain.Entities;

namespace Gym.Application.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByLoginIdentifierAsync(string loginIdentifier, Guid? gymId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByLoginIdentifierAsync(string loginIdentifier, Guid? gymId, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> AnyExistsAsync(CancellationToken cancellationToken = default);
}
