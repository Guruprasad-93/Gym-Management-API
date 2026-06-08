# Diet Plan Management Module

## Database (`015_DietPlanModule.sql`)

| Table | Purpose |
|-------|---------|
| `DietCategories` | Per-gym categories (Weight Loss, Muscle Gain, etc.) |
| `DietPlans` | Plan header: name, category, target calories, active flag |
| `DietPlanItems` | Meals: meal time, food, quantity, calories, notes |
| `AssignedDietPlans` | Member assignments with start/end dates |

Default categories are seeded for each gym on deploy.

## API (`/api/diet-plans`)

| Method | Route | Permission |
|--------|-------|------------|
| GET | `/categories` | VIEW_DIET_PLANS |
| POST | `/categories` | MANAGE_DIET_PLANS |
| GET | `/` | VIEW_DIET_PLANS |
| GET | `/{id}` | VIEW_DIET_PLANS |
| POST | `/` | MANAGE_DIET_PLANS |
| PUT | `/{id}` | MANAGE_DIET_PLANS |
| DELETE | `/{id}` | MANAGE_DIET_PLANS |
| PATCH | `/{id}/active` | MANAGE_DIET_PLANS |
| POST | `/{id}/clone` | MANAGE_DIET_PLANS |
| POST | `/assign` | ASSIGN_DIET_PLAN |
| DELETE | `/assignments/{id}` | ASSIGN_DIET_PLAN |
| GET | `/members/me` | VIEW_MEMBER_DIET |
| GET | `/members/{memberId}` | VIEW_MEMBER_DIET / VIEW_MEMBERS |
| GET | `/{id}/export/pdf` | EXPORT_DIET_PLANS |
| GET | `/{id}/export/excel` | EXPORT_DIET_PLANS |

Gym admins are scoped to their gym; Super Admin can query all gyms when `GymId` is null in SPs.

## Audit events

- `DietPlan`: Create, Update, Delete, Clone
- `AssignedDietPlan`: Assign, Delete (unassign)

## Frontend

| Route | Role |
|-------|------|
| `/gym-admin/diet-plans` | List, clone, assign, export |
| `/gym-admin/diet-plans/new` | Create |
| `/gym-admin/diet-plans/:id/edit` | Edit |
| `/gym-admin/members/:id/diet` | Member diet view |
| `/member/diet` | Member own diet |

## Permissions

- `VIEW_DIET_PLANS` — Gym Admin, Trainer
- `MANAGE_DIET_PLANS` — Gym Admin
- `ASSIGN_DIET_PLAN` — Gym Admin, Trainer
- `VIEW_MEMBER_DIET` — Gym Admin, Trainer, Member
- `EXPORT_DIET_PLANS` — Gym Admin

Re-login after API restart so JWT includes new privileges.

## Test checklist

1. Restart API (deploys `015_DietPlanModule.sql`).
2. Login as Gym Admin → Diet Plans menu → create plan with meal items.
3. Clone plan → verify copy with items.
4. Assign to member → view `/gym-admin/members/{id}/diet`.
5. Login as Member → My Diet Plan.
6. Export PDF and Excel from list or member view.
