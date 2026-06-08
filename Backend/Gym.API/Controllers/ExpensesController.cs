using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Financial;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService) => _expenseService = expenseService;

    [HttpGet("categories")]
    [RequirePermission(Permissions.ViewExpenses)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ExpenseCategoryDto>>>> GetCategories(
        [FromQuery] Guid? gymId, CancellationToken cancellationToken)
    {
        var items = await _expenseService.GetCategoriesAsync(gymId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ExpenseCategoryDto>>.Ok(items));
    }

    [HttpGet]
    [RequirePermission(Permissions.ViewExpenses)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<ExpenseDto>>>> GetPaged(
        [FromQuery] ExpenseSearchQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _expenseService.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<ExpenseDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ViewExpenses)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> GetById(
        int id, [FromQuery] Guid? gymId, CancellationToken cancellationToken)
    {
        var item = await _expenseService.GetByIdAsync(id, gymId, cancellationToken);
        return Ok(ApiResponse<ExpenseDto>.Ok(item));
    }

    [HttpPost]
    [RequirePermission(Permissions.ManageExpenses)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> Create(
        [FromBody] CreateExpenseDto dto, CancellationToken cancellationToken)
    {
        var item = await _expenseService.CreateAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ExpenseDto>.Ok(item, "Expense created."));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.ManageExpenses)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> Update(
        int id, [FromBody] UpdateExpenseDto dto, CancellationToken cancellationToken)
    {
        var item = await _expenseService.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<ExpenseDto>.Ok(item, "Expense updated."));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ManageExpenses)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        int id, [FromQuery] Guid? gymId, CancellationToken cancellationToken)
    {
        await _expenseService.DeleteAsync(id, gymId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Expense deleted."));
    }

    [HttpGet("export/pdf")]
    [RequirePermission(Permissions.ViewFinancialAnalytics)]
    public async Task<IActionResult> ExportPdf([FromQuery] FinancialReportQueryDto query, CancellationToken cancellationToken)
    {
        var bytes = await _expenseService.ExportPdfAsync(query, cancellationToken);
        return File(bytes, "application/pdf", $"expenses-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet("export/excel")]
    [RequirePermission(Permissions.ViewFinancialAnalytics)]
    public async Task<IActionResult> ExportExcel([FromQuery] FinancialReportQueryDto query, CancellationToken cancellationToken)
    {
        var bytes = await _expenseService.ExportExcelAsync(query, cancellationToken);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"expenses-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
