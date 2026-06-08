using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.MemberSelfService;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/member")]
[Authorize]
public class MemberSelfServiceController : ControllerBase
{
    private readonly IMemberSelfService _memberSelfService;

    public MemberSelfServiceController(IMemberSelfService memberSelfService) => _memberSelfService = memberSelfService;

    [HttpGet("dashboard")]
    [RequirePermission(Permissions.ViewMemberDashboard)]
    public async Task<ActionResult<ApiResponse<MemberSelfServiceDashboardDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.GetDashboardAsync(cancellationToken);
        return Ok(ApiResponse<MemberSelfServiceDashboardDto>.Ok(result));
    }

    [HttpGet("analytics")]
    [RequirePermission(Permissions.ViewMemberDashboard)]
    public async Task<ActionResult<ApiResponse<MemberSelfServiceAnalyticsDto>>> GetAnalytics(CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.GetAnalyticsAsync(cancellationToken);
        return Ok(ApiResponse<MemberSelfServiceAnalyticsDto>.Ok(result));
    }

    [HttpGet("goals")]
    [RequirePermission(Permissions.ManageMemberGoals)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MemberGoalDto>>>> GetGoals([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.GetGoalsAsync(status, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MemberGoalDto>>.Ok(result));
    }

    [HttpPost("goals")]
    [RequirePermission(Permissions.ManageMemberGoals)]
    public async Task<ActionResult<ApiResponse<MemberGoalDto>>> CreateGoal([FromBody] CreateMemberGoalDto dto, CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.CreateGoalAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<MemberGoalDto>.Ok(result, "Goal created."));
    }

    [HttpPut("goals/{goalId:int}")]
    [RequirePermission(Permissions.ManageMemberGoals)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateGoal(int goalId, [FromBody] UpdateMemberGoalDto dto, CancellationToken cancellationToken)
    {
        await _memberSelfService.UpdateGoalAsync(goalId, dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Goal updated."));
    }

    [HttpPatch("goals/{goalId:int}/progress")]
    [RequirePermission(Permissions.ManageMemberGoals)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateGoalProgress(int goalId, [FromBody] UpdateGoalProgressDto dto, CancellationToken cancellationToken)
    {
        await _memberSelfService.UpdateGoalProgressAsync(goalId, dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Goal progress updated."));
    }

    [HttpPost("goals/{goalId:int}/complete")]
    [RequirePermission(Permissions.ManageMemberGoals)]
    public async Task<ActionResult<ApiResponse<object>>> CompleteGoal(int goalId, CancellationToken cancellationToken)
    {
        await _memberSelfService.CompleteGoalAsync(goalId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Goal completed."));
    }

    [HttpGet("progress")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<ActionResult<ApiResponse<ProgressTrendDto>>> GetProgress([FromQuery] ProgressQueryDto query, CancellationToken cancellationToken)
    {
        var trends = await _memberSelfService.GetProgressTrendsAsync(query, cancellationToken);
        return Ok(ApiResponse<ProgressTrendDto>.Ok(trends));
    }

    [HttpPost("progress")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<ActionResult<ApiResponse<MemberProgressEntryDto>>> CreateProgress([FromBody] CreateMemberProgressDto dto, CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.CreateProgressAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<MemberProgressEntryDto>.Ok(result, "Progress recorded."));
    }

    [HttpGet("progress/history")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MemberProgressEntryDto>>>> GetProgressHistory([FromQuery] ProgressQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.GetProgressHistoryAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MemberProgressEntryDto>>.Ok(result));
    }

    [HttpGet("progress/photos")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MemberProgressPhotoDto>>>> GetProgressPhotos(CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.GetProgressPhotosAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MemberProgressPhotoDto>>.Ok(result));
    }

    [HttpPost("progress/photos")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<ActionResult<ApiResponse<MemberProgressPhotoDto>>> CreateProgressPhoto([FromBody] CreateProgressPhotoDto dto, CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.CreateProgressPhotoAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<MemberProgressPhotoDto>.Ok(result, "Photo linked."));
    }

    [HttpGet("workouts")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkoutTrackingDto>>>> GetWorkouts([FromQuery] ProgressQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.GetWorkoutHistoryAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<WorkoutTrackingDto>>.Ok(result));
    }

    [HttpPost("workouts")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<ActionResult<ApiResponse<WorkoutTrackingDto>>> UpsertWorkout([FromBody] UpsertWorkoutTrackingDto dto, CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.UpsertWorkoutTrackingAsync(dto, cancellationToken);
        return Ok(ApiResponse<WorkoutTrackingDto>.Ok(result, "Workout tracked."));
    }

    [HttpGet("workouts/streak")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<ActionResult<ApiResponse<int>>> GetWorkoutStreak(CancellationToken cancellationToken)
    {
        var streak = await _memberSelfService.GetWorkoutStreakAsync(cancellationToken);
        return Ok(ApiResponse<int>.Ok(streak));
    }

    [HttpGet("diets")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DietTrackingDto>>>> GetDiets([FromQuery] ProgressQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.GetDietHistoryAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DietTrackingDto>>.Ok(result));
    }

    [HttpGet("diets/compliance")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<ActionResult<ApiResponse<DietComplianceSummaryDto>>> GetDietCompliance(CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.GetDietComplianceAsync(cancellationToken);
        return Ok(ApiResponse<DietComplianceSummaryDto>.Ok(result));
    }

    [HttpPost("diets")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<ActionResult<ApiResponse<DietTrackingDto>>> UpsertDiet([FromBody] UpsertDietTrackingDto dto, CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.UpsertDietTrackingAsync(dto, cancellationToken);
        return Ok(ApiResponse<DietTrackingDto>.Ok(result, "Diet tracked."));
    }

    [HttpGet("water")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<ActionResult<ApiResponse<WaterIntakeDto?>>> GetTodayWater(CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.GetTodayWaterIntakeAsync(cancellationToken);
        return Ok(ApiResponse<WaterIntakeDto?>.Ok(result));
    }

    [HttpPost("water")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<ActionResult<ApiResponse<WaterIntakeDto>>> UpsertWater([FromBody] UpsertWaterIntakeDto dto, CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.UpsertWaterIntakeAsync(dto, cancellationToken);
        return Ok(ApiResponse<WaterIntakeDto>.Ok(result, "Water intake saved."));
    }

    [HttpGet("referrals")]
    [RequirePermission(Permissions.ViewMemberDashboard)]
    public async Task<ActionResult<ApiResponse<ReferralStatsDto>>> GetReferrals(CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.GetReferralsAsync(cancellationToken);
        return Ok(ApiResponse<ReferralStatsDto>.Ok(result));
    }

    [HttpGet("feedback")]
    [RequirePermission(Permissions.SubmitMemberFeedback)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MemberFeedbackDto>>>> GetFeedback(CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.GetFeedbackAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MemberFeedbackDto>>.Ok(result));
    }

    [HttpPost("feedback")]
    [RequirePermission(Permissions.SubmitMemberFeedback)]
    public async Task<ActionResult<ApiResponse<MemberFeedbackDto>>> SubmitFeedback([FromBody] CreateMemberFeedbackDto dto, CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.SubmitFeedbackAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<MemberFeedbackDto>.Ok(result, "Feedback submitted."));
    }

    [HttpGet("qr-code")]
    [RequirePermission(Permissions.ViewMemberDashboard)]
    public async Task<ActionResult<ApiResponse<MemberQrCodeDto>>> GetQrCode(CancellationToken cancellationToken)
    {
        var result = await _memberSelfService.GetQrCodeAsync(cancellationToken);
        return Ok(ApiResponse<MemberQrCodeDto>.Ok(result));
    }

    [HttpPost("attendance/qr-scan")]
    [RequirePermission(Permissions.ManageAttendance)]
    public async Task<ActionResult<ApiResponse<int>>> ScanQrCheckIn([FromBody] QrCheckInDto dto, CancellationToken cancellationToken)
    {
        var attendanceId = await _memberSelfService.ScanQrCheckInAsync(dto, cancellationToken);
        return Ok(ApiResponse<int>.Ok(attendanceId, "Member checked in via QR."));
    }

    [HttpGet("progress/export/pdf")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<IActionResult> ExportProgressPdf(CancellationToken cancellationToken)
    {
        var bytes = await _memberSelfService.ExportProgressPdfAsync(cancellationToken);
        return File(bytes, "application/pdf", $"member-progress-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet("attendance/export/pdf")]
    [RequirePermission(Permissions.TrackMemberProgress)]
    public async Task<IActionResult> ExportAttendancePdf(CancellationToken cancellationToken)
    {
        var bytes = await _memberSelfService.ExportAttendancePdfAsync(cancellationToken);
        return File(bytes, "application/pdf", $"member-attendance-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet("goals/export/pdf")]
    [RequirePermission(Permissions.ManageMemberGoals)]
    public async Task<IActionResult> ExportGoalsPdf(CancellationToken cancellationToken)
    {
        var bytes = await _memberSelfService.ExportGoalSummaryPdfAsync(cancellationToken);
        return File(bytes, "application/pdf", $"member-goals-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
