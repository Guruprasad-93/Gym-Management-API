using FluentValidation;

namespace Gym.Application.Features.Members.Commands.ActivateMember;

public class ActivateMemberCommandValidator : AbstractValidator<ActivateMemberCommand>
{
    public ActivateMemberCommandValidator() => RuleFor(x => x.MemberId).GreaterThan(0);
}
