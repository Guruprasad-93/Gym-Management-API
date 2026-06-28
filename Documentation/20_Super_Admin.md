# Super Admin

## Module Overview
Platform administration: gyms, gym admins, SaaS subscription plans, RBAC, audit, platform white-label.

## Navigation
See [UI_NAVIGATION.md](./UI_NAVIGATION.md) Super Admin section.

## Key Modules
| Area | Route | API base |
|------|-------|----------|
| Gyms | `/super-admin/gyms` | `/api/Gyms` |
| Gym admins | `/super-admin/gym-admins` | `/api/gym-admins` |
| Subscription plans | `/super-admin/subscription-plans` | `/api/platform/subscription-plans` |
| Roles / Privileges | `/super-admin/roles`, `/privileges` | `/api/Roles`, `/api/Privileges` |
| Role matrix | `/super-admin/role-matrix` | `/api/role-privileges` |
| Audit | `/super-admin/audit` | `/api/audit-logs` |
| White label | `/super-admin/white-label` | `/api/platform/white-label` |
| Tenant menus | (API only) | `/api/platform/tenant-menus` |

## Components
`super-admin-layout`, `gym-list`, `gym-admin-list`, `subscription-plans/*`, `role-list`, `privilege-list`, `role-matrix`, `audit-dashboard`, `super-admin-white-label`

## Roles
**SuperAdmin** only (no `GymId`)

## Tables
`Gyms`, `SaasSubscriptionPlans`, `PlanFeatures`, `PlanPricingOptions`, `SystemFeatures`, `Menus`, `GymMenus`, RBAC tables
