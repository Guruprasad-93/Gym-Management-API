using FluentValidation;

namespace Gym.Application.Features.Payments.Queries.GetMonthlyRevenue;

public class GetMonthlyRevenueQueryValidator : AbstractValidator<GetMonthlyRevenueQuery>
{
    public GetMonthlyRevenueQueryValidator() => RuleFor(x => x.Months).InclusiveBetween(1, 24);
}
