# Branch Management

## Module Overview
Multi-branch operations: branches, managers, transfers, targets, announcements, analytics.

## Navigation
| Page | Route |
|------|-------|
| Branch list | `/gym-admin/branches` |
| Dashboard | `/gym-admin/branches/dashboard` |
| Analytics | `/gym-admin/branches/analytics` |
| Transfers | `/gym-admin/branches/transfers` |
| Targets | `/gym-admin/branches/targets` |

## Buttons (typical)
- Create / edit branch
- Assign branch manager
- Transfer member or trainer between branches
- Set branch targets
- Post announcements

## Tables
`Branches`, `BranchManagers`, `BranchTransferHistory`, `BranchTargets`, `BranchAnnouncements`

## APIs
`BranchesController` — `/api/branches/*` (CRUD, dashboard, analytics, transfers, targets, announcements)

## Stored Procedures
`sp_CreateBranch`, `sp_UpdateBranch`, `sp_GetBranchesPaged`, `sp_TransferMemberBranch`, `sp_TransferTrainerBranch`, `sp_GetBranchDashboard`, `sp_GetBranchAnalyticsComparison`, etc.

## Components
`branch-list`, `branch-dashboard`, `branch-analytics`, `branch-transfers`, `branch-targets`

## Roles
**Gym Admin** — `VIEW_BRANCHES`, `MANAGE_BRANCHES`, `TRANSFER_MEMBERS`, `TRANSFER_TRAINERS`

## SaaS Feature
`MULTI_BRANCH`
