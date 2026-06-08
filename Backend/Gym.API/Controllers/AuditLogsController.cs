using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

/// <summary>Search and export system audit logs.</summary>
[ApiController]
[Route("api/audit-logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly IAuditReportExporter _exporter;

    public AuditLogsController(IAuditService auditService, IAuditReportExporter exporter)
    {
        _auditService = auditService;
        _exporter = exporter;
    }

    [HttpGet("dashboard")]
    [RequirePermission(Permissions.ViewAuditLogs)]
    [ProducesResponseType(typeof(ApiResponse<AuditDashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuditDashboardDto>>> GetDashboard(
        [FromQuery] Guid? gymId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var dashboard = await _auditService.GetDashboardAsync(gymId, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<AuditDashboardDto>.Ok(dashboard));
    }

    [HttpGet]
    [RequirePermission(Permissions.ViewAuditLogs)]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<AuditLogDto>>>> Search(
        [FromQuery] AuditSearchQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _auditService.SearchAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<AuditLogDto>>.Ok(result));
    }

    [HttpGet("export/pdf")]
    [RequirePermission(Permissions.ExportAuditLogs)]
    public async Task<IActionResult> ExportPdf([FromQuery] AuditSearchQueryDto query, CancellationToken cancellationToken)
    {
        var logs = await _auditService.GetExportDataAsync(query, cancellationToken);
        var bytes = _exporter.ExportPdf(logs, "Audit Log Report");
        return File(bytes, "application/pdf", $"audit-logs-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet("export/excel")]
    [RequirePermission(Permissions.ExportAuditLogs)]
    public async Task<IActionResult> ExportExcel([FromQuery] AuditSearchQueryDto query, CancellationToken cancellationToken)
    {
        var logs = await _auditService.GetExportDataAsync(query, cancellationToken);
        var bytes = _exporter.ExportExcel(logs, "Audit Log Report");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"audit-logs-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
