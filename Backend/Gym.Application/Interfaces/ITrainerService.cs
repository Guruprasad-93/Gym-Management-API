using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Members;
using Gym.Application.DTOs.Trainers;

namespace Gym.Application.Interfaces;

public interface ITrainerService
{
    Task<TrainerDto> CreateAsync(CreateTrainerDto dto, CancellationToken cancellationToken = default);
    Task<TrainerDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TrainerDto> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<TrainerDto>> GetPagedAsync(GetTrainersQueryDto query, CancellationToken cancellationToken = default);
    Task<TrainerDto> UpdateAsync(int id, UpdateTrainerDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task AssignMembersAsync(int trainerId, AssignMembersToTrainerDto dto, CancellationToken cancellationToken = default);
    Task RemoveMemberAssignmentAsync(int memberId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberDto>> GetMembersAsync(int trainerId, string? search, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberDto>> GetUnassignedMembersAsync(int trainerId, string? search, CancellationToken cancellationToken = default);
    Task<TrainerDashboardDto> GetDashboardAsync(int trainerId, CancellationToken cancellationToken = default);
}
