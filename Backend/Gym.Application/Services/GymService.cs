using Gym.Application.DTOs.Gyms;
using Gym.Application.Interfaces;
using Gym.Domain.Entities;

namespace Gym.Application.Services;

public class GymService : IGymService
{
    private readonly IGymRepository _gymRepository;

    public GymService(IGymRepository gymRepository) => _gymRepository = gymRepository;

    public async Task<GymDto> CreateAsync(CreateGymDto dto, CancellationToken cancellationToken = default)
    {
        var gym = Gym.Domain.Entities.Gym.Create(dto.Name, dto.Address, dto.Phone, dto.Email);
        return await _gymRepository.CreateAsync(gym.Id, dto, cancellationToken);
    }

    public Task<GymDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _gymRepository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<GymDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _gymRepository.GetAllAsync(cancellationToken);

    public async Task<GymDto> UpdateAsync(Guid id, UpdateGymDto dto, CancellationToken cancellationToken = default)
    {
        _ = await _gymRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Gym not found.");
        await _gymRepository.UpdateAsync(id, dto, cancellationToken);
        return (await _gymRepository.GetByIdAsync(id, cancellationToken))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _ = await _gymRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Gym not found.");
        await _gymRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        _ = await _gymRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Gym not found.");
        await _gymRepository.SetActiveAsync(id, isActive, cancellationToken);
    }
}
