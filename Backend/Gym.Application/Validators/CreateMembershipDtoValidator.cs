using FluentValidation;
using Gym.Application.DTOs.Memberships;

namespace Gym.Application.Validators;

public class CreateMembershipDtoValidator : AbstractValidator<CreateMembershipDto>
{
    public CreateMembershipDtoValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.MembershipPlanId).GreaterThan(0);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
