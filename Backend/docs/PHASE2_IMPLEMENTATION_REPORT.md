# Phase 2 Implementation Report — Dynamic Feature-Driven SaaS

**Date:** 2026-06-20  
**Status:** Complete  
**Build:** `GymManagementSystem.sln` — succeeded  
**Tests:** 30/30 SaaS billing + renewal tests passed  

---

## Executive Summary

Phase 2 implements the runtime feature-resolution layer, API enforcement, platform plan management, gym-facing catalog APIs, and payment flow updates on top of the Phase 1 database schema (scripts 059–061). All pre-approval constraints were preserved:

| Constraint | Status |
|---|---|
| PlanQuotas (6 fields) | Extended in `062` |
| Access formula: Plan ∩ Gym Menu ∩ RBAC | Implemented in `FeatureResolverService` |
| Existing RBAC (`RequirePermission`) | Unchanged |
| Renewal carry-forward | Preserved in `sp_Saas_UpdateSubscriptionPlan` |
| Razorpay flow | Preserved; dual-read `PricingOptionId` + `BillingCycle` |

---

## 1. Pre-Implementation Review

### PlanQuotas — Confirmed Support

| Field | Phase 1 | Phase 2 (`062`) |
|---|---|---|
| MaxMembers | ✓ | ✓ (unchanged) |
| MaxTrainers | ✓ | ✓ (unchanged) |
| MaxBranches | — | ✓ Added |
| MaxStorageGB | — (StorageLimitMb) | ✓ Added; migrated from MB |
| MaxSmsPerMonth | — | ✓ Added |
| MaxWhatsappMessages | — (WhatsAppNotificationLimit) | ✓ Added; migrated from legacy column |

Legacy columns `StorageLimitMb` and `WhatsAppNotificationLimit` are retained for backward compatibility. `sp_Saas_CheckTenantLimit` now reads from `PlanQuotas` with fallback to legacy plan columns.

### Access Formula

```
Final Access = Subscription Features ∩ Gym Menu Settings ∩ Role Permissions
```

| Layer | Component | Mechanism |
|---|---|---|
| Subscription | Plan features from active subscription | `sp_Gym_GetEnabledFeatureCodes` |
| Gym menu | SA toggles intersected with plan menus | `sp_Gym_GetVisibleMenuCodes` |
| RBAC | Role permissions via `MenuPermissionMap` | `GetAccessibleMenuCodesAsync`, `RequirePermission` |

**No architectural blockers were found.** Implementation proceeded.

---

## 2. Database Changes

### Script 062 — `062_PlanQuotasExtension.sql`

- Adds `MaxBranches`, `MaxStorageGB`, `MaxSmsPerMonth`, `MaxWhatsappMessages` to `PlanQuotas`
- Backfills from legacy columns
- Updates `sp_PlanQuota_GetByPlanId` and `sp_Saas_CheckTenantLimit`

### Script 063 — `063_Phase2FeatureSubscription.sql`

| Object | Purpose |
|---|---|
| `sp_Feature_GetApiRoutes` | Route → feature mapping for middleware |
| `sp_Saas_UpdateSubscriptionPlan` | Optional `@PricingOptionId`; carry-forward unchanged |
| `sp_Saas_CreatePendingPayment` / `sp_Saas_GetPendingPayment` | Pricing option support |
| `sp_Saas_GetAllPlans` / `GetPlanById` / `GetPlanByCode` | Extended quota + metadata columns |
| `sp_Saas_GetPlanCatalog` | Gym/public catalog (plans + pricing + features) |
| `sp_Saas_Platform_*` | Create/update/delete plan |
| `sp_PlanQuota_Upsert` | Quota CRUD |
| `sp_PlanFeature_SetForPlan` | Feature assignment |
| `sp_PlanPricing_*` | Pricing option CRUD |

**Total migration scripts:** 63 (001–063)

---

## 3. Backend Services

### FeatureResolverService (`IFeatureResolverService`)

| Method | Description |
|---|---|
| `GetSubscriptionFeatureCodesAsync` | Plan entitlements only |
| `GetVisibleMenuCodesAsync` | Plan menus ∩ gym-enabled menus |
| `GetAccessibleMenuCodesAsync` | Above ∩ RBAC (`MenuPermissionMap`) |
| `GetEnabledFeatureCodesAsync` | Plan features gated by gym menu visibility |
| `HasFeatureAsync` | Used by middleware and `[RequireFeature]` |
| `IsMenuVisibleAsync` | Used by `GymMenuAccessMiddleware` |
| `ResolveFeatureCodeForPathAsync` | DB routes + `ApiRouteFeatureMap` fallback |

### Updated Services

| Service | Change |
|---|---|
| `GymMenuService` | Uses `FeatureResolverService` for menu visibility |
| `AuthService` | Session includes `enabledMenuCodes` (full intersection) + `enabledFeatureCodes` |
| `SaasSubscriptionService` | `PricingOptionId` path; `IsTrialPlan` check (not hardcoded plan code) |
| `PlanManagementService` | Platform CRUD + gym catalog + my-features |

---

## 4. Middleware & Authorization

### Pipeline Order (`Program.cs`)

```
SubscriptionAccessMiddleware → FeatureAccessMiddleware → GymMenuAccessMiddleware
```

| Component | Role |
|---|---|
| `FeatureAccessMiddleware` | 403 when route maps to a feature not in subscription ∩ gym |
| `GymMenuAccessMiddleware` | 403 when route maps to menu not visible (plan ∩ gym) |
| `RequirePermission` | RBAC — unchanged |
| `[RequireFeature("CODE")]` | Declarative feature check via `FeatureAuthorizationHandler` |

Super Admin bypasses feature and menu middleware checks.

---

## 5. API Endpoints

### Platform (Super Admin) — `/api/platform/subscription-plans`

| Method | Route | Permission |
|---|---|---|
| GET | `/features` | `MANAGE_SUBSCRIPTION_PLANS` |
| GET | `/` | `MANAGE_SUBSCRIPTION_PLANS` |
| GET | `/{id}` | `MANAGE_SUBSCRIPTION_PLANS` |
| POST | `/` | `MANAGE_SUBSCRIPTION_PLANS` |
| PUT | `/{id}` | `MANAGE_SUBSCRIPTION_PLANS` |
| DELETE | `/{id}` | `MANAGE_SUBSCRIPTION_PLANS` |
| POST | `/{planId}/pricing-options` | `MANAGE_SUBSCRIPTION_PLANS` |
| PUT | `/pricing-options/{id}` | `MANAGE_SUBSCRIPTION_PLANS` |
| DELETE | `/pricing-options/{id}` | `MANAGE_SUBSCRIPTION_PLANS` |

### Gym — `/api/saas`

| Method | Route | Description |
|---|---|---|
| GET | `/plans/catalog` | Dynamic plan catalog with pricing + features |
| GET | `/my-features` | Subscription, enabled, and visible menu codes |
| POST | `/payments/order` | Accepts `pricingOptionId` (preferred) or `billingCycle` (legacy) |

### Public Onboarding — `/api/onboarding/plans`

Returns `SaasPlanCatalogDto` (public plans with pricing options and features).

---

## 6. Payment & Renewal Preservation

### Razorpay Flow (unchanged sequence)

1. `POST /api/saas/payments/order` → creates Razorpay order + pending payment
2. Client completes Razorpay checkout
3. `POST /api/saas/payments/verify` → signature verify → `sp_Saas_UpdateSubscriptionPlan`

### Dual-read pricing

```json
{ "saasPlanId": 2, "pricingOptionId": 5 }
// OR legacy:
{ "saasPlanId": 2, "billingCycle": "Quarterly" }
```

### Carry-forward logic (unchanged from 057)

Renewal period start = day after existing `CurrentPeriodEnd` when subscription is still active/in grace. Only period-end calculation switches to `fn_CalculateSubscriptionPeriodEnd` when `@PricingOptionId` is provided.

---

## 7. Session Payload Changes

Login and session refresh now include:

```json
{
  "enabledMenuCodes": ["DASHBOARD", "MEMBERS", ...],
  "enabledFeatureCodes": ["DASHBOARD", "MEMBERS", ...]
}
```

- `enabledMenuCodes` = Plan ∩ Gym Menu ∩ RBAC  
- `enabledFeatureCodes` = Plan features gated by gym menu toggles  

---

## 8. New Files

| Path | Purpose |
|---|---|
| `062_PlanQuotasExtension.sql` | Quota column extension |
| `063_Phase2FeatureSubscription.sql` | SP updates + platform CRUD |
| `FeatureResolverService.cs` | Core access resolver |
| `PlanManagementService.cs` | Platform + catalog service |
| `FeatureRepository.cs` | Feature SP access |
| `PlanManagementRepository.cs` | Plan/pricing SP access |
| `FeatureAccessMiddleware.cs` | API feature enforcement |
| `RequireFeatureAttribute.cs` | Declarative feature auth |
| `FeatureAuthorizationHandler.cs` | Feature policy handler |
| `ApiRouteFeatureMap.cs` | C# route fallback |
| `FeatureDtos.cs` | Phase 2 DTOs |
| `PlatformSubscriptionPlansController.cs` | Platform CRUD API |

---

## 9. Verification

| Check | Result |
|---|---|
| Solution build | ✓ Pass |
| SaasBillingCycleTests (30) | ✓ Pass |
| SubscriptionRenewalDateTests | ✓ Pass (included in filter) |
| RBAC `RequirePermission` | ✓ Unmodified |
| Carry-forward SP logic | ✓ Preserved |
| Razorpay order/verify | ✓ Wired with `PricingOptionId` |

---

## 10. Operational Notes

1. **Seed privilege:** Add `MANAGE_SUBSCRIPTION_PLANS` to Super Admin role in production (or via existing privilege seed script).
2. **Apply migrations:** Restart API against target DB; migrator applies `062` and `063` automatically.
3. **Frontend:** Onboarding `/plans` response shape changed from `SaasPlanDto[]` to `SaasPlanCatalogDto` — update onboarding UI to consume catalog structure.
4. **Deprecation path:** `SaasBillingCycleHelper` and legacy price columns remain for backward compatibility; new purchases should prefer `pricingOptionId`.

---

## 11. Phase 2 Checklist (Completed)

- [x] Extend PlanQuotas (6 quota fields)
- [x] FeatureResolverService (Plan ∩ Gym ∩ RBAC)
- [x] FeatureAccessMiddleware
- [x] `[RequireFeature]` attribute + handler
- [x] Platform Plan CRUD APIs
- [x] Gym Plan Catalog APIs
- [x] Pricing Option APIs
- [x] Payment SP updates (`PricingOptionId`)
- [x] Subscription feature resolution in auth/session/menus
- [x] Preserve RBAC, renewal carry-forward, Razorpay flow
- [x] Build + targeted tests
- [x] Implementation report

---

## 12. Recommended Phase 3 (Out of Scope)

- Frontend: dynamic plan picker using catalog + pricing options
- Privilege seed migration for `MANAGE_SUBSCRIPTION_PLANS`
- Integration tests against live DB for feature middleware 403 paths
- Remove legacy `SaasPlanCodes` / hardcoded billing cycle UI after frontend migration
