using FluentValidation;
using Gym.Application.DTOs.Attendance;

namespace Gym.Application.Validators;

public class CheckInMemberDtoValidator : AbstractValidator<CheckInMemberDto>
{
    public CheckInMemberDtoValidator() => RuleFor(x => x.MemberId).GreaterThan(0);
}

public class CheckOutMemberDtoValidator : AbstractValidator<CheckOutMemberDto>
{
    public CheckOutMemberDtoValidator() => RuleFor(x => x.MemberId).GreaterThan(0);
}

public class MarkAttendanceDtoValidator : AbstractValidator<MarkAttendanceDto>
{
    public MarkAttendanceDtoValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.AttendanceStatusId).InclusiveBetween(3, 6);
    }
}
