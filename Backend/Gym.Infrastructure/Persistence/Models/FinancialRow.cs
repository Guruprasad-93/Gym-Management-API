namespace Gym.Infrastructure.Persistence.Models;

internal sealed class ExpenseRow
{
    public int ExpenseId { get; set; }
    public Guid GymId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string? VendorName { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public long? AttachmentFileId { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

internal sealed class ExpenseCategoryRow
{
    public int CategoryId { get; set; }
    public Guid GymId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

internal sealed class PayrollRow
{
    public int PayrollId { get; set; }
    public Guid GymId { get; set; }
    public string EmployeeType { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public Guid? EmployeeUserId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime SalaryMonth { get; set; }
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

internal sealed class TrainerCommissionRow
{
    public int CommissionId { get; set; }
    public Guid GymId { get; set; }
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int? MemberId { get; set; }
    public string? MemberName { get; set; }
    public int? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedDate { get; set; }
}

internal sealed class ExpenseDashboardRow
{
    public decimal ExpensesThisMonth { get; set; }
    public decimal TotalExpenses { get; set; }
    public int ExpenseCountThisMonth { get; set; }
}

internal sealed class PayrollDashboardRow
{
    public decimal PayrollCostThisMonth { get; set; }
    public decimal PendingSalaries { get; set; }
    public int PaidCountThisMonth { get; set; }
}

internal sealed class ProfitLossRow
{
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal PayrollCost { get; set; }
    public decimal TrainerCommissions { get; set; }
    public decimal Profit { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

internal sealed class MonthlyProfitRow
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal PayrollCost { get; set; }
}

internal sealed class ExpenseBreakdownRow
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

internal sealed class TrendPointRow
{
    public string MonthLabel { get; set; } = string.Empty;
    public decimal PayrollCost { get; set; }
    public decimal CommissionTotal { get; set; }
    public decimal Value { get; set; }
}

internal sealed class FinancialRevenueRow
{
    public decimal RevenueThisMonth { get; set; }
    public decimal CommissionsThisMonth { get; set; }
}
