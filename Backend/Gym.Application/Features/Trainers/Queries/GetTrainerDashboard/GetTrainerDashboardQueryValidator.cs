using FluentValidation;

namespace Gym.Application.Features.Trainers.Queries.GetTrainerDashboard;

public class GetTrainerDashboardQueryValidator : AbstractValidator<GetTrainerDashboardQuery>
{
    public GetTrainerDashboardQueryValidator() =>
        RuleFor(x => x.TrainerId).GreaterThan(0);
}
