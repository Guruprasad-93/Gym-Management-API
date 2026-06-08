# Multi-Branch Management — Implementation Report

## Overview

Multi-branch / franchise management for gym organizations operating multiple locations under one `GymId`. Includes branch CRUD, manager assignment, member/trainer transfers, monthly targets, announcements with WhatsApp, and comparison analytics.

## Database — `030_MultiBranchManagementModule.sql`

**New tables:** `Branches`, `BranchManagers`, `BranchTransferHistory`, `BranchTargets`, `BranchAnnouncements`

**BranchId added to:** `Members`, `Trainers`, `MemberAttendance`, `Payments`, `Leads`, `Expenses`, `Payrolls`

**Backfill:** `sp_EnsureDefaultBranch` creates "Main Branch" per gym and assigns existing records.

> Note: Legacy `GymBranches` table from script 004 remains unused; `Branches` is the canonical table for this module.

## Backend

| Layer | Files |
|-------|-------|
| DTOs | `Gym.Application/DTOs/Branches/BranchDtos.cs` |
| Interfaces | `IBranchRepository.cs` |
| Service | `BranchService.cs` |
| Repository | `BranchRepository.cs` |
| Controller | `BranchesController.cs` (`/api/branches/*`) |

## API Endpoints

- `GET/POST /api/branches`, `GET /api/branches/list`, `GET/PUT/PATCH/DELETE /api/branches/{id}`
- `GET /api/branches/dashboard`, `GET /api/branches/analytics`
- `POST /api/branches/transfers/members`, `POST /api/branches/transfers/trainers`, `GET /api/branches/transfers`
- `GET/POST /api/branches/targets`
- `GET/POST/DELETE /api/branches/announcements`

## Permissions (GymAdmin)

- `VIEW_BRANCHES`, `MANAGE_BRANCHES`, `VIEW_BRANCH_ANALYTICS`, `TRANSFER_MEMBERS`, `TRANSFER_TRAINERS`

## WhatsApp

- Notification type: `BranchAnnouncement`
- Triggered when creating announcement with `sendWhatsApp: true`

## Frontend Routes

- `/gym-admin/branches` — branch list + create
- `/gym-admin/branches/dashboard` — KPI cards per branch
- `/gym-admin/branches/analytics` — Chart.js ranking + comparison
- `/gym-admin/branches/transfers` — member/trainer transfers
- `/gym-admin/branches/targets` — monthly targets + achievement

## Tests

`BranchManagementTests.cs` — CRUD, dashboard, analytics, transfer validation, tenant isolation

## Deployment

See `MULTI_BRANCH_DEPLOYMENT.md`

## Related fixes (prerequisite migrations)

- **028**: Legacy `Expenses` table from script 004 is upgraded in separate `GO` batches before payroll SPs compile.
- **029**: Legacy `MemberProgress` table from script 004 is dropped when incompatible so self-service schema can be created.
- **Integration tests**: `GymWebApplicationFactory` pins `DATABASE_CONNECTION` to the integration test database.
