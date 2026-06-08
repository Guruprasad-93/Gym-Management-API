using FluentValidation;

namespace Gym.Application.Features.Memberships.Commands.RenewMembership;

public class RenewMembershipCommandValidator : AbstractValidator<RenewMembershipCommand>
{
    public RenewMembershipCommandValidator()
    {
        RuleFor(x => x.MembershipId).GreaterThan(0);
        RuleFor(x => x.Dto.Notes).MaximumLength(500);
    }
}
