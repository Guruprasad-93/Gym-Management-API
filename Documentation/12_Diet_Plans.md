# Diet Plans

## Module Overview
Diet categories, meal plans with items, member assignment, exports.

## Navigation
| Page | Route |
|------|-------|
| Plan list | `/gym-admin/diet-plans` |
| New / edit | `/gym-admin/diet-plans/new`, `/:id/edit` |
| Member diet | `/gym-admin/members/:id/diet`, `/member/diet` |

## Buttons
- Create/edit/delete plan and categories
- Clone plan
- Assign to member
- Export PDF/Excel
- Member diet tracking (`/member/diets`)

## Tables
`DietCategories`, `DietPlans`, `DietPlanItems`, `AssignedDietPlans`, `DietTracking`

## APIs
`DietPlansController` — `/api/diet-plans/*`

## Components
`diet-plan-list`, `diet-plan-editor`, `assign-diet-plan-dialog`, `member-diet-view`, `member-diets`

## Roles
**Gym Admin**, **Member**

## SaaS Feature
`DIET_PLANS`
