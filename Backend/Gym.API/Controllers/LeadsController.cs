using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Leads;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/leads")]
[Authorize]
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;

    public LeadsController(ILeadService leadService) => _leadService = leadService;

    [HttpGet("dashboard")]
    [RequirePermission(Permissions.ViewLeads)]
    [ProducesResponseType(typeof(ApiResponse<LeadDashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadDashboardDto>>> GetDashboard(
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var dashboard = await _leadService.GetDashboardAsync(gymId, cancellationToken);
        return Ok(ApiResponse<LeadDashboardDto>.Ok(dashboard));
    }

    [HttpGet("analytics")]
    [RequirePermission(Permissions.ViewLeadAnalytics)]
    [ProducesResponseType(typeof(ApiResponse<LeadAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadAnalyticsDto>>> GetAnalytics(
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var analytics = await _leadService.GetAnalyticsAsync(gymId, cancellationToken);
        return Ok(ApiResponse<LeadAnalyticsDto>.Ok(analytics));
    }

    [HttpGet("followups/pending")]
    [RequirePermission(Permissions.ViewLeads)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LeadFollowUpDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LeadFollowUpDto>>>> GetPendingFollowUps(
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var items = await _leadService.GetPendingFollowUpsAsync(gymId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<LeadFollowUpDto>>.Ok(items));
    }

    [HttpGet("trials/today")]
    [RequirePermission(Permissions.ViewLeads)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LeadTrialDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LeadTrialDto>>>> GetTodaysTrials(
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var items = await _leadService.GetTodaysTrialsAsync(gymId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<LeadTrialDto>>.Ok(items));
    }

    [HttpGet]
    [RequirePermission(Permissions.ViewLeads)]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<LeadDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<LeadDto>>>> GetPaged(
        [FromQuery] LeadSearchQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _leadService.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<LeadDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ViewLeads)]
    [ProducesResponseType(typeof(ApiResponse<LeadDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadDetailDto>>> GetById(
        int id,
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var detail = await _leadService.GetDetailAsync(id, gymId, cancellationToken);
        return Ok(ApiResponse<LeadDetailDto>.Ok(detail));
    }

    [HttpPost]
    [RequirePermission(Permissions.ManageLeads)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<LeadDto>>> Create(
        [FromBody] CreateLeadDto dto,
        CancellationToken cancellationToken)
    {
        var lead = await _leadService.CreateAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<LeadDto>.Ok(lead, "Lead created successfully."));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.ManageLeads)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadDto>>> Update(
        int id,
        [FromBody] UpdateLeadDto dto,
        CancellationToken cancellationToken)
    {
        var lead = await _leadService.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<LeadDto>.Ok(lead, "Lead updated successfully."));
    }

    [HttpPatch("{id:int}/status")]
    [RequirePermission(Permissions.ManageLeads)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadDto>>> UpdateStatus(
        int id,
        [FromBody] UpdateLeadStatusDto dto,
        CancellationToken cancellationToken)
    {
        var lead = await _leadService.UpdateStatusAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<LeadDto>.Ok(lead, "Lead status updated."));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ManageLeads)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        int id,
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        await _leadService.DeleteAsync(id, gymId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Lead deleted successfully."));
    }

    [HttpPost("assign-trainer")]
    [RequirePermission(Permissions.ManageLeads)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadDto>>> AssignTrainer(
        [FromBody] AssignTrainerToLeadDto dto,
        CancellationToken cancellationToken)
    {
        var lead = await _leadService.AssignTrainerAsync(dto, cancellationToken);
        return Ok(ApiResponse<LeadDto>.Ok(lead, "Trainer assigned successfully."));
    }

    [HttpPost("schedule-trial")]
    [RequirePermission(Permissions.ManageLeads)]
    [ProducesResponseType(typeof(ApiResponse<LeadTrialDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadTrialDto>>> ScheduleTrial(
        [FromBody] ScheduleTrialDto dto,
        CancellationToken cancellationToken)
    {
        var trial = await _leadService.ScheduleTrialAsync(dto, cancellationToken);
        return Ok(ApiResponse<LeadTrialDto>.Ok(trial, "Trial scheduled successfully."));
    }

    [HttpPost("followup")]
    [RequirePermission(Permissions.ManageLeads)]
    [ProducesResponseType(typeof(ApiResponse<LeadFollowUpDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadFollowUpDto>>> CreateFollowUp(
        [FromBody] CreateLeadFollowUpDto dto,
        CancellationToken cancellationToken)
    {
        var followUp = await _leadService.CreateFollowUpAsync(dto, cancellationToken);
        return Ok(ApiResponse<LeadFollowUpDto>.Ok(followUp, "Follow-up created successfully."));
    }

    [HttpPut("followup/{followUpId:int}")]
    [RequirePermission(Permissions.ManageLeads)]
    [ProducesResponseType(typeof(ApiResponse<LeadFollowUpDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadFollowUpDto>>> UpdateFollowUp(
        int followUpId,
        [FromBody] UpdateLeadFollowUpDto dto,
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var followUp = await _leadService.UpdateFollowUpAsync(followUpId, dto, gymId, cancellationToken);
        return Ok(ApiResponse<LeadFollowUpDto>.Ok(followUp, "Follow-up updated successfully."));
    }

    [HttpPost("trial-feedback")]
    [RequireAnyPermission(Permissions.ManageLeads, Permissions.ViewLeads)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> RecordTrialFeedback(
        [FromBody] RecordTrialFeedbackDto dto,
        CancellationToken cancellationToken)
    {
        await _leadService.RecordTrialFeedbackAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Trial feedback recorded."));
    }

    [HttpPost("convert-member")]
    [RequirePermission(Permissions.ConvertLeads)]
    [ProducesResponseType(typeof(ApiResponse<ConvertLeadResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ConvertLeadResultDto>>> ConvertToMember(
        [FromBody] ConvertLeadToMemberDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _leadService.ConvertToMemberAsync(dto, cancellationToken);
        return Ok(ApiResponse<ConvertLeadResultDto>.Ok(result, "Lead converted to member successfully."));
    }

    [HttpGet("export/pdf")]
    [RequirePermission(Permissions.ViewLeadAnalytics)]
    public async Task<IActionResult> ExportPdf(
        [FromQuery] string reportType = "summary",
        [FromQuery] LeadSearchQueryDto query = null!,
        CancellationToken cancellationToken = default)
    {
        query ??= new LeadSearchQueryDto();
        var bytes = await _leadService.ExportPdfAsync(reportType, query, cancellationToken);
        return File(bytes, "application/pdf", $"leads-{reportType}-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet("export/excel")]
    [RequirePermission(Permissions.ViewLeadAnalytics)]
    public async Task<IActionResult> ExportExcel(
        [FromQuery] string reportType = "summary",
        [FromQuery] LeadSearchQueryDto query = null!,
        CancellationToken cancellationToken = default)
    {
        query ??= new LeadSearchQueryDto();
        var bytes = await _leadService.ExportExcelAsync(reportType, query, cancellationToken);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"leads-{reportType}-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
