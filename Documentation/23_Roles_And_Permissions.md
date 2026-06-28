# Roles & Permissions

## Module Overview
Role-based access control (RBAC) with privileges assigned to roles and users assigned roles. Enforced on API and Angular routes/menus.

## Roles (`RoleNames`)
| Role | Value | Scope |
|------|-------|-------|
| SuperAdmin | `SuperAdmin` | Platform-wide |
| GymAdmin | `GymAdmin` | Single gym tenant |
| Trainer | `Trainer` | Single gym, assigned members |
| Member | `Member` | Self-service only |

## Permission Model
- **98 permissions** in `Gym.Application/Constants/Permissions.cs`
- Frontend mirrors in `permissions.ts` (107 entries including matrix helpers)
- JWT/cookie claims include permission list
- `RequirePermissionAttribute` maps to policy per action

## Permission Groups (summary)

| Group | Example permissions |
|-------|---------------------|
| Gyms | VIEW_GYMS, CREATE_GYM, UPDATE_GYM |
| Members | VIEW_MEMBERS, CREATE_MEMBER, UPDATE_MEMBER, DELETE_MEMBER |
| Trainers | VIEW_TRAINERS, ASSIGN_MEMBER_TO_TRAINER |
| Memberships | VIEW_MEMBERSHIPS, RENEW_MEMBERSHIP |
| Payments | VIEW_PAYMENTS, REFUND_PAYMENT, INITIATE_ONLINE_PAYMENT |
| Attendance | VIEW_ATTENDANCE, MANAGE_ATTENDANCE, EXPORT_ATTENDANCE_REPORTS |
| Diet / Workout | VIEW_DIET_PLANS, MANAGE_DIET_PLANS, ASSIGN_DIET_PLAN |
| Bookings | VIEW_BOOKINGS, MANAGE_BOOKINGS, MANAGE_SCHEDULES, VIEW_BOOKING_ANALYTICS |
| Leads | VIEW_LEADS, MANAGE_LEADS, CONVERT_LEADS |
| Financial | VIEW_EXPENSES, MANAGE_PAYROLL, VIEW_FINANCIAL_ANALYTICS |
| Website | VIEW_WEBSITE_BUILDER, MANAGE_WEBSITE_BUILDER |
| White label | VIEW_WHITE_LABEL, MANAGE_WHITE_LABEL |
| SaaS | VIEW_SAAS_SUBSCRIPTION, MANAGE_SAAS_SUBSCRIPTION |
| AI | VIEW_AI_INSIGHTS, VIEW_AI_RECOMMENDATIONS |
| Audit | VIEW_AUDIT_LOGS, EXPORT_AUDIT_LOGS |
| Branches | VIEW_BRANCHES, MANAGE_BRANCHES, TRANSFER_MEMBERS |
| Platform | MANAGE_SUBSCRIPTION_PLANS, VIEW_PLATFORM_SAAS |

## Menu ↔ Permission Mapping
`MenuPermissionMap.cs` maps menu codes to required permissions for tenant menu visibility.

## API Route ↔ Feature Mapping
`ApiRouteFeatureMap.cs` maps URL prefixes to SaaS feature codes (e.g. `/api/schedules` → `BOOKINGS`).

## Super Admin RBAC UI
- `/super-admin/roles` — CRUD roles
- `/super-admin/privileges` — CRUD privileges
- `/super-admin/role-matrix` — assign privileges to roles

## APIs
`RolesController`, `PrivilegesController`, `RolePrivilegesController`, `UserRolesController`

## Database Tables
`Roles`, `Privileges`, `RolePrivileges`, `UserRoles`

## Angular Guards
- `authGuard` — logged in
- `roleGuard(Roles.X)` — role check
- `permissionGuard(Permissions.X)` — permission check
- `featureGuard('FEATURE_CODE')` — SaaS plan feature

## Notes
- Demo gym admin receives full booking/schedule permissions via `DatabaseSeeder`
- Empty `enabledFeatureCodes` in session disables feature gating (legacy/Super Admin behavior)
