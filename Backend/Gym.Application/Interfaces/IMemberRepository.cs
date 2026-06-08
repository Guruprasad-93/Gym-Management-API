using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Members;

namespace Gym.Application.Interfaces;

public interface IMemberRepository
{
    Task<MemberResponseDto> CreateAsync(Guid gymId, Guid userId, CreateMemberDto dto, CancellationToken cancellationToken = default);
    Task<MemberResponseDto?> GetByIdAsync(int memberId, Guid? gymId, int? trainerId, CancellationToken cancellationToken = default);
    Task<MemberResponseDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<MemberResponseDto>> GetPagedAsync(
        Guid? gymId,
        int? trainerId,
        string? search,
        bool includeInactive,
        PagedRequestDto paging,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(int memberId, Guid gymId, UpdateMemberDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int memberId, Guid gymId, CancellationToken cancellationToken = default);
    Task ActivateAsync(int memberId, Guid gymId, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int memberId, Guid gymId, CancellationToken cancellationToken = default);
    Task AssignTrainerAsync(int memberId, int trainerId, Guid gymId, CancellationToken cancellationToken = default);
    Task RemoveTrainerAssignmentAsync(int memberId, Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberPaymentHistoryDto>> GetPaymentHistoryAsync(int memberId, Guid? gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberProgressDto>> GetProgressAsync(int memberId, Guid? gymId, CancellationToken cancellationToken = default);
    Task<Guid?> GetGymIdAsync(int memberId, CancellationToken cancellationToken = default);
}
