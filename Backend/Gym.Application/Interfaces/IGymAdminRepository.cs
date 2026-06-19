using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.GymAdmins;

namespace Gym.Application.Interfaces;

public interface IGymAdminRepository
{
    Task CreateAsync(
        Guid userId,
        Guid gymId,
        string name,
        string loginIdentifier,
        string? email,
        string passwordHash,
        bool mustChangePassword,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<GymAdminDto>> GetAllAsync(
        Guid? gymId,
        PagedRequestDto paging,
        CancellationToken cancellationToken = default);

    Task<GymAdminDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid userId, UpdateGymAdminDto dto, CancellationToken cancellationToken = default);

    Task SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(
        Guid userId,
        string passwordHash,
        bool mustChangePassword,
        CancellationToken cancellationToken = default);
}
