using FluentValidation;

namespace Gym.Application.Features.Payments.Commands.GenerateInvoice;

public class GenerateInvoiceCommandValidator : AbstractValidator<GenerateInvoiceCommand>
{
    public GenerateInvoiceCommandValidator() => RuleFor(x => x.PaymentId).GreaterThan(0);
}
