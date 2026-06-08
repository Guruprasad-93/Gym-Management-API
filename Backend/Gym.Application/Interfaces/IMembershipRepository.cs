using Gym.Application.DTOs.Memberships;

namespace Gym.Application.Interfaces;

public interface IMembershipRepository
{
    Task<MembershipResponseDto> CreateAsync(Guid gymId, CreateMembershipDto dto, CancellationToken cancellationToken = default);
    Task RenewAsync(int membershipId, Guid gymId, RenewMembershipDto dto, CancellationToken cancellationToken = default);
    Task CancelAsync(int membershipId, Guid gymId, CancelMembershipDto dto, CancellationToken cancellationToken = default);
    Task<MembershipResponseDto?> GetByIdAsync(int membershipId, Guid? gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MembershipResponseDto>> GetAllAsync(Guid? gymId, int? memberId, string? search, bool includeInactive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MembershipResponseDto>> GetExpiredAsync(Guid? gymId, CancellationToken cancellationToken = default);
}
