# SaaS Subscription & Gym Onboarding Module — Implementation Report

## Overview

This module adds public gym registration, SaaS subscription plans with trial/grace periods, Razorpay billing, tenant limits, gym branding, and a Super Admin platform dashboard (MRR/ARR).

## Database

**Script:** `Backend/Gym.Infrastructure/Persistence/Scripts/026_SaasSubscriptionModule.sql`

| Artifact | Purpose |
|----------|---------|
| `SaasSubscriptionPlans` | Trial, Basic, Premium, Enterprise plan definitions |
| `GymSubscriptions` (extended) | Trial/active billing, Razorpay IDs, grace period |
| `SaasSubscriptionPayments` | SaaS billing payment history |
| `Gyms` branding columns | Primary/secondary color, banner, receipt/invoice text |
| SPs | Plans, subscription CRUD, tenant limits, platform MRR/ARR, branding |

**Seeded plans:**

| Plan | Members | Trainers | Storage | WhatsApp/mo | Monthly | Yearly |
|------|---------|----------|---------|-------------|---------|--------|
| Trial | 50 | 5 | 512 MB | 100 | Free | Free |
| Basic | 200 | 10 | 2 GB | 500 | ₹999 | ₹9,990 |
| Premium | 500 | 25 | 5 GB | 2,000 | ₹2,499 | ₹24,990 |
| Enterprise | Unlimited | Unlimited | 10 GB | 10,000 | ₹4,999 | ₹49,990 |

## Permissions

| Permission | Role |
|------------|------|
| `VIEW_SAAS_SUBSCRIPTION` | GymAdmin |
| `MANAGE_SAAS_SUBSCRIPTION` | GymAdmin |
| `MANAGE_GYM_BRANDING` | GymAdmin |
| `VIEW_PLATFORM_SAAS` | SuperAdmin |

## API Endpoints

### Public onboarding
| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/onboarding/register` | Anonymous |
| GET | `/api/onboarding/plans` | Anonymous |

### Gym admin SaaS
| Method | Route | Permission |
|--------|-------|------------|
| GET | `/api/saas/subscription` | VIEW_SAAS_SUBSCRIPTION |
| GET | `/api/saas/usage` | VIEW_SAAS_SUBSCRIPTION |
| GET | `/api/saas/plans` | VIEW_SAAS_SUBSCRIPTION |
| POST | `/api/saas/payments/order` | MANAGE_SAAS_SUBSCRIPTION |
| POST | `/api/saas/payments/verify` | MANAGE_SAAS_SUBSCRIPTION |
| POST | `/api/saas/subscription/cancel` | MANAGE_SAAS_SUBSCRIPTION |
| GET/PUT | `/api/saas/branding` | MANAGE_GYM_BRANDING |

### Super Admin platform
| Method | Route | Permission |
|--------|-------|------------|
| GET | `/api/saas/platform/dashboard` | VIEW_PLATFORM_SAAS |

## Key Backend Files

- DTOs: `Backend/Gym.Application/DTOs/Saas/SaasDtos.cs`
- Services: `GymOnboardingService`, `SaasSubscriptionService`, `TenantLimitService`, `GymBrandingService`
- Repository: `SaasSubscriptionRepository.cs`
- Controllers: `SaasControllers.cs` (onboarding + saas + platform)
- Background job: `SaasSubscriptionBackgroundJob` (hourly expiry)
- Config: `SaasSubscription` section in `appsettings.json`

## Registration Flow

1. Owner submits gym name, contact, email, optional password at `/register`
2. System creates Gym + GymAdmin + 15-day Trial subscription
3. Notification settings seeded including `GymOwnerWelcome`
4. WhatsApp welcome sent to mobile number
5. Owner logs in at `/auth/login`

## Trial & Access Control

- **Trial:** Auto-activated on registration (15 days)
- **Grace period:** 3 days after trial/subscription expiry (configurable)
- **Lock access:** `AuthService` blocks login when subscription `HasAccess = false`
- **Background job:** Expires subscriptions and deactivates gyms after grace

## Tenant Limits

Enforced in `MemberService.CreateAsync` and `TrainerService.CreateAsync` via `ITenantLimitService`:
- Blocks member/trainer creation when plan limit reached
- Returns upgrade message with current plan name and counts

## Billing (Razorpay)

Uses existing Razorpay order flow (same as member payments):
1. Create order → pending `SaasSubscriptionPayments` row
2. Razorpay checkout on Angular subscription page
3. Verify signature → activate/upgrade subscription

## Frontend

| Route | Component |
|-------|-----------|
| `/register` | Public gym owner registration (Ahana theme) |
| `/gym-admin/subscription` | Plan management + Razorpay upgrade |
| `/gym-admin/settings/branding` | Logo, colors, receipt/invoice text |
| Super Admin dashboard | MRR, ARR, trial/expired subscription KPIs |

**Services:** `onboarding.service.ts`, `saas-subscription.service.ts`  
**Models:** `saas.models.ts`

## Configuration

```json
"SaasSubscription": {
  "TrialDays": 15,
  "GracePeriodDays": 3,
  "AllowPublicRegistration": true
}
```

## Deployment

1. Run `026_SaasSubscriptionModule.sql`
2. Restart API (seeder adds privileges)
3. Enable Razorpay in `appsettings.json` for paid upgrades
4. Configure WhatsApp for `GymOwnerWelcome` template
5. Re-login users to refresh JWT permissions

## Build Status

- Backend: `dotnet build GymManagementSaaS.sln` — succeeded
- Frontend: `npm run build` — succeeded

## Notes

- Enterprise plan uses `-1` for unlimited members/trainers in SQL limit checks
- Razorpay recurring subscriptions API can be added later; v1 uses one-time orders per billing cycle
- Demo gym seeder does not auto-create trial subscription; run registration or SuperAdmin provisioning
