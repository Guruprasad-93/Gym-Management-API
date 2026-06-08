using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Financial;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/financial")]
[Authorize]
public class FinancialController : ControllerBase
{
    private readonly IFinancialAnalyticsService _financialService;

    public FinancialController(IFinancialAnalyticsService financialService) => _financialService = financialService;

    [HttpGet("dashboard")]
    [RequirePermission(Permissions.ViewFinancialAnalytics)]
    public async Task<ActionResult<ApiResponse<FinancialDashboardDto>>> GetDashboard(
        [FromQuery] Guid? gymId, CancellationToken cancellationToken)
    {
        var dashboard = await _financialService.GetDashboardAsync(gymId, cancellationToken);
        return Ok(ApiResponse<FinancialDashboardDto>.Ok(dashboard));
    }

    [HttpGet("profit-loss")]
    [RequirePermission(Permissions.ViewFinancialAnalytics)]
    public async Task<ActionResult<ApiResponse<ProfitLossSummaryDto>>> GetProfitLoss(
        [FromQuery] Guid? gymId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var summary = await _financialService.GetProfitLossAsync(gymId, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<ProfitLossSummaryDto>.Ok(summary));
    }

    [HttpGet("export/pdf")]
    [RequirePermission(Permissions.ViewFinancialAnalytics)]
    public async Task<IActionResult> ExportPdf([FromQuery] FinancialReportQueryDto query, CancellationToken cancellationToken)
    {
        var bytes = await _financialService.ExportProfitLossPdfAsync(query, cancellationToken);
        return File(bytes, "application/pdf", $"profit-loss-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet("export/excel")]
    [RequirePermission(Permissions.ViewFinancialAnalytics)]
    public async Task<IActionResult> ExportExcel([FromQuery] FinancialReportQueryDto query, CancellationToken cancellationToken)
    {
        var bytes = await _financialService.ExportProfitLossExcelAsync(query, cancellationToken);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"profit-loss-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
