using FluentValidation;

namespace Gym.Application.Features.Trainers.Queries.GetTrainerById;

public class GetTrainerByIdQueryValidator : AbstractValidator<GetTrainerByIdQuery>
{
    public GetTrainerByIdQueryValidator() =>
        RuleFor(x => x.TrainerId).GreaterThan(0);
}
