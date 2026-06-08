using FluentValidation;
using Gym.Application.Validators;

namespace Gym.Application.Features.Trainers.Queries.GetTrainers;

public class GetTrainersQueryValidator : AbstractValidator<GetTrainersQuery>
{
    public GetTrainersQueryValidator() =>
        RuleFor(x => x.Paging).SetValidator(new PagedRequestDtoValidator());
}
