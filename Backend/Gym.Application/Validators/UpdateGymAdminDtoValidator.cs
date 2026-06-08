using FluentValidation;
using Gym.Application.DTOs.GymAdmins;

namespace Gym.Application.Validators;

public class UpdateGymAdminDtoValidator : AbstractValidator<UpdateGymAdminDto>
{
    public UpdateGymAdminDtoValidator()
    {
        RuleFor(x => x.GymId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}
