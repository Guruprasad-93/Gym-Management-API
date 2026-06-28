using FluentValidation;
using Gym.Application.Validation;

namespace Gym.Application.Validators;

public static class LoginIdentifierValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidLoginIdentifier<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .MaximumLength(LoginIdentifierRules.MaxLength)
            .WithMessage($"Login identifier cannot exceed {LoginIdentifierRules.MaxLength} characters.");
}
