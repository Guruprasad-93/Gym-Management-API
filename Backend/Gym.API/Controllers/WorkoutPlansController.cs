using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.WorkoutPlans;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/workout-plans")]
[Authorize]
public class WorkoutPlansController : ControllerBase
{
    private readonly IWorkoutPlanService _workoutPlanService;
    private readonly IWorkoutPlanReportExporter _exporter;

    public WorkoutPlansController(IWorkoutPlanService workoutPlanService, IWorkoutPlanReportExporter exporter)
    {
        _workoutPlanService = workoutPlanService;
        _exporter = exporter;
    }

    [HttpGet("exercise-categories")]
    [RequirePermission(Permissions.ViewWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ExerciseCategoryDto>>>> GetCategories(
        [FromQuery] bool includeInactive, CancellationToken cancellationToken)
    {
        var list = await _workoutPlanService.GetCategoriesAsync(includeInactive, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ExerciseCategoryDto>>.Ok(list));
    }

    [HttpPost("exercise-categories")]
    [RequirePermission(Permissions.ManageWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<ExerciseCategoryDto>>> CreateCategory(
        [FromBody] CreateExerciseCategoryDto dto, CancellationToken cancellationToken)
    {
        var cat = await _workoutPlanService.CreateCategoryAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ExerciseCategoryDto>.Ok(cat));
    }

    [HttpGet("exercises")]
    [RequirePermission(Permissions.ViewWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ExerciseDto>>>> GetExercises(
        [FromQuery] bool includeInactive,
        [FromQuery] int? categoryId,
        [FromQuery] string? muscleGroup,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var list = await _workoutPlanService.GetExercisesAsync(includeInactive, categoryId, muscleGroup, search, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ExerciseDto>>.Ok(list));
    }

    [HttpGet("exercises/{id:int}")]
    [RequirePermission(Permissions.ViewWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<ExerciseDto>>> GetExercise(int id, CancellationToken cancellationToken)
    {
        var ex = await _workoutPlanService.GetExerciseByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ExerciseDto>.Ok(ex));
    }

    [HttpPost("exercises")]
    [RequirePermission(Permissions.ManageWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<ExerciseDto>>> CreateExercise(
        [FromBody] CreateExerciseDto dto, CancellationToken cancellationToken)
    {
        var ex = await _workoutPlanService.CreateExerciseAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ExerciseDto>.Ok(ex));
    }

    [HttpPut("exercises/{id:int}")]
    [RequirePermission(Permissions.ManageWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<ExerciseDto>>> UpdateExercise(
        int id, [FromBody] UpdateExerciseDto dto, CancellationToken cancellationToken)
    {
        var ex = await _workoutPlanService.UpdateExerciseAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<ExerciseDto>.Ok(ex));
    }

    [HttpDelete("exercises/{id:int}")]
    [RequirePermission(Permissions.ManageWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteExercise(int id, CancellationToken cancellationToken)
    {
        await _workoutPlanService.DeleteExerciseAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Exercise deleted."));
    }

    [HttpGet]
    [RequirePermission(Permissions.ViewWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkoutPlanListDto>>>> GetPlans(
        [FromQuery] bool includeInactive, [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var plans = await _workoutPlanService.GetPlansAsync(includeInactive, search, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<WorkoutPlanListDto>>.Ok(plans));
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ViewWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<WorkoutPlanDetailDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var plan = await _workoutPlanService.GetPlanByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<WorkoutPlanDetailDto>.Ok(plan));
    }

    [HttpPost]
    [RequirePermission(Permissions.ManageWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<WorkoutPlanDetailDto>>> Create(
        [FromBody] CreateWorkoutPlanDto dto, CancellationToken cancellationToken)
    {
        var plan = await _workoutPlanService.CreatePlanAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<WorkoutPlanDetailDto>.Ok(plan));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.ManageWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<WorkoutPlanDetailDto>>> Update(
        int id, [FromBody] UpdateWorkoutPlanDto dto, CancellationToken cancellationToken)
    {
        var plan = await _workoutPlanService.UpdatePlanAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<WorkoutPlanDetailDto>.Ok(plan));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ManageWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _workoutPlanService.DeletePlanAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Workout plan deleted."));
    }

    [HttpPatch("{id:int}/active")]
    [RequirePermission(Permissions.ManageWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<object>>> SetActive(int id, [FromQuery] bool isActive, CancellationToken cancellationToken)
    {
        await _workoutPlanService.SetActiveAsync(id, isActive, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpPost("{id:int}/clone")]
    [RequirePermission(Permissions.ManageWorkoutPlans)]
    public async Task<ActionResult<ApiResponse<WorkoutPlanDetailDto>>> Clone(
        int id, [FromBody] CloneWorkoutPlanDto? dto, CancellationToken cancellationToken)
    {
        var plan = await _workoutPlanService.ClonePlanAsync(id, dto ?? new CloneWorkoutPlanDto(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<WorkoutPlanDetailDto>.Ok(plan));
    }

    [HttpPost("assign")]
    [RequirePermission(Permissions.AssignWorkoutPlan)]
    public async Task<ActionResult<ApiResponse<MemberWorkoutPlanViewDto>>> Assign(
        [FromBody] AssignWorkoutPlanDto dto, CancellationToken cancellationToken)
    {
        var view = await _workoutPlanService.AssignToMemberAsync(dto, cancellationToken);
        return Ok(ApiResponse<MemberWorkoutPlanViewDto>.Ok(view));
    }

    [HttpPost("progress")]
    [RequireAnyPermission(Permissions.ViewMemberWorkout, Permissions.AssignWorkoutPlan)]
    public async Task<ActionResult<ApiResponse<WorkoutPlanExerciseDto>>> UpdateProgress(
        [FromBody] UpdateWorkoutProgressDto dto, CancellationToken cancellationToken)
    {
        var ex = await _workoutPlanService.UpdateProgressAsync(dto, cancellationToken);
        return Ok(ApiResponse<WorkoutPlanExerciseDto>.Ok(ex));
    }

    [HttpGet("members/me")]
    [RequirePermission(Permissions.ViewMemberWorkout)]
    public async Task<ActionResult<ApiResponse<MemberWorkoutPlanViewDto>>> GetMyWorkout(CancellationToken cancellationToken)
    {
        var view = await _workoutPlanService.GetCurrentMemberWorkoutAsync(cancellationToken);
        return Ok(ApiResponse<MemberWorkoutPlanViewDto>.Ok(view));
    }

    [HttpGet("members/{memberId:int}")]
    [RequireAnyPermission(Permissions.ViewMemberWorkout, Permissions.ViewMembers)]
    public async Task<ActionResult<ApiResponse<MemberWorkoutPlanViewDto>>> GetMemberWorkout(
        int memberId, CancellationToken cancellationToken)
    {
        var view = await _workoutPlanService.GetMemberWorkoutAsync(memberId, cancellationToken);
        return Ok(ApiResponse<MemberWorkoutPlanViewDto>.Ok(view));
    }

    [HttpGet("{id:int}/export/pdf")]
    [RequirePermission(Permissions.ExportWorkoutPlans)]
    public async Task<IActionResult> ExportPdf(int id, CancellationToken cancellationToken)
    {
        var plan = await _workoutPlanService.GetPlanForExportAsync(id, cancellationToken);
        return File(_exporter.ExportPdf(plan), "application/pdf", $"workout-plan-{id}.pdf");
    }

    [HttpGet("{id:int}/export/excel")]
    [RequirePermission(Permissions.ExportWorkoutPlans)]
    public async Task<IActionResult> ExportExcel(int id, CancellationToken cancellationToken)
    {
        var plan = await _workoutPlanService.GetPlanForExportAsync(id, cancellationToken);
        return File(_exporter.ExportExcel(plan), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"workout-plan-{id}.xlsx");
    }
}
