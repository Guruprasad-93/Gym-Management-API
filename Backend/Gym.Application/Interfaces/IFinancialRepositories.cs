using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Financial;

namespace Gym.Application.Interfaces;

public interface IExpenseRepository
{
    Task SeedCategoriesAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<ExpenseDto> CreateAsync(Guid gymId, Guid? createdBy, CreateExpenseDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int expenseId, Guid gymId, UpdateExpenseDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int expenseId, Guid gymId, CancellationToken cancellationToken = default);
    Task<ExpenseDto?> GetByIdAsync(int expenseId, Guid gymId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<ExpenseDto>> GetPagedAsync(Guid gymId, ExpenseSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseDto>> SearchAllAsync(Guid gymId, ExpenseSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<ExpenseDashboardDto> GetDashboardAsync(Guid? gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseCategoryBreakdownDto>> GetCategoryBreakdownAsync(Guid? gymId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}

public interface IPayrollRepository
{
    Task<PayrollDto?> GetByIdAsync(int payrollId, Guid gymId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<PayrollDto>> GetPagedAsync(Guid gymId, PayrollSearchQueryDto query, CancellationToken cancellationToken = default);
    Task UpdateAsync(int payrollId, Guid gymId, UpdatePayrollDto dto, CancellationToken cancellationToken = default);
    Task<int> GenerateMonthlyAsync(Guid gymId, DateOnly salaryMonth, decimal trainerSalary, decimal staffSalary, Guid? createdBy, CancellationToken cancellationToken = default);
    Task ApproveAsync(int payrollId, Guid gymId, CancellationToken cancellationToken = default);
    Task PayAsync(int payrollId, Guid gymId, CancellationToken cancellationToken = default);
    Task<int> CreateCommissionAsync(Guid gymId, Guid? createdBy, CreateTrainerCommissionDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainerCommissionDto>> GetCommissionReportAsync(Guid? gymId, int? trainerId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
    Task<PayrollDashboardDto> GetDashboardAsync(Guid? gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrendPointDto>> GetPayrollCostTrendAsync(Guid? gymId, int months, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrendPointDto>> GetCommissionTrendAsync(Guid? gymId, int months, CancellationToken cancellationToken = default);
}

public interface IFinancialAnalyticsRepository
{
    Task<(decimal RevenueThisMonth, decimal CommissionsThisMonth)> GetRevenueSummaryAsync(Guid? gymId, CancellationToken cancellationToken = default);
    Task<ProfitLossSummaryDto> GetProfitLossAsync(Guid? gymId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlyProfitPointDto>> GetMonthlyProfitTrendAsync(Guid? gymId, int months, CancellationToken cancellationToken = default);
}

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<ExpenseDto> CreateAsync(CreateExpenseDto dto, CancellationToken cancellationToken = default);
    Task<ExpenseDto> UpdateAsync(int id, UpdateExpenseDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<ExpenseDto> GetByIdAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<PagedResultDto<ExpenseDto>> GetPagedAsync(ExpenseSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<byte[]> ExportPdfAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default);
    Task<byte[]> ExportExcelAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default);
}

public interface IPayrollService
{
    Task<PayrollDto> GetByIdAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<PagedResultDto<PayrollDto>> GetPagedAsync(PayrollSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<PayrollDto> UpdateAsync(int id, UpdatePayrollDto dto, CancellationToken cancellationToken = default);
    Task<GeneratePayrollResultDto> GenerateAsync(GeneratePayrollDto dto, CancellationToken cancellationToken = default);
    Task ApproveAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default);
    Task PayAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<TrainerCommissionDto> CreateCommissionAsync(CreateTrainerCommissionDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainerCommissionDto>> GetCommissionsAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainerCommissionDto>> GetMyCommissionsAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default);
    Task<byte[]> ExportPdfAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default);
    Task<byte[]> ExportExcelAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default);
}

public interface IFinancialAnalyticsService
{
    Task<FinancialDashboardDto> GetDashboardAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<ProfitLossSummaryDto> GetProfitLossAsync(Guid? gymId = null, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default);
    Task<byte[]> ExportProfitLossPdfAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default);
    Task<byte[]> ExportProfitLossExcelAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default);
}

public interface IFinancialReportExporter
{
    byte[] ExportExpenseReportPdf(IReadOnlyList<ExpenseDto> expenses, string title);
    byte[] ExportExpenseLedgerExcel(IReadOnlyList<ExpenseDto> expenses, string title);
    byte[] ExportPayrollReportPdf(IReadOnlyList<PayrollDto> payrolls, string title);
    byte[] ExportPayrollLedgerExcel(IReadOnlyList<PayrollDto> payrolls, string title);
    byte[] ExportProfitLossPdf(FinancialDashboardDto dashboard, string title);
    byte[] ExportProfitLossExcel(FinancialDashboardDto dashboard, string title);
}
