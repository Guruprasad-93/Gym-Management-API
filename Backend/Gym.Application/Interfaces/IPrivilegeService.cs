using Gym.Application.DTOs.Privileges;

namespace Gym.Application.Interfaces;

public interface IPrivilegeService
{
    Task<PrivilegeDto> CreateAsync(CreatePrivilegeDto dto, CancellationToken cancellationToken = default);
    Task<PrivilegeDto> UpdateAsync(int id, UpdatePrivilegeDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrivilegeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PrivilegeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
