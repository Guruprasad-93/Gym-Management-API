using Gym.Application.DTOs.Memberships;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.MembershipPlans.Commands.UpdateMembershipPlan;

public class UpdateMembershipPlanCommandHandler : IRequestHandler<UpdateMembershipPlanCommand, MembershipPlanResponseDto>
{
    private readonly IMembershipPlanService _membershipPlanService;

    public UpdateMembershipPlanCommandHandler(IMembershipPlanService membershipPlanService) =>
        _membershipPlanService = membershipPlanService;

    public Task<MembershipPlanResponseDto> Handle(UpdateMembershipPlanCommand request, CancellationToken cancellationToken) =>
        _membershipPlanService.UpdateAsync(request.PlanId, request.Dto, cancellationToken);
}
