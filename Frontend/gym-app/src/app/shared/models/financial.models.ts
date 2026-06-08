export interface ExpenseCategory {
  id: number;
  gymId: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface Expense {
  id: number;
  gymId: string;
  categoryId: number;
  categoryName: string;
  amount: number;
  expenseDate: string;
  description?: string;
  vendorName?: string;
  paymentMethod: string;
  attachmentFileId?: number;
  createdDate: string;
}

export interface CreateExpenseRequest {
  gymId?: string;
  categoryId: number;
  amount: number;
  expenseDate: string;
  description?: string;
  vendorName?: string;
  paymentMethod: string;
  attachmentFileId?: number;
}

export interface ExpenseSearchQuery {
  gymId?: string;
  search?: string;
  categoryId?: number;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface Payroll {
  id: number;
  gymId: string;
  employeeType: string;
  employeeId: number;
  employeeName: string;
  salaryMonth: string;
  baseSalary: number;
  incentiveAmount: number;
  commissionAmount: number;
  deductionAmount: number;
  netSalary: number;
  status: string;
  paidDate?: string;
  createdDate: string;
}

export interface GeneratePayrollRequest {
  gymId?: string;
  salaryMonth: string;
  defaultTrainerBaseSalary?: number;
  defaultStaffBaseSalary?: number;
}

export interface UpdatePayrollRequest {
  gymId?: string;
  baseSalary: number;
  incentiveAmount: number;
  commissionAmount: number;
  deductionAmount: number;
}

export interface TrainerCommission {
  id: number;
  gymId: string;
  trainerId: number;
  trainerName: string;
  memberId?: number;
  memberName?: string;
  paymentId?: number;
  amount: number;
  createdDate: string;
}

export interface FinancialDashboard {
  revenueThisMonth: number;
  expensesThisMonth: number;
  profitThisMonth: number;
  pendingSalaries: number;
  totalTrainerCommissions: number;
  summary: ProfitLossSummary;
  monthlyProfitTrend: MonthlyProfitPoint[];
  expenseBreakdown: CategoryBreakdown[];
  payrollCostTrend: TrendPoint[];
  commissionTrend: TrendPoint[];
}

export interface ProfitLossSummary {
  revenue: number;
  expenses: number;
  payrollCost: number;
  trainerCommissions: number;
  profit: number;
  fromDate: string;
  toDate: string;
}

export interface MonthlyProfitPoint {
  year: number;
  month: number;
  monthLabel: string;
  revenue: number;
  expenses: number;
  payrollCost: number;
  profit: number;
}

export interface CategoryBreakdown {
  name: string;
  amount: number;
  count: number;
}

export interface TrendPoint {
  monthLabel: string;
  value: number;
}

export const EXPENSE_PAYMENT_METHODS = ['Cash', 'Card', 'UPI', 'BankTransfer', 'Other'] as const;
export const PAYROLL_STATUSES = ['Draft', 'Approved', 'Paid'] as const;
