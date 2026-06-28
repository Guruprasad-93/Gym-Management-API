# Project Overview

## Solution Name
**Gym Management System (GymManagementSystem)**

## Purpose
Multi-tenant SaaS gym management platform for gym operators, trainers, and members. Supports memberships, payments, attendance, workouts, diet plans, CRM, multi-branch operations, class scheduling/booking, public websites, white-label branding, and platform administration.

## Technology Stack

| Layer | Technology |
|-------|------------|
| Frontend | Angular 19 (standalone components), Angular Material, RxJS |
| Backend | ASP.NET Core 8 Web API |
| Architecture | Clean Architecture (Domain → Application → Infrastructure → API) |
| Data access | Dapper + SQL Server stored procedures |
| Auth | JWT + HTTP-only cookies, CSRF (`XSRF-TOKEN`), RBAC permissions |
| Tenancy | `GymId` isolation on tenant-scoped data |
| Migrations | EF Core (Users) + numbered SQL scripts (`001`–`076`) |
| Payments | Razorpay (with mock gateway for development) |
| Notifications | WhatsApp templates/logs, Firebase push (mock in dev), in-app notifications |

## Solution Structure

```
GymManagementSystem/
├── Backend/
│   ├── Gym.Domain/           # Entities, constants
│   ├── Gym.Application/      # Services, DTOs, validators, authorization maps
│   ├── Gym.Infrastructure/   # Dapper repos, SQL scripts, external providers
│   ├── Gym.API/              # Controllers, middleware, startup
│   └── Gym.API.IntegrationTests/
├── Frontend/gym-app/         # Angular SPA
└── Documentation/            # This documentation set
```

## User Roles

| Role | Portal route | Description |
|------|--------------|-------------|
| SuperAdmin | `/super-admin` | Platform-wide gym, plan, RBAC management |
| GymAdmin | `/gym-admin` | Full gym operations |
| Trainer | `/trainer` | Assigned members, attendance, schedule, workouts |
| Member | `/member` | Self-service portal |
| Public | `/website/:gymSlug` | Published gym marketing site (no login) |

## Cross-Cutting Concerns

- **RBAC:** Permissions on API (`RequirePermission`) and Angular (`permissionGuard`)
- **SaaS features:** Subscription plan features gate menus/routes (`featureCode`, `featureGuard`)
- **Tenant menus:** Per-gym module enable/disable (`GymMenus`, middleware)
- **Audit:** `AuditLogs` for many write operations
- **Exception handling:** Central middleware returns JSON `ApiResponse`

## Related Documents

- [PROJECT_FEATURE_MATRIX.md](./PROJECT_FEATURE_MATRIX.md)
- [PROJECT_STATISTICS.md](./PROJECT_STATISTICS.md)
- [UI_NAVIGATION.md](./UI_NAVIGATION.md)
- [API_REFERENCE.md](./API_REFERENCE.md)
- [DATABASE_OBJECTS.md](./DATABASE_OBJECTS.md)
- Module docs `02`–`24`
