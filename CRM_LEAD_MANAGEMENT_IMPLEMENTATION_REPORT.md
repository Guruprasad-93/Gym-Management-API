# CRM & Lead Management Module — Implementation Report

## Overview

Full CRM pipeline for gym owners: capture enquiries, schedule trials, follow up, track conversions, analytics, exports, WhatsApp notifications, and hourly reminders.

## Database

**Script:** `Backend/Gym.Infrastructure/Persistence/Scripts/027_CrmLeadManagementModule.sql`

| Table | Purpose |
|-------|---------|
| `Leads` | Prospect records with source, status, trainer, plan interest |
| `LeadFollowUps` | Scheduled follow-up tasks |
| `LeadTrials` | Trial sessions and feedback |
| `LeadActivities` | Timeline / audit trail entries |

**Stored procedures:** Create/Update/Delete lead, paging/search, assign trainer, schedule trial, record feedback, follow-ups, activities, dashboard, analytics, reminders.

**Lead statuses:** New, Contacted, TrialScheduled, TrialCompleted, FollowUpPending, Converted, Lost

**Lead sources:** WalkIn, Referral, Facebook, Instagram, Google, Website, WhatsApp, Other

## Permissions

| Permission | GymAdmin | Trainer | SuperAdmin |
|------------|----------|---------|------------|
| `VIEW_LEADS` | ✓ | ✓ (assigned) | ✓ |
| `MANAGE_LEADS` | ✓ | — | ✓ |
| `CONVERT_LEADS` | ✓ | — | ✓ |
| `VIEW_LEAD_ANALYTICS` | ✓ | — | ✓ |

## API Endpoints

| Method | Route | Permission |
|--------|-------|------------|
| GET | `/api/leads/dashboard` | VIEW_LEADS |
| GET | `/api/leads/analytics` | VIEW_LEAD_ANALYTICS |
| GET | `/api/leads/followups/pending` | VIEW_LEADS |
| GET | `/api/leads/trials/today` | VIEW_LEADS |
| GET | `/api/leads` | VIEW_LEADS |
| GET | `/api/leads/{id}` | VIEW_LEADS |
| POST | `/api/leads` | MANAGE_LEADS |
| PUT | `/api/leads/{id}` | MANAGE_LEADS |
| PATCH | `/api/leads/{id}/status` | MANAGE_LEADS |
| DELETE | `/api/leads/{id}` | MANAGE_LEADS |
| POST | `/api/leads/assign-trainer` | MANAGE_LEADS |
| POST | `/api/leads/schedule-trial` | MANAGE_LEADS |
| POST | `/api/leads/followup` | MANAGE_LEADS |
| PUT | `/api/leads/followup/{id}` | MANAGE_LEADS |
| POST | `/api/leads/trial-feedback` | VIEW_LEADS / MANAGE_LEADS |
| POST | `/api/leads/convert-member` | CONVERT_LEADS |
| GET | `/api/leads/export/pdf` | VIEW_LEAD_ANALYTICS |
| GET | `/api/leads/export/excel` | VIEW_LEAD_ANALYTICS |

## Conversion Flow

1. Create user + member from lead data
2. Create membership with selected plan
3. Mark lead `Converted` via `sp_ConvertLeadToMember`
4. Audit logs (lead + member)
5. WhatsApp: `NewMemberRegistration` + `LeadConverted`
6. Lead activity entry

## WhatsApp Notifications

| Event | Notification Type |
|-------|-------------------|
| New lead | `LeadCreated` |
| Trial scheduled | `TrialScheduled` |
| Trial reminder (job) | `TrialReminder` |
| Follow-up reminder (job) | `FollowUpReminder` |
| Conversion | `LeadConverted` + member welcome |

## Background Job

`LeadReminderBackgroundJob` — runs hourly, calls `ProcessRemindersAsync` using `sp_GetLeadReminderCandidates`.

## Key Backend Files

- DTOs: `Gym.Application/DTOs/Leads/LeadDtos.cs`
- Service: `LeadService.cs`
- Repository: `LeadRepository.cs`
- Controller: `LeadsController.cs`
- Exporter: `LeadReportExporter.cs`
- Validators: `LeadDtoValidators.cs`
- Constants: `LeadConstants.cs`

## Frontend Routes

| Route | Component |
|-------|-----------|
| `/gym-admin/leads` | List + Kanban |
| `/gym-admin/leads/create` | Create form |
| `/gym-admin/leads/edit/:id` | Edit form |
| `/gym-admin/leads/:id` | Detail + timeline + convert |
| `/gym-admin/leads/followups` | Pending follow-ups |
| `/gym-admin/leads/trials` | Today's trials |
| `/gym-admin/leads/analytics` | KPIs + charts + exports |
| `/trainer/leads` | Trainer assigned leads |

## Deployment

1. Run `027_CrmLeadManagementModule.sql` (auto via migrator on startup)
2. Restart API (seeder adds CRM privileges to roles)
3. Configure WhatsApp templates for lead notification types
4. Re-login users to refresh JWT permissions

## Testing

Integration tests: `Backend/Gym.API.IntegrationTests/LeadManagementTests.cs`

- Lead CRUD
- Follow-up creation
- Trial scheduling
- Tenant isolation
- Analytics/dashboard endpoints

## Build Status

- Backend: `dotnet build GymManagementSaaS.sln` — succeeded
- Frontend: `npm run build` — succeeded

## Notes

- Trainers see only leads assigned to them (`AssignedTrainerId` filter)
- Kanban drag-and-drop updates status via `PATCH /api/leads/{id}/status`
- Super Admin analytics accepts optional `gymId`; omit for platform-wide metrics
- Enterprise/multi-gym demo: use FitZone demo gym admin (`admin@fitzone-demo.com`)
