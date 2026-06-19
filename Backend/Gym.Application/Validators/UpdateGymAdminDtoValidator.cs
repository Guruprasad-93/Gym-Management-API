using FluentValidation;
using Gym.Application.DTOs.GymAdmins;
using Gym.Application.Validation;

namespace Gym.Application.Validators;

public class UpdateGymAdminDtoValidator : AbstractValidator<UpdateGymAdminDto>
{
    public UpdateGymAdminDtoValidator()
    {
        RuleFor(x => x.GymId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LoginIdentifier).ValidLoginIdentifier();
        RuleFor(x => x.Email).EmailAddress().MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
