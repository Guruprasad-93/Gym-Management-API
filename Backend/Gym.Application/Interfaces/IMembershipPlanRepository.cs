using Gym.Application.DTOs.Memberships;

namespace Gym.Application.Interfaces;

public interface IMembershipPlanRepository
{
    Task<MembershipPlanResponseDto> CreateAsync(Guid gymId, CreateMembershipPlanDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int planId, Guid gymId, UpdateMembershipPlanDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int planId, Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MembershipPlanResponseDto>> GetAllAsync(Guid? gymId, bool includeInactive, CancellationToken cancellationToken = default);
}
