using FluentValidation;

namespace Gym.Application.Features.Trainers.Queries.GetTrainerMembers;

public class GetTrainerMembersQueryValidator : AbstractValidator<GetTrainerMembersQuery>
{
    public GetTrainerMembersQueryValidator() =>
        RuleFor(x => x.TrainerId).GreaterThan(0);
}
