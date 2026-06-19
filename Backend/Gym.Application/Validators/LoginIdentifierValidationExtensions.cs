using FluentValidation;
using Gym.Application.Validation;

namespace Gym.Application.Validators;

public static class LoginIdentifierValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidLoginIdentifier<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .MaximumLength(LoginIdentifierRules.MaxLength)
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Login identifier may only contain letters, numbers, dots, underscores, and hyphens.");
}
