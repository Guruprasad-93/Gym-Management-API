using FluentValidation;
using Gym.Application.DTOs.Website;
using Gym.Application.Validation;

namespace Gym.Application.Validators;

public class PublicTrialBookingDtoValidator : AbstractValidator<PublicTrialBookingDto>
{
    public PublicTrialBookingDtoValidator()
    {
        RuleFor(x => x.WebsiteSlug).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MobileNumber).RequiredPhoneNumber();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).MaximumLength(256);
        RuleFor(x => x.PreferredDate).NotEmpty();
        RuleFor(x => x.PreferredTime).NotEmpty();
    }
}
