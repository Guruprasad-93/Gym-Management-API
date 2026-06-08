using Gym.Application.DTOs.Memberships;
using MediatR;

namespace Gym.Application.Features.Memberships.Queries.GetMemberships;

public record GetMembershipsQuery(string? Search, bool IncludeInactive) : IRequest<IReadOnlyList<MembershipResponseDto>>;
