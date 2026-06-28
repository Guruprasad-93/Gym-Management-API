# Reports & Analytics

## Module Overview
Business intelligence: revenue, members, attendance, trainers, financial P&L, branch analytics, exports.

## Navigation
| Report area | Route |
|-------------|-------|
| Revenue analytics | `/gym-admin/analytics/revenue` |
| Member analytics | `/gym-admin/analytics/members` |
| Attendance analytics | `/gym-admin/analytics/attendance` |
| Trainer analytics | `/gym-admin/analytics/trainers` |
| Financial dashboard | `/gym-admin/financial` |
| Expenses | `/gym-admin/expenses` |
| Payroll | `/gym-admin/payroll` |
| Attendance reports | `/gym-admin/attendance/reports` |
| Lead analytics | `/gym-admin/leads/analytics` |
| Booking analytics | `/gym-admin/booking-analytics` |
| Website analytics | `/gym-admin/website-builder/analytics` |
| Audit export | `/gym-admin/audit` |

## Buttons
- Date/branch filters per page
- Export PDF / Excel where implemented (`export/pdf`, `export/excel` API routes)

## APIs
`AnalyticsController`, `FinancialController`, `ExpensesController`, `PayrollController`, `AttendanceController` reports, `BookingAnalyticsController`, `WebsiteAnalyticsController`, `AuditLogsController`

## Stored Procedures
`sp_GetAnalytics*`, `sp_GetProfitLossSummary`, expense/payroll report SPs, `sp_GetBookingAnalytics`, etc.

## Components
`analytics-*`, `financial-dashboard`, `expense-list`, `payroll-list`, `attendance-reports`, `booking-analytics`, `lead-analytics`, `audit-dashboard`

## Roles
**Gym Admin** with respective `VIEW_*_ANALYTICS` permissions

## SaaS Feature
`REPORTS` (analytics menu items); financial items also use REPORTS feature code
