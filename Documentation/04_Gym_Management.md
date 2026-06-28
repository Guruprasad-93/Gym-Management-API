# Gym Management (Platform)

## Module Overview
Super Admin management of gym tenants (create, activate, deactivate, configure).

## Navigation
`/super-admin/gyms` — Gym list with create/edit dialogs

## Buttons
| Button | API |
|--------|-----|
| Add gym | `POST /api/Gyms` |
| Edit | `PUT /api/Gyms/{id}` |
| Activate / Deactivate | `PATCH /api/Gyms/{id}/activate|deactivate` |
| Delete | `DELETE /api/Gyms/{id}` |

## Tables
`Gyms`, related subscription seed data

## Stored Procedures
`sp_CreateGym`, `sp_GetGymById`, `sp_GetAllGyms`, `sp_UpdateGym`, `sp_DeleteGym`, `sp_SetGymActive`

## APIs
`GymsController` — `/api/Gyms`

## Components
`gym-list`, `gym-form-dialog`

## Roles
**SuperAdmin** — `VIEW_GYMS`, `CREATE_GYM`, `UPDATE_GYM`, etc.

## Related
Gym Admins: `/super-admin/gym-admins` — `GymAdminsController` `/api/gym-admins`
