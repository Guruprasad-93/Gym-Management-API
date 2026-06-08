using FluentValidation;

namespace Gym.Application.Features.MembershipPlans.Commands.DeleteMembershipPlan;

public class DeleteMembershipPlanCommandValidator : AbstractValidator<DeleteMembershipPlanCommand>
{
    public DeleteMembershipPlanCommandValidator() => RuleFor(x => x.PlanId).GreaterThan(0);
}
