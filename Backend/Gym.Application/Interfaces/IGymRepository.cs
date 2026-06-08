using Gym.Application.DTOs.Gyms;

namespace Gym.Application.Interfaces;

public interface IGymRepository
{
    Task<GymDto> CreateAsync(Guid gymId, CreateGymDto dto, CancellationToken cancellationToken = default);
    Task<GymDto?> GetByIdAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid gymId, UpdateGymDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task SetActiveAsync(Guid gymId, bool isActive, CancellationToken cancellationToken = default);
}
