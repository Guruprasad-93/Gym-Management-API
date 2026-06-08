using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Members;

namespace Gym.Application.Interfaces;

public interface IMemberService
{
    Task<MemberResponseDto> CreateAsync(CreateMemberDto dto, CancellationToken cancellationToken = default);
    Task<MemberResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<MemberResponseDto> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<MemberResponseDto>> GetPagedAsync(GetMembersQueryDto query, CancellationToken cancellationToken = default);
    Task<MemberDetailsDto> GetDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<MemberResponseDto> UpdateAsync(int id, UpdateMemberDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task ActivateAsync(int id, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);
    Task AssignTrainerAsync(int memberId, AssignTrainerToMemberDto dto, CancellationToken cancellationToken = default);
    Task RemoveTrainerAssignmentAsync(int memberId, CancellationToken cancellationToken = default);
}
