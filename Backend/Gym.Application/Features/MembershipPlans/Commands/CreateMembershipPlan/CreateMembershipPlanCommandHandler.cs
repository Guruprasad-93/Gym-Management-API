using Gym.Application.DTOs.Memberships;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;

public class CreateMembershipPlanCommandHandler : IRequestHandler<CreateMembershipPlanCommand, MembershipPlanResponseDto>
{
    private readonly IMembershipPlanService _membershipPlanService;

    public CreateMembershipPlanCommandHandler(IMembershipPlanService membershipPlanService) =>
        _membershipPlanService = membershipPlanService;

    public Task<MembershipPlanResponseDto> Handle(CreateMembershipPlanCommand request, CancellationToken cancellationToken) =>
        _membershipPlanService.CreateAsync(request.Dto, cancellationToken);
}
