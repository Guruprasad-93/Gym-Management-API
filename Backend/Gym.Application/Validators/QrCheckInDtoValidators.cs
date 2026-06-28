using FluentValidation;
using Gym.Application.DTOs.Booking;
using Gym.Application.DTOs.MemberSelfService;

namespace Gym.Application.Validators;

public sealed class QrCheckInDtoValidator : AbstractValidator<QrCheckInDto>
{
    public QrCheckInDtoValidator()
    {
        RuleFor(x => x.QrPayload)
            .NotEmpty()
            .Must(BeValidGmsPayload)
            .WithMessage("Invalid QR code format. Expected GMS:{gymId}:{memberId}:{token}.");
    }

    private static bool BeValidGmsPayload(string payload)
    {
        if (!payload.StartsWith("GMS:", StringComparison.Ordinal))
            return false;

        var parts = payload.Split(':');
        return parts.Length == 4
            && Guid.TryParse(parts[1], out _)
            && int.TryParse(parts[2], out _);
    }
}

public sealed class BookingCheckInDtoValidator : AbstractValidator<BookingCheckInDto>
{
    public BookingCheckInDtoValidator()
    {
        RuleFor(x => x.QrPayload)
            .NotEmpty()
            .Must(BeValidGmsPayload)
            .WithMessage("Invalid QR code format. Expected GMS:{gymId}:{memberId}:{token}.");
    }

    private static bool BeValidGmsPayload(string payload)
    {
        if (!payload.StartsWith("GMS:", StringComparison.Ordinal))
            return false;

        var parts = payload.Split(':');
        return parts.Length == 4
            && Guid.TryParse(parts[1], out _)
            && int.TryParse(parts[2], out _);
    }
}
