using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Booking;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService) => _bookingService = bookingService;

    [HttpGet("available-slots")]
    [RequirePermission(Permissions.ViewBookings)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AvailableSlotDto>>>> GetAvailableSlots(
        [FromQuery] AvailableSlotsQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetAvailableSlotsAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AvailableSlotDto>>.Ok(result));
    }

    [HttpPost("book")]
    [RequirePermission(Permissions.ViewBookings)]
    public async Task<ActionResult<ApiResponse<SlotBookingDto>>> Book([FromBody] BookSlotDto dto, CancellationToken cancellationToken)
    {
        var result = await _bookingService.BookSlotAsync(dto, cancellationToken);
        return Ok(ApiResponse<SlotBookingDto>.Ok(result, "Slot booked."));
    }

    [HttpPost("cancel")]
    [RequirePermission(Permissions.ViewBookings)]
    public async Task<ActionResult<ApiResponse<object>>> Cancel([FromBody] CancelBookingDto dto, CancellationToken cancellationToken)
    {
        await _bookingService.CancelBookingAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Booking cancelled."));
    }

    [HttpPost("waitlist")]
    [RequirePermission(Permissions.ViewBookings)]
    public async Task<ActionResult<ApiResponse<object>>> Waitlist([FromBody] JoinWaitlistDto dto, CancellationToken cancellationToken)
    {
        await _bookingService.JoinWaitlistAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Added to waitlist."));
    }

    [HttpGet]
    [RequirePermission(Permissions.ViewBookings)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<SlotBookingDto>>>> GetBookings(
        [FromQuery] BookingQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetBookingsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<SlotBookingDto>>.Ok(result));
    }

    [HttpGet("settings")]
    [RequirePermission(Permissions.ManageSchedules)]
    public async Task<ActionResult<ApiResponse<BookingSettingsDto>>> GetSettings(CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetSettingsAsync(cancellationToken);
        return Ok(ApiResponse<BookingSettingsDto>.Ok(result));
    }

    [HttpPut("settings")]
    [RequirePermission(Permissions.ManageSchedules)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateSettings([FromBody] UpdateBookingSettingsDto dto, CancellationToken cancellationToken)
    {
        await _bookingService.UpdateSettingsAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Settings updated."));
    }
}

[ApiController]
[Route("api/schedules")]
[Authorize]
public class SchedulesController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public SchedulesController(IBookingService bookingService) => _bookingService = bookingService;

    [HttpGet]
    [RequirePermission(Permissions.ViewBookings)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<ClassScheduleDto>>>> Get([FromQuery] ClassScheduleQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetSchedulesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<ClassScheduleDto>>.Ok(result));
    }

    [HttpPost]
    [RequirePermission(Permissions.ManageSchedules)]
    public async Task<ActionResult<ApiResponse<ClassScheduleDto>>> Create([FromBody] CreateClassScheduleDto dto, CancellationToken cancellationToken)
    {
        var result = await _bookingService.CreateScheduleAsync(dto, cancellationToken);
        return Ok(ApiResponse<ClassScheduleDto>.Ok(result));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.ManageSchedules)]
    public async Task<ActionResult<ApiResponse<ClassScheduleDto>>> Update(int id, [FromBody] UpdateClassScheduleDto dto, CancellationToken cancellationToken)
    {
        dto.Id = id;
        var result = await _bookingService.UpdateScheduleAsync(dto, cancellationToken);
        return Ok(ApiResponse<ClassScheduleDto>.Ok(result));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ManageSchedules)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _bookingService.DeleteScheduleAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Schedule cancelled."));
    }
}

[ApiController]
[Route("api/trainer-schedule")]
[Authorize]
public class TrainerScheduleController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public TrainerScheduleController(IBookingService bookingService) => _bookingService = bookingService;

    [HttpGet]
    [RequirePermission(Permissions.ViewBookings)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TrainerScheduleDto>>>> Get(
        [FromQuery] TrainerScheduleQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetTrainerScheduleAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TrainerScheduleDto>>.Ok(result));
    }
}

[ApiController]
[Route("api/booking-analytics")]
[Authorize]
public class BookingAnalyticsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingAnalyticsController(IBookingService bookingService) => _bookingService = bookingService;

    [HttpGet]
    [RequirePermission(Permissions.ViewBookingAnalytics)]
    public async Task<ActionResult<ApiResponse<BookingAnalyticsDto>>> Get(
        [FromQuery] int? branchId, [FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        var result = await _bookingService.GetAnalyticsAsync(branchId, days, cancellationToken);
        return Ok(ApiResponse<BookingAnalyticsDto>.Ok(result));
    }

    [HttpGet("export/{format}")]
    [RequirePermission(Permissions.ViewBookingAnalytics)]
    public async Task<IActionResult> Export(string format, [FromQuery] BookingExportQueryDto query, CancellationToken cancellationToken)
    {
        var bytes = await _bookingService.ExportAsync(format, query.ReportType, query, cancellationToken);
        var contentType = format.Equals("excel", StringComparison.OrdinalIgnoreCase)
            ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            : "application/pdf";
        var ext = format.Equals("excel", StringComparison.OrdinalIgnoreCase) ? "xlsx" : "pdf";
        return File(bytes, contentType, $"booking-report.{ext}");
    }
}

[ApiController]
[Route("api/booking-checkin")]
[Authorize]
public class BookingCheckInController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingCheckInController(IBookingService bookingService) => _bookingService = bookingService;

    [HttpPost]
    [RequirePermission(Permissions.ManageBookings)]
    public async Task<ActionResult<ApiResponse<int>>> CheckIn([FromBody] BookingCheckInDto dto, CancellationToken cancellationToken)
    {
        var bookingId = await _bookingService.CheckInAsync(dto, cancellationToken);
        return Ok(ApiResponse<int>.Ok(bookingId, "Check-in successful."));
    }
}
