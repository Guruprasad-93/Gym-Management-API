using Gym.Application.DTOs.Memberships;
using MediatR;

namespace Gym.Application.Features.Memberships.Queries.GetExpiredMemberships;

public record GetExpiredMembershipsQuery() : IRequest<IReadOnlyList<MembershipResponseDto>>;
