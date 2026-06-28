# Phase 3 — Milestone 3: Super Admin Plan Management UI

**Status:** Complete  
**Date:** 2026-06-20

## Summary

Super Admin subscription plan management UI is implemented using the existing Ahana design system (gym-page layout, table-shell, status badges, Material forms/dialogs). Plans can be listed, viewed, created, edited, cloned, and activated/deactivated. Hard delete is not exposed in the UI.

---

## Screens Created

| Screen | Route | Component |
|--------|-------|-----------|
| Plan List | `/super-admin/subscription-plans` | `SubscriptionPlanListComponent` |
| Create Plan | `/super-admin/subscription-plans/new` | `SubscriptionPlanFormComponent` (create mode) |
| View Plan | `/super-admin/subscription-plans/:id` | `SubscriptionPlanFormComponent` (view mode) |
| Edit Plan | `/super-admin/subscription-plans/:id/edit` | `SubscriptionPlanFormComponent` (edit mode) |

### Plan List columns

- Plan Name (+ code subtitle)
- Status (Active / Inactive)
- Active Subscribers
- Features Count
- Pricing Options Count
- Trial Plan indicator
- Created Date

### Plan List actions

- **View** — read-only form
- **Edit** — full edit form
- **Clone** — dialog with default name `"{Original} Copy"`, copies features/pricing/quotas/deps via API
- **Activate / Deactivate** — `PUT /api/platform/subscription-plans/{id}` with `isActive` toggle (no hard delete)

### Create/Edit form sections

| Section | Capabilities |
|---------|----------------|
| **A. Basic Information** | Plan name, code (auto-slug on create), description, trial toggle, trial days, active status (edit) |
| **B. Features** | Grouped by category, search, select/deselect, live dependency validation + highlight |
| **C. Pricing Options** | Add / edit / delete / reorder (up-down); duration value, unit (Day/Month/Year), price |
| **D. Quotas** | Max members, trainers, branches, storage GB, SMS, WhatsApp; unlimited toggle (`-1`) |

---

## Routes Added

File: `Frontend/gym-app/src/app/features/super-admin/super-admin.routes.ts`

```
subscription-plans
subscription-plans/new
subscription-plans/:id
subscription-plans/:id/edit
```

All routes guarded by `permissionGuard(Permissions.ManageSubscriptionPlans)`.

Menu entry already present: **Super Admin → Subscription Plans** (`menu.config.ts`).

---

## Components Created

| Component | Path |
|-----------|------|
| `SubscriptionPlanListComponent` | `features/super-admin/subscription-plans/plan-list.component.*` |
| `SubscriptionPlanFormComponent` | `features/super-admin/subscription-plans/plan-form.component.*` |
| `PlanFeaturePickerComponent` | `features/super-admin/subscription-plans/plan-feature-picker.component.*` |
| `PlanQuotaEditorComponent` | `features/super-admin/subscription-plans/plan-quota-editor.component.*` |
| `PlanCloneDialogComponent` | `features/super-admin/subscription-plans/plan-clone-dialog.component.ts` |
| `PricingOptionDialogComponent` | `features/super-admin/subscription-plans/pricing-option-dialog.component.ts` |

### Shared utilities / styles

- `subscription-plans.shared.css` — Ahana-aligned page/table/form styles
- `feature-dependency.rules.ts` — client-side dependency validation (mirrors backend rules)
- `plan.utils.ts` — plan code slugify, clone name helper

---

## Services Used

| Service | Purpose |
|---------|---------|
| `PlanManagementService` | Platform plan CRUD, clone, features, dependencies, validation, pricing |
| `DialogService` | Clone confirm, pricing dialogs, activate/deactivate confirm |
| `NotificationService` | Success/error toasts |
| `AuthService` | Permission context (via route guard) |

---

## API Integrations

Base: `/api/platform/subscription-plans` (requires `MANAGE_SUBSCRIPTION_PLANS`)

| UI action | API |
|-----------|-----|
| Load list | `GET /` |
| View / edit load | `GET /{id}` |
| Create | `POST /` + `POST /{id}/pricing-options` (per option) |
| Update | `PUT /{id}` |
| Clone | `POST /{id}/clone` |
| Activate / deactivate | `PUT /{id}` (`isActive`) |
| Feature catalog | `GET /features` |
| Dependency map | `GET /feature-dependencies` |
| Save validation | `POST /validate-features` |
| Pricing CRUD | `POST/PUT/DELETE /pricing-options/*`, `PUT /{planId}/pricing-options/reorder` |

### Backend addition (Milestone 3)

- SQL `065_Phase3PlanListCreatedAt.sql` — exposes `CreatedAt` on `sp_Saas_Platform_ListPlans`
- `PlanSummaryDto.CreatedAt` + repository mapping

---

## Dependency Validation UX

Rules (same as backend):

| Feature | Requires |
|---------|----------|
| WEBSITE_BUILDER | PUBLIC_WEBSITE |
| AI_INSIGHTS | REPORTS |
| MULTI_BRANCH | MEMBERS, TRAINERS |

Behavior:

1. Validation runs immediately on feature toggle (client-side).
2. Friendly messages, e.g. *"Website Builder requires Public Website."*
3. Missing dependency rows are highlighted (red border/background).
4. Save blocked if client or server validation fails (`POST /validate-features`).

---

## Build Result

```
Frontend: npm run build --configuration development  → SUCCESS
Output:   Frontend/gym-app/dist/gym-app
Backend:  dotnet build → verify locally after SQL 065 migration
```

> Production `ng build` may still fail on pre-existing CSS budget limits (`saas-data-table.css`); not introduced by Milestone 3.

---

## Screenshot Paths (capture after running app)

Place screenshots under `docs/screenshots/phase3/milestone3/`:

| File | Screen |
|------|--------|
| `docs/screenshots/phase3/milestone3/01-plan-list.png` | Plan list with filters |
| `docs/screenshots/phase3/milestone3/02-plan-create-basic.png` | Create plan — basic info |
| `docs/screenshots/phase3/milestone3/03-plan-features-validation.png` | Feature picker with dependency highlight |
| `docs/screenshots/phase3/milestone3/04-plan-pricing.png` | Pricing options table |
| `docs/screenshots/phase3/milestone3/05-plan-quotas.png` | Quota editor with unlimited toggles |
| `docs/screenshots/phase3/milestone3/06-plan-clone-dialog.png` | Clone dialog |

*Screenshots are not auto-generated; capture while logged in as Super Admin with `MANAGE_SUBSCRIPTION_PLANS`.*

---

## Remaining Work — Milestone 4 (Gym Admin Subscription Experience)

1. **Plan catalog page** — browse public plans from `GET /api/saas/plans/catalog`
2. **Plan comparison matrix** — feature/pricing side-by-side
3. **Purchase / upgrade / renew** — dynamic `pricingOptionId` + Razorpay flow (preserve legacy path)
4. **My features display** — `GET /api/saas/my-features` for entitled modules
5. **Subscription status UX** — trial/active/past-due messaging, renewal carry-forward visibility
6. **Menu gating verification** — subscription ∩ gym menu ∩ role permissions in real gym sessions

---

## Access Formula (unchanged)

```
Subscription Features ∩ Gym Menu Settings ∩ Role Permissions
```

Razorpay flow, renewal carry-forward, existing RBAC, and gym menu overrides remain intact.
