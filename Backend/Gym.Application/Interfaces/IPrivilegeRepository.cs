using Gym.Domain.Entities;

namespace Gym.Application.Interfaces;

public interface IPrivilegeRepository
{
    Task<Privilege?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Privilege>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Privilege> AddAsync(Privilege privilege, CancellationToken cancellationToken = default);
    Task UpdateAsync(Privilege privilege, CancellationToken cancellationToken = default);
    Task RemoveAsync(int id, CancellationToken cancellationToken = default);
    Task<Privilege?> GetByNameAsync(string privilegeName, CancellationToken cancellationToken = default);
    Task<bool> IsAssignedToRolesAsync(int privilegeId, CancellationToken cancellationToken = default);
}
