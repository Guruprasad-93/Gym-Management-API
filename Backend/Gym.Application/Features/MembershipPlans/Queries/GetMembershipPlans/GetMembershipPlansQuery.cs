using Gym.Application.DTOs.Memberships;
using MediatR;

namespace Gym.Application.Features.MembershipPlans.Queries.GetMembershipPlans;

public record GetMembershipPlansQuery(bool IncludeInactive) : IRequest<IReadOnlyList<MembershipPlanResponseDto>>;
