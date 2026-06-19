using FluentValidation;
using Gym.Application.DTOs.Members;
using Gym.Application.Validation;

namespace Gym.Application.Validators;

public class CreateMemberDtoValidator : AbstractValidator<CreateMemberDto>
{
    public CreateMemberDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LoginIdentifier).ValidLoginIdentifier();
        RuleFor(x => x.Email)
            .EmailAddress().MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.JoinDate).NotEmpty();
        RuleFor(x => x.Gender).MaximumLength(20);
        RuleFor(x => x.Phone).OptionalPhoneNumber();
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.EmergencyContact).MaximumLength(200);
    }
}
