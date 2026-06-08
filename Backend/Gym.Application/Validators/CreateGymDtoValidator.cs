using FluentValidation;
using Gym.Application.DTOs.Gyms;

namespace Gym.Application.Validators;

public class CreateGymDtoValidator : AbstractValidator<CreateGymDto>
{
    public CreateGymDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
