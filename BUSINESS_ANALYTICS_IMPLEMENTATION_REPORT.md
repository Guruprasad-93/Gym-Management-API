# Business Analytics & Dashboard Module — Implementation Report

## Overview

The Business Analytics module delivers gym-scoped KPIs, revenue/member/attendance/trainer/workout/diet analytics, PDF/Excel exports with audit logging, and an enhanced Angular gym-admin dashboard with Chart.js visualizations.

## Database

**Script:** `Backend/Gym.Infrastructure/Persistence/Scripts/025_BusinessAnalyticsModule.sql`

| Artifact | Purpose |
|----------|---------|
| `AnalyticsDashboardCache` | Optional cache table for future pre-aggregation |
| `sp_GetAnalyticsDashboardOverview` | Top-level KPI summary |
| `sp_GetAnalyticsRevenueSummary` | Today/week/month/year revenue + failed payments |
| `sp_GetAnalyticsRevenueByPlan` | Revenue grouped by membership plan |
| `sp_GetAnalyticsRevenueByPaymentMethod` | Revenue grouped by payment method |
| `sp_GetAnalyticsMembershipSummary` | Active/expired/expiring/new member counts |
| `sp_GetAnalyticsMembershipGrowthTrend` | Monthly new member trend |
| `sp_GetAnalyticsPlanDistribution` | Members per plan |
| `sp_GetAnalyticsAttendanceSummary` | Today's attendance counts |
| `sp_GetAnalyticsAttendanceWeeklyTrend` | 7-day attendance trend |
| `sp_GetAnalyticsAttendanceMonthlyTrend` | Monthly attendance trend |
| `sp_GetAnalyticsMostActiveMembers` | Top N active members |
| `sp_GetAnalyticsLeastActiveMembers` | Bottom N active members |
| `sp_GetAnalyticsMemberAttendancePercentage` | Attendance % per member |
| `sp_GetAnalyticsTrainerSummary` | Active trainers + assigned members |
| `sp_GetAnalyticsTrainerPerformance` | Per-trainer metrics |
| `sp_GetAnalyticsWorkoutSummary` | Active/completed workout plans + completion % |
| `sp_GetAnalyticsDietSummary` | Active diet plans + compliance % |
| `sp_GetAnalyticsRecentPayments` | Dashboard widget |
| `sp_GetAnalyticsExpiringMemberships` | Dashboard widget |
| `sp_GetAnalyticsNewMembers` | Dashboard widget |
| `sp_GetAnalyticsRecentNotifications` | Dashboard widget |

**Deployment:** Run script `025_BusinessAnalyticsModule.sql` against the application database. Restart the API so `DatabaseSeeder` adds new privileges for existing deployments.

## Permissions

| Permission | Description | Typical roles |
|------------|-------------|---------------|
| `VIEW_ANALYTICS` | Full business dashboard + attendance/trainer/workout/diet APIs + exports | GymAdmin, SuperAdmin, Trainer (partial) |
| `VIEW_REVENUE_ANALYTICS` | Revenue analytics API + revenue export | GymAdmin, SuperAdmin |
| `VIEW_MEMBER_ANALYTICS` | Member analytics API | GymAdmin, SuperAdmin, Trainer |

Seeded in `DatabaseSeeder.cs` for SuperAdmin and GymAdmin. Trainers receive `VIEW_ANALYTICS` and `VIEW_MEMBER_ANALYTICS`.

## Backend API

**Base route:** `/api/analytics`

| Method | Route | Permission | Response |
|--------|-------|------------|----------|
| GET | `/dashboard` | `VIEW_ANALYTICS` | Full `AnalyticsDashboardDto` |
| GET | `/revenue` | `VIEW_REVENUE_ANALYTICS` | `RevenueAnalyticsDto` |
| GET | `/members` | `VIEW_MEMBER_ANALYTICS` | `MembershipAnalyticsDto` |
| GET | `/attendance` | `VIEW_ANALYTICS` | `AttendanceAnalyticsDto` |
| GET | `/trainers` | `VIEW_ANALYTICS` | `TrainerAnalyticsDto` |
| GET | `/workouts` | `VIEW_ANALYTICS` | `WorkoutAnalyticsDto` |
| GET | `/diets` | `VIEW_ANALYTICS` | `DietAnalyticsDto` |
| GET | `/export/pdf?reportType=dashboard\|revenue` | `VIEW_ANALYTICS` | PDF file |
| GET | `/export/excel?reportType=dashboard\|revenue` | `VIEW_ANALYTICS` | Excel file |

**SuperAdmin:** Pass optional `gymId` query parameter (same pattern as other gym-scoped modules).

### Key files

- DTOs: `Backend/Gym.Application/DTOs/Analytics/AnalyticsDtos.cs`
- Repository: `Backend/Gym.Infrastructure/Repositories/AnalyticsRepository.cs`
- Service: `Backend/Gym.Application/Services/AnalyticsService.cs`
- Exporter: `Backend/Gym.Infrastructure/Services/AnalyticsReportExporter.cs` (QuestPDF + ClosedXML)
- Controller: `Backend/Gym.API/Controllers/AnalyticsController.cs`

### Audit

Exports call `IAuditService.LogAsync` with entity `Analytics`, action `Export`, recording `reportType` and format.

## Frontend (Angular)

### Routes

| Route | Component |
|-------|-----------|
| `/gym-admin/dashboard` | Enhanced business dashboard |
| `/gym-admin/analytics/revenue` | Revenue analytics page |
| `/gym-admin/analytics/members` | Member analytics page |
| `/gym-admin/analytics/attendance` | Attendance analytics page |
| `/gym-admin/analytics/trainers` | Trainer analytics page |

### Dashboard features

- **KPI cards:** Total Members, Active Members, Revenue Today, Revenue This Month, Expiring Memberships, Active Trainers
- **Charts (Chart.js):** Revenue trend, membership growth, attendance trend, plan distribution, payment methods
- **Widgets:** Recent payments, expiring memberships, new members, recent notifications
- **UX:** Loading skeletons, empty states, PDF/Excel export buttons, responsive grid layout

### Key files

- Models: `Frontend/gym-app/src/app/shared/models/analytics.models.ts`
- Service: `Frontend/gym-app/src/app/core/services/analytics.service.ts`
- Dashboard: `Frontend/gym-app/src/app/features/gym-admin/dashboard/gym-admin-dashboard.component.ts`
- Analytics pages: `Frontend/gym-app/src/app/features/gym-admin/analytics/`
- Menu: `Frontend/gym-app/src/app/core/constants/menu.config.ts`
- Permissions: `Frontend/gym-app/src/app/core/constants/permissions.ts`

## Build verification

- **Backend:** `dotnet build GymManagementSaaS.sln` — succeeded
- **Frontend:** `npm run build` — succeeded

## Notes

1. **Diet compliance** uses a placeholder calculation in SQL until dedicated compliance tracking exists.
2. **`AnalyticsDashboardCache`** is created but not wired in C# yet; SPs query live data.
3. **Revenue trend** reuses `sp_GetMonthlyRevenueSummary` (12-month window).
4. Existing `/gym-admin/revenue` page remains for legacy revenue dashboard; new analytics pages provide richer breakdowns.

## Quick test checklist

1. Run SQL script `025_BusinessAnalyticsModule.sql`
2. Log in as Gym Admin → open `/gym-admin/dashboard`
3. Verify KPI cards, charts, and widgets populate
4. Open each analytics sub-page from menu or quick links
5. Export PDF/Excel and confirm audit log entry under Audit Logs
6. Log in as SuperAdmin with `?gymId={guid}` on API calls if testing cross-gym scope
