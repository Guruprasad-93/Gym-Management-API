# Workout Plans

## Module Overview
Exercise library, workout plan templates, member assignment, progress tracking, exports.

## Navigation
| Page | Route |
|------|-------|
| Plan list | `/gym-admin/workout-plans` |
| Exercise library | `/gym-admin/workout-plans/exercises` |
| New / edit plan | `/gym-admin/workout-plans/new`, `/:id/edit` |
| Member workout view | `/gym-admin/members/:id/workout`, `/member/workout` |

## Buttons
- Create/edit/delete exercise and plan
- Clone plan
- Assign to member (dialog)
- Log progress (`PUT` progress API)
- Export PDF/Excel

## Tables
`ExerciseCategories`, `ExerciseLibrary`, `WorkoutPlans`, `WorkoutPlanExercises`, `AssignedWorkoutPlans`, `MemberWorkoutProgress`

## APIs
`WorkoutPlansController` — `/api/workout-plans/*`

## Components
`workout-plan-list`, `workout-plan-editor`, `exercise-library`, `assign-workout-plan-dialog`, `member-workout-view`, `member-workouts` (tracking)

## Roles
**Gym Admin**, **Trainer**, **Member** (view assigned / track)

## SaaS Feature
`WORKOUT_PLANS`
