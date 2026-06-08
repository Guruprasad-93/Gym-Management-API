using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Members;
using Gym.Application.DTOs.Trainers;

namespace Gym.Application.Interfaces;

public interface ITrainerRepository
{
    Task<TrainerDto> CreateAsync(Guid gymId, CreateTrainerDto dto, CancellationToken cancellationToken = default);
    Task<TrainerDto?> GetByIdAsync(int trainerId, Guid? gymId, CancellationToken cancellationToken = default);
    Task<TrainerDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<TrainerDto>> GetPagedAsync(
        Guid? gymId,
        string? search,
        bool includeInactive,
        PagedRequestDto paging,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(int trainerId, Guid gymId, UpdateTrainerDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int trainerId, Guid gymId, bool unassignMembers = true, CancellationToken cancellationToken = default);
    Task AssignMemberAsync(int trainerId, int memberId, Guid gymId, CancellationToken cancellationToken = default);
    Task RemoveMemberAssignmentAsync(int memberId, Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberDto>> GetMembersAsync(
        int trainerId,
        Guid? gymId,
        string? search,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberDto>> GetUnassignedMembersAsync(
        Guid gymId,
        string? search,
        CancellationToken cancellationToken = default);
    Task<TrainerDashboardDto?> GetDashboardAsync(
        int trainerId,
        Guid? gymId,
        CancellationToken cancellationToken = default);
}
