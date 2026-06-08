using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.GymAdmins;

namespace Gym.Application.Interfaces;

public interface IGymAdminService
{
    Task<CreateGymAdminResultDto> CreateAsync(CreateGymAdminDto dto, CancellationToken cancellationToken = default);
    Task<PagedResultDto<GymAdminDto>> GetAllAsync(Guid? gymId, PagedRequestDto paging, CancellationToken cancellationToken = default);
    Task<GymAdminDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<GymAdminDto> UpdateAsync(Guid userId, UpdateGymAdminDto dto, CancellationToken cancellationToken = default);
    Task SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default);
    Task<ResendTemporaryPasswordResultDto> ResendTemporaryPasswordAsync(Guid userId, CancellationToken cancellationToken = default);
}
