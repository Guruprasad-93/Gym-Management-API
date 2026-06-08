using FluentValidation;

namespace Gym.Application.Features.Payments.Commands.RefundPayment;

public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).GreaterThan(0);
        RuleFor(x => x.Dto.Amount).GreaterThan(0).When(x => x.Dto.Amount.HasValue);
        RuleFor(x => x.Dto.Reason).MaximumLength(500).When(x => x.Dto.Reason != null);
    }
}
