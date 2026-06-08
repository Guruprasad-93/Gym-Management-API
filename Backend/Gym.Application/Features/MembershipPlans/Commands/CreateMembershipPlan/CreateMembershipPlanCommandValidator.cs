using FluentValidation;

namespace Gym.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;

public class CreateMembershipPlanCommandValidator : AbstractValidator<CreateMembershipPlanCommand>
{
    public CreateMembershipPlanCommandValidator() =>
        RuleFor(x => x.Dto).SetValidator(new Validators.CreateMembershipPlanDtoValidator());
}
