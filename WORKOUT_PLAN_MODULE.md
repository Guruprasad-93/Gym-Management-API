# Workout Plan Management Module

## Database (`016_WorkoutPlanModule.sql`)

| Table | Purpose |
|-------|---------|
| ExerciseCategories | Per-gym exercise groupings |
| ExerciseLibrary | Exercise catalog (name, muscle, difficulty, instructions) |
| WorkoutPlans | Program header (goal, duration weeks, active) |
| WorkoutPlanExercises | Day-based exercise rows (sets, reps, weight, rest) |
| AssignedWorkoutPlans | Member assignments |
| MemberWorkoutProgress | Per-exercise completion, notes, percentage |

## API (`/api/workout-plans`)

Exercise library: `GET/POST /exercises`, `PUT/DELETE /exercises/{id}`  
Categories: `GET/POST /exercise-categories`  
Plans: CRUD, clone, active toggle  
Assign: `POST /assign`  
Progress: `POST /progress`  
Member views: `GET /members/me`, `GET /members/{id}`  
Export: `GET /{id}/export/pdf|excel`

## Permissions

- `VIEW_WORKOUT_PLANS`
- `MANAGE_WORKOUT_PLANS`
- `ASSIGN_WORKOUT_PLAN`
- `VIEW_MEMBER_WORKOUT`
- `EXPORT_WORKOUT_PLANS`

Re-login after API restart to refresh JWT privileges.

## Frontend routes

| Route | Audience |
|-------|----------|
| `/gym-admin/workout-plans` | Gym admin |
| `/gym-admin/workout-plans/exercises` | Exercise library |
| `/gym-admin/members/:id/workout` | Member workout + progress |
| `/trainer/workout-plans` | Trainer plan list |
| `/trainer/members/:memberId/workout` | Trainer member view |
| `/member/workout` | Member portal |

## Test checklist

1. Restart API (deploys `016_WorkoutPlanModule.sql`).
2. Create exercises in Exercise Library.
3. Create workout plan with multi-day exercises.
4. Clone plan and assign to a member.
5. Mark exercises complete as member; add trainer notes as trainer.
6. Export PDF and Excel.
