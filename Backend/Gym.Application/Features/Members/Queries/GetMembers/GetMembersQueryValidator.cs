using FluentValidation;
using Gym.Application.Validators;

namespace Gym.Application.Features.Members.Queries.GetMembers;

public class GetMembersQueryValidator : AbstractValidator<GetMembersQuery>
{
    public GetMembersQueryValidator() =>
        RuleFor(x => x.Paging).SetValidator(new PagedRequestDtoValidator());
}
