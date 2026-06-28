# Database

## Module Overview
SQL Server database accessed via Dapper stored procedures. Schema evolved through 76 numbered scripts in `Backend/Gym.Infrastructure/Persistence/Scripts/`.

## Architecture
- **EF Core:** `Users` table migrations only (`ApplicationDbContext`)
- **SQL scripts:** All business tables and SPs via `DatabaseMigrator` on API startup
- **Tracking:** `SchemaVersions` / applied script log in database
- **Tenancy:** `GymId` on tenant-scoped tables; SPs always accept `@GymId`

## Object Counts
| Type | Count |
|------|------:|
| Tables | 110 |
| Stored procedures (unique) | 497 |
| Functions | 2 |
| Views | 0 |
| Triggers | 0 |

## Table Groups (by domain)

| Domain | Key tables |
|--------|------------|
| Auth/RBAC | Users, Roles, Privileges, RolePrivileges, UserRoles, RefreshTokens |
| Core gym | Gyms, Members, Trainers, Branches, MembershipPlans, Memberships, Payments |
| Attendance | MemberAttendance, TrainerAttendance, AttendanceStatuses |
| Diet/Workout | DietPlans, DietPlanItems, AssignedDietPlans, WorkoutPlans, ExerciseLibrary, AssignedWorkoutPlans |
| Bookings | ClassSchedules, SlotBookings, BookingWaitlist, BookingSettings |
| CRM | Leads, LeadFollowUps, LeadTrials, LeadActivities |
| Financial | Expenses, ExpenseCategories, Payrolls, TrainerCommissions |
| Member self-service | MemberGoals, MemberProgress, WaterIntakeLogs, WorkoutTracking, DietTracking, MemberReferrals, MemberFeedback, MemberQrTokens |
| SaaS | SaasSubscriptionPlans, PlanFeatures, PlanPricingOptions, SaasSubscriptionPayments, SystemFeatures |
| Website | GymWebsiteSettings, GymWebsitePages, GymWebsiteSections, GymWebsiteGallery, GymWebsiteTestimonials, WebsiteLeadCaptures |
| White label | WhiteLabelSettings, WhiteLabelEmailTemplates, WhiteLabelMobileSettings |
| Notifications | NotificationTemplates, NotificationSettings, NotificationLogs, DeviceTokens, PushNotifications |
| AI | AiRecommendations, AiInsights, MemberRiskScores, AiGenerationLogs |
| Menus | Menus, GymMenus, FeatureMenus, FeatureApiRoutes |
| Audit | AuditLogs |

## Legacy / Schema-only Tables
Tables from `004_FutureTablesSchema.sql` without full application modules: `SupportTickets`, `Coupons`, `PaymentMethods`, `GymBranches` (superseded by `Branches`), `SmsTemplates`, `ActivityLogs`, `SystemSettings`, etc.

## Full Inventory
See **[DATABASE_OBJECTS.md](./DATABASE_OBJECTS.md)** for complete table and stored procedure lists.

## Migration Scripts (latest)
`076_ClassScheduleHardDelete.sql` — permanent schedule delete with cascade

## Conventions
- `CREATE OR ALTER PROCEDURE` for idempotent SP updates
- Soft delete pattern on some entities (Members, WorkoutPlans); **hard delete** on class schedules
- `SET XACT_ABORT ON` + transactions on critical deletes
