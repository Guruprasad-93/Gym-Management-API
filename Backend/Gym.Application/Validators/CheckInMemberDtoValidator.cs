using FluentValidation;
using Gym.Application.DTOs.Attendance;

namespace Gym.Application.Validators;

public class CheckInMemberDtoValidator : AbstractValidator<CheckInMemberDto>
{
    public CheckInMemberDtoValidator() => RuleFor(x => x.MemberId).GreaterThan(0);
}

public class CheckOutMemberDtoValidator : AbstractValidator<CheckOutMemberDto>
{
    public CheckOutMemberDtoValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.MemberAttendanceId).GreaterThan(0).When(x => x.MemberAttendanceId.HasValue);
    }
}

public class UpdateAttendanceSettingsDtoValidator : AbstractValidator<UpdateAttendanceSettingsDto>
{
    public UpdateAttendanceSettingsDtoValidator()
    {
        RuleFor(x => x.CheckoutReminderMinutesBefore).InclusiveBetween(0, 180);
        RuleFor(x => x.MaximumSessionHours).InclusiveBetween(1, 48);
        RuleFor(x => x.TimeZoneId).NotEmpty().MaximumLength(100);
        When(x => !x.Is24Hours, () =>
        {
            RuleFor(x => x.ClosingTime).Must(t => t > TimeOnly.MinValue);
        });
    }
}

public class MarkAttendanceDtoValidator : AbstractValidator<MarkAttendanceDto>
{
    public MarkAttendanceDtoValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.AttendanceStatusId).InclusiveBetween(3, 6);
    }
}
