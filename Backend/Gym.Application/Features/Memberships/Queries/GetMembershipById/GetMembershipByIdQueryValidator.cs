using FluentValidation;

namespace Gym.Application.Features.Memberships.Queries.GetMembershipById;

public class GetMembershipByIdQueryValidator : AbstractValidator<GetMembershipByIdQuery>
{
    public GetMembershipByIdQueryValidator() => RuleFor(x => x.MembershipId).GreaterThan(0);
}
