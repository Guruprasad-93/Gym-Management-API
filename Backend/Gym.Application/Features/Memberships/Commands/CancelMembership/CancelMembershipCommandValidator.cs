using FluentValidation;

namespace Gym.Application.Features.Memberships.Commands.CancelMembership;

public class CancelMembershipCommandValidator : AbstractValidator<CancelMembershipCommand>
{
    public CancelMembershipCommandValidator()
    {
        RuleFor(x => x.MembershipId).GreaterThan(0);
        RuleFor(x => x.Dto.Notes).MaximumLength(500);
    }
}
