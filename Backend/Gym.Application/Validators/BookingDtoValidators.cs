using FluentValidation;
using Gym.Application.DTOs.Booking;

namespace Gym.Application.Validators;

public sealed class CreateClassScheduleDtoValidator : AbstractValidator<CreateClassScheduleDto>
{
    public CreateClassScheduleDtoValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.TrainerId).GreaterThan(0);
        RuleFor(x => x.ClassName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DayOfWeek).InclusiveBetween(0, 6);
        RuleFor(x => x.Capacity).GreaterThan(0).LessThanOrEqualTo(500);
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");
    }
}

public sealed class UpdateClassScheduleDtoValidator : AbstractValidator<UpdateClassScheduleDto>
{
    public UpdateClassScheduleDtoValidator()
    {
        Include(new CreateClassScheduleDtoValidator());
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Status).Equal("Active");
    }
}

public sealed class BookSlotDtoValidator : AbstractValidator<BookSlotDto>
{
    public BookSlotDtoValidator()
    {
        RuleFor(x => x.ClassScheduleId).GreaterThan(0);
        RuleFor(x => x.BookingDate).NotEmpty();
    }
}

public sealed class UpdateBookingSettingsDtoValidator : AbstractValidator<UpdateBookingSettingsDto>
{
    public UpdateBookingSettingsDtoValidator()
    {
        RuleFor(x => x.MaxBookingsPerDay).GreaterThan(0).LessThanOrEqualTo(20);
        RuleFor(x => x.CancellationWindowHours).GreaterThanOrEqualTo(0).LessThanOrEqualTo(72);
        RuleFor(x => x.ReminderMinutesBefore).GreaterThanOrEqualTo(0).LessThanOrEqualTo(1440);
    }
}
