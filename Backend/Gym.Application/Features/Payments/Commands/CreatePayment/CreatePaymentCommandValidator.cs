using FluentValidation;

namespace Gym.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator() =>
        RuleFor(x => x.Dto).SetValidator(new Validators.CreatePaymentDtoValidator());
}
