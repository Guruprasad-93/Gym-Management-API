# Audit Logging Module

## Overview

Centralized audit trail for security and compliance: who did what, when, from which IP, with optional before/after JSON snapshots.

## Database (`014_AuditModule.sql`)

Table `AuditLogs` (from `004_FutureTablesSchema.sql`) extended with:

| Column | Notes |
|--------|--------|
| EntityName | Member, Trainer, Membership, etc. |
| EntityId | Primary key as string |
| Action | Create, Update, Delete, Login, CheckIn, … |
| OldValues / NewValues | JSON snapshots |
| UserId, GymId | Actor and tenant scope |
| IpAddress | Client IP (added in 014) |
| CreatedAt | UTC timestamp |

Stored procedures:

- `sp_AuditLog_Insert` — write log entry
- `sp_SearchAuditLogs` — paged search with filters
- `sp_GetAuditLogSummary` — dashboard aggregates

## Tracked events

| Entity | Actions |
|--------|---------|
| Member | Create, Update, Delete |
| Trainer | Create, Update, Delete |
| Membership | Create, Renew, Cancel |
| MembershipPlan | Create, Update, Delete |
| Payment | Create |
| MemberAttendance | CheckIn, CheckOut, Mark |
| TrainerAttendance | CheckIn, CheckOut |
| Auth | Login, Logout |

## API

Base: `GET /api/audit-logs`

| Endpoint | Permission |
|----------|------------|
| `GET /api/audit-logs` | VIEW_AUDIT_LOGS |
| `GET /api/audit-logs/dashboard` | VIEW_AUDIT_LOGS |
| `GET /api/audit-logs/export/pdf` | EXPORT_AUDIT_LOGS |
| `GET /api/audit-logs/export/excel` | EXPORT_AUDIT_LOGS |

Query filters: `userId`, `entityName`, `actionType`, `entityId`, `search` (user name/email), `fromDate`, `toDate`, paging.

Gym admins see only their gym; Super Admin sees all gyms.

## Permissions

- `VIEW_AUDIT_LOGS` — Gym Admin, Super Admin
- `EXPORT_AUDIT_LOGS` — Gym Admin, Super Admin

Re-login after seeding so JWT includes new permissions.

## Frontend

- Gym Admin: `/gym-admin/audit`
- Super Admin: `/super-admin/audit`

Filters: date range, entity, action, entity ID, user search. PDF/Excel export when `EXPORT_AUDIT_LOGS` is granted.

## Test checklist

1. Restart API so `014_AuditModule.sql` deploys.
2. Login as Gym Admin — verify Audit Logs menu and dashboard load.
3. Create/update/delete a member — confirm audit rows with JSON.
4. Check in a member — MemberAttendance + CheckIn logged.
5. Logout and login — Auth Login/Logout entries with IP.
6. Export PDF and Excel with current filters.
7. Super Admin — audit shows multiple gyms (if demo data spans gyms).
