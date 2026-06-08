# Gym Management System — Implementation Summary

This document describes work completed on the **Gym Management SaaS** project: backend (ASP.NET Core 8, Clean Architecture, MediatR, Dapper + stored procedures) and frontend (Angular 19 + Material). It is intended as a single reference for features delivered, bug fixes, security behavior, and how to run the application.

**Last updated:** June 2026

---

## Table of contents

1. [Project layout](#1-project-layout)
2. [How to run](#2-how-to-run)
3. [Demo credentials](#3-demo-credentials)
4. [Authorization and gym isolation](#4-authorization-and-gym-isolation)
5. [Authentication and role fixes](#5-authentication-and-role-fixes)
6. [SQL and stored procedure fixes](#6-sql-and-stored-procedure-fixes)
7. [Paging and validation fixes](#7-paging-and-validation-fixes)
8. [Member details access](#8-member-details-access)
9. [Membership and payments module](#9-membership-and-payments-module)
10. [Frontend features by role](#10-frontend-features-by-role)
11. [Trainer “My Members” pagination](#11-trainer-my-members-pagination)
12. [Key files reference](#12-key-files-reference)
13. [Known limitations and troubleshooting](#13-known-limitations-and-troubleshooting)

---

## 1. Project layout

| Area | Path |
|------|------|
| Backend solution | `Backend/GymManagementSaaS.sln` (same projects as `Backend/GymManagementSystem.sln`) |
| API host | `Backend/Gym.API/` |
| Application layer | `Backend/Gym.Application/` |
| Infrastructure (Dapper, SPs, seeding) | `Backend/Gym.Infrastructure/` |
| SQL scripts (deployed on startup) | `Backend/Gym.Infrastructure/Persistence/Scripts/001–012_*.sql` |
| Angular app | `Frontend/gym-app/` |
| Architecture notes | `Backend/ARCHITECTURE.md` |

**Stack:** JWT auth, dynamic permissions from database, all business CRUD via SQL Server stored procedures (not EF at runtime).

---

## 2. How to run

### Backend API

```powershell
cd g:\GymManagementSystem\Backend
dotnet build GymManagementSaaS.sln
dotnet run --project Gym.API\Gym.API.csproj --launch-profile http
```

- **API URL:** `http://localhost:5088`
- On startup: EF migrations, embedded SQL scripts deploy, demo data seed (if not present).

**Important:** Stop any running `Gym.API` / IIS Express / Visual Studio debug session before rebuilding, or you may get MSB3027 file-lock errors.

### Frontend (Angular)

```powershell
cd g:\GymManagementSystem\Frontend\gym-app
npm install
ng serve
```

- **App URL:** `http://localhost:4200`
- **API proxy:** `Frontend/gym-app/proxy.conf.json` forwards `/api` → `http://localhost:5088`

### Verify health

```http
GET http://localhost:5088/api/health
```

---

## 3. Demo credentials

| Role | Email | Password |
|------|--------|----------|
| Super Admin | `superadmin@gym.com` | `SuperAdmin@123` |
| Gym Admin (FitZone demo) | `admin@fitzone-demo.com` | `Demo@123` |
| Trainer | `trainer1@fitzone-demo.com` | `Demo@123` |
| Member | `member1@fitzone-demo.com` … `member5@fitzone-demo.com` | `Demo@123` |

After privilege/role seeding changes, **trainers and members may need to log out and log in again** so JWT claims include new permissions.

---

## 4. Authorization and gym isolation

### Design pattern

- **`ICurrentUserService`** — reads `UserId`, `GymId`, roles, and permission claims from JWT.
- **Application services** — resolve gym scope and trainer filters before calling repositories.
- **Stored procedures** — use `(@GymId IS NULL OR …)` (and trainer filters where applicable) so Super Admin can see all gyms; Gym Admin / Trainer / Member are scoped.

### Expected behavior

| Role | Scope |
|------|--------|
| **Super Admin** | All gyms; can pass `gymId` on queries |
| **Gym Admin** | Own gym only; cannot read/write other gyms’ entities |
| **Trainer** | Own gym; member lists filtered to **assigned members** only |
| **Member** | Own profile / own-gym data only |

### Services updated for isolation

- `GymAdminService` — cross-gym admin access blocked
- `MemberService` / `TrainerService` — gym scoping, `ResolveGymScopeForMember` / `ResolveGymScopeForTrainer`, trainer filter on paged members
- `MembershipService`, `PaymentService`, `MembershipPlanService` — Super Admin writes use entity `GymId`

---

## 5. Authentication and role fixes

### Problem: Trainer / member login “succeeded” but no redirect

**Cause:** `getDefaultRouteForUser()` navigated to `/trainer` and `/member`, but those routes were missing from `app.routes.ts` → wildcard sent users back to login.

### Solution

**Angular routes** (`Frontend/gym-app/src/app/app.routes.ts`):

- `/trainer` → `features/trainer/trainer.routes.ts`
- `/member` → `features/member/member.routes.ts`

**Trainer area**

- Layout, dashboard, **My Members** list
- Guards: `authGuard`, `roleGuard(Trainer)`, `permissionGuard` where needed

**Member area**

- Layout, profile page

**Backend**

- `GET /api/trainers/me` — current trainer profile
- `GET /api/members/me` — current member profile

**Seeding / login**

- `DatabaseSeeder` — `EnsureMemberRoleAndPrivilegesAsync`, backfill Member role; `EnsureTrainerHasPrivilegeAsync` for trainer privileges
- `AuthService.EnsureProfileRolesAsync` — assigns Member/Trainer role on login if profile exists but role missing
- Login page — removed stray `debugger`, restored form validation
- `menu.config.ts` — `TRAINER_MENU`, `MEMBER_MENU`

---

## 6. SQL and stored procedure fixes

### Problem: Gym admin members/trainers/gym-admins list returned 500

**Cause:** Paging stored procedures (`sp_GetAllMembers`, `sp_GetAllTrainers`, `sp_GetGymAdmins`) used a CTE named `Filtered` in one batch and referenced it in a second statement. SQL Server does not persist CTEs across statements → `Invalid object name 'Filtered'`.

### Solution

- Replaced CTE with temp table **`#Filtered`**
- Set total count: `SET @TotalCount = (SELECT COUNT(*) FROM #Filtered)`

**Scripts:** `008_GymAdminModule.sql`, `010_TrainerModule.sql`, `011_MemberModule.sql` (redeployed on API startup via `StoredProcedureDeployer`).

### Other SQL fix

- **`sp_GetMemberProgress`** — `CAST(wh.RecordedAt AS DATE)` in UNION for consistent types

---

## 7. Paging and validation fixes

### Problem: HTTP 400 on list APIs — `SortColumn` validation

Example:

```http
GET /api/members?pageNumber=1&pageSize=10&sortColumn=FullName&sortDirection=asc
```

Error (before fix): `Sort column must be one of: UserName, Name, Specialization, …`

### Root causes

1. **Strict FluentValidation allow-lists** on `PagedRequestDto` that did not match Angular column names (`fullName` vs `FullName`).
2. **Multiple validators registered for the same type:** `TrainerPagedRequestDtoValidator` and `MemberPagedRequestDtoValidator` both implemented `AbstractValidator<PagedRequestDto>`. With `AddFluentValidationAutoValidation()`, **all** of them ran on **every** paged endpoint — so the trainer validator rejected `FullName` on `/api/members`.

### Solution

- **Removed** `TrainerPagedRequestDtoValidator.cs` and `MemberPagedRequestDtoValidator.cs`
- **Single shared** `PagedRequestDtoValidator` — validates page number/size, sort direction, max length of sort column (no fixed column whitelist)
- **Query validators** (`GetMembersQueryValidator`, `GetTrainersQueryValidator`, `GetGymAdminsQueryValidator`) use `PagedRequestDtoValidator` via `SetValidator`
- **Repositories** normalize sort columns:
  - `MemberRepository.NormalizeSortColumn` — e.g. `fullname` → `FullName`, `joindate` → `JoinDate`
  - `TrainerRepository.NormalizeSortColumn` — e.g. `fullname` → `UserName`, `email` → `UserEmail`

### Angular alignment

- `member-list.component.ts` / `trainer-list.component.ts` — map Material sort ids to API columns; ignore empty sort events
- `trainer-list` — map `fullName` → `UserName`, `email` → `UserEmail`

---

## 8. Member details access

### Problems

1. `GET /api/members/{id}/details` required `VIEW_MEMBER_DETAILS`; many gym admins only had `VIEW_MEMBERS`.
2. Payment/progress stored procedure errors could fail the entire details response.

### Solutions

**Authorization**

- `RequireAnyPermissionAttribute` + `AnyPermissionRequirement` + `AnyPermissionAuthorizationHandler`
- `DynamicAuthorizationPolicyProvider` supports pipe-separated policies
- `MembersController.GetDetails` — `[RequireAnyPermission(ViewMemberDetails, ViewMembers)]`

**Service**

- `MemberService` — gym-admin access for details; try/catch on payment/progress so profile still loads
- `GetMemberWithAccessCheckAsync` — own-profile shortcut only when `IsMemberOnly()`

**Seeder**

- New privileges assigned to GymAdmin/Member on startup where applicable

---

## 9. Membership and payments module

Delivered as part of the broader MVP (script `012_MembershipPaymentModule.sql`).

### Backend

- Repositories, services, DTOs, CQRS handlers, permissions
- API controllers for membership plans, memberships, payments
- Invoice generation endpoint (PDF placeholder / basic implementation as built)
- Stored procedures in `012_MembershipPaymentModule.sql`

### Frontend (gym admin)

- Membership list, create/renew dialogs
- Membership plan list and form dialog
- Expired memberships list
- Payment UI and dashboard integration (as wired in gym-admin feature module)

---

## 10. Frontend features by role

### Routes (`app.routes.ts`)

| Path | Role |
|------|------|
| `/auth/login` | Public |
| `/super-admin/*` | Super Admin |
| `/gym-admin/*` | Gym Admin |
| `/trainer/*` | Trainer |
| `/member/*` | Member |

### Gym admin (high level)

- Dashboard, gyms (super-admin), gym admins, trainers, members (paged, sortable)
- Member details, assign trainer, memberships, plans, payments, revenue views

### Trainer

| Route | Component | Notes |
|-------|-----------|--------|
| `/trainer` | Dashboard | Stats via `GET /api/trainers/{id}/dashboard` |
| `/trainer/members` | My Members | Paged list (see §11) |

### Member

| Route | Component | Notes |
|-------|-----------|--------|
| `/member` | Profile | `GET /api/members/me` |

---

## 11. Trainer “My Members” pagination

### Before

- Loaded up to **100** members in one request (`pageSize: 100`), no paginator UI.

### After (`trainer-members.component.ts`)

- **Material paginator** — page sizes 5, 10, 25, 50, first/last buttons
- **Server-side paging** via `MemberService.getPaged()` → `GET /api/members`
- Backend already filters to the logged-in trainer’s assigned members (`MemberService.ResolveTrainerFilterAsync`)
- Search debounced (350ms); resets to page 1
- Default sort: `FullName` ascending

No new API endpoint was required; reuses the same paged members API as gym admin with trainer scoping.

---

## 12. Key files reference

### Authorization

| File | Purpose |
|------|---------|
| `Gym.Application/Authorization/RequireAnyPermissionAttribute.cs` | OR permission on actions |
| `Gym.Application/Authorization/AnyPermissionRequirement.cs` | Policy requirement |
| `Gym.Infrastructure/Authorization/AnyPermissionAuthorizationHandler.cs` | Handler |
| `Gym.Application/Services/MemberService.cs` | Gym/trainer/member scope |
| `Gym.Application/Services/TrainerService.cs` | Trainer scope |
| `Gym.Application/Services/GymAdminService.cs` | Gym admin scope |

### Validation

| File | Purpose |
|------|---------|
| `Gym.Application/Validators/PagedRequestDtoValidator.cs` | Shared paging rules |
| `Gym.Application/Features/Members/Queries/GetMembers/GetMembersQueryValidator.cs` | Members query |
| `Gym.Application/Features/Trainers/Queries/GetTrainers/GetTrainersQueryValidator.cs` | Trainers query |

### SQL

| Script | Topics |
|--------|--------|
| `001_AuthorizationSchema.sql` | Roles, privileges |
| `008_GymAdminModule.sql` | Gym admins, paging SPs |
| `010_TrainerModule.sql` | Trainers, assignments |
| `011_MemberModule.sql` | Members, paging SPs |
| `012_MembershipPaymentModule.sql` | Plans, memberships, payments |

### API controllers (examples)

| Controller | Base route |
|------------|------------|
| `MembersController` | `/api/members` |
| `TrainersController` | `/api/trainers` |
| `AuthController` | `/api/auth` |
| `MembershipsController` | `/api/memberships` |
| `MembershipPlansController` | `/api/membership-plans` |
| `PaymentsController` | `/api/payments` |

### Frontend

| File | Purpose |
|------|---------|
| `proxy.conf.json` | Proxy to port 5088 |
| `app.routes.ts` | Top-level routes |
| `features/trainer/trainer.routes.ts` | Trainer child routes |
| `features/trainer/members/trainer-members.component.ts` | Paged member list |
| `features/gym-admin/members/member-list.component.ts` | Gym admin member list |
| `core/services/auth.service.ts` | Login, role ensure, default route |
| `core/config/menu.config.ts` | Role menus |

---

## 13. Known limitations and troubleshooting

| Issue | What to do |
|-------|------------|
| **400 on `SortColumn`** after code change | Restart API so new validators load |
| **Build file lock** | Stop `Gym.API` process, then `dotnet build` |
| **401 on members** | Log in again; check JWT and `VIEW_MEMBERS` for trainers |
| **Trainer menu missing items** | Re-login after `DatabaseSeeder` privilege updates |
| **SQL changes not applied** | Restart API (scripts redeploy on startup) |
| **Angular calls wrong API port** | Confirm `proxy.conf.json` target is `http://localhost:5088` |

### Optional follow-ups (not fully done)

- Full automated regression test suite after all fixes
- Reduce broad try/catch in `GetDetailsAsync` once all SPs are stable in production
- Production-grade PDF invoices if only a stub exists
- Super-admin dashboard parity with gym-admin widgets

---

## Summary checklist

- [x] Gym-scoped authorization for admin, trainer, member, super-admin
- [x] Trainer and member login routes and UI shells
- [x] SQL paging fix (`#Filtered` temp tables)
- [x] Paged list validation fix (single `PagedRequestDtoValidator`)
- [x] Member details for gym admins (`RequireAnyPermission`)
- [x] Membership & payment module (backend + gym-admin UI)
- [x] API proxy and launch profile on port **5088**
- [x] Trainer **My Members** server-side pagination

---

*For low-level data access rules, see `Backend/ARCHITECTURE.md`.*
