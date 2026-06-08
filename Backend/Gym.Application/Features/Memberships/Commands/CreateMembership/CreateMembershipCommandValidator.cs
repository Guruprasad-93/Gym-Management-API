using FluentValidation;

namespace Gym.Application.Features.Memberships.Commands.CreateMembership;

public class CreateMembershipCommandValidator : AbstractValidator<CreateMembershipCommand>
{
    public CreateMembershipCommandValidator() =>
        RuleFor(x => x.Dto).SetValidator(new Validators.CreateMembershipDtoValidator());
}
