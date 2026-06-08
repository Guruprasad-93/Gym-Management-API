using FluentValidation;

namespace Gym.Application.Features.Members.Queries.GetMemberDetails;

public class GetMemberDetailsQueryValidator : AbstractValidator<GetMemberDetailsQuery>
{
    public GetMemberDetailsQueryValidator() => RuleFor(x => x.MemberId).GreaterThan(0);
}
