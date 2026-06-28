using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Attendance;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

/// <summary>Member and trainer attendance management, reports, and exports.</summary>
[ApiController]
[Route("api/attendance")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly IAttendanceReportExporter _reportExporter;

    public AttendanceController(IAttendanceService attendanceService, IAttendanceReportExporter reportExporter)
    {
        _attendanceService = attendanceService;
        _reportExporter = reportExporter;
    }

    /// <summary>Attendance status lookup values.</summary>
    [HttpGet("statuses")]
    [RequirePermission(Permissions.ViewAttendance)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AttendanceStatusDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AttendanceStatusDto>>>> GetStatuses(CancellationToken cancellationToken)
    {
        var statuses = await _attendanceService.GetStatusesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AttendanceStatusDto>>.Ok(statuses));
    }

    /// <summary>Attendance KPIs for today.</summary>
    [HttpGet("dashboard")]
    [RequirePermission(Permissions.ViewAttendance)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AttendanceDashboardDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await _attendanceService.GetDashboardAsync(cancellationToken);
        return Ok(ApiResponse<AttendanceDashboardDto>.Ok(dashboard));
    }

    /// <summary>Today's attendance records.</summary>
    [HttpGet("today")]
    [RequirePermission(Permissions.ViewAttendance)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MemberAttendanceDto>>>> GetToday(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var list = await _attendanceService.GetTodayAsync(search, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MemberAttendanceDto>>.Ok(list));
    }

    /// <summary>Paged attendance by date range with filters.</summary>
    [HttpGet]
    [RequirePermission(Permissions.ViewAttendance)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<MemberAttendanceDto>>>> GetByDateRange(
        [FromQuery] AttendanceQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetByDateRangeAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<MemberAttendanceDto>>.Ok(result));
    }

    /// <summary>Member attendance history (paged).</summary>
    [HttpGet("members/{memberId:int}/history")]
    [RequirePermission(Permissions.ViewAttendance)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<MemberAttendanceDto>>>> GetMemberHistory(
        int memberId,
        [FromQuery] AttendanceQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetMemberHistoryAsync(memberId, query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<MemberAttendanceDto>>.Ok(result));
    }

    /// <summary>Check in a member.</summary>
    [HttpPost("check-in")]
    [RequirePermission(Permissions.ManageAttendance)]
    public async Task<ActionResult<ApiResponse<MemberAttendanceDto>>> CheckIn(
        [FromBody] CheckInMemberDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.CheckInAsync(dto, cancellationToken);
        return Ok(ApiResponse<MemberAttendanceDto>.Ok(result, "Member checked in."));
    }

    /// <summary>Check out a member (closes open session).</summary>
    [HttpPost("check-out")]
    [RequirePermission(Permissions.ManageAttendance)]
    public async Task<ActionResult<ApiResponse<MemberAttendanceDto>>> CheckOut(
        [FromBody] CheckOutMemberDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.CheckOutAsync(dto, cancellationToken);
        var message = dto.IsManualCheckout ? "Member manually checked out." : "Member checked out.";
        return Ok(ApiResponse<MemberAttendanceDto>.Ok(result, message));
    }

    /// <summary>Gym attendance settings (hours, auto checkout).</summary>
    [HttpGet("settings")]
    [RequirePermission(Permissions.ManageAttendance)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceSettingsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AttendanceSettingsDto>>> GetSettings(CancellationToken cancellationToken)
    {
        var settings = await _attendanceService.GetSettingsAsync(cancellationToken);
        return Ok(ApiResponse<AttendanceSettingsDto>.Ok(settings));
    }

    /// <summary>Update gym attendance settings.</summary>
    [HttpPut("settings")]
    [RequirePermission(Permissions.ManageAttendance)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateSettings(
        [FromBody] UpdateAttendanceSettingsDto dto,
        CancellationToken cancellationToken)
    {
        await _attendanceService.UpdateSettingsAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Attendance settings updated."));
    }

    /// <summary>Manually mark attendance (Present/Absent/Late/Excused).</summary>
    [HttpPost("mark")]
    [RequirePermission(Permissions.ManageAttendance)]
    public async Task<ActionResult<ApiResponse<MemberAttendanceDto>>> Mark(
        [FromBody] MarkAttendanceDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.MarkAsync(dto, cancellationToken);
        return Ok(ApiResponse<MemberAttendanceDto>.Ok(result, "Attendance marked."));
    }

    /// <summary>Daily attendance report.</summary>
    [HttpGet("reports/daily")]
    [RequirePermission(Permissions.ViewAttendance)]
    public async Task<ActionResult<ApiResponse<DailyAttendanceReportDto>>> DailyReport(
        [FromQuery] DateOnly? date,
        [FromQuery] bool openOnly = false,
        [FromQuery] string? checkoutTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var report = await _attendanceService.GetDailyReportAsync(reportDate, openOnly, checkoutTypeFilter, cancellationToken);
        return Ok(ApiResponse<DailyAttendanceReportDto>.Ok(report));
    }

    /// <summary>Monthly attendance summary per member.</summary>
    [HttpGet("reports/forgot-check-out")]
    [RequirePermission(Permissions.ViewAttendance)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<ForgotCheckOutReportItemDto>>>> ForgotCheckOutReport(
        [FromQuery] ForgotCheckOutReportQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetForgotCheckOutReportAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<ForgotCheckOutReportItemDto>>.Ok(result));
    }

    /// <summary>Monthly attendance summary per member.</summary>
    [HttpGet("reports/monthly")]
    [RequirePermission(Permissions.ViewAttendance)]
    public async Task<ActionResult<ApiResponse<MonthlyAttendanceReportDto>>> MonthlyReport(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var report = await _attendanceService.GetMonthlyReportAsync(
            year ?? now.Year, month ?? now.Month, cancellationToken);
        return Ok(ApiResponse<MonthlyAttendanceReportDto>.Ok(report));
    }

    /// <summary>Export daily report as PDF.</summary>
    [HttpGet("reports/daily/export/pdf")]
    [RequirePermission(Permissions.ExportAttendanceReports)]
    public async Task<IActionResult> ExportDailyPdf(
        [FromQuery] DateOnly? date,
        [FromQuery] bool openOnly = false,
        [FromQuery] string? checkoutTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var report = await _attendanceService.GetDailyReportAsync(reportDate, openOnly, checkoutTypeFilter, cancellationToken);
        var bytes = _reportExporter.ExportDailyReportPdf(report);
        return File(bytes, "application/pdf", $"daily-attendance-{reportDate:yyyy-MM-dd}.pdf");
    }

    /// <summary>Export daily report as Excel.</summary>
    [HttpGet("reports/daily/export/excel")]
    [RequirePermission(Permissions.ExportAttendanceReports)]
    public async Task<IActionResult> ExportDailyExcel(
        [FromQuery] DateOnly? date,
        [FromQuery] bool openOnly = false,
        [FromQuery] string? checkoutTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var report = await _attendanceService.GetDailyReportAsync(reportDate, openOnly, checkoutTypeFilter, cancellationToken);
        var bytes = _reportExporter.ExportDailyReportExcel(report);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"daily-attendance-{reportDate:yyyy-MM-dd}.xlsx");
    }

    /// <summary>Export monthly report as PDF.</summary>
    [HttpGet("reports/monthly/export/pdf")]
    [RequirePermission(Permissions.ExportAttendanceReports)]
    public async Task<IActionResult> ExportMonthlyPdf(
        [FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;
        var report = await _attendanceService.GetMonthlyReportAsync(y, m, cancellationToken);
        var bytes = _reportExporter.ExportMonthlyReportPdf(report);
        return File(bytes, "application/pdf", $"monthly-attendance-{y}-{m:D2}.pdf");
    }

    /// <summary>Export monthly report as Excel.</summary>
    [HttpGet("reports/monthly/export/excel")]
    [RequirePermission(Permissions.ExportAttendanceReports)]
    public async Task<IActionResult> ExportMonthlyExcel(
        [FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var mo = month ?? now.Month;
        var report = await _attendanceService.GetMonthlyReportAsync(y, mo, cancellationToken);
        var bytes = _reportExporter.ExportMonthlyReportExcel(report);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"monthly-attendance-{y}-{mo:D2}.xlsx");
    }

    /// <summary>Export member history as Excel.</summary>
    [HttpGet("members/{memberId:int}/history/export/excel")]
    [RequirePermission(Permissions.ExportAttendanceReports)]
    public async Task<IActionResult> ExportMemberHistoryExcel(
        int memberId,
        [FromQuery] AttendanceQueryDto query,
        CancellationToken cancellationToken)
    {
        query.PageNumber = 1;
        query.PageSize = 5000;
        var result = await _attendanceService.GetMemberHistoryAsync(memberId, query, cancellationToken);
        var name = result.Items.FirstOrDefault()?.MemberName ?? $"member-{memberId}";
        var bytes = _reportExporter.ExportMemberHistoryExcel(result.Items, name);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"member-attendance-{memberId}.xlsx");
    }

    /// <summary>Trainer check-in.</summary>
    [HttpPost("trainers/check-in")]
    [RequirePermission(Permissions.ManageTrainerAttendance)]
    public async Task<ActionResult<ApiResponse<TrainerAttendanceDto>>> TrainerCheckIn(
        [FromBody] TrainerCheckInDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.TrainerCheckInAsync(dto, cancellationToken);
        return Ok(ApiResponse<TrainerAttendanceDto>.Ok(result, "Trainer checked in."));
    }

    /// <summary>Trainer check-out.</summary>
    [HttpPost("trainers/check-out")]
    [RequirePermission(Permissions.ManageTrainerAttendance)]
    public async Task<ActionResult<ApiResponse<TrainerAttendanceDto>>> TrainerCheckOut(
        [FromBody] TrainerCheckInDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.TrainerCheckOutAsync(dto.TrainerId, cancellationToken);
        return Ok(ApiResponse<TrainerAttendanceDto>.Ok(result, "Trainer checked out."));
    }

    /// <summary>Trainer attendance list (paged).</summary>
    [HttpGet("trainers")]
    [RequirePermission(Permissions.ViewTrainerAttendance)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<TrainerAttendanceDto>>>> GetTrainerAttendance(
        [FromQuery] int? trainerId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var to = toDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _attendanceService.GetTrainerAttendanceAsync(
            trainerId, from, to, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<TrainerAttendanceDto>>.Ok(result));
    }
}
