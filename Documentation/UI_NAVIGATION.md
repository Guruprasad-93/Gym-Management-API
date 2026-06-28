# UI Navigation

Complete menu and route structure derived from `menu.config.ts` and Angular route files.

## Default Routes After Login

| Role | Default route |
|------|---------------|
| SuperAdmin | `/super-admin` |
| GymAdmin | `/gym-admin/dashboard` |
| Trainer | `/trainer` |
| Member | `/member/dashboard` |

## Super Admin Menu (`/super-admin`)

| Menu label | Route | Permission |
|------------|-------|------------|
| Dashboard | `/super-admin` | VIEW_DASHBOARD |
| Gyms | `/super-admin/gyms` | VIEW_GYMS |
| Gym Admins | `/super-admin/gym-admins` | VIEW_GYM_ADMINS |
| Subscription Plans | `/super-admin/subscription-plans` | MANAGE_SUBSCRIPTION_PLANS |
| Roles | `/super-admin/roles` | VIEW_ROLES |
| Privileges | `/super-admin/privileges` | VIEW_PRIVILEGES |
| Role Matrix | `/super-admin/role-matrix` | VIEW_PERMISSION_MATRIX |
| Audit Logs | `/super-admin/audit` | VIEW_AUDIT_LOGS |
| White Label | `/super-admin/white-label` | VIEW_PLATFORM_SAAS |

**Additional routes (no menu entry):** `/super-admin/subscription-plans/new`, `/super-admin/subscription-plans/:id/edit`

## Gym Admin Menu (`/gym-admin`)

| Menu label | Route | Permission | SaaS feature |
|------------|-------|------------|--------------|
| Dashboard | `/gym-admin/dashboard` | VIEW_ANALYTICS, VIEW_DASHBOARD | DASHBOARD |
| Revenue Analytics | `/gym-admin/analytics/revenue` | VIEW_REVENUE_ANALYTICS | REPORTS |
| Member Analytics | `/gym-admin/analytics/members` | VIEW_MEMBER_ANALYTICS | REPORTS |
| Attendance Analytics | `/gym-admin/analytics/attendance` | VIEW_ANALYTICS | REPORTS |
| Trainer Analytics | `/gym-admin/analytics/trainers` | VIEW_ANALYTICS | REPORTS |
| Members | `/gym-admin/members` | VIEW_MEMBERS | MEMBERS |
| Leads & CRM | `/gym-admin/leads` | VIEW_LEADS | CRM |
| Expenses | `/gym-admin/expenses` | VIEW_EXPENSES | REPORTS |
| Payroll | `/gym-admin/payroll` | VIEW_PAYROLL | REPORTS |
| Financial | `/gym-admin/financial` | VIEW_FINANCIAL_ANALYTICS | REPORTS |
| Trainers | `/gym-admin/trainers` | VIEW_TRAINERS | TRAINERS |
| Membership Plans | `/gym-admin/membership-plans` | VIEW_MEMBERSHIPS | MEMBERSHIPS |
| Memberships | `/gym-admin/memberships` | VIEW_MEMBERSHIPS | MEMBERSHIPS |
| Payments | `/gym-admin/payments` | VIEW_PAYMENTS | PAYMENTS |
| Notifications | `/gym-admin/notifications` | VIEW_NOTIFICATIONS | NOTIFICATIONS |
| Revenue | `/gym-admin/revenue` | VIEW_REVENUE | PAYMENTS |
| Attendance | `/gym-admin/attendance` | VIEW_ATTENDANCE | ATTENDANCE |
| Attendance Reports | `/gym-admin/attendance/reports` | VIEW_ATTENDANCE | ATTENDANCE |
| Audit Logs | `/gym-admin/audit` | VIEW_AUDIT_LOGS | — |
| Diet Plans | `/gym-admin/diet-plans` | VIEW_DIET_PLANS | DIET_PLANS |
| Workout Plans | `/gym-admin/workout-plans` | VIEW_WORKOUT_PLANS | WORKOUT_PLANS |
| Subscription | `/gym-admin/subscription` | VIEW_SAAS_SUBSCRIPTION | SUBSCRIPTIONS |
| Gym Branding | `/gym-admin/settings/branding` | MANAGE_GYM_BRANDING | CUSTOM_BRANDING |
| Branches | `/gym-admin/branches` | VIEW_BRANCHES | MULTI_BRANCH |
| Branch Dashboard | `/gym-admin/branches/dashboard` | VIEW_BRANCHES | MULTI_BRANCH |
| Branch Analytics | `/gym-admin/branches/analytics` | VIEW_BRANCH_ANALYTICS | MULTI_BRANCH |
| Branch Transfers | `/gym-admin/branches/transfers` | VIEW_BRANCHES | MULTI_BRANCH |
| Branch Targets | `/gym-admin/branches/targets` | VIEW_BRANCHES | MULTI_BRANCH |
| Mobile Push | `/gym-admin/mobile-notifications` | SEND_NOTIFICATIONS | NOTIFICATIONS |
| Mobile Analytics | `/gym-admin/mobile-analytics` | VIEW_NOTIFICATIONS | NOTIFICATIONS |
| AI Dashboard | `/gym-admin/ai` | VIEW_AI_INSIGHTS | AI_INSIGHTS |
| AI Insights | `/gym-admin/ai/insights` | VIEW_AI_INSIGHTS | AI_INSIGHTS |
| Bookings | `/gym-admin/bookings` | VIEW_BOOKINGS | BOOKINGS |
| Class Schedules | `/gym-admin/schedules` | MANAGE_SCHEDULES | BOOKINGS |
| Booking Analytics | `/gym-admin/booking-analytics` | VIEW_BOOKING_ANALYTICS | BOOKINGS |
| Website Builder | `/gym-admin/website-builder` | VIEW_WEBSITE_BUILDER | WEBSITE_BUILDER |
| Website Analytics | `/gym-admin/website-builder/analytics` | VIEW_WEBSITE_ANALYTICS | WEBSITE_BUILDER |
| White Label | `/gym-admin/branding` | VIEW_WHITE_LABEL | WHITE_LABEL |

**Routes without menu items:** member/trainer detail pages, attendance check-in/out, diet/workout editors, `schedules/new`, `schedules/:id/edit`, `schedules/settings`, `memberships/expired`, `renew-subscription`, `white-label/preview`, lead sub-routes.

## Trainer Menu (`/trainer`)

| Menu label | Route | Permission |
|------------|-------|------------|
| Dashboard | `/trainer` | VIEW_DASHBOARD |
| My Members | `/trainer/members` | VIEW_MEMBERS |
| Attendance | `/trainer/attendance` | VIEW_ATTENDANCE |
| Workout Plans | `/trainer/workout-plans` | VIEW_WORKOUT_PLANS |
| AI Recommendations | `/trainer/ai-recommendations` | VIEW_AI_RECOMMENDATIONS |
| My Schedule | `/trainer/schedule` | VIEW_BOOKINGS |
| Class Bookings | `/trainer/bookings` | VIEW_BOOKINGS |

**Route without menu:** `/trainer/leads`, `/trainer/attendance/check-in`, `/trainer/attendance/check-out`, `/trainer/workout-plans/exercises`, `/trainer/workout-plans/:id/edit`, `/trainer/members/:memberId/workout`

## Member Menu (`/member`)

| Menu label | Route | Permission |
|------------|-------|------------|
| Dashboard | `/member/dashboard` | VIEW_MEMBER_DASHBOARD |
| Goals | `/member/goals` | MANAGE_MEMBER_GOALS |
| Progress | `/member/progress` | TRACK_MEMBER_PROGRESS |
| Workouts | `/member/workouts` | TRACK_MEMBER_PROGRESS |
| Diet Tracker | `/member/diets` | TRACK_MEMBER_PROGRESS |
| Water | `/member/water` | TRACK_MEMBER_PROGRESS |
| Referrals | `/member/referrals` | VIEW_MEMBER_DASHBOARD |
| Feedback | `/member/feedback` | SUBMIT_MEMBER_FEEDBACK |
| My Profile | `/member/profile` | VIEW_MEMBER_DETAILS |
| My Diet Plan | `/member/diet` | VIEW_MEMBER_DIET |
| My Workout Plan | `/member/workout` | VIEW_MEMBER_WORKOUT |
| Pay Membership | `/member/checkout` | INITIATE_ONLINE_PAYMENT |
| Book a Class | `/member/bookings` | VIEW_BOOKINGS |

**Route without menu:** `/member/bookings/history`

## Auth Routes (`/auth`)

| Route | Page |
|-------|------|
| `/auth/login` | Login |
| `/auth/forgot-password` | Forgot password |
| `/auth/reset-password` | Reset password |
| `/auth/change-password` | Change password (forced) |
| `/register` | Gym registration onboarding |

## Public Website (`/website/:gymSlug`)

| Route | Page |
|-------|------|
| `/website/:gymSlug` | Home |
| `/website/:gymSlug/about` | About |
| `/website/:gymSlug/plans` | Plans |
| `/website/:gymSlug/trainers` | Trainers |
| `/website/:gymSlug/gallery` | Gallery |
| `/website/:gymSlug/contact` | Contact / lead capture |
| `/website/:gymSlug/:pageSlug` | Custom CMS page |
