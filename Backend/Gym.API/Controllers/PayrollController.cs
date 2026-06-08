using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Financial;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/payroll")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService) => _payrollService = payrollService;

    [HttpGet]
    [RequirePermission(Permissions.ViewPayroll)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<PayrollDto>>>> GetPaged(
        [FromQuery] PayrollSearchQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _payrollService.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<PayrollDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ViewPayroll)]
    public async Task<ActionResult<ApiResponse<PayrollDto>>> GetById(
        int id, [FromQuery] Guid? gymId, CancellationToken cancellationToken)
    {
        var item = await _payrollService.GetByIdAsync(id, gymId, cancellationToken);
        return Ok(ApiResponse<PayrollDto>.Ok(item));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.ManagePayroll)]
    public async Task<ActionResult<ApiResponse<PayrollDto>>> Update(
        int id, [FromBody] UpdatePayrollDto dto, CancellationToken cancellationToken)
    {
        var item = await _payrollService.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<PayrollDto>.Ok(item, "Payroll updated."));
    }

    [HttpPost("generate")]
    [RequirePermission(Permissions.ManagePayroll)]
    public async Task<ActionResult<ApiResponse<GeneratePayrollResultDto>>> Generate(
        [FromBody] GeneratePayrollDto dto, CancellationToken cancellationToken)
    {
        var result = await _payrollService.GenerateAsync(dto, cancellationToken);
        return Ok(ApiResponse<GeneratePayrollResultDto>.Ok(result, "Payroll generated."));
    }

    [HttpPost("{id:int}/approve")]
    [RequirePermission(Permissions.ManagePayroll)]
    public async Task<ActionResult<ApiResponse<object>>> Approve(
        int id, [FromQuery] Guid? gymId, CancellationToken cancellationToken)
    {
        await _payrollService.ApproveAsync(id, gymId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Payroll approved."));
    }

    [HttpPost("{id:int}/pay")]
    [RequirePermission(Permissions.ManagePayroll)]
    public async Task<ActionResult<ApiResponse<object>>> Pay(
        int id, [FromQuery] Guid? gymId, CancellationToken cancellationToken)
    {
        await _payrollService.PayAsync(id, gymId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Payroll marked as paid."));
    }

    [HttpPost("commissions")]
    [RequirePermission(Permissions.ManagePayroll)]
    public async Task<ActionResult<ApiResponse<TrainerCommissionDto>>> CreateCommission(
        [FromBody] CreateTrainerCommissionDto dto, CancellationToken cancellationToken)
    {
        var item = await _payrollService.CreateCommissionAsync(dto, cancellationToken);
        return Ok(ApiResponse<TrainerCommissionDto>.Ok(item, "Commission recorded."));
    }

    [HttpGet("commissions")]
    [RequirePermission(Permissions.ViewPayroll)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TrainerCommissionDto>>>> GetCommissions(
        [FromQuery] FinancialReportQueryDto query, CancellationToken cancellationToken)
    {
        var items = await _payrollService.GetCommissionsAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TrainerCommissionDto>>.Ok(items));
    }

    [HttpGet("commissions/me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TrainerCommissionDto>>>> GetMyCommissions(
        [FromQuery] FinancialReportQueryDto query, CancellationToken cancellationToken)
    {
        var items = await _payrollService.GetMyCommissionsAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TrainerCommissionDto>>.Ok(items));
    }

    [HttpGet("export/pdf")]
    [RequirePermission(Permissions.ViewPayroll)]
    public async Task<IActionResult> ExportPdf([FromQuery] FinancialReportQueryDto query, CancellationToken cancellationToken)
    {
        var bytes = await _payrollService.ExportPdfAsync(query, cancellationToken);
        return File(bytes, "application/pdf", $"payroll-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet("export/excel")]
    [RequirePermission(Permissions.ViewPayroll)]
    public async Task<IActionResult> ExportExcel([FromQuery] FinancialReportQueryDto query, CancellationToken cancellationToken)
    {
        var bytes = await _payrollService.ExportExcelAsync(query, cancellationToken);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"payroll-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
