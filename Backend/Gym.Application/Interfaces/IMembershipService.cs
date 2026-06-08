using Gym.Application.DTOs.Memberships;

namespace Gym.Application.Interfaces;

public interface IMembershipService
{
    Task<MembershipResponseDto> CreateAsync(CreateMembershipDto dto, CancellationToken cancellationToken = default);
    Task<MembershipResponseDto> RenewAsync(int id, RenewMembershipDto dto, CancellationToken cancellationToken = default);
    Task CancelAsync(int id, CancelMembershipDto dto, CancellationToken cancellationToken = default);
    Task<MembershipResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MembershipResponseDto>> GetAllAsync(string? search, bool includeInactive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MembershipResponseDto>> GetExpiredAsync(CancellationToken cancellationToken = default);
}
