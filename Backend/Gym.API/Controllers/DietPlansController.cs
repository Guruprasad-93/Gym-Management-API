using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.DietPlans;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/diet-plans")]
[Authorize]
public class DietPlansController : ControllerBase
{
    private readonly IDietPlanService _dietPlanService;
    private readonly IDietPlanReportExporter _exporter;

    public DietPlansController(IDietPlanService dietPlanService, IDietPlanReportExporter exporter)
    {
        _dietPlanService = dietPlanService;
        _exporter = exporter;
    }

    [HttpGet("categories")]
    [RequirePermission(Permissions.ViewDietPlans)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DietCategoryDto>>>> GetCategories(
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var categories = await _dietPlanService.GetCategoriesAsync(includeInactive, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DietCategoryDto>>.Ok(categories));
    }

    [HttpPost("categories")]
    [RequirePermission(Permissions.ManageDietPlans)]
    public async Task<ActionResult<ApiResponse<DietCategoryDto>>> CreateCategory(
        [FromBody] CreateDietCategoryDto dto,
        CancellationToken cancellationToken)
    {
        var category = await _dietPlanService.CreateCategoryAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<DietCategoryDto>.Ok(category, "Category created."));
    }

    [HttpGet]
    [RequirePermission(Permissions.ViewDietPlans)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DietPlanListDto>>>> GetPlans(
        [FromQuery] bool includeInactive,
        [FromQuery] int? categoryId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var plans = await _dietPlanService.GetPlansAsync(includeInactive, categoryId, search, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DietPlanListDto>>.Ok(plans));
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ViewDietPlans)]
    public async Task<ActionResult<ApiResponse<DietPlanDetailDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var plan = await _dietPlanService.GetPlanByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<DietPlanDetailDto>.Ok(plan));
    }

    [HttpPost]
    [RequirePermission(Permissions.ManageDietPlans)]
    public async Task<ActionResult<ApiResponse<DietPlanDetailDto>>> Create(
        [FromBody] CreateDietPlanDto dto,
        CancellationToken cancellationToken)
    {
        var plan = await _dietPlanService.CreatePlanAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<DietPlanDetailDto>.Ok(plan, "Diet plan created."));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.ManageDietPlans)]
    public async Task<ActionResult<ApiResponse<DietPlanDetailDto>>> Update(
        int id,
        [FromBody] UpdateDietPlanDto dto,
        CancellationToken cancellationToken)
    {
        var plan = await _dietPlanService.UpdatePlanAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<DietPlanDetailDto>.Ok(plan, "Diet plan updated."));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ManageDietPlans)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _dietPlanService.DeletePlanAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Diet plan deleted."));
    }

    [HttpPatch("{id:int}/active")]
    [RequirePermission(Permissions.ManageDietPlans)]
    public async Task<ActionResult<ApiResponse<object>>> SetActive(
        int id,
        [FromQuery] bool isActive,
        CancellationToken cancellationToken)
    {
        await _dietPlanService.SetActiveAsync(id, isActive, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Status updated."));
    }

    [HttpPost("{id:int}/clone")]
    [RequirePermission(Permissions.ManageDietPlans)]
    public async Task<ActionResult<ApiResponse<DietPlanDetailDto>>> Clone(
        int id,
        [FromBody] CloneDietPlanDto? dto,
        CancellationToken cancellationToken)
    {
        var plan = await _dietPlanService.ClonePlanAsync(id, dto ?? new CloneDietPlanDto(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<DietPlanDetailDto>.Ok(plan, "Diet plan cloned."));
    }

    [HttpPost("assign")]
    [RequirePermission(Permissions.AssignDietPlan)]
    public async Task<ActionResult<ApiResponse<MemberDietPlanViewDto>>> Assign(
        [FromBody] AssignDietPlanDto dto,
        CancellationToken cancellationToken)
    {
        var view = await _dietPlanService.AssignToMemberAsync(dto, cancellationToken);
        return Ok(ApiResponse<MemberDietPlanViewDto>.Ok(view, "Diet plan assigned."));
    }

    [HttpDelete("assignments/{assignedId:int}")]
    [RequirePermission(Permissions.AssignDietPlan)]
    public async Task<ActionResult<ApiResponse<object>>> Unassign(int assignedId, CancellationToken cancellationToken)
    {
        await _dietPlanService.UnassignAsync(assignedId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Assignment ended."));
    }

    [HttpGet("members/me")]
    [RequirePermission(Permissions.ViewMemberDiet)]
    public async Task<ActionResult<ApiResponse<MemberDietPlanViewDto>>> GetMyDiet(CancellationToken cancellationToken)
    {
        var view = await _dietPlanService.GetCurrentMemberDietAsync(cancellationToken);
        return Ok(ApiResponse<MemberDietPlanViewDto>.Ok(view));
    }

    [HttpGet("members/{memberId:int}")]
    [RequireAnyPermission(Permissions.ViewMemberDiet, Permissions.ViewMembers)]
    public async Task<ActionResult<ApiResponse<MemberDietPlanViewDto>>> GetMemberDiet(
        int memberId,
        CancellationToken cancellationToken)
    {
        var view = await _dietPlanService.GetMemberDietAsync(memberId, cancellationToken);
        return Ok(ApiResponse<MemberDietPlanViewDto>.Ok(view));
    }

    [HttpGet("members/{memberId:int}/assignments")]
    [RequireAnyPermission(Permissions.ViewMemberDiet, Permissions.ViewMembers)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MemberDietAssignmentDto>>>> GetMemberAssignments(
        int memberId,
        CancellationToken cancellationToken)
    {
        var list = await _dietPlanService.GetMemberAssignmentsAsync(memberId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MemberDietAssignmentDto>>.Ok(list));
    }

    [HttpGet("{id:int}/export/pdf")]
    [RequirePermission(Permissions.ExportDietPlans)]
    public async Task<IActionResult> ExportPdf(int id, CancellationToken cancellationToken)
    {
        var plan = await _dietPlanService.GetPlanForExportAsync(id, cancellationToken);
        var bytes = _exporter.ExportPdf(plan);
        return File(bytes, "application/pdf", $"diet-plan-{id}.pdf");
    }

    [HttpGet("{id:int}/export/excel")]
    [RequirePermission(Permissions.ExportDietPlans)]
    public async Task<IActionResult> ExportExcel(int id, CancellationToken cancellationToken)
    {
        var plan = await _dietPlanService.GetPlanForExportAsync(id, cancellationToken);
        var bytes = _exporter.ExportExcel(plan);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"diet-plan-{id}.xlsx");
    }
}
