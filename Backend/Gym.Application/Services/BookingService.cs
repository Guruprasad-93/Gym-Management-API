using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Ai;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Booking;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.MemberSelfService;
using Gym.Application.DTOs.Members;
using Gym.Application.DTOs.Mobile;
using Gym.Application.DTOs.Notifications;
using Gym.Application.DTOs.Trainers;
using Gym.Application.Interfaces;

namespace Gym.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;
    private readonly IMemberRepository _memberRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    private readonly IMobilePushService _mobilePushService;
    private readonly IAiRecommendationRepository _aiRepository;
    private readonly IAuditService _auditService;
    private readonly IBookingReportExporter _exporter;

    public BookingService(
        IBookingRepository repository,
        IMemberRepository memberRepository,
        ITrainerRepository trainerRepository,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        IMobilePushService mobilePushService,
        IAiRecommendationRepository aiRepository,
        IAuditService auditService,
        IBookingReportExporter exporter)
    {
        _repository = repository;
        _memberRepository = memberRepository;
        _trainerRepository = trainerRepository;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _mobilePushService = mobilePushService;
        _aiRepository = aiRepository;
        _auditService = auditService;
        _exporter = exporter;
    }

    public async Task<BookingSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanManageSchedules();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetSettingsAsync(gymId, cancellationToken);
    }

    public async Task UpdateSettingsAsync(UpdateBookingSettingsDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManageSchedules();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.UpdateSettingsAsync(gymId, dto, cancellationToken);
    }

    public async Task<ClassScheduleDto> CreateScheduleAsync(CreateClassScheduleDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManageSchedules();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var id = await _repository.CreateScheduleAsync(gymId, dto, cancellationToken);
        var schedule = (await _repository.GetScheduleByIdAsync(gymId, id, cancellationToken))!;
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.ClassSchedule,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = schedule
        }, cancellationToken);
        return schedule;
    }

    public async Task<ClassScheduleDto> UpdateScheduleAsync(UpdateClassScheduleDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManageSchedules();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.UpdateScheduleAsync(gymId, dto, cancellationToken);
        var schedule = (await _repository.GetScheduleByIdAsync(gymId, dto.Id, cancellationToken))!;
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.ClassSchedule,
            EntityId = dto.Id.ToString(),
            ActionType = AuditActionTypes.Update,
            NewValue = schedule
        }, cancellationToken);
        return schedule;
    }

    public async Task<ClassScheduleDto> GetScheduleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureCanViewBookings();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var schedule = await _repository.GetScheduleByIdAsync(gymId, id, cancellationToken);
        if (schedule is null)
            throw new KeyNotFoundException("Class schedule not found.");
        return schedule;
    }

    public async Task<PagedResultDto<ClassScheduleDto>> GetSchedulesAsync(ClassScheduleQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewBookings();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetSchedulesPagedAsync(gymId, query, cancellationToken);
    }

    public async Task DeleteScheduleAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureCanManageSchedules();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var schedule = await _repository.GetScheduleByIdAsync(gymId, id, cancellationToken);
        if (schedule is null)
            throw new KeyNotFoundException("Class schedule not found.");

        await _repository.DeleteScheduleAsync(gymId, id, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.ClassSchedule,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Delete,
            OldValue = schedule
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(AvailableSlotsQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewBookings();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetAvailableSlotsAsync(gymId, query, cancellationToken);
    }

    public async Task<SlotBookingDto> BookSlotAsync(BookSlotDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var member = await ResolveCurrentMemberAsync(cancellationToken);
        var (bookingId, error) = await _repository.CreateBookingAsync(gymId, member.Id, dto, cancellationToken);
        if (bookingId <= 0)
            throw new InvalidOperationException(error ?? "Booking failed.");

        var bookings = await _repository.GetBookingsPagedAsync(gymId, new BookingQueryDto { PageNumber = 1, PageSize = 50, MemberId = member.Id }, cancellationToken);
        var booking = bookings.Items.FirstOrDefault(b => b.Id == bookingId)
            ?? throw new InvalidOperationException("Booking not found after creation.");

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.SlotBooking,
            EntityId = bookingId.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = booking
        }, cancellationToken);

        await SendBookingNotificationAsync(gymId, member.Id, member.UserId, member.Phone, member.FullName,
            booking.ClassName, BookingNotificationTypes.BookingCreated, PushNotificationTypes.BookingCreated,
            "Booking Confirmed", $"Your slot for {booking.ClassName} on {booking.BookingDate:yyyy-MM-dd} is confirmed.", cancellationToken);

        var schedule = await _repository.GetScheduleByIdAsync(gymId, dto.ClassScheduleId, cancellationToken);
        if (schedule is not null)
        {
            var trainer = await _trainerRepository.GetByIdAsync(schedule.TrainerId, gymId, cancellationToken);
            if (trainer is not null && trainer.UserId.HasValue)
            {
                await _mobilePushService.SendEventPushAsync(gymId, new SendEventPushRequest
                {
                    UserId = trainer.UserId.Value,
                    NotificationType = PushNotificationTypes.TrainerAssignment,
                    Title = "New Booking",
                    Message = $"{member.FullName} booked {schedule.ClassName} on {dto.BookingDate:yyyy-MM-dd}."
                }, cancellationToken);
            }
        }

        return booking;
    }

    public async Task CancelBookingAsync(CancelBookingDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        int? memberId = _currentUser.HasPermission(Permissions.ManageBookings) ? null : (await ResolveCurrentMemberAsync(cancellationToken)).Id;
        var result = await _repository.CancelBookingAsync(gymId, dto.BookingId, memberId, cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.SlotBooking,
            EntityId = dto.BookingId.ToString(),
            ActionType = AuditActionTypes.Cancel
        }, cancellationToken);

        if (result.PromotedBookingId > 0 && result.PromotedMemberId > 0)
        {
            var promoted = await _memberRepository.GetByIdAsync(result.PromotedMemberId, gymId, null, cancellationToken);
            if (promoted is not null)
            {
                await SendBookingNotificationAsync(gymId, promoted.Id, promoted.UserId, promoted.Phone, promoted.FullName,
                    "Class", BookingNotificationTypes.WaitlistPromoted, PushNotificationTypes.WaitlistPromoted,
                    "Waitlist Promotion", "You have been moved from the waitlist. Your slot is confirmed.", cancellationToken);
                await _auditService.LogAsync(new AuditLogEntryDto
                {
                    GymId = gymId,
                    EntityName = AuditEntityNames.BookingWaitlist,
                    EntityId = result.PromotedBookingId.ToString(),
                    ActionType = AuditActionTypes.Assign
                }, cancellationToken);
            }
        }
    }

    public async Task JoinWaitlistAsync(JoinWaitlistDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var member = await ResolveCurrentMemberAsync(cancellationToken);
        var (_, error) = await _repository.JoinWaitlistAsync(gymId, member.Id, dto, cancellationToken);
        if (!string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException(error);
    }

    public async Task<PagedResultDto<SlotBookingDto>> GetBookingsAsync(BookingQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewBookings();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        if (!_currentUser.HasPermission(Permissions.ManageBookings) && query.MemberId is null)
        {
            var member = await ResolveCurrentMemberAsync(cancellationToken);
            query.MemberId = member.Id;
        }
        return await _repository.GetBookingsPagedAsync(gymId, query, cancellationToken);
    }

    public async Task<QrScanResultDto> CheckInAsync(BookingCheckInDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasPermission(Permissions.ManageBookings))
            throw new UnauthorizedAccessException("Manage bookings permission required.");

        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var (payloadGymId, memberId, token) = ParseQrPayload(dto.QrPayload);
        if (payloadGymId != gymId)
            throw new UnauthorizedAccessException("QR code gym mismatch.");

        var (bookingId, error) = await _repository.QrCheckInAsync(gymId, memberId, token, _currentUser.UserId!.Value, cancellationToken);
        if (bookingId <= 0)
            throw new InvalidOperationException(error ?? "Check-in failed.");

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.SlotBooking,
            EntityId = bookingId.ToString(),
            ActionType = AuditActionTypes.CheckIn
        }, cancellationToken);

        var bookings = await _repository.GetBookingsPagedAsync(
            gymId,
            new BookingQueryDto { MemberId = memberId, PageNumber = 1, PageSize = 50 },
            cancellationToken);
        var booking = bookings.Items.FirstOrDefault(b => b.Id == bookingId);

        return await BuildQrScanResultAsync(gymId, memberId, bookingId, booking, cancellationToken);
    }

    public async Task<IReadOnlyList<TrainerScheduleDto>> GetTrainerScheduleAsync(TrainerScheduleQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewBookings();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var trainer = await ResolveCurrentTrainerAsync(gymId, cancellationToken);
        return await _repository.GetTrainerScheduleAsync(gymId, trainer.Id, query, cancellationToken);
    }

    public async Task<BookingAnalyticsDto> GetAnalyticsAsync(int? branchId, int days, CancellationToken cancellationToken = default)
    {
        EnsureCanViewAnalytics();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetAnalyticsAsync(gymId, branchId, days, cancellationToken);
    }

    public async Task<byte[]> ExportAsync(string format, string reportType, BookingExportQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewAnalytics();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        query.PageSize = 5000;
        if (string.Equals(reportType, "occupancy", StringComparison.OrdinalIgnoreCase))
        {
            var analytics = await _repository.GetAnalyticsAsync(gymId, query.BranchId, 30, cancellationToken);
            return format.Equals("excel", StringComparison.OrdinalIgnoreCase)
                ? _exporter.ExportOccupancyExcel(analytics, "Occupancy Report")
                : _exporter.ExportOccupancyPdf(analytics, "Occupancy Report");
        }

        var bookings = await _repository.GetBookingsPagedAsync(gymId, query, cancellationToken);
        return format.Equals("excel", StringComparison.OrdinalIgnoreCase)
            ? _exporter.ExportBookingsExcel(bookings.Items, "Booking Report")
            : _exporter.ExportBookingsPdf(bookings.Items, "Booking Report");
    }

    public async Task RunReminderAndNoShowProcessingAsync(CancellationToken cancellationToken = default)
    {
        var gyms = await _repository.GetGymsForJobAsync(cancellationToken);
        foreach (var gymId in gyms)
        {
            await _repository.ProcessNoShowsAsync(gymId, cancellationToken);
            var settings = await _repository.GetSettingsAsync(gymId, cancellationToken);
            var reminders = await _repository.GetBookingsForReminderAsync(settings.ReminderMinutesBefore, cancellationToken);
            foreach (var row in reminders.Where(r => r.GymId == gymId))
            {
                await SendBookingNotificationAsync(gymId, row.MemberId, row.UserId, row.Phone, row.MemberName,
                    row.ClassName, BookingNotificationTypes.BookingReminder, PushNotificationTypes.BookingReminder,
                    "Class Reminder", $"Your {row.ClassName} class starts in {settings.ReminderMinutesBefore} minutes.", cancellationToken);
            }
        }
    }

    public async Task GenerateBookingAiInsightsAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var context = await _aiRepository.GetBusinessContextAsync(gymId, cancellationToken);
        if (context is null) return;

        var insights = new List<AiGeneratedInsight>();
        insights.Add(new AiGeneratedInsight
        {
            InsightType = AiInsightTypes.AttendanceDecline,
            InsightText = "Review peak booking hours and consider adding classes during high-demand slots.",
            Severity = AiInsightSeverities.Info
        });
        foreach (var insight in insights)
            await _aiRepository.CreateInsightAsync(gymId, insight, cancellationToken);
    }

    private async Task SendBookingNotificationAsync(
        Guid gymId, int memberId, Guid userId, string? phone, string memberName, string className,
        string notificationType, string pushType, string title, string message,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(phone))
        {
            await _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
            {
                NotificationType = notificationType,
                PhoneNumber = phone,
                RecipientUserId = userId,
                MemberId = memberId,
                Variables = new Dictionary<string, string> { ["memberName"] = memberName, ["className"] = className }
            }, cancellationToken);
        }

        await _mobilePushService.SendEventPushAsync(gymId, new SendEventPushRequest
        {
            UserId = userId,
            NotificationType = pushType,
            Title = title,
            Message = message
        }, cancellationToken);
    }

    private static (Guid GymId, int MemberId, string Token) ParseQrPayload(string payload)
    {
        var parts = payload.Split(':');
        if (parts.Length != 4 || !parts[0].Equals("GMS", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid QR payload.");
        if (!Guid.TryParse(parts[1], out var gymId) || !int.TryParse(parts[2], out var memberId))
            throw new InvalidOperationException("Invalid QR payload.");
        return (gymId, memberId, parts[3]);
    }

    private async Task<QrScanResultDto> BuildQrScanResultAsync(
        Guid gymId,
        int memberId,
        int bookingId,
        SlotBookingDto? booking,
        CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(memberId, gymId, null, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");

        return new QrScanResultDto
        {
            BookingId = bookingId,
            MemberId = memberId,
            MemberName = member.FullName,
            MembershipStatus = member.MembershipStatus,
            MembershipPlanName = member.MembershipPlanName,
            BookingStatus = booking?.Status ?? "CheckedIn",
            ClassName = booking?.ClassName
        };
    }

    private async Task<MemberResponseDto> ResolveCurrentMemberAsync(CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Member profile not found.");
        return member;
    }

    private async Task<TrainerDto> ResolveCurrentTrainerAsync(Guid gymId, CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(Permissions.ManageSchedules))
        {
            var trainers = await _trainerRepository.GetPagedAsync(gymId, null, true, new PagedRequestDto { PageNumber = 1, PageSize = 1 }, cancellationToken);
            return trainers.Items.FirstOrDefault() ?? throw new InvalidOperationException("No trainer found.");
        }

        return await _trainerRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Trainer profile not found.");
    }

    private void EnsureCanViewBookings()
    {
        if (!_currentUser.HasPermission(Permissions.ViewBookings))
            throw new UnauthorizedAccessException("Missing VIEW_BOOKINGS permission.");
    }

    private void EnsureCanManageSchedules()
    {
        if (!_currentUser.HasPermission(Permissions.ManageSchedules))
            throw new UnauthorizedAccessException("Missing MANAGE_SCHEDULES permission.");
    }

    private void EnsureCanViewAnalytics()
    {
        if (!_currentUser.HasPermission(Permissions.ViewBookingAnalytics))
            throw new UnauthorizedAccessException("Missing VIEW_BOOKING_ANALYTICS permission.");
    }
}
