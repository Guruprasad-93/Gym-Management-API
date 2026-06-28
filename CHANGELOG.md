# Changelog

All notable changes to the Gym Management SaaS platform are documented in this file.

Format based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).  
Versioning follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0-RC1] — 2026-06-20

First release candidate for production deployment.

### Added

#### Authentication & Platform
- **LoginIdentifier authentication** — primary tenant login field; email optional (`051_LoginIdentifier.sql`)
- **Tenant Menu Management** — per-gym module enable/disable (`052_TenantMenuManagement.sql`)
- Cookie-based auth with CSRF protection and refresh token rotation
- Session permission refresh via `GET /api/auth/session`
- Super Admin platform dashboard and gym onboarding
- SaaS subscription module with trial limits and grace period

#### Core Gym Operations
- Member module with LoginIdentifier accounts, trainer assignment, soft delete
- Trainer module with member assignment dashboard
- Membership plans and active membership lifecycle (assign, renew, expire)
- Payment recording, history, and revenue reporting
- Razorpay online payment integration (optional)
- Attendance check-in/check-out and reporting

#### CRM & Wellness
- CRM / Lead management with pipeline, conversion to member, analytics
- Diet plan builder, categories, member assignment, PDF export
- Workout plan builder, exercise library, member assignment, completion tracking
- WhatsApp notification templates, settings, test send, and history
- Booking and slot reservation for trainer schedules

#### Business & SaaS
- Business analytics dashboard (revenue, members, trainers)
- Multi-branch management with branch dashboard and member transfer
- White-label branding (gym + platform), login page customization, preview
- Public gym website builder with page publish and lead capture forms
- Member self-service portal (workouts, diet, goals, progress, bookings)
- Payroll and expense management
- AI trainer recommendations, churn risk, lead scoring (Mock/OpenAI)
- Mobile push notification campaigns (Firebase/Mock)
- File management with local/Azure blob storage
- Audit logging across platform and gym entities

#### Frontend (Angular)
- Role-based layouts: Super Admin, Gym Admin, Trainer, Member, Public Website
- Tenant menu guard and dynamic sidebar
- Playwright E2E suite covering 16 modules (46 tests)
- Global HTTP error interceptor and Angular ErrorHandler
- White-label theming and branded login pages

#### Testing & DevOps
- `Gym.API.IntegrationTests` project (97 tests across 14 test classes)
- PowerShell API QA runner (`Backend/scripts/e2e-qa-runner.ps1`)
- Docker Compose for local API + SQL Server
- Azure deployment guide and backup/disaster recovery documentation

### Changed

- **BREAKING:** Login uses `loginIdentifier` instead of email for authentication requests
- **BREAKING:** User creation forms require Login ID; email is optional
- **BREAKING:** Disabled tenant menus block API access (403) even if user has permission
- Password reset flow updated to accept LoginIdentifier
- All business data access consolidated to stored procedures via Dapper
- Paging validation: `pageSize` capped at 100 for member lists
- Production defaults: migrations and seed disabled on startup

### Fixed (selected)

- Super Admin gym admin list paging (`036_SuperAdminGymAdminListFix.sql`)
- Super Admin audit log queries (`037_SuperAdminAuditLogsFix.sql`)
- Trainer dashboard statistics (`038_TrainerDashboardFix.sql`)
- Branch paging and duplicate branch codes (`039`, `046`)
- Lead paging and CRM queries (`040_LeadsPagedFix.sql`)
- Expense date filtering (`041_ExpensesCreatedDateFix.sql`)
- Diet/workout exercise seed data (`042`–`044`)
- Gym logo branding sync (`045_GymLogoBrandingSync.sql`)
- Branch member transfer (`047_BranchTransferMemberFix.sql`)
- Push campaign filters and history (`048`, `049`)
- Financial trend ordering (`050_FinancialTrendOrderingFix.sql`)

### Security

- HttpOnly secure cookies in production
- CSRF validation on mutating requests
- Rate limiting on auth endpoints (10 req/min default)
- GymId isolation enforced in services and stored procedures
- Forwarded headers for reverse proxy / Azure App Service
- JWT secret minimum 32 characters enforced at startup

---

## Database Migrations (001–052)

Scripts are embedded in `Backend/Gym.Infrastructure` and applied by `DatabaseMigrator` in numeric order. Applied scripts are recorded in `dbo.SchemaVersions`.

| # | Script | Module / Purpose |
|---|--------|------------------|
| 001 | `AuthorizationSchema.sql` | Roles, privileges, role-privilege mappings |
| 002 | `StoredProcedures.sql` | Initial authorization stored procedures |
| 003 | `MvpBusinessSchema.sql` | Core business tables (gyms, users foundation) |
| 004 | `FutureTablesSchema.sql` | Forward-compatible schema placeholders |
| 005 | `MvpApiStoredProcedures.sql` | MVP API stored procedures |
| 006 | `UserAuthColumns.sql` | User authentication columns |
| 007 | `AuthStoredProcedures.sql` | Login, register, password reset procedures |
| 008 | `GymAdminModule.sql` | Gym admin management |
| 009 | `StandardStoredProcedureNames.sql` | Standardized SP naming, TRY/CATCH patterns |
| 010 | `TrainerModule.sql` | Trainers, assignments, trainer CRUD SPs |
| 011 | `MemberModule.sql` | Members, paging, member CRUD SPs |
| 012 | `MembershipPaymentModule.sql` | Memberships, plans, payments |
| 013 | `AttendanceModule.sql` | Attendance tracking and reports |
| 014 | `AuditModule.sql` | Audit log tables and procedures |
| 015 | `DietPlanModule.sql` | Diet plans, categories, assignments |
| 016 | `WorkoutPlanModule.sql` | Workout plans, exercises, assignments |
| 017 | `FileManagementModule.sql` | File metadata and storage references |
| 018 | `ProductionHardening.sql` | Production security and config tables |
| 019 | `SchemaVersions.sql` | Migration tracking table |
| 020 | `RefreshTokenHardening.sql` | Refresh token security improvements |
| 021 | `PerformanceIndexes.sql` | Indexes for attendance, membership, payment, audit |
| 022 | `StoredProcedureOptimization.sql` | SP performance tuning |
| 023 | `RazorpayPaymentModule.sql` | Online payment integration schema |
| 024 | `WhatsAppNotificationModule.sql` | Notification templates, settings, logs |
| 025 | `BusinessAnalyticsModule.sql` | Analytics dashboards and KPIs |
| 026 | `SaasSubscriptionModule.sql` | Subscription plans, limits, billing |
| 027 | `CrmLeadManagementModule.sql` | Leads, pipeline, conversion |
| 028 | `PayrollExpenseManagementModule.sql` | Payroll and expenses |
| 029 | `MemberSelfServiceModule.sql` | Member portal features |
| 030 | `MultiBranchManagementModule.sql` | Branches, transfers, branch dashboard |
| 031 | `MobileAppPushNotificationModule.sql` | Push tokens and campaigns |
| 032 | `AiTrainerRecommendationModule.sql` | AI insights and recommendations |
| 033 | `BookingAndSlotReservationModule.sql` | Schedules, slots, bookings |
| 034 | `PublicGymWebsiteBuilderModule.sql` | Website pages, sections, public forms |
| 035 | `WhiteLabelSaasModule.sql` | Branding settings and theming |
| 036 | `SuperAdminGymAdminListFix.sql` | Super Admin gym admin list fix |
| 037 | `SuperAdminAuditLogsFix.sql` | Super Admin audit log fix |
| 038 | `TrainerDashboardFix.sql` | Trainer dashboard statistics fix |
| 039 | `BranchesPagedFix.sql` | Branch list paging fix |
| 040 | `LeadsPagedFix.sql` | Lead list paging fix |
| 041 | `ExpensesCreatedDateFix.sql` | Expense date column fix |
| 042 | `DietCategoriesSeedFix.sql` | Diet category seed data fix |
| 043 | `WorkoutExerciseLibrarySeedFix.sql` | Exercise library seed fix |
| 044 | `WorkoutExerciseSeedHardening.sql` | Exercise seed hardening |
| 045 | `GymLogoBrandingSync.sql` | Logo/branding synchronization |
| 046 | `BranchCodeDuplicateFix.sql` | Branch code uniqueness fix |
| 047 | `BranchTransferMemberFix.sql` | Branch member transfer fix |
| 048 | `PushCampaignAudienceFilters.sql` | Push campaign audience filters |
| 049 | `PushCampaignHistoryFix.sql` | Push campaign history fix |
| 050 | `FinancialTrendOrderingFix.sql` | Financial trend report ordering |
| 051 | `LoginIdentifier.sql` | **LoginIdentifier column, backfill, auth SP updates** |
| 052 | `TenantMenuManagement.sql` | **Menus catalog, GymMenus, menu access SPs** |

---

## Breaking Changes Summary (upgrade from pre-RC builds)

| Change | Impact | Migration |
|--------|--------|-----------|
| Email → LoginIdentifier login | All login forms and API auth payloads must use `loginIdentifier` | 051 |
| Email optional on user create | Validation changed; empty email allowed | 051 |
| Tenant menu enforcement | Disabled modules return 403; sidebar hidden | 052 |
| Cookie auth default | Bearer-only clients must enable cookies or use CSRF flow | 018, 020 |
| Demo seed disabled in production | Fresh deploy requires bootstrap Super Admin credentials | Config |
| `pageSize` max 100 | Clients requesting >100 members get HTTP 400 | 011 |

---

## [Unreleased]

Planned for v1.0.0 GA:
- CI/CD pipeline YAML for automated E2E and integration tests
- Trainer limit quota fix (exclude soft-deleted from count)
- Member dropdown pageSize alignment in assign dialogs
- Full multi-browser E2E gate in CI

---

[1.0.0-RC1]: https://github.com/Guruprasad-93/Gym-Management-API/releases/tag/v1.0.0-RC1
