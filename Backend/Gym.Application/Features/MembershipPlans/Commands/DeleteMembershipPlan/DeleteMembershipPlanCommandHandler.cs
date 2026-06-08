using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.MembershipPlans.Commands.DeleteMembershipPlan;

public class DeleteMembershipPlanCommandHandler : IRequestHandler<DeleteMembershipPlanCommand, Unit>
{
    private readonly IMembershipPlanService _membershipPlanService;

    public DeleteMembershipPlanCommandHandler(IMembershipPlanService membershipPlanService) =>
        _membershipPlanService = membershipPlanService;

    public async Task<Unit> Handle(DeleteMembershipPlanCommand request, CancellationToken cancellationToken)
    {
        await _membershipPlanService.DeleteAsync(request.PlanId, cancellationToken);
        return Unit.Value;
    }
}
