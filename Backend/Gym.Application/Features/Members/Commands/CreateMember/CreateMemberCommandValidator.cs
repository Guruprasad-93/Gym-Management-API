using FluentValidation;

namespace Gym.Application.Features.Members.Commands.CreateMember;

public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator() =>
        RuleFor(x => x.Dto).SetValidator(new Validators.CreateMemberDtoValidator());
}
