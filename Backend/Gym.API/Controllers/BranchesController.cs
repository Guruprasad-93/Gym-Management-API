using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Branches;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/branches")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService) => _branchService = branchService;

    [HttpGet]
    [RequirePermission(Permissions.ViewBranches)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<BranchDto>>>> GetPaged([FromQuery] BranchSearchQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _branchService.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<BranchDto>>.Ok(result));
    }

    [HttpGet("list")]
    [RequirePermission(Permissions.ViewBranches)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BranchDto>>>> GetAll([FromQuery] Guid? gymId, [FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var result = await _branchService.GetAllAsync(gymId, includeInactive, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BranchDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ViewBranches)]
    public async Task<ActionResult<ApiResponse<BranchDto>>> GetById(int id, [FromQuery] Guid? gymId, CancellationToken cancellationToken)
    {
        var branch = await _branchService.GetByIdAsync(id, gymId, cancellationToken);
        if (branch is null) return NotFound(ApiResponse<BranchDto>.Fail("Branch not found."));
        return Ok(ApiResponse<BranchDto>.Ok(branch));
    }

    [HttpPost]
    [RequirePermission(Permissions.ManageBranches)]
    public async Task<ActionResult<ApiResponse<BranchDto>>> Create([FromBody] CreateBranchDto dto, CancellationToken cancellationToken)
    {
        var branch = await _branchService.CreateAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<BranchDto>.Ok(branch, "Branch created."));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.ManageBranches)]
    public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] UpdateBranchDto dto, CancellationToken cancellationToken)
    {
        await _branchService.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Branch updated."));
    }

    [HttpPatch("{id:int}/activate")]
    [RequirePermission(Permissions.ManageBranches)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id, CancellationToken cancellationToken)
    {
        await _branchService.SetActiveAsync(id, true, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Branch activated."));
    }

    [HttpPatch("{id:int}/deactivate")]
    [RequirePermission(Permissions.ManageBranches)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id, CancellationToken cancellationToken)
    {
        await _branchService.SetActiveAsync(id, false, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Branch deactivated."));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ManageBranches)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _branchService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Branch deleted."));
    }

    [HttpPost("{id:int}/manager")]
    [RequirePermission(Permissions.ManageBranches)]
    public async Task<ActionResult<ApiResponse<object>>> AssignManager(int id, [FromBody] AssignBranchManagerDto dto, CancellationToken cancellationToken)
    {
        await _branchService.AssignManagerAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Branch manager assigned."));
    }

    [HttpGet("dashboard")]
    [RequirePermission(Permissions.ViewBranches)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BranchDashboardItemDto>>>> GetDashboard([FromQuery] int? branchId, CancellationToken cancellationToken)
    {
        var result = await _branchService.GetDashboardAsync(branchId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BranchDashboardItemDto>>.Ok(result));
    }

    [HttpGet("analytics")]
    [RequirePermission(Permissions.ViewBranchAnalytics)]
    public async Task<ActionResult<ApiResponse<BranchAnalyticsDto>>> GetAnalytics([FromQuery] int months = 6, CancellationToken cancellationToken = default)
    {
        var result = await _branchService.GetAnalyticsAsync(months, cancellationToken);
        return Ok(ApiResponse<BranchAnalyticsDto>.Ok(result));
    }

    [HttpGet("transfers")]
    [RequirePermission(Permissions.ViewBranches)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<BranchTransferDto>>>> GetTransfers([FromQuery] BranchTransferQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _branchService.GetTransferHistoryAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<BranchTransferDto>>.Ok(result));
    }

    [HttpPost("transfers/members")]
    [RequirePermission(Permissions.TransferMembers)]
    public async Task<ActionResult<ApiResponse<int>>> TransferMember([FromBody] TransferMemberBranchDto dto, CancellationToken cancellationToken)
    {
        var transferId = await _branchService.TransferMemberAsync(dto, cancellationToken);
        return Ok(ApiResponse<int>.Ok(transferId, "Member transferred."));
    }

    [HttpPost("transfers/trainers")]
    [RequirePermission(Permissions.TransferTrainers)]
    public async Task<ActionResult<ApiResponse<int>>> TransferTrainer([FromBody] TransferTrainerBranchDto dto, CancellationToken cancellationToken)
    {
        var transferId = await _branchService.TransferTrainerAsync(dto, cancellationToken);
        return Ok(ApiResponse<int>.Ok(transferId, "Trainer transferred."));
    }

    [HttpGet("targets")]
    [RequirePermission(Permissions.ViewBranches)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BranchTargetDto>>>> GetTargets([FromQuery] int? branchId, [FromQuery] DateOnly? targetMonth, CancellationToken cancellationToken)
    {
        var result = await _branchService.GetTargetsAsync(branchId, targetMonth, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BranchTargetDto>>.Ok(result));
    }

    [HttpPost("targets")]
    [RequirePermission(Permissions.ManageBranches)]
    public async Task<ActionResult<ApiResponse<BranchTargetDto>>> UpsertTarget([FromBody] UpsertBranchTargetDto dto, CancellationToken cancellationToken)
    {
        var result = await _branchService.UpsertTargetAsync(dto, cancellationToken);
        return Ok(ApiResponse<BranchTargetDto>.Ok(result, "Target saved."));
    }

    [HttpGet("announcements")]
    [RequirePermission(Permissions.ViewBranches)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BranchAnnouncementDto>>>> GetAnnouncements([FromQuery] int? branchId, [FromQuery] string? audience, CancellationToken cancellationToken)
    {
        var result = await _branchService.GetAnnouncementsAsync(branchId, audience, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BranchAnnouncementDto>>.Ok(result));
    }

    [HttpPost("announcements")]
    [RequirePermission(Permissions.ManageBranches)]
    public async Task<ActionResult<ApiResponse<BranchAnnouncementDto>>> CreateAnnouncement([FromBody] CreateBranchAnnouncementDto dto, CancellationToken cancellationToken)
    {
        var result = await _branchService.CreateAnnouncementAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<BranchAnnouncementDto>.Ok(result, "Announcement published."));
    }

    [HttpDelete("announcements/{id:int}")]
    [RequirePermission(Permissions.ManageBranches)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAnnouncement(int id, CancellationToken cancellationToken)
    {
        await _branchService.DeleteAnnouncementAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Announcement deleted."));
    }
}
