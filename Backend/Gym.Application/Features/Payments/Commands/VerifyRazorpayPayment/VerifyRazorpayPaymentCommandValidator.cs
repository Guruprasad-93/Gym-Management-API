using FluentValidation;

namespace Gym.Application.Features.Payments.Commands.VerifyRazorpayPayment;

public class VerifyRazorpayPaymentCommandValidator : AbstractValidator<VerifyRazorpayPaymentCommand>
{
    public VerifyRazorpayPaymentCommandValidator()
    {
        RuleFor(x => x.Dto.RazorpayOrderId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.RazorpayPaymentId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.RazorpaySignature).NotEmpty().MaximumLength(256);
    }
}
