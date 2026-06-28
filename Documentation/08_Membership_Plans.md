# Membership Plans

## Module Overview
Define membership products (duration, price, features) for the gym.

## Navigation
`/gym-admin/membership-plans`

## Buttons
- Create plan dialog
- Edit plan
- Delete plan (if no active memberships)

## Tables
`MembershipPlans`

## APIs
`MembershipPlansController` — `/api/membership-plans`

## Components
`membership-plan-list`, `membership-plan-form-dialog`

## Roles
**Gym Admin** — `VIEW_MEMBERSHIPS`, create/update/delete permissions

## SaaS Feature
`MEMBERSHIPS`

## Related
Active memberships: `/gym-admin/memberships`, expired: `/gym-admin/memberships/expired` — `MembershipsController`
