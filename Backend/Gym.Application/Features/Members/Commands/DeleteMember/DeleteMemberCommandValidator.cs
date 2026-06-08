using FluentValidation;

namespace Gym.Application.Features.Members.Commands.DeleteMember;

public class DeleteMemberCommandValidator : AbstractValidator<DeleteMemberCommand>
{
    public DeleteMemberCommandValidator() => RuleFor(x => x.MemberId).GreaterThan(0);
}
