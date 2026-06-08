using Gym.Application.DTOs.Gyms;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Mappers;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class GymRepository : IGymRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public GymRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<GymDto> CreateAsync(Guid gymId, CreateGymDto dto, CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(StoredProcedureNames.CreateGym, new
        {
            GymId = gymId,
            dto.Name,
            dto.Address,
            dto.Phone,
            dto.Email,
            dto.LogoUrl
        }, cancellationToken);

        return (await GetByIdAsync(gymId, cancellationToken))!;
    }

    public async Task<GymDto?> GetByIdAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<GymRow>(
            StoredProcedureNames.GetGymById, new { GymId = gymId }, cancellationToken);
        return row is null ? null : DtoMapper.ToGymDto(row);
    }

    public async Task<IReadOnlyList<GymDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<GymRow>(StoredProcedureNames.GetAllGyms, cancellationToken: cancellationToken);
        return rows.Select(DtoMapper.ToGymDto).ToList();
    }

    public async Task UpdateAsync(Guid gymId, UpdateGymDto dto, CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(StoredProcedureNames.UpdateGym, new
        {
            GymId = gymId,
            dto.Name,
            dto.Address,
            dto.Phone,
            dto.Email,
            dto.LogoUrl
        }, cancellationToken);
    }

    public Task DeleteAsync(Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteGym, new { GymId = gymId }, cancellationToken);

    public Task SetActiveAsync(Guid gymId, bool isActive, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SetGymActive, new { GymId = gymId, IsActive = isActive }, cancellationToken);
}
