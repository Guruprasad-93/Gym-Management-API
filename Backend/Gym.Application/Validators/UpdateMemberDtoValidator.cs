using FluentValidation;
using Gym.Application.DTOs.Members;
using Gym.Application.Validation;

namespace Gym.Application.Validators;

public class UpdateMemberDtoValidator : AbstractValidator<UpdateMemberDto>
{
    public UpdateMemberDtoValidator()
    {
        RuleFor(x => x.FullName).MaximumLength(100);
        RuleFor(x => x.LoginIdentifier!)
            .ValidLoginIdentifier()
            .When(x => !string.IsNullOrWhiteSpace(x.LoginIdentifier));
        RuleFor(x => x.Email).EmailAddress().MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Gender).MaximumLength(20);
        RuleFor(x => x.Phone).OptionalPhoneNumber();
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.EmergencyContact).MaximumLength(200);
    }
}
