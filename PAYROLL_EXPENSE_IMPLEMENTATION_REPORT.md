# Payroll, Expense & Profit/Loss Module — Implementation Report

## Overview

Full financial management for gym owners: expense tracking, payroll generation/approval/payment, trainer commissions, and profit & loss analytics with PDF/Excel exports.

## Database

**Script:** `Backend/Gym.Infrastructure/Persistence/Scripts/028_PayrollExpenseManagementModule.sql`

| Table | Purpose |
|-------|---------|
| `ExpenseCategories` | Per-gym expense categories (8 seeded defaults) |
| `Expenses` | Expense records with optional bill attachment |
| `Payrolls` | Monthly salary records (trainers + gym admins) |
| `TrainerCommissions` | Commission entries linked to payments/members |

**Seeded categories:** Rent, Electricity, Water, Equipment Purchase, Equipment Maintenance, Marketing, Staff Salary, Miscellaneous

**Payroll statuses:** Draft → Approved → Paid

**Employee types:** Trainer (EmployeeId = TrainerId), GymAdmin (EmployeeUserId = User Id)

## Permissions

| Permission | GymAdmin | SuperAdmin | Trainer |
|------------|----------|------------|---------|
| `VIEW_EXPENSES` | ✓ | ✓ | — |
| `MANAGE_EXPENSES` | ✓ | ✓ | — |
| `VIEW_PAYROLL` | ✓ | ✓ | — |
| `MANAGE_PAYROLL` | ✓ | ✓ | — |
| `VIEW_FINANCIAL_ANALYTICS` | ✓ | ✓ | — |

Trainers view own commissions via `GET /api/payroll/commissions/me` (role-scoped, no full payroll access).

## API Endpoints

### Expenses (`/api/expenses`)
| Method | Route | Permission |
|--------|-------|------------|
| GET | `/categories` | VIEW_EXPENSES |
| GET | `/` | VIEW_EXPENSES |
| GET | `/{id}` | VIEW_EXPENSES |
| POST | `/` | MANAGE_EXPENSES |
| PUT | `/{id}` | MANAGE_EXPENSES |
| DELETE | `/{id}` | MANAGE_EXPENSES |
| GET | `/export/pdf`, `/export/excel` | VIEW_FINANCIAL_ANALYTICS |

### Payroll (`/api/payroll`)
| Method | Route | Permission |
|--------|-------|------------|
| GET | `/` | VIEW_PAYROLL |
| GET | `/{id}` | VIEW_PAYROLL |
| PUT | `/{id}` | MANAGE_PAYROLL |
| POST | `/generate` | MANAGE_PAYROLL |
| POST | `/{id}/approve` | MANAGE_PAYROLL |
| POST | `/{id}/pay` | MANAGE_PAYROLL |
| POST | `/commissions` | MANAGE_PAYROLL |
| GET | `/commissions` | VIEW_PAYROLL |
| GET | `/commissions/me` | Trainer (own) |
| GET | `/export/pdf`, `/export/excel` | VIEW_PAYROLL |

### Financial (`/api/financial`)
| Method | Route | Permission |
|--------|-------|------------|
| GET | `/dashboard` | VIEW_FINANCIAL_ANALYTICS |
| GET | `/profit-loss` | VIEW_FINANCIAL_ANALYTICS |
| GET | `/export/pdf`, `/export/excel` | VIEW_FINANCIAL_ANALYTICS |

## Analytics KPIs

- Revenue This Month (from completed payments)
- Expenses This Month
- Profit This Month (revenue − expenses − payroll cost)
- Pending Salaries (Draft + Approved payroll)
- Total Trainer Commissions

## Charts

- Monthly profit trend (revenue, expenses, profit)
- Expense category breakdown
- Payroll cost trend
- Trainer commission trend

## Key Backend Files

- DTOs: `Gym.Application/DTOs/Financial/FinancialDtos.cs`
- Services: `ExpenseService`, `PayrollService`, `FinancialAnalyticsService`
- Repositories: `ExpenseRepository`, `PayrollRepository`, `FinancialAnalyticsRepository`
- Controllers: `ExpensesController`, `PayrollController`, `FinancialController`
- Exporter: `FinancialReportExporter.cs`

## Frontend Routes

| Route | Feature |
|-------|---------|
| `/gym-admin/expenses` | Expense list, create/edit, attachment file ID |
| `/gym-admin/payroll` | Payroll list, generate, approve, pay |
| `/gym-admin/financial` | P&L dashboard with charts and exports |

## Audit Events

- Expense created / updated / deleted
- Payroll generated / approved / paid
- Trainer commission created
- Financial report exports

## Deployment

1. Run `028_PayrollExpenseManagementModule.sql` (auto via migrator)
2. Restart API (seeder adds financial permissions)
3. Re-login users to refresh JWT permissions

## Testing

`Backend/Gym.API.IntegrationTests/FinancialManagementTests.cs` — expense CRUD, payroll generation, financial analytics, tenant isolation.

## Build Status

- Backend: `dotnet build GymManagementSaaS.sln` — succeeded
- Frontend: `npm run build` — succeeded

## Notes

- Bill attachments use existing `Files` table via `AttachmentFileId`
- `GenerateMonthlyPayroll` auto-includes trainer commissions for the salary month
- Super Admin analytics accepts optional `gymId`; omit for platform-wide metrics
- Profit calculation: `Revenue − Expenses − Paid Payroll` in P&L summary SP
