using FluentValidation;

namespace Gym.Application.Features.Members.Queries.GetMemberById;

public class GetMemberByIdQueryValidator : AbstractValidator<GetMemberByIdQuery>
{
    public GetMemberByIdQueryValidator() => RuleFor(x => x.MemberId).GreaterThan(0);
}
