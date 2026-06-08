using FluentValidation;

namespace Gym.Application.Features.Members.Commands.DeactivateMember;

public class DeactivateMemberCommandValidator : AbstractValidator<DeactivateMemberCommand>
{
    public DeactivateMemberCommandValidator() => RuleFor(x => x.MemberId).GreaterThan(0);
}
