# Trainer Management

## Module Overview
Trainer profiles, member assignments, dashboard metrics.

## Navigation
| Page | Route |
|------|-------|
| Trainer list | `/gym-admin/trainers` |
| Trainer detail | `/gym-admin/trainers/:id` |
| Trainer members (trainer portal) | `/trainer/members` |

## Buttons
- Create / edit / delete trainer
- Assign members to trainer (bulk dialog)
- Remove assignment

## Tables
`Trainers`, `TrainerFiles`, assignment via `Members.TrainerId`

## APIs
`TrainersController` — `/api/trainers`

## Stored Procedures
`sp_CreateTrainer`, `sp_UpdateTrainer`, `sp_DeleteTrainer`, `sp_AssignMemberToTrainer`, `sp_GetTrainerDashboard`, etc.

## Components
`trainer-list`, `trainer-detail`, `trainer-form-dialog`, `assign-members-dialog`, `trainer-members`

## Roles
**Gym Admin** — full CRUD; **Trainer** — own profile/members

## SaaS Feature
`TRAINERS`
