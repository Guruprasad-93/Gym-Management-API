using FluentValidation;
using Gym.Application.DTOs.Memberships;

namespace Gym.Application.Validators;

public class CreateMembershipPlanDtoValidator : AbstractValidator<CreateMembershipPlanDto>
{
    public CreateMembershipPlanDtoValidator()
    {
        RuleFor(x => x.PlanName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DurationInMonths).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
