using FluentValidation;

namespace Gym.Application.Features.MembershipPlans.Commands.UpdateMembershipPlan;

public class UpdateMembershipPlanCommandValidator : AbstractValidator<UpdateMembershipPlanCommand>
{
    public UpdateMembershipPlanCommandValidator()
    {
        RuleFor(x => x.PlanId).GreaterThan(0);
        RuleFor(x => x.Dto).SetValidator(new Validators.UpdateMembershipPlanDtoValidator());
    }
}
