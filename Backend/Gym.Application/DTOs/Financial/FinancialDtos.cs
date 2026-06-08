namespace Gym.Application.DTOs.Financial;

public class ExpenseCategoryDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class ExpenseDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string? VendorName { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public long? AttachmentFileId { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public class CreateExpenseDto
{
    public Guid? GymId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string? VendorName { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public long? AttachmentFileId { get; set; }
}

public class UpdateExpenseDto : CreateExpenseDto { }

public class ExpenseSearchQueryDto
{
    public Guid? GymId { get; set; }
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortColumn { get; set; } = "ExpenseDate";
    public string SortDirection { get; set; } = "DESC";
}

public class PayrollDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public string EmployeeType { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public Guid? EmployeeUserId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly SalaryMonth { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal IncentiveAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal DeductionAmount { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public class UpdatePayrollDto
{
    public Guid? GymId { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal IncentiveAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal DeductionAmount { get; set; }
}

public class GeneratePayrollDto
{
    public Guid? GymId { get; set; }
    public DateOnly SalaryMonth { get; set; }
    public decimal DefaultTrainerBaseSalary { get; set; } = 15000;
    public decimal DefaultStaffBaseSalary { get; set; } = 25000;
}

public class GeneratePayrollResultDto
{
    public int GeneratedCount { get; set; }
    public DateOnly SalaryMonth { get; set; }
}

public class PayrollSearchQueryDto
{
    public Guid? GymId { get; set; }
    public DateOnly? SalaryMonth { get; set; }
    public string? Status { get; set; }
    public string? EmployeeType { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class CreateTrainerCommissionDto
{
    public Guid? GymId { get; set; }
    public int TrainerId { get; set; }
    public int? MemberId { get; set; }
    public int? PaymentId { get; set; }
    public decimal Amount { get; set; }
}

public class TrainerCommissionDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int? MemberId { get; set; }
    public string? MemberName { get; set; }
    public int? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class ExpenseDashboardDto
{
    public decimal ExpensesThisMonth { get; set; }
    public decimal TotalExpenses { get; set; }
    public int ExpenseCountThisMonth { get; set; }
}

public class PayrollDashboardDto
{
    public decimal PayrollCostThisMonth { get; set; }
    public decimal PendingSalaries { get; set; }
    public int PaidCountThisMonth { get; set; }
}

public class ProfitLossSummaryDto
{
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal PayrollCost { get; set; }
    public decimal TrainerCommissions { get; set; }
    public decimal Profit { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
}

public class MonthlyProfitPointDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal PayrollCost { get; set; }
    public decimal Profit => Revenue - Expenses - PayrollCost;
}

public class ExpenseCategoryBreakdownDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class TrendPointDto
{
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class FinancialDashboardDto
{
    public decimal RevenueThisMonth { get; set; }
    public decimal ExpensesThisMonth { get; set; }
    public decimal ProfitThisMonth { get; set; }
    public decimal PendingSalaries { get; set; }
    public decimal TotalTrainerCommissions { get; set; }
    public ProfitLossSummaryDto Summary { get; set; } = new();
    public ExpenseDashboardDto ExpenseDashboard { get; set; } = new();
    public PayrollDashboardDto PayrollDashboard { get; set; } = new();
    public IReadOnlyList<MonthlyProfitPointDto> MonthlyProfitTrend { get; set; } = Array.Empty<MonthlyProfitPointDto>();
    public IReadOnlyList<ExpenseCategoryBreakdownDto> ExpenseBreakdown { get; set; } = Array.Empty<ExpenseCategoryBreakdownDto>();
    public IReadOnlyList<TrendPointDto> PayrollCostTrend { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyList<TrendPointDto> CommissionTrend { get; set; } = Array.Empty<TrendPointDto>();
}

public class FinancialReportQueryDto
{
    public Guid? GymId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public DateOnly? SalaryMonth { get; set; }
}
