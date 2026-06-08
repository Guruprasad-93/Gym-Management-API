using FluentValidation;

namespace Gym.Application.Features.Members.Commands.UpdateMember;

public class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.Dto).SetValidator(new Validators.UpdateMemberDtoValidator());
    }
}
