using FluentValidation;

namespace Gym.Application.Features.Gyms.Commands.CreateGym;

public class CreateGymCommandValidator : AbstractValidator<CreateGymCommand>
{
    public CreateGymCommandValidator()
    {
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Dto.Email));
    }
}
