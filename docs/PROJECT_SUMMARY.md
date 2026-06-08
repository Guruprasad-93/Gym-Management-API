# Gym Management System — Full Project Summary

**Version:** 1.0  
**Stack:** Angular 19 (frontend) + ASP.NET Core 8 (backend) + SQL Server  
**Type:** Multi-tenant SaaS gym / fitness center management platform

---

## 1. What Is This Project?

Gym Management System is an **all-in-one SaaS platform** for running fitness businesses. A **Super Admin** manages the platform and onboarded gyms. Each **Gym Admin** runs day-to-day operations (members, trainers, payments, attendance, CRM, branches, etc.). **Trainers** manage assigned members and classes. **Members** use a self-service portal for workouts, diet, bookings, and payments.

The system also includes:
- **Public gym websites** (builder + live site)
- **White-label / branding** for gyms and the platform
- **AI insights** and trainer recommendations
- **WhatsApp / push notifications**
- **Online payments** (Razorpay)
- **Multi-branch** franchise management
- **Audit logging** across the platform

---

## 2. Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│  Angular SPA (localhost:4200)                               │
│  Super Admin │ Gym Admin │ Trainer │ Member │ Public Site   │
└──────────────────────────┬──────────────────────────────────┘
                           │ REST API (/api/*)
┌──────────────────────────▼──────────────────────────────────┐
│  ASP.NET Core 8 API (localhost:5088)                        │
│  Controllers → Application Services → Repositories          │
└──────────────────────────┬──────────────────────────────────┘
                           │ Stored Procedures (Dapper)
┌──────────────────────────▼──────────────────────────────────┐
│  SQL Server (GymDb)                                         │
│  Multi-tenant by GymId │ Role-based permissions             │
└─────────────────────────────────────────────────────────────┘
```

**Key concepts:**
- **Multi-tenancy:** Each gym is isolated by `GymId`; users belong to a gym (except Super Admin).
- **RBAC:** Roles (SuperAdmin, GymAdmin, Trainer, Member) + granular permissions control menu visibility and API access.
- **Cookie/JWT auth:** Login sets session; guards protect routes on frontend and permissions on backend.

---

## 3. User Roles

| Role | Portal | Purpose |
|------|--------|---------|
| **Super Admin** | `/super-admin` | Platform owner — manage gyms, gym admins, roles, privileges, platform SaaS |
| **Gym Admin** | `/gym-admin` | Gym owner/manager — full gym operations |
| **Trainer** | `/trainer` | Coach — assigned members, attendance, workouts, schedule |
| **Member** | `/member` | Gym customer — profile, progress, diet/workout plans, bookings, payments |
| **Public visitor** | `/website/:slug` | Prospective customers — view gym site, enquire, book trial |

---

## 4. Authentication & Registration Pages

| Route | Page | What It Does |
|-------|------|--------------|
| `/auth/login` | **Login** | Sign in with email/password. Redirects to the correct portal based on role (Super Admin, Gym Admin, Trainer, or Member). Supports remember-me and password visibility toggle. |
| `/auth/forgot-password` | **Forgot Password** | Request a password reset link/token by email. |
| `/auth/reset-password` | **Reset Password** | Set a new password using email + reset token from the link. |
| `/auth/change-password` | **Change Password** | For logged-in users who must change password (e.g. first login or policy). |
| `/register` | **Gym Registration** | Self-service signup for new gym owners — creates gym + admin account (free trial flow). |

---

## 5. Super Admin Portal (`/super-admin`)

Platform-level administration for the SaaS operator.

| Route | Page | What It Does |
|-------|------|--------------|
| `/super-admin` | **Dashboard** | Platform overview — KPIs (total gyms, admins, revenue), charts, and quick links to key admin areas. |
| `/super-admin/gyms` | **Gyms** | List all registered gyms. Create, edit, activate/deactivate, and delete gyms. Entry point for multi-tenant management. |
| `/super-admin/gym-admins` | **Gym Admins** | Manage admin users for each gym. Create admins, edit details, resend temporary passwords, activate/deactivate accounts. |
| `/super-admin/roles` | **Roles** | Define system roles (e.g. custom roles beyond defaults). Create, edit, delete roles. |
| `/super-admin/privileges` | **Privileges** | Define granular permissions (e.g. VIEW_MEMBERS, MANAGE_LEADS). Create and delete privileges. |
| `/super-admin/role-matrix` | **Role Matrix** | Visual grid to assign/remove privileges per role via checkboxes. Controls what each role can do across the system. |
| `/super-admin/audit` | **Audit Logs** | Searchable log of all system actions (create/update/delete) across gyms. Filter by user, entity, action, date. Export PDF/Excel. |
| `/super-admin/white-label` | **White Label (Platform SaaS)** | Platform-level view of premium/branded customers, subscription status, and SaaS analytics for white-label tenants. |

---

## 6. Gym Admin Portal (`/gym-admin`)

The main operations hub for a single gym. ~68 routes covering all business functions.

### 6.1 Dashboard & Analytics

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/dashboard` | **Dashboard** | Gym home — member count, revenue, attendance, expiring memberships, charts, and quick navigation to all modules. Export reports. |
| `/gym-admin/analytics/revenue` | **Revenue Analytics** | Deep dive into revenue trends, payment breakdowns, monthly comparisons. Charts + export. |
| `/gym-admin/analytics/members` | **Member Analytics** | Member growth, retention, demographics, registration trends. |
| `/gym-admin/analytics/attendance` | **Attendance Analytics** | Check-in patterns, peak hours, attendance rates over time. |
| `/gym-admin/analytics/trainers` | **Trainer Analytics** | Trainer performance — assigned members, session counts, productivity metrics. |

### 6.2 Members & Trainers

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/members` | **Members List** | Searchable table of all gym members. Add, edit, assign trainer, deactivate/delete. Filter active/inactive. |
| `/gym-admin/members/:id` | **Member Detail** | Full member profile — contact info, membership status, trainer, measurements. Links to diet and workout plans. |
| `/gym-admin/members/:id/diet` | **Member Diet View** | View and assign/change the diet plan for a specific member. Export plan as PDF. |
| `/gym-admin/members/:id/workout` | **Member Workout View** | View and assign/change the workout plan for a specific member. |
| `/gym-admin/trainers` | **Trainers List** | Manage gym trainers. Add, edit, search, deactivate. View assigned member counts. |
| `/gym-admin/trainers/:id` | **Trainer Detail** | Trainer profile, assigned members list, assign/unassign members to this trainer. |

### 6.3 Leads & CRM

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/leads` | **Leads List** | CRM pipeline — all prospective customers. Search, filter by status/source, toggle list vs kanban view. |
| `/gym-admin/leads/create` | **Create Lead** | Form to add a new lead (name, contact, source, interested plan, notes). |
| `/gym-admin/leads/edit/:id` | **Edit Lead** | Update lead information and status. |
| `/gym-admin/leads/:id` | **Lead Detail** | Full lead view. Edit, add notes, **convert lead to member** (creates member record). |
| `/gym-admin/leads/followups` | **Follow-ups** | Today's/pending follow-up tasks for sales team. Quick links to lead details. |
| `/gym-admin/leads/trials` | **Trial Bookings** | Leads scheduled for free trial sessions today. |
| `/gym-admin/leads/analytics` | **Lead Analytics** | Conversion rates, source performance, funnel charts. Links to follow-ups and trials. |

### 6.4 Memberships & Payments

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/membership-plans` | **Membership Plans** | Define plan types (name, price, duration, features). Create, edit, deactivate plans. |
| `/gym-admin/memberships` | **Active Memberships** | All active member subscriptions. Create new membership, renew, cancel. Link to expired list. |
| `/gym-admin/memberships/expired` | **Expired Memberships** | Historical view of lapsed memberships for renewal campaigns. |
| `/gym-admin/payments` | **Payments** | Payment ledger — all transactions. Record manual payment, download invoice, process refund. |
| `/gym-admin/revenue` | **Revenue Dashboard** | Revenue KPIs, trends, links to payments and analytics. |

### 6.5 Financial Management

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/expenses` | **Expenses** | Track gym operating costs by category, vendor, date. Add, edit, delete expenses. Export Excel. |
| `/gym-admin/payroll` | **Payroll** | Trainer/staff payroll. Generate monthly payroll, approve, mark as paid. Export PDF. |
| `/gym-admin/financial` | **Financial Dashboard** | Combined P&L view — revenue vs expenses vs payroll, profit charts, financial KPIs. Export reports. |

### 6.6 Multi-Branch Management

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/branches` | **Branches List** | Manage gym locations/branches. Create branch (name, code, city). View member/trainer counts per branch. |
| `/gym-admin/branches/dashboard` | **Branch Dashboard** | Per-branch KPIs — members, trainers, monthly revenue, expenses, profit, attendance, open leads. |
| `/gym-admin/branches/analytics` | **Branch Analytics** | Compare branches — revenue rankings, monthly trends, performance charts. |
| `/gym-admin/branches/transfers` | **Branch Transfers** | Move members or trainers between branches with transfer history. |
| `/gym-admin/branches/targets` | **Branch Targets** | Set monthly targets per branch (revenue, new members, lead conversions) and track achievement. |

### 6.7 Attendance

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/attendance` | **Attendance Hub** | Central navigation for all attendance features — check-in, check-out, list, reports, trainer attendance. |
| `/gym-admin/attendance/list` | **Attendance List** | All check-in/check-out records. Filter and view member session history. |
| `/gym-admin/attendance/check-in` | **Member Check-In** | Mark a member as present — select member, optional notes, record entry time. |
| `/gym-admin/attendance/check-out` | **Member Check-Out** | Close open sessions — select active check-in, record exit time. |
| `/gym-admin/attendance/members/:id/history` | **Member Attendance History** | Full attendance history for one member. Export Excel. |
| `/gym-admin/attendance/reports` | **Attendance Reports** | Daily and monthly attendance reports with charts. Export PDF/Excel. |
| `/gym-admin/attendance/trainers` | **Trainer Attendance** | Check trainers in/out separately from members. |

### 6.8 Diet & Workout Plans

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/diet-plans` | **Diet Plans List** | Library of diet/nutrition plans. Search, assign to members, clone, edit, delete, export PDF. |
| `/gym-admin/diet-plans/new` | **Create Diet Plan** | Build a plan — meals, calories, macros, items per day. Save as reusable template. |
| `/gym-admin/diet-plans/:id/edit` | **Edit Diet Plan** | Modify existing diet plan template. |
| `/gym-admin/workout-plans` | **Workout Plans List** | Library of workout programs. Search, assign, clone, edit, delete, export PDF. Link to exercise library. |
| `/gym-admin/workout-plans/exercises` | **Exercise Library** | Master list of exercises (name, muscle group, equipment). Add, edit, delete exercises used in plans. |
| `/gym-admin/workout-plans/new` | **Create Workout Plan** | Build a program — add exercise rows with sets, reps, rest days. |
| `/gym-admin/workout-plans/:id/edit` | **Edit Workout Plan** | Modify existing workout program. |

### 6.9 Bookings & Class Schedules

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/bookings` | **Bookings** | All class/session bookings — who booked what, status, capacity. Links to schedules and analytics. |
| `/gym-admin/schedules` | **Class Schedules** | Manage recurring class timetables (yoga, HIIT, etc.). View schedule cards, cancel sessions. |
| `/gym-admin/booking-analytics` | **Booking Analytics** | Booking trends, popular classes, utilization rates. Export reports. |

### 6.10 Notifications & Mobile

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/notifications` | **Notifications Hub** | Overview with links to templates, history, and test send. |
| `/gym-admin/notifications/templates` | **Notification Templates** | WhatsApp/SMS/email message templates. Create, edit, delete templates with placeholders. |
| `/gym-admin/notifications/history` | **Notification History** | Log of all sent notifications — recipient, channel, status, timestamp. |
| `/gym-admin/notifications/test` | **Test Notification** | Send a test message to verify template and delivery configuration. |
| `/gym-admin/mobile-notifications` | **Mobile Push** | Send push notifications to member mobile app users. |
| `/gym-admin/mobile-analytics` | **Mobile Analytics** | Push notification delivery stats, open rates, device analytics. |

### 6.11 AI & Insights

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/ai` | **AI Dashboard** | Overview of AI-powered gym insights — churn risk, engagement scores, recommendations summary. |
| `/gym-admin/ai/insights` | **AI Insights** | Detailed AI analysis — member insights, lead scoring, tabs for different insight categories. |

### 6.12 Website Builder & Public Presence

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/website-builder` | **Website Builder** | Configure public gym website — title, description, colors, logo, banner. Save and publish/unpublish site. |
| `/gym-admin/website-builder/pages` | **Website Pages** | Manage custom content pages (About sections, etc.). Add and delete pages. |
| `/gym-admin/website-builder/gallery` | **Website Gallery** | Upload and manage photos displayed on the public site gallery page. |
| `/gym-admin/website-builder/testimonials` | **Testimonials** | Add member testimonials shown on the public website. |
| `/gym-admin/website-builder/analytics` | **Website Analytics** | Traffic and visitor stats for the public gym website. |

### 6.13 Branding, White Label & Subscription

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/settings/branding` | **Gym Branding** | Upload gym logo, receipt header/footer, colors for in-gym materials and receipts. |
| `/gym-admin/branding` | **Branding (alias)** | Same as White Label settings — custom app branding for the gym's tenant. |
| `/gym-admin/white-label` | **White Label Settings** | Customize gym's app appearance — primary color, logo, app name. Save and preview. |
| `/gym-admin/white-label/preview` | **White Label Preview** | Live preview of how branded screens will look to members/trainers. |
| `/gym-admin/subscription` | **SaaS Subscription** | Gym's own subscription to the platform — current plan, upgrade monthly/yearly, cancel. |

### 6.14 Audit

| Route | Page | What It Does |
|-------|------|--------------|
| `/gym-admin/audit` | **Audit Logs** | Gym-scoped activity log — who changed what (members, payments, settings, etc.). Search, filter, export. |

---

## 7. Trainer Portal (`/trainer`)

Focused workspace for coaches — only their assigned members and classes.

| Route | Page | What It Does |
|-------|------|--------------|
| `/trainer` | **Dashboard** | Trainer home — assigned member count, today's sessions, quick links to members, schedule, bookings, AI. |
| `/trainer/members` | **My Members** | List of members assigned to this trainer. View workout plan link per member. |
| `/trainer/leads` | **Leads** | *(Routable but not in sidebar)* Shared leads list — trainer can view/manage assigned leads. |
| `/trainer/attendance` | **Attendance Hub** | Links to check-in and check-out for members. |
| `/trainer/attendance/check-in` | **Check-In** | Mark assigned members as checked in. |
| `/trainer/attendance/check-out` | **Check-Out** | Check out members with open sessions. |
| `/trainer/workout-plans` | **Workout Plans** | Full workout plan library — same as gym admin view for creating/managing plans. |
| `/trainer/members/:id/workout` | **Member Workout** | View and assign workout plan for a specific assigned member. |
| `/trainer/ai-recommendations` | **AI Recommendations** | AI-suggested workout/diet adjustments for members. Search, filter, mark recommendations as accepted. |
| `/trainer/schedule` | **My Schedule** | Trainer's class/session timetable. Link to bookings. |
| `/trainer/bookings` | **Class Bookings** | Members booked into trainer's classes. Search and filter by status. |

---

## 8. Member Portal (`/member`)

Self-service app for gym customers.

| Route | Page | What It Does |
|-------|------|--------------|
| `/member/dashboard` | **Dashboard** | Member home — membership status, goal progress, quick actions (book class, pay, log workout). |
| `/member/profile` | **My Profile** | Personal info, membership details, payment history, measurements. Tabs for different profile sections. Links to checkout and bookings. |
| `/member/goals` | **Goals** | Set fitness goals (weight loss, muscle gain, etc.). Add goals, mark complete. |
| `/member/progress` | **Progress Tracker** | Log body measurements (weight, body fat, etc.) over time. Charts and export PDF. |
| `/member/workouts` | **Workout Log** | Log daily workout completion. Mark exercises complete. |
| `/member/diets` | **Diet Tracker** | Log daily meals and adherence to diet plan. |
| `/member/water` | **Water Intake** | Track daily water consumption. |
| `/member/referrals` | **Referrals** | Personal referral code to invite friends. Copy code to share. |
| `/member/feedback` | **Feedback** | Submit rating and comments about the gym/trainer experience. |
| `/member/diet` | **My Diet Plan** | View trainer/admin-assigned diet plan with meals and macros. |
| `/member/workout` | **My Workout Plan** | View assigned workout program with exercises, sets, reps. |
| `/member/checkout` | **Pay Membership** | Select membership plan and pay online via Razorpay integration. |
| `/member/bookings` | **Book a Class** | Browse available class slots, book a spot, or join waitlist. |
| `/member/bookings/history` | **Booking History** | Past and upcoming class bookings. |

---

## 9. Public Gym Website (`/website/:gymSlug`)

Customer-facing marketing site generated from the Website Builder. Each gym gets a unique URL slug.

| Route | Page | What It Does |
|-------|------|--------------|
| `/website/:slug` | **Home** | Landing page — hero banner, gym description, featured plans, testimonials, CTA to book trial. |
| `/website/:slug/about` | **About** | About the gym — story, mission, custom content sections from builder. |
| `/website/:slug/plans` | **Membership Plans** | Public pricing page — all active membership plans with prices and descriptions. |
| `/website/:slug/trainers` | **Trainers** | Meet the team — trainer profiles and photos. |
| `/website/:slug/gallery` | **Gallery** | Photo gallery of gym facilities and events. |
| `/website/:slug/contact` | **Contact & Enquiry** | Contact form (name, email, message) creates a lead. **Book Free Trial** form schedules a trial visit. |

---

## 10. Backend API Modules

The API mirrors frontend features. Main controller groups:

| API Area | Controllers | Purpose |
|----------|-------------|---------|
| Auth | `AuthController` | Login, logout, refresh token, password reset, register |
| Gyms & Admins | `GymsController`, `GymAdminsController` | Super admin gym and admin CRUD |
| Members | `MembersController`, `MemberSelfServiceController` | Member CRUD + member portal APIs |
| Trainers | `TrainersController` | Trainer CRUD, dashboard, member assignment |
| Memberships | `MembershipPlansController`, `MembershipsController` | Plans and active memberships |
| Payments | `PaymentsController` | Payments, invoices, refunds, Razorpay |
| Leads | `LeadsController` | CRM leads, conversion, follow-ups |
| Attendance | `AttendanceController` | Member and trainer check-in/out, reports |
| Diet & Workout | `DietPlansController`, `WorkoutPlansController` | Plan templates and member assignments |
| Financial | `ExpensesController`, `PayrollController`, `FinancialController` | Expenses, payroll, P&L analytics |
| Branches | `BranchesController` | Multi-branch CRUD, transfers, targets, dashboard |
| Bookings | `BookingController` | Schedules, bookings, waitlist |
| Notifications | `NotificationsController`, `MobileController` | WhatsApp, SMS, push notifications |
| AI | `AiController` | Insights and trainer recommendations |
| Website | `WebsiteController` | Public site content and enquiries |
| White Label | `WhiteLabelController` | Branding and SaaS subscription |
| Analytics | `AnalyticsController`, `DashboardController` | Business intelligence dashboards |
| Audit | `AuditLogsController` | Activity logging |
| Files | `FilesController` | Logo/image uploads |
| Roles | `RolesController`, `PrivilegesController`, `RolePrivilegesController`, `UserRolesController` | RBAC management |

---

## 11. Database & Migrations

- **Database:** SQL Server `GymDb`
- **Migrations:** 39+ SQL scripts in `Backend/Gym.Infrastructure/Persistence/Scripts/` (001–039)
- **Auto-run:** Migrations and seed data run on API startup when configured in `appsettings.Development.json`
- **Data access:** Primarily stored procedures via Dapper (no EF for business queries)

---

## 12. Key Integrations

| Integration | Used For |
|-------------|----------|
| **Razorpay** | Online membership payments (member checkout) |
| **WhatsApp** | Notification templates and automated messages |
| **Push notifications** | Mobile app alerts to members |
| **File storage (local/S3)** | Logos, gallery images, receipt branding |
| **Chart.js** | Dashboard and analytics charts in Angular |
| **PDF/Excel export** | Reports across audit, attendance, financial, analytics modules |

---

## 13. Project Structure

```
GymManagementSystem/
├── Backend/
│   ├── Gym.API/              # REST API, controllers, middleware
│   ├── Gym.Application/      # Services, DTOs, validators, CQRS handlers
│   ├── Gym.Domain/           # Entities, constants
│   └── Gym.Infrastructure/   # Repositories, SQL scripts, auth, file storage
├── Frontend/
│   └── gym-app/              # Angular SPA
│       └── src/app/
│           ├── features/     # auth, super-admin, gym-admin, trainer, member, public-website
│           ├── core/         # guards, services, constants, interceptors
│           ├── layout/       # sidebar, header, main layout
│           └── shared/       # reusable components, models, styles
└── docs/
    ├── MANUAL_TEST_CASES.md  # Manual test cases (239)
    ├── MANUAL_TEST_CASES.csv # Test tracking spreadsheet
    └── PROJECT_SUMMARY.md    # This document
```

---

## 14. Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Super Admin | superadmin@gym.com | SuperAdmin@123 |
| Gym Admin | admin@fitzone-demo.com | Demo@123 |

---

## 15. Running the Project

| Service | Command | URL |
|---------|---------|-----|
| Backend API | `dotnet run --project Backend/Gym.API` | http://localhost:5088 |
| Frontend | `npm start` (in Frontend/gym-app) | http://localhost:4200 |
| Database | SQL Server local instance | GymDb |

---

## 16. Page Count Summary

| Portal | Pages |
|--------|-------|
| Auth & Registration | 5 |
| Super Admin | 8 |
| Gym Admin | 68 |
| Trainer | 11 |
| Member | 15 |
| Public Website | 6 |
| **Total** | **~113 routes** |

---

*This document describes the Gym Management System as implemented in the codebase. For step-by-step test procedures, see `MANUAL_TEST_CASES.md`.*
