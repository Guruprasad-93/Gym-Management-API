using FluentValidation;
using Gym.Application.DTOs.Payments;

namespace Gym.Application.Validators;

public class CreatePaymentDtoValidator : AbstractValidator<CreatePaymentDto>
{
    private static readonly string[] AllowedPaymentMethods = ["Cash", "UPI", "Card", "Bank Transfer"];

    public CreatePaymentDtoValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.MembershipId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .Must(method => AllowedPaymentMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Payment method must be one of: {string.Join(", ", AllowedPaymentMethods)}.");
        RuleFor(x => x.TransactionReference).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
