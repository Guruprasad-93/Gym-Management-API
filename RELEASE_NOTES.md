# Gym Management SaaS — Release Notes

## v1.0.0-RC1 (Release Candidate 1)

**Release date:** June 2026  
**Status:** Release Candidate — production deployment with checklist sign-off recommended  
**Repositories:** [GymManagementSystem](https://github.com/Guruprasad-93/Gym-Management-API) (API) · [GymManagementSystem-UI](https://github.com/Guruprasad-93/Gym-Management-UI) (Angular SPA)

---

## Overview

Gym Management SaaS v1.0.0-RC1 is the first release candidate of a multi-tenant fitness business platform. It delivers end-to-end gym operations—from member onboarding and billing to trainer workflows, CRM, analytics, white-label branding, and public gym websites—through a single Angular application backed by an ASP.NET Core 8 API and SQL Server stored procedures.

This RC consolidates **52 database migration scripts**, **16 browser E2E test suites**, and **comprehensive API QA coverage** across all major modules.

---

## Major Features Completed

### Platform & Security

| Feature | Description |
|---------|-------------|
| **LoginIdentifier authentication** | Tenant-scoped login IDs (max 20 chars, alphanumeric + `.` `_` `-`) replace email as the primary sign-in identifier. Email is optional. Supports Super Admin (no gym), Gym Admin, Trainer, and Member portals. |
| **Cookie + CSRF auth** | HttpOnly access/refresh cookies with CSRF double-submit protection for browser clients. |
| **Role-based access control (RBAC)** | Dynamic permissions from database; route guards (Angular) and `[RequirePermission]` attributes (API). |
| **Tenant Menu Management** | Super Admin enables/disables modules per gym. Disabled modules hide from sidebar and return HTTP 403 on API. |
| **Multi-tenant isolation** | All gym-scoped data filtered by `GymId`; cross-tenant access blocked at service and stored-procedure layers. |
| **Audit logging** | Create/update/delete/renew actions recorded with actor, entity, and before/after values. |
| **Production hardening** | Rate limiting, HSTS, forwarded headers, Serilog request logging, health checks, schema version tracking. |

### Core Operations

| Module | Capabilities |
|--------|--------------|
| **Members** | CRUD, LoginIdentifier accounts, trainer assignment, profile, diet/workout views, file attachments |
| **Trainers** | CRUD, member assignment, dashboard, specialization/bio, trainer portal |
| **Memberships** | Plan management, assign/renew memberships, expiry tracking |
| **Payments** | Record payments, payment history, revenue totals, Razorpay integration (optional) |
| **Attendance** | Member check-in/check-out, attendance reports, trainer attendance |

### Growth & Engagement

| Module | Capabilities |
|--------|--------------|
| **CRM & Leads** | Lead capture, pipeline, edit, search, convert to member, lead analytics dashboard |
| **Diet Plans** | Plan builder, categories, assign to members, PDF export, member diet view |
| **Workout Plans** | Exercise library, plan builder, assign to members, completion tracking |
| **Notifications** | WhatsApp template mapping, test send, notification history (Mock/Interakt providers) |
| **Bookings** | Trainer schedules, slot reservations, booking management |

### Business Intelligence

| Module | Capabilities |
|--------|--------------|
| **Analytics** | Gym dashboard KPIs, revenue analytics, member analytics, trainer analytics |
| **Reports** | Revenue, attendance, membership reports; financial dashboard |
| **Audit Reports** | Platform and gym audit log search |

### SaaS Platform

| Module | Capabilities |
|--------|--------------|
| **SaaS Subscriptions** | Trial plans, member/trainer limits, grace period, upgrade prompts |
| **Multi-Branch** | Branch CRUD, branch dashboard, member transfer between branches |
| **White Label** | Gym branding (logo, colors, login page), platform branding, live preview |
| **Public Website** | Page builder, publish workflow, public routes, contact/trial lead forms |
| **Member Self-Service** | Member portal: workouts, diet, goals, progress, bookings, payments |
| **Payroll & Expenses** | Expense tracking, payroll management |
| **AI Insights** | Churn risk, lead scoring, trainer recommendations (Mock/OpenAI providers) |
| **Mobile Push** | Push notification campaigns and device token management (Firebase/Mock) |

---

## LoginIdentifier Authentication (v1.0 highlight)

- Users sign in with **Login ID + Password** (and **Gym ID** for tenant users via URL query or branding context).
- Login IDs are **unique per gym**; platform users (Super Admin) have `GymId = NULL`.
- Existing accounts were backfilled from email local-part during migration `051_LoginIdentifier.sql`.
- Password reset and forgot-password flows accept LoginIdentifier.
- Angular login forms use `#loginIdentifier` field; demo gym admin URL pattern: `/auth/login?gymId={uuid}`.

---

## Tenant Menu Management (v1.0 highlight)

- Catalog of all application modules stored in `dbo.Menus`.
- Per-gym enable/disable in `dbo.GymMenus` (default: all enabled for existing gyms).
- Super Admin manages menus at **Tenant Menus** (`/super-admin/tenant-menus`).
- API middleware (`GymMenuAccessMiddleware`) enforces module access; returns **403 Forbidden** when disabled.
- Angular `gymMenuGuard` hides disabled routes from gym-admin navigation.

---

## Technology Stack

| Layer | Technology |
|-------|------------|
| Frontend | Angular 19, Angular Material, standalone components |
| Backend | ASP.NET Core 8, MediatR, FluentValidation, Clean Architecture |
| Data | SQL Server, Dapper, stored procedures (EF Core for migrations only) |
| Auth | JWT (cookie transport), CSRF tokens, refresh tokens |
| E2E Testing | Playwright (Chromium, Firefox, Edge, Chrome) |
| API Testing | Integration tests + PowerShell QA runner |

---

## Demo Environment

| Role | Login ID | Password | Notes |
|------|----------|----------|-------|
| Super Admin | `superadmin` | `SuperAdmin@123` | No gymId |
| Gym Admin | `admin` | `Demo@123` | FitZone Demo Gym |
| Trainer | `trainer1` | `Demo@123` | Assigned members |
| Member | `member1` … `member5` | `Demo@123` | Self-service portal |

**API:** `http://localhost:5088` · **UI:** `http://localhost:4200`

> Disable demo seeding in production (`Demo__Enabled=false`, `Database__RunSeedOnStartup=false`).

---

## Known RC1 Limitations

1. **Trial plan trainer limit (5/5)** — new trainer creation blocked when limit reached; soft-deleted trainers still count toward quota.
2. **Member list `pageSize` max 100** — UI dialogs requesting 200 members receive HTTP 400.
3. **One WhatsApp template per notification type per gym** — duplicate template types rejected.
4. **Angular NG0955 warning** — duplicate sidebar track key for `/gym-admin/ai` route (cosmetic).
5. Integration test suite requires a clean SQL Server instance with all migrations applied (see `TESTING_SUMMARY.md`).

---

## Git Tag Recommendation

```bash
git tag -a v1.0.0-RC1 -m "Gym SaaS v1.0.0 Release Candidate 1"
git push origin v1.0.0-RC1
```

Tag both API and UI repositories at the same commit snapshot used for RC validation.

---

## Related Documentation

| Document | Purpose |
|----------|---------|
| [CHANGELOG.md](./CHANGELOG.md) | Version history and migration index |
| [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md) | Production deployment steps |
| [PRODUCTION_CHECKLIST.md](./PRODUCTION_CHECKLIST.md) | Pre-launch verification |
| [SYSTEM_ARCHITECTURE.md](./SYSTEM_ARCHITECTURE.md) | Technical architecture |
| [TESTING_SUMMARY.md](./TESTING_SUMMARY.md) | QA results and known test issues |

---

## Upgrade Path to v1.0.0 GA

1. Complete all items in `PRODUCTION_CHECKLIST.md`.
2. Resolve RC1 known limitations marked **blocker** for your deployment.
3. Run full migration on staging: `dotnet run --project Backend/Gym.API -- migrate`.
4. Execute Playwright E2E suite and API QA runner against staging.
5. Promote tag `v1.0.0-RC1` → `v1.0.0` after sign-off.
