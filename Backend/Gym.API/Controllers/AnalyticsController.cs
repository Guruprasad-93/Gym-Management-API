using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Analytics;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService) => _analyticsService = analyticsService;

    [HttpGet("dashboard")]
    [RequirePermission(Permissions.ViewAnalytics)]
    [ProducesResponseType(typeof(ApiResponse<AnalyticsDashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AnalyticsDashboardDto>>> GetDashboard(
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var dashboard = await _analyticsService.GetDashboardAsync(gymId, cancellationToken);
        return Ok(ApiResponse<AnalyticsDashboardDto>.Ok(dashboard));
    }

    [HttpGet("revenue")]
    [RequirePermission(Permissions.ViewRevenueAnalytics)]
    [ProducesResponseType(typeof(ApiResponse<RevenueAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RevenueAnalyticsDto>>> GetRevenue(
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var revenue = await _analyticsService.GetRevenueAsync(gymId, cancellationToken);
        return Ok(ApiResponse<RevenueAnalyticsDto>.Ok(revenue));
    }

    [HttpGet("members")]
    [RequirePermission(Permissions.ViewMemberAnalytics)]
    [ProducesResponseType(typeof(ApiResponse<MembershipAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MembershipAnalyticsDto>>> GetMembers(
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var members = await _analyticsService.GetMembershipAsync(gymId, cancellationToken);
        return Ok(ApiResponse<MembershipAnalyticsDto>.Ok(members));
    }

    [HttpGet("attendance")]
    [RequirePermission(Permissions.ViewAnalytics)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AttendanceAnalyticsDto>>> GetAttendance(
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var attendance = await _analyticsService.GetAttendanceAsync(gymId, cancellationToken);
        return Ok(ApiResponse<AttendanceAnalyticsDto>.Ok(attendance));
    }

    [HttpGet("trainers")]
    [RequirePermission(Permissions.ViewAnalytics)]
    [ProducesResponseType(typeof(ApiResponse<TrainerAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TrainerAnalyticsDto>>> GetTrainers(
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var trainers = await _analyticsService.GetTrainersAsync(gymId, cancellationToken);
        return Ok(ApiResponse<TrainerAnalyticsDto>.Ok(trainers));
    }

    [HttpGet("workouts")]
    [RequirePermission(Permissions.ViewAnalytics)]
    [ProducesResponseType(typeof(ApiResponse<WorkoutAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WorkoutAnalyticsDto>>> GetWorkouts(
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var workouts = await _analyticsService.GetWorkoutsAsync(gymId, cancellationToken);
        return Ok(ApiResponse<WorkoutAnalyticsDto>.Ok(workouts));
    }

    [HttpGet("diets")]
    [RequirePermission(Permissions.ViewAnalytics)]
    [ProducesResponseType(typeof(ApiResponse<DietAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DietAnalyticsDto>>> GetDiets(
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var diets = await _analyticsService.GetDietsAsync(gymId, cancellationToken);
        return Ok(ApiResponse<DietAnalyticsDto>.Ok(diets));
    }

    [HttpGet("export/pdf")]
    [RequirePermission(Permissions.ViewAnalytics)]
    public async Task<IActionResult> ExportPdf(
        [FromQuery] string reportType = "dashboard",
        [FromQuery] Guid? gymId = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = await _analyticsService.ExportPdfAsync(reportType, gymId, cancellationToken);
        return File(bytes, "application/pdf", $"analytics-{reportType}-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet("export/excel")]
    [RequirePermission(Permissions.ViewAnalytics)]
    public async Task<IActionResult> ExportExcel(
        [FromQuery] string reportType = "dashboard",
        [FromQuery] Guid? gymId = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = await _analyticsService.ExportExcelAsync(reportType, gymId, cancellationToken);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"analytics-{reportType}-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
