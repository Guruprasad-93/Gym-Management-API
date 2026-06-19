using System.Data;
using Dapper;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Financial;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public ExpenseRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public Task SeedCategoriesAsync(Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SeedExpenseCategories, new { GymId = gymId }, cancellationToken);

    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<ExpenseCategoryRow>(
            StoredProcedureNames.GetExpenseCategories, new { GymId = gymId }, cancellationToken);
        return rows.Select(r => new ExpenseCategoryDto
        {
            Id = r.CategoryId, GymId = r.GymId, Name = r.Name, Description = r.Description, IsActive = r.IsActive
        }).ToList();
    }

    public async Task<ExpenseDto> CreateAsync(Guid gymId, Guid? createdBy, CreateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@CategoryId", dto.CategoryId);
        parameters.Add("@Amount", dto.Amount);
        parameters.Add("@ExpenseDate", dto.ExpenseDate);
        parameters.Add("@Description", dto.Description);
        parameters.Add("@VendorName", dto.VendorName);
        parameters.Add("@PaymentMethod", dto.PaymentMethod);
        parameters.Add("@AttachmentFileId", dto.AttachmentFileId);
        parameters.Add("@CreatedBy", createdBy);
        parameters.Add("@ExpenseId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.CreateExpense, parameters, cancellationToken);
        return (await GetByIdAsync(parameters.Get<int>("@ExpenseId"), gymId, cancellationToken))!;
    }

    public Task UpdateAsync(int expenseId, Guid gymId, UpdateExpenseDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateExpense, new
        {
            ExpenseId = expenseId,
            GymId = gymId,
            dto.CategoryId,
            dto.Amount,
            dto.ExpenseDate,
            dto.Description,
            dto.VendorName,
            dto.PaymentMethod,
            dto.AttachmentFileId
        }, cancellationToken);

    public Task DeleteAsync(int expenseId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteExpense, new { ExpenseId = expenseId, GymId = gymId }, cancellationToken);

    public async Task<ExpenseDto?> GetByIdAsync(int expenseId, Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<ExpenseRow>(
            StoredProcedureNames.GetExpenseById, new { ExpenseId = expenseId, GymId = gymId }, cancellationToken);
        return row is null ? null : MapExpense(row);
    }

    public async Task<PagedResultDto<ExpenseDto>> GetPagedAsync(Guid gymId, ExpenseSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@Search", query.Search);
        parameters.Add("@CategoryId", query.CategoryId);
        parameters.Add("@FromDate", query.FromDate);
        parameters.Add("@ToDate", query.ToDate);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@SortColumn", query.SortColumn);
        parameters.Add("@SortDirection", query.SortDirection);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rows = await _sp.QueryAsync<ExpenseRow>(StoredProcedureNames.GetExpensesPaged, parameters, cancellationToken);
        return new PagedResultDto<ExpenseDto>
        {
            Items = rows.Select(MapExpense).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<IReadOnlyList<ExpenseDto>> SearchAllAsync(Guid gymId, ExpenseSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        query.PageNumber = 1;
        query.PageSize = 5000;
        return (await GetPagedAsync(gymId, query, cancellationToken)).Items;
    }

    public async Task<ExpenseDashboardDto> GetDashboardAsync(Guid? gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<ExpenseDashboardRow>(
            StoredProcedureNames.GetExpenseDashboard, new { GymId = gymId }, cancellationToken);
        return new ExpenseDashboardDto
        {
            ExpensesThisMonth = row?.ExpensesThisMonth ?? 0,
            TotalExpenses = row?.TotalExpenses ?? 0,
            ExpenseCountThisMonth = row?.ExpenseCountThisMonth ?? 0
        };
    }

    public async Task<IReadOnlyList<ExpenseCategoryBreakdownDto>> GetCategoryBreakdownAsync(
        Guid? gymId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<ExpenseBreakdownRow>(
            StoredProcedureNames.GetExpenseCategoryBreakdown, new { GymId = gymId, FromDate = from, ToDate = to }, cancellationToken);
        return rows.Select(r => new ExpenseCategoryBreakdownDto { Name = r.Name, Amount = r.Amount, Count = r.Count }).ToList();
    }

    private static ExpenseDto MapExpense(ExpenseRow r) => new()
    {
        Id = r.ExpenseId, GymId = r.GymId, CategoryId = r.CategoryId, CategoryName = r.CategoryName,
        Amount = r.Amount, ExpenseDate = DateOnly.FromDateTime(r.ExpenseDate),
        Description = r.Description, VendorName = r.VendorName, PaymentMethod = r.PaymentMethod,
        AttachmentFileId = r.AttachmentFileId, CreatedBy = r.CreatedBy,
        CreatedDate = r.CreatedDate, UpdatedDate = r.UpdatedDate
    };
}
