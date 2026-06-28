# APIs

## Module Overview
ASP.NET Core Web API with controller-based routing, `ApiResponse<T>` wrapper, permission attributes, and middleware pipeline.

## Base URL
- Development API: `http://localhost:5088`
- Angular proxy: `/api` → backend

## Middleware Pipeline (order matters)
1. Exception handling
2. Authentication / authorization
3. CSRF validation (mutating cookie requests)
4. Subscription access
5. Feature access (SaaS entitlements)
6. Gym menu access (tenant modules)

## Response Shape
```json
{
  "success": true,
  "message": "Optional message",
  "data": { }
}
```

## Authorization Patterns
- `[Authorize]` on controllers
- `[RequirePermission(Permissions.X)]` on actions
- `[AllowAnonymous]` on public/auth endpoints
- Super Admin bypasses tenant feature/menu checks

## Controller Summary (~364 endpoints)

| Controller group | Route prefix | Purpose |
|------------------|--------------|---------|
| Auth | `/api/auth` | Login, session, CSRF |
| Gyms / GymAdmins | `/api/Gyms`, `/api/gym-admins` | Platform tenants |
| Members / Trainers | `/api/members`, `/api/trainers` | People management |
| Memberships / Plans | `/api/memberships`, `/api/membership-plans` | Subscriptions |
| Payments | `/api/payments` | Billing, Razorpay |
| Attendance | `/api/attendance` | Check-in/out, reports |
| Diet / Workout | `/api/diet-plans`, `/api/workout-plans` | Plans |
| Bookings | `/api/bookings`, `/api/schedules`, `/api/booking-analytics`, `/api/trainer-schedule`, `/api/booking-checkin` | Classes |
| Leads | `/api/leads` | CRM |
| Branches | `/api/branches` | Multi-branch |
| Analytics / Financial | `/api/analytics`, `/api/financial`, `/api/expenses`, `/api/payroll` | Reports |
| Notifications / Mobile | `/api/notifications`, `/api/mobile` | Messaging |
| AI | `/api/ai` | Insights |
| Website | `/api/website`, `/api/public/website` | CMS + public |
| White label | `/api/white-label`, `/api/public/white-label` | Branding |
| SaaS | `/api/saas`, `/api/platform/subscription-plans` | Subscriptions |
| RBAC | `/api/Roles`, `/api/Privileges`, `/api/role-privileges`, `/api/user-roles` | Access control |
| Member self-service | `/api/member` | Member portal API |
| Menus | `/api/menus`, `/api/platform/tenant-menus` | Navigation |
| Files | `/api/files` | Uploads |
| Audit | `/api/audit-logs` | Compliance |
| Health | `/api/health` | Liveness |

## Full Endpoint Reference
See **[API_REFERENCE.md](./API_REFERENCE.md)** for the complete method + route + controller table.

## Integration Tests
`Gym.API.IntegrationTests` — WebApplicationFactory with in-memory/test DB patterns, CSRF disabled for tests
