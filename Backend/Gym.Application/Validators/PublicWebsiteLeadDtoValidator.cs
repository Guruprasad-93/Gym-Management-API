using FluentValidation;
using Gym.Application.DTOs.Website;
using Gym.Application.Validation;

namespace Gym.Application.Validators;

public class PublicWebsiteLeadDtoValidator : AbstractValidator<PublicWebsiteLeadDto>
{
    public PublicWebsiteLeadDtoValidator()
    {
        RuleFor(x => x.WebsiteSlug).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MobileNumber).RequiredPhoneNumber();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).MaximumLength(256);
    }
}
