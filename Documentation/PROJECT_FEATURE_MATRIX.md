# Project Feature Matrix

Status legend: **Implemented** | **Partial** | **Not Implemented**

## Implemented Modules

| Module | Backend | Frontend | Database | Notes |
|--------|---------|----------|----------|-------|
| Login & Authentication | Yes | Yes | Yes | Cookie + JWT, CSRF, refresh |
| Dashboard (Super/Gym/Trainer/Member) | Yes | Yes | Yes | Role-specific dashboards |
| Gym Management (platform) | Yes | Yes | Yes | Super Admin |
| Gym Admin accounts | Yes | Yes | Yes | |
| Branch Management | Yes | Yes | Yes | Multi-branch |
| Member Management | Yes | Yes | Yes | CRUD, assign trainer |
| Trainer Management | Yes | Yes | Yes | |
| Membership Plans | Yes | Yes | Yes | |
| Memberships | Yes | Yes | Yes | Renew, cancel, expired list |
| Payments & Revenue | Yes | Yes | Yes | Razorpay, invoices, refunds |
| Attendance | Yes | Yes | Yes | Member + trainer attendance |
| Workout Plans | Yes | Yes | Yes | Exercise library, assign, progress |
| Diet Plans | Yes | Yes | Yes | Categories, assign, export |
| Class Scheduling | Yes | Yes | Yes | Recurring weekly schedules |
| Class Booking | Yes | Yes | Yes | Book, waitlist, cancel, analytics |
| Website Builder | Yes | Yes | Yes | Pages, gallery, testimonials, public site |
| CRM / Leads | Yes | Yes | Yes | Follow-ups, trials, conversion |
| QR Check-in (attendance) | Yes | Yes | Yes | Member QR + admin check-in |
| QR Check-in (class booking) | Yes | Partial UI | Yes | `POST /api/booking-checkin` |
| Reports / Analytics | Yes | Yes | Yes | Revenue, member, attendance, financial |
| Notifications (WhatsApp) | Yes | Yes | Yes | Templates, history, test send |
| Mobile Push | Yes | Yes | Yes | Device tokens, campaigns |
| AI Insights | Yes | Yes | Yes | Mock provider default |
| SaaS Subscriptions | Yes | Yes | Yes | Plans, checkout, features |
| White Label | Yes | Yes | Yes | Branding, domain, email templates |
| Audit Logs | Yes | Yes | Yes | Export PDF/Excel |
| RBAC (Roles/Privileges) | Yes | Yes | Yes | Role matrix |
| Tenant Menu Management | Yes | Partial UI | Yes | Platform API; gym uses seeded menus |
| Member Self-Service | Yes | Yes | Yes | Goals, progress, tracking, referrals |
| Financial (Expenses/Payroll) | Yes | Yes | Yes | |
| File Management | Yes | Partial UI | Yes | Upload API; limited dedicated UI |
| Onboarding / Register gym | Yes | Yes | Yes | Public registration |
| Health check | Yes | N/A | N/A | `/api/health` |

## Partially Implemented

| Area | What exists | Gap |
|------|-------------|-----|
| **Trainer Availability** | `TrainerAvailability` table, `sp_CreateTrainerAvailability`, `sp_GetTrainerAvailability` | No API controller or Angular UI |
| **AI module** | Full API + UI | Default `Mock` AI provider; OpenAI optional via config |
| **Push notifications** | Full pipeline | Firebase `Mock` provider in development |
| **WhatsApp** | Templates, logs, jobs | `Mock` sender when disabled |
| **Razorpay** | Real + mock gateways | Mock auto-enabled in dev without secrets |
| **Legacy `004_FutureTablesSchema` tables** | Schema only | No business logic (e.g. SupportTickets, Coupons) |
| **Booking QR check-in** | API endpoint | No dedicated gym-admin scan UI page |
| **GymSubscriptionComponent** | Component file | Not wired to routes |

## Not Implemented (no documentation module created)

| Feature | Evidence |
|---------|----------|
| POS / Inventory retail | Menu code exists; no full module |
| Dedicated Support Tickets UI | Table in future schema only |
| Native mobile apps | Mobile APIs exist for sync/push; no app in repo |

## SaaS Feature Codes (subscription gating)

`DASHBOARD`, `MEMBERS`, `TRAINERS`, `MEMBERSHIPS`, `PAYMENTS`, `ATTENDANCE`, `DIET_PLANS`, `WORKOUT_PLANS`, `BOOKINGS`, `CRM`, `REPORTS`, `MULTI_BRANCH`, `NOTIFICATIONS`, `AI_INSIGHTS`, `WEBSITE_BUILDER`, `WHITE_LABEL`, `SUBSCRIPTIONS`, `CUSTOM_BRANDING`
