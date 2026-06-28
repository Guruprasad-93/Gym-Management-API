# Dashboard

## Module Overview
Role-specific home dashboards with KPIs and quick insights.

## Purpose
Landing page after login for each role with operational summary metrics.

## Navigation Paths
| Role | Route |
|------|-------|
| Super Admin | `/super-admin` |
| Gym Admin | `/gym-admin/dashboard` |
| Trainer | `/trainer` |
| Member | `/member/dashboard` |

## Screen Description

### Gym Admin Dashboard
- KPI cards (members, revenue, attendance, etc.)
- Charts and trend widgets
- Links to detailed analytics modules

### Trainer Dashboard
- Assigned member counts, plan summaries

### Member Dashboard
- Goals, progress snapshot, membership status, quick links

### Super Admin Dashboard
- Platform gym counts, subscription overview

### Buttons & Actions
- Navigation via KPI cards and menu (no destructive actions on dashboard itself)
- Export actions on analytics sub-pages (separate module)

### Filters / Search
- Date ranges on embedded analytics widgets where present

### APIs
| Method | Endpoint | Permission |
|--------|----------|------------|
| GET | `/api/Dashboard/stats` | VIEW_DASHBOARD |
| GET | `/api/analytics/dashboard` | VIEW_ANALYTICS |
| GET | `/api/member/dashboard` | VIEW_MEMBER_DASHBOARD |
| GET | `/api/ai/dashboard` | VIEW_AI_INSIGHTS |

### Stored Procedures
`sp_Dashboard_SuperAdmin`, `sp_Dashboard_GymAdmin`, `sp_GetDashboardStatistics`, `sp_GetGymDashboardStatistics`, `sp_GetMemberSelfServiceDashboard`

### Angular Components
`gym-admin-dashboard`, `trainer-dashboard`, `member-dashboard`, `super-admin-dashboard`

### Roles
Per-role dashboard; guards enforce role + permission

### SaaS Feature
Gym Admin dashboard gated by `DASHBOARD` feature code
