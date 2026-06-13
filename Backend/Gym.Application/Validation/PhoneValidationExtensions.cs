using FluentValidation;

namespace Gym.Application.Validation;

public static class PhoneValidationExtensions
{
    public static IRuleBuilderOptions<T, string?> OptionalPhoneNumber<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder
            .MaximumLength(20)
            .Must(PhoneNumberRules.IsValidOptional)
            .WithMessage(PhoneNumberRules.InvalidMessage);

    public static IRuleBuilderOptions<T, string> RequiredPhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .MaximumLength(20)
            .Must(PhoneNumberRules.IsValid)
            .WithMessage(PhoneNumberRules.InvalidMessage);
}
