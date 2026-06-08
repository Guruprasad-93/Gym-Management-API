using FluentValidation;

namespace Gym.Application.Features.Trainers.Queries.GetUnassignedMembers;

public class GetUnassignedMembersQueryValidator : AbstractValidator<GetUnassignedMembersQuery>
{
    public GetUnassignedMembersQueryValidator() =>
        RuleFor(x => x.TrainerId).GreaterThan(0);
}
