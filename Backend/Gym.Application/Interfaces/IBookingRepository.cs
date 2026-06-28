using Gym.Application.DTOs.Booking;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.MemberSelfService;

namespace Gym.Application.Interfaces;

public interface IBookingRepository
{
    Task<BookingSettingsDto> GetSettingsAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task UpdateSettingsAsync(Guid gymId, UpdateBookingSettingsDto dto, CancellationToken cancellationToken = default);
    Task<int> CreateScheduleAsync(Guid gymId, CreateClassScheduleDto dto, CancellationToken cancellationToken = default);
    Task UpdateScheduleAsync(Guid gymId, UpdateClassScheduleDto dto, CancellationToken cancellationToken = default);
    Task<ClassScheduleDto?> GetScheduleByIdAsync(Guid gymId, int id, CancellationToken cancellationToken = default);
    Task<PagedResultDto<ClassScheduleDto>> GetSchedulesPagedAsync(Guid gymId, ClassScheduleQueryDto query, CancellationToken cancellationToken = default);
    Task DeleteScheduleAsync(Guid gymId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(Guid gymId, AvailableSlotsQueryDto query, CancellationToken cancellationToken = default);
    Task<(int BookingId, string? Error)> CreateBookingAsync(Guid gymId, int memberId, BookSlotDto dto, CancellationToken cancellationToken = default);
    Task<PromoteWaitlistResultDto> CancelBookingAsync(Guid gymId, int bookingId, int? memberId, CancellationToken cancellationToken = default);
    Task<(int WaitlistId, string? Error)> JoinWaitlistAsync(Guid gymId, int memberId, JoinWaitlistDto dto, CancellationToken cancellationToken = default);
    Task<PagedResultDto<SlotBookingDto>> GetBookingsPagedAsync(Guid gymId, BookingQueryDto query, CancellationToken cancellationToken = default);
    Task<(int BookingId, string? Error)> QrCheckInAsync(Guid gymId, int memberId, string qrToken, Guid markedByUserId, CancellationToken cancellationToken = default);
    Task ProcessNoShowsAsync(Guid? gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookingReminderRowDto>> GetBookingsForReminderAsync(int minutesBefore, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainerScheduleDto>> GetTrainerScheduleAsync(Guid gymId, int trainerId, TrainerScheduleQueryDto query, CancellationToken cancellationToken = default);
    Task<int> CreateTrainerAvailabilityAsync(Guid gymId, CreateTrainerAvailabilityDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainerAvailabilityDto>> GetTrainerAvailabilityAsync(Guid gymId, int? trainerId, int? branchId, CancellationToken cancellationToken = default);
    Task<BookingAnalyticsDto> GetAnalyticsAsync(Guid gymId, int? branchId, int days, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetGymsForJobAsync(CancellationToken cancellationToken = default);
}

public interface IBookingReportExporter
{
    byte[] ExportBookingsPdf(IReadOnlyList<SlotBookingDto> bookings, string title);
    byte[] ExportBookingsExcel(IReadOnlyList<SlotBookingDto> bookings, string title);
    byte[] ExportOccupancyPdf(BookingAnalyticsDto analytics, string title);
    byte[] ExportOccupancyExcel(BookingAnalyticsDto analytics, string title);
}

public interface IBookingService
{
    Task<BookingSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task UpdateSettingsAsync(UpdateBookingSettingsDto dto, CancellationToken cancellationToken = default);
    Task<ClassScheduleDto> CreateScheduleAsync(CreateClassScheduleDto dto, CancellationToken cancellationToken = default);
    Task<ClassScheduleDto> UpdateScheduleAsync(UpdateClassScheduleDto dto, CancellationToken cancellationToken = default);
    Task<ClassScheduleDto> GetScheduleByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResultDto<ClassScheduleDto>> GetSchedulesAsync(ClassScheduleQueryDto query, CancellationToken cancellationToken = default);
    Task DeleteScheduleAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(AvailableSlotsQueryDto query, CancellationToken cancellationToken = default);
    Task<SlotBookingDto> BookSlotAsync(BookSlotDto dto, CancellationToken cancellationToken = default);
    Task CancelBookingAsync(CancelBookingDto dto, CancellationToken cancellationToken = default);
    Task JoinWaitlistAsync(JoinWaitlistDto dto, CancellationToken cancellationToken = default);
    Task<PagedResultDto<SlotBookingDto>> GetBookingsAsync(BookingQueryDto query, CancellationToken cancellationToken = default);
    Task<QrScanResultDto> CheckInAsync(BookingCheckInDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainerScheduleDto>> GetTrainerScheduleAsync(TrainerScheduleQueryDto query, CancellationToken cancellationToken = default);
    Task<BookingAnalyticsDto> GetAnalyticsAsync(int? branchId, int days, CancellationToken cancellationToken = default);
    Task<byte[]> ExportAsync(string format, string reportType, BookingExportQueryDto query, CancellationToken cancellationToken = default);
    Task RunReminderAndNoShowProcessingAsync(CancellationToken cancellationToken = default);
    Task GenerateBookingAiInsightsAsync(Guid gymId, CancellationToken cancellationToken = default);
}
