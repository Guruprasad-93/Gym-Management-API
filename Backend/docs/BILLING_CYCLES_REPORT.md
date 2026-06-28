# SaaS Billing Cycles — Implementation Report

**Generated:** 2026-06-21  
**Scope:** Monthly, Quarterly (3 months), Half-Yearly (6 months), Yearly (12 months)

---

## Supported Billing Cycles

| Cycle | Code (API/DB) | Duration | Period End Formula | Example (Start 01-Jul-2026) |
|-------|---------------|----------|--------------------|-----------------------------|
| Monthly | `Monthly` | 1 month | `PeriodStart + 1 month` | End **01-Aug-2026** |
| Quarterly | `Quarterly` | 3 months | `PeriodStart + 3 months` | End **01-Oct-2026** |
| Half-Yearly | `HalfYearly` | 6 months | `PeriodStart + 6 months` | End **01-Jan-2027** |
| Yearly | `Yearly` | 12 months | `PeriodStart + 1 year` | End **01-Jul-2027** |

**Aliases accepted (normalized to `HalfYearly`):** `Half-Yearly`, `HALF YEARLY`, `halfyearly`, `HALF_YEARLY`

---

## Renewal Date Logic (All Cycles)

Carry-forward rules are **billing-cycle agnostic** — only the period length changes.

| Scenario | Condition | New Period Start | New Period End |
|----------|-----------|------------------|----------------|
| **Fresh start** | No prior subscription | Payment date (today) | `fn_Saas_CalculatePeriodEnd(Start, Cycle)` |
| **Early renewal** | Today ≤ current period end OR within grace | `CurrentPeriodEnd + 1 day` | + cycle duration |
| **Grace renewal** | Today ≤ `GraceEndsAt` (3 days after period end) | `CurrentPeriodEnd + 1 day` | + cycle duration |
| **Expired renewal** | Today > `GraceEndsAt` OR period ended with no grace | Payment date (today) | + cycle duration |

**Grace period:** 3 days after `CurrentPeriodEnd` (`GraceEndsAt = PeriodEnd + 3 days`).

### Worked Examples

**Early renewal (Quarterly)** — Period ends 30-Jun-2026, grace until 03-Jul-2026, payment on 10-Jun-2026:

- Start: **01-Jul-2026** (preserves remaining paid days)
- End: **01-Oct-2026**

**Grace renewal (Half-Yearly)** — Period ended 01-Jun-2026, grace until 04-Jun-2026, payment on 03-Jun-2026:

- Start: **02-Jun-2026**
- End: **02-Dec-2026**

**Expired renewal (Yearly)** — Grace ended 04-Jun-2026, payment on 05-Jun-2026:

- Start: **05-Jun-2026**
- End: **05-Jun-2027**

---

## Implementation Summary

### Database — `057_SaasBillingCycles.sql`

- Added `QuarterlyPrice`, `HalfYearlyPrice` columns to `SaasSubscriptionPlans`
- Seeded defaults: `QuarterlyPrice = MonthlyPrice × 3`, `HalfYearlyPrice = MonthlyPrice × 6`
- Created `dbo.fn_Saas_CalculatePeriodEnd(@PeriodStart, @BillingCycle)`
- Updated `sp_Saas_UpdateSubscriptionPlan` to use the function for all cycles
- Updated `sp_Saas_GetAllPlans`, `sp_Saas_GetPlanById`, `sp_Saas_GetPlanByCode` to return new price columns

**Migration order:** `051` → `052` → `053` → `054` → `055` → `056` → **`057`**

### Backend (C#)

| Component | Purpose |
|-----------|---------|
| `SaasBillingCycles` constants | `Monthly`, `Quarterly`, `HalfYearly`, `Yearly` |
| `SaasBillingCycleHelper` | Normalize, `CalculatePeriodEnd`, `ResolvePrice`, `GetDurationMonths` |
| `SaasSubscriptionService` | Razorpay order amount + billing cycle metadata via helper |
| `SaasPlanDto` / `SaasPlanRow` | `QuarterlyPrice`, `HalfYearlyPrice` fields |
| `SaasSubscriptionRepository.MapPlan` | Maps new price columns from DB |

### Razorpay Payment Flow

1. **Create order** — `CreatePaymentOrderAsync` normalizes cycle, resolves price from plan, stores cycle on pending payment and Razorpay order notes.
2. **Verify payment** — `VerifyPaymentAsync` calls `sp_Saas_UpdateSubscriptionPlan` with stored `BillingCycle`; SP applies carry-forward logic and `fn_Saas_CalculatePeriodEnd`.

### Frontend (Angular)

| File | Change |
|------|--------|
| `core/constants/saas-billing-cycles.ts` | Cycle constants, options, `getPlanPriceForCycle` |
| `shared/models/saas.models.ts` | `quarterlyPrice`, `halfYearlyPrice` on `SaasPlan` |
| `gym-subscription.component.*` | Four billing options per plan with Razorpay upgrade buttons |

---

## Test Results

**Command:**
```powershell
dotnet test Gym.API.IntegrationTests\Gym.API.IntegrationTests.csproj `
  --filter "FullyQualifiedName~SaasBillingCycle|FullyQualifiedName~SubscriptionRenewalDate"
```

**Result: 30 / 30 passed** (Duration ~0.5s)

### SaasBillingCycleTests (12 tests)

| Test | Status |
|------|--------|
| `CalculatePeriodEnd_MatchesDocumentedExamples` — Monthly → 2026-08-01 | ✅ Pass |
| `CalculatePeriodEnd_MatchesDocumentedExamples` — Quarterly → 2026-10-01 | ✅ Pass |
| `CalculatePeriodEnd_MatchesDocumentedExamples` — HalfYearly → 2027-01-01 | ✅ Pass |
| `CalculatePeriodEnd_MatchesDocumentedExamples` — Yearly → 2027-07-01 | ✅ Pass |
| `Normalize_AcceptsHalfYearlyAliases` — Half-Yearly | ✅ Pass |
| `Normalize_AcceptsHalfYearlyAliases` — halfyearly | ✅ Pass |
| `Normalize_AcceptsHalfYearlyAliases` — HALF YEARLY | ✅ Pass |
| `GetDurationMonths_ReturnsExpectedValues` — Monthly (1) | ✅ Pass |
| `GetDurationMonths_ReturnsExpectedValues` — Quarterly (3) | ✅ Pass |
| `GetDurationMonths_ReturnsExpectedValues` — HalfYearly (6) | ✅ Pass |
| `GetDurationMonths_ReturnsExpectedValues` — Yearly (12) | ✅ Pass |
| `ResolvePrice_UsesCycleSpecificPlanPrice` | ✅ Pass |

### SubscriptionRenewalDateTests (18 tests)

| Test Group | Cases | Status |
|------------|-------|--------|
| `EarlyRenewal_Monthly_PreservesRemainingDays` | 2 | ✅ Pass |
| `GracePeriodRenewal_Monthly_ContinuesFromPeriodEnd` | 2 | ✅ Pass |
| `ExpiredRenewal_Monthly_StartsFromToday` | 2 | ✅ Pass |
| `YearlyRenewal_UsesOneYearPeriod` | 4 | ✅ Pass |
| `FreshStart_AllBillingCycles_UseCorrectPeriodLength` | 4 (all cycles) | ✅ Pass |
| `EarlyRenewal_AllBillingCycles_PreservesRemainingDays` | 4 (all cycles) | ✅ Pass |

### Build Verification

| Target | Status |
|--------|--------|
| Backend (`dotnet build`) | ✅ Success |
| Frontend (`npm run build` in GymManagementSystem-UI) | ✅ Success |

---

## Deployment Notes

1. **Apply migration 057** on each environment:
   ```powershell
   cd Backend\Gym.API
   dotnet run -- migrate
   ```
2. Existing plans receive computed quarterly/half-yearly prices from monthly price if not set manually.
3. Super Admin can override `QuarterlyPrice` / `HalfYearlyPrice` directly in `SaasSubscriptionPlans` if needed.

### Known Environment Issue

On databases where earlier scripts (e.g. `051_LoginIdentifier.sql`) have not fully applied, `dotnet run -- migrate` may fail with `Invalid column name 'IsDeleted'`. This is unrelated to billing cycles — ensure scripts through `051` are applied before `057`. Integration tests for billing cycles run in-process and do not require a live DB connection.

---

## Manual QA Checklist

- [ ] Gym Admin → Subscription page shows 4 billing options per plan
- [ ] Prices match API (`GET /api/saas/plans`) for all cycles
- [ ] Razorpay checkout opens with correct amount for Quarterly / Half-Yearly
- [ ] After payment, subscription shows correct `billingCycle`, `currentPeriodEnd`, and `graceEndsAt`
- [ ] Early renewal stacks period (verify `CurrentPeriodEnd + 1 day` start)
- [ ] Expired renewal starts from payment date

---

## Key Source Files

| Layer | Path |
|-------|------|
| SQL migration | `Backend/Gym.Infrastructure/Persistence/Scripts/057_SaasBillingCycles.sql` |
| Renewal logic (prior) | `Backend/Gym.Infrastructure/Persistence/Scripts/054_SubscriptionRenewalDateLogic.sql` |
| C# helper | `Backend/Gym.Application/Services/SaasBillingCycleHelper.cs` |
| Payment service | `Backend/Gym.Application/Services/SaasSubscriptionService.cs` |
| Tests | `Backend/Gym.API.IntegrationTests/SaasBillingCycleTests.cs` |
| Tests | `Backend/Gym.API.IntegrationTests/SubscriptionRenewalDateTests.cs` |
| UI | `GymManagementSystem-UI/src/app/features/gym-admin/subscription/` |
