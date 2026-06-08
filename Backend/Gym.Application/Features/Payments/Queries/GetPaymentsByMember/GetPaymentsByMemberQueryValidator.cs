using FluentValidation;

namespace Gym.Application.Features.Payments.Queries.GetPaymentsByMember;

public class GetPaymentsByMemberQueryValidator : AbstractValidator<GetPaymentsByMemberQuery>
{
    public GetPaymentsByMemberQueryValidator() => RuleFor(x => x.MemberId).GreaterThan(0);
}
