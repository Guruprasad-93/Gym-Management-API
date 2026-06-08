using Gym.Application.DTOs.Memberships;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.MembershipPlans.Queries.GetMembershipPlans;

public class GetMembershipPlansQueryHandler : IRequestHandler<GetMembershipPlansQuery, IReadOnlyList<MembershipPlanResponseDto>>
{
    private readonly IMembershipPlanService _membershipPlanService;

    public GetMembershipPlansQueryHandler(IMembershipPlanService membershipPlanService) =>
        _membershipPlanService = membershipPlanService;

    public Task<IReadOnlyList<MembershipPlanResponseDto>> Handle(GetMembershipPlansQuery request, CancellationToken cancellationToken) =>
        _membershipPlanService.GetAllAsync(request.IncludeInactive, cancellationToken);
}
