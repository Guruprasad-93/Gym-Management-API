using Gym.Application.DTOs.Common;

namespace Gym.Application.DTOs.Booking;

public sealed class ClassScheduleDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Capacity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class CreateClassScheduleDto
{
    public int BranchId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TrainerId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Capacity { get; set; } = 20;
}

public sealed class UpdateClassScheduleDto : CreateClassScheduleDto
{
    public int Id { get; set; }
    public string Status { get; set; } = "Active";
}

public sealed class ClassScheduleQueryDto : PagedRequestDto
{
    public int? BranchId { get; set; }
    public string? Status { get; set; }
}

public sealed class AvailableSlotDto
{
    public int ClassScheduleId { get; set; }
    public Guid GymId { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Capacity { get; set; }
    public int RemainingCapacity { get; set; }
    public int WaitlistCount { get; set; }
}

public sealed class AvailableSlotsQueryDto
{
    public int? BranchId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public sealed class SlotBookingDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int ClassScheduleId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateTime BookingDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CheckInTime { get; set; }
    public DateTime CreatedDate { get; set; }
    public string TrainerName { get; set; } = string.Empty;
}

public sealed class BookSlotDto
{
    public int ClassScheduleId { get; set; }
    public DateTime BookingDate { get; set; }
}

public sealed class CancelBookingDto
{
    public int BookingId { get; set; }
}

public sealed class JoinWaitlistDto
{
    public int ClassScheduleId { get; set; }
    public DateTime BookingDate { get; set; }
}

public class BookingQueryDto : PagedRequestDto
{
    public int? BranchId { get; set; }
    public int? MemberId { get; set; }
    public int? ClassScheduleId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public sealed class BookingCheckInDto
{
    public string QrPayload { get; set; } = string.Empty;
}

public sealed class BookingSettingsDto
{
    public Guid GymId { get; set; }
    public int MaxBookingsPerDay { get; set; }
    public bool AllowWaitlist { get; set; }
    public int CancellationWindowHours { get; set; }
    public int ReminderMinutesBefore { get; set; }
}

public sealed class UpdateBookingSettingsDto
{
    public int MaxBookingsPerDay { get; set; } = 3;
    public bool AllowWaitlist { get; set; } = true;
    public int CancellationWindowHours { get; set; } = 2;
    public int ReminderMinutesBefore { get; set; } = 60;
}

public sealed class TrainerAvailabilityDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}

public sealed class CreateTrainerAvailabilityDto
{
    public int BranchId { get; set; }
    public int TrainerId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

public sealed class TrainerScheduleDto
{
    public int ClassScheduleId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Capacity { get; set; }
    public string Status { get; set; } = string.Empty;
    public int BookingCount { get; set; }
}

public sealed class TrainerScheduleQueryDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public sealed class BookingAnalyticsDto
{
    public int TotalBookings { get; set; }
    public int TodaysBookings { get; set; }
    public decimal OccupancyPercent { get; set; }
    public decimal NoShowPercent { get; set; }
    public decimal CancellationPercent { get; set; }
    public IReadOnlyList<BookingChartPointDto> BookingTrend { get; set; } = Array.Empty<BookingChartPointDto>();
    public IReadOnlyList<BookingChartPointDto> PopularClasses { get; set; } = Array.Empty<BookingChartPointDto>();
    public IReadOnlyList<BookingChartPointDto> PeakHours { get; set; } = Array.Empty<BookingChartPointDto>();
    public IReadOnlyList<BookingChartPointDto> BranchComparison { get; set; } = Array.Empty<BookingChartPointDto>();
}

public sealed class BookingChartPointDto
{
    public string Label { get; set; } = string.Empty;
    public int BookingCount { get; set; }
}

public sealed class BookingReminderRowDto
{
    public int BookingId { get; set; }
    public Guid GymId { get; set; }
    public int MemberId { get; set; }
    public Guid UserId { get; set; }
    public string? Phone { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public Guid TrainerUserId { get; set; }
}

public sealed class BookingExportQueryDto : BookingQueryDto
{
    public string ReportType { get; set; } = "bookings";
}

public sealed class PromoteWaitlistResultDto
{
    public int PromotedBookingId { get; set; }
    public int PromotedMemberId { get; set; }
}
