# Member Management

## Module Overview
Gym member CRUD, detail view, trainer assignment, membership linkage, diet/workout assignment entry points.

## Navigation
| Page | Route |
|------|-------|
| Member list | `/gym-admin/members` |
| Member detail | `/gym-admin/members/:id` |
| Member diet | `/gym-admin/members/:id/diet` |
| Member workout | `/gym-admin/members/:id/workout` |
| Attendance history | `/gym-admin/attendance/members/:id/history` |

## Buttons
| Button | Action |
|--------|--------|
| Add member | Dialog → `POST /api/members` |
| Edit | `PUT /api/members/{id}` |
| Activate / Deactivate | PATCH endpoints |
| Delete | Soft delete `DELETE /api/members/{id}` |
| Assign trainer | `POST /api/members/{id}/assign-trainer` |
| Search / paging | Query params on list API |

## Validation
Member form validators on create/update DTOs (name, contact, branch, etc.)

## Tables
`Members`, `Users` (linked), `MemberFiles`

## APIs
`MembersController` — `/api/members`

## Components
`member-list`, `member-detail`, `member-form-dialog`, `assign-trainer-dialog`

## Roles
**Gym Admin**, **Trainer** (read assigned members)

## SaaS Feature
`MEMBERS`
