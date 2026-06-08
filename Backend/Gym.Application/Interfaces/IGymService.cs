using Gym.Application.DTOs.Gyms;

namespace Gym.Application.Interfaces;

public interface IGymService
{
    Task<GymDto> CreateAsync(CreateGymDto dto, CancellationToken cancellationToken = default);
    Task<GymDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GymDto> UpdateAsync(Guid id, UpdateGymDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
}
