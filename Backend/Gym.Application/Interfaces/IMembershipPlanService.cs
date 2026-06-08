using Gym.Application.DTOs.Memberships;

namespace Gym.Application.Interfaces;

public interface IMembershipPlanService
{
    Task<MembershipPlanResponseDto> CreateAsync(CreateMembershipPlanDto dto, CancellationToken cancellationToken = default);
    Task<MembershipPlanResponseDto> UpdateAsync(int id, UpdateMembershipPlanDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MembershipPlanResponseDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default);
}
