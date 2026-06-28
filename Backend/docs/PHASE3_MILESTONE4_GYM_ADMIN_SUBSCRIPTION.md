# Phase 3 — Milestone 4: Gym Admin Subscription Experience

**Status:** Complete  
**Date:** 2026-06-20

## Summary

Gym Admin subscription UX is rebuilt around the **dynamic plan catalog API**. Plans, features, quotas, and pricing options are loaded at runtime — no hardcoded Basic/Premium/Premium Pro. Checkout uses `pricingOptionId` with the existing Razorpay order → verify flow preserved.

---

## Screens Created

| # | Screen | Route | Component |
|---|--------|-------|-----------|
| 1 | **Subscription Layout** (nav shell) | `/gym-admin/subscription/*` | `SubscriptionLayoutComponent` |
| 2 | **Overview / Status** | `/gym-admin/subscription/overview` | `SubscriptionOverviewComponent` |
| 3 | **Plan Catalog** | `/gym-admin/subscription/catalog` | `PlanCatalogComponent` |
| 4 | **Plan Comparison** | `/gym-admin/subscription/compare?plans=1,2` | `PlanCompareComponent` |
| 5 | **Checkout** (purchase / upgrade / renew) | `/gym-admin/subscription/checkout?planId=&pricingOptionId=&action=` | `PlanCheckoutComponent` |
| 6 | **My Features** | `/gym-admin/subscription/my-features` | `MyFeaturesComponent` |
| 7 | **Renew Entry** | `/gym-admin/subscription/renew-subscription` | `RenewSubscriptionRedirectComponent` |

Default `/gym-admin/subscription` redirects to **Overview**.

### Screen capabilities

| Requirement | Implementation |
|-------------|----------------|
| Dynamic plans | `GET /api/saas/plans/catalog` |
| Current plan | Badge on catalog/compare; status card on overview |
| Feature comparison | Compare table grouped by category |
| Quota comparison | Max members, trainers, branches, storage, SMS, WhatsApp |
| Purchase | `action=purchase` checkout |
| Upgrade | `action=upgrade` when target plan ≠ current |
| Renew | `action=renew` from overview, renew route, or same-plan catalog |
| My features | Subscription / enabled / visible menu codes |
| Status UX | Banner, period end, grace end, days to expiry, grace days remaining, trial end |

---

## Routes Added

File: `Frontend/gym-app/src/app/features/gym-admin/gym-admin.routes.ts`

```
/gym-admin/subscription              → layout (redirect → overview)
/gym-admin/subscription/overview
/gym-admin/subscription/catalog
/gym-admin/subscription/compare
/gym-admin/subscription/checkout     (requires MANAGE_SAAS_SUBSCRIPTION)
/gym-admin/subscription/my-features
/gym-admin/renew-subscription        → redirect to checkout (renew)
```

Menu entry unchanged: **Gym Admin → Subscription** → `/gym-admin/subscription`.

---

## Components & Services Created

| File | Purpose |
|------|---------|
| `subscription-layout.component.ts` | Tab navigation (Overview, Catalog, Compare, My Features) |
| `subscription-overview.component.*` | Current plan, dates, grace/expiry, usage KPIs, quick actions |
| `plan-catalog.component.*` | Dynamic plan cards, pricing options, compare selection |
| `plan-compare.component.*` | Feature + quota matrix, pricing CTAs |
| `plan-checkout.component.*` | Unified Razorpay checkout |
| `my-features.component.*` | Entitled features display |
| `renew-subscription-redirect.component.ts` | Legacy renew route → checkout |
| `subscription-checkout.service.ts` | Razorpay order + verify + permission refresh |
| `plan-catalog.utils.ts` | Comparison helpers, checkout action resolution |
| `subscription.shared.css` | Ahana-aligned subscription styles |

### Updated models

- `saas.models.ts` — extended `GymSubscription` and `SaasPaymentOrder` with period/grace/pricing fields

### Superseded (not routed)

- `gym-subscription.component.*` — legacy hardcoded Monthly/Yearly UI (replaced by catalog flow)

---

## APIs Used

| API | Used by |
|-----|---------|
| `GET /api/saas/plans/catalog` | Catalog, Compare, Checkout |
| `GET /api/saas/subscription` | Overview, Catalog, Compare, Checkout, Renew redirect |
| `GET /api/saas/my-features` | My Features |
| `GET /api/saas/usage` | Overview KPIs |
| `POST /api/saas/payments/order` | Checkout (`pricingOptionId`) |
| `POST /api/saas/payments/verify` | Checkout (Razorpay callback) |
| `POST /api/saas/subscription/cancel` | Overview (cancel at period end) |
| `GET /api/auth/session` | Post-payment `refreshPermissions()` |

Legacy path preserved in `SaasSubscriptionService.createPaymentOrderLegacy()` for backward compatibility (not used by new UI).

---

## Razorpay Flow Verification

### New flow (Milestone 4)

```
PlanCatalog / Compare / Overview
  → navigate to Checkout (?planId & pricingOptionId & action)
  → POST /api/saas/payments/order { saasPlanId, pricingOptionId }
  → RazorpayService.openCheckout(order)
  → POST /api/saas/payments/verify { saasPaymentId, razorpayOrderId, ... }
  → AuthService.refreshPermissions()
  → navigate to /gym-admin/subscription/overview
```

### Preserved behavior

- Same `RazorpayService.openCheckout()` used by member payments
- Same verify endpoint and payload shape
- Backend `SaasSubscriptionService.CreatePaymentOrderAsync` resolves price from `pricingOptionId` when provided
- Renewal carry-forward logic unchanged (backend SP)
- Legacy `billingCycle`-only orders still supported via `createPaymentOrderLegacy()`

### Action resolution

| Scenario | `action` |
|----------|----------|
| No access / Trial / Expired | `purchase` |
| Different plan than current | `upgrade` |
| Same plan as current | `renew` |

---

## Build Result

```
Frontend: npm run build --configuration development  → SUCCESS
Output:   Frontend/gym-app/dist/gym-app
```

---

## Test Result

```
dotnet test --filter FeatureDependencyRulesTests|SubscriptionRenewalDateTests
→ Passed: 24, Failed: 0, Skipped: 0
```

No new frontend unit tests added (UI integration tests recommended in Milestone 5).

---

## Production Readiness Status

| Area | Status | Notes |
|------|--------|-------|
| Dynamic catalog UI | ✅ Ready | No hardcoded plan names |
| pricingOptionId checkout | ✅ Ready | Primary path in new UI |
| Razorpay integration | ✅ Ready | Existing service reused |
| Renewal carry-forward | ✅ Ready | Backend unchanged; 24 tests pass |
| RBAC | ✅ Ready | View vs Manage permissions on routes |
| Feature/menu gating | ⚠️ Partial | My Features screen shows resolved codes; full E2E gating verification in M5 |
| Subscription guards/banners | ⚠️ Existing | Pre-existing grace/expiry banners not modified in M4 |
| Production CSS budget | ⚠️ Pre-existing | `saas-data-table.css` budget may fail prod build |
| Screenshots / E2E | ⏳ Milestone 5 | Manual QA + automated E2E recommended |

**Overall:** Milestone 4 is **functionally complete** for gym-admin subscription management. Milestone 5 should add E2E tests (catalog → checkout → verify), subscription guard integration with new routes, and production build budget fix.

---

## Remaining Work — Milestone 5

1. E2E: catalog load → compare → checkout → Razorpay mock verify
2. Subscription access guard allowlist for new child routes
3. Production build CSS budget fix
4. Phase 3 production readiness report
5. Optional: feature name labels on My Features (map codes to friendly names)

---

## Access Formula (unchanged)

```
Subscription Features ∩ Gym Menu Settings ∩ Role Permissions
```
