using FluentValidation;

namespace Gym.Application.Features.Payments.Commands.CreateRazorpayOrder;

public class CreateRazorpayOrderCommandValidator : AbstractValidator<CreateRazorpayOrderCommand>
{
    public CreateRazorpayOrderCommandValidator()
    {
        RuleFor(x => x.Dto.MembershipId).GreaterThan(0);
        RuleFor(x => x.Dto.MemberId).GreaterThan(0).When(x => x.Dto.MemberId.HasValue);
        RuleFor(x => x.Dto.Notes).MaximumLength(500).When(x => x.Dto.Notes != null);
    }
}
