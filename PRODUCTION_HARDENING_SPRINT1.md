# Production Hardening Sprint 1 — Implementation Report

**Date:** Sprint 1  
**Scope:** Security blockers from production readiness audit (Priority 1)

---

## Summary

Six priority-1 hardening items were implemented across backend, SQL, and configuration. Backend build **succeeds**.

| # | Requirement | Status | Key changes |
|---|-------------|--------|-------------|
| 1 | Signed file download URLs | Done | HMAC-SHA256 signed URLs with expiry |
| 2 | Remove `@GymId IS NULL` bypass | Done | SQL + `GymScopeResolver` in services |
| 3 | Auth rate limiting | Done | Fixed-window limiter on auth endpoints |
| 4 | Fix `Trainers.IsDeleted` bug | Done | Use `IsActive = 1` in attendance SP |
| 5 | CORS from environment | Done | `Cors:AllowedOrigins` + `CORS_ALLOWED_ORIGINS` |
| 6 | HSTS + ForwardedHeaders | Done | Azure/App Service proxy support |

---

## 1. File Download Security (Signed URLs)

### Implementation
- **`IFileDownloadUrlSigner`** / **`FileDownloadUrlSigner`** — HMAC-SHA256 over `{fileId}|{gymId}|{exp}` using `FileStorage:UrlSigningSecret` (falls back to `Jwt:Secret`).
- **URL format:** `/api/files/{id}/content?g={gymId}&exp={unix}&sig={hex}`
- **Default expiry:** 60 minutes (`FileStorage:DownloadUrlExpiryMinutes`)
- **`FileService`** generates signed URLs on upload and when listing files.
- **`DownloadAsync`** accepts signature query params; validates expiry + signature before loading file by **required** `gymId`.
- Authenticated users without a signature can still download if they pass gym-scoped authorization checks.

### Files
- `Gym.Application/Interfaces/IFileDownloadUrlSigner.cs`
- `Gym.Infrastructure/Security/FileDownloadUrlSigner.cs`
- `Gym.Application/Services/FileService.cs`
- `Gym.API/Controllers/FilesController.cs`

### Breaking change
- Raw `/api/files/{id}/content` URLs **no longer work** without `g`, `exp`, and `sig`.
- Frontend `FileService.contentUrl()` continues to work — API returns full signed URLs in `publicUrl`.

---

## 2. Tenant Isolation — Remove `@GymId IS NULL` Bypass

### SQL
- Removed `(@GymId IS NULL OR …)` patterns from scripts **005–017** (53 occurrences).
- **`sp_File_GetById`** now throws if `@GymId IS NULL` and requires `GymId = @GymId`.
- **`018_ProductionHardening.sql`** adds:
  - `sp_Member_GetGymId` / `sp_Trainer_GetGymId` (Super Admin entity resolution)
  - Hardened `sp_GetRevenueDashboard` / `sp_GetMonthlyRevenueSummary` (require `@GymId`)

### Application
- **`GymScopeResolver`** — central helper:
  - `ResolveRequired(user, requestedGymId)` — Super Admin **must** supply `gymId`; gym users use JWT gym.
  - `ResolveForEntity(user, entityGymId)` — entity-scoped access for Super Admin.
- Updated services: `FileService`, `MemberService`, `TrainerService`, `PaymentService`, `AttendanceService`, `MembershipService`, `MembershipPlanService`, `DietPlanService`, `WorkoutPlanService`, `AuditService`, `GymAdminService`.

### API changes (Super Admin)
Super Admin must pass **`gymId`** query parameter for tenant-scoped operations:
- Audit dashboard/search/export — `?gymId=`
- Revenue dashboard — `?gymId=`
- Existing list endpoints already supported `gymId` (members, trainers, etc.)

**Note:** Platform-wide aggregates (e.g. `sp_Dashboard_SuperAdmin`) are unchanged — they do not use tenant bypass patterns.

---

## 3. Rate Limiting (Auth Endpoints)

### Implementation
- Policy **`AuthEndpoints`**: fixed window, default **10 requests / 60 seconds / IP+path**.
- Applied to:
  - `POST /api/auth/login`
  - `POST /api/auth/refresh`
  - `POST /api/auth/forgot-password`
  - `POST /api/auth/reset-password`
- Returns **HTTP 429** when exceeded.

### Configuration (`appsettings.json`)
```json
"RateLimiting": {
  "AuthPermitLimit": 10,
  "AuthWindowSeconds": 60
}
```

### Files
- `Gym.API/Extensions/RateLimitingExtensions.cs`
- `Gym.API/Controllers/AuthController.cs`
- `Gym.API/Program.cs` — `app.UseRateLimiter()`

---

## 4. Trainers.IsDeleted Runtime Bug

### Fix
- `sp_TrainerAttendance_CheckIn` in **`013_AttendanceModule.sql`** now checks `t.IsActive = 1` instead of non-existent `t.IsDeleted`.

---

## 5. CORS from Environment

### Configuration
```json
"Cors": {
  "AllowedOrigins": [ "http://localhost:4200" ]
}
```

### Environment variable
```bash
CORS_ALLOWED_ORIGINS=https://app.example.com,https://admin.example.com
```
(comma-separated; mapped to `Cors:AllowedOrigins:*`)

### Files
- `Gym.Application/Options/CorsSettings.cs`
- `Gym.API/Extensions/ServiceCollectionExtensions.cs` — `AddCorsFromConfiguration`

---

## 6. HSTS + ForwardedHeaders (Azure / App Service)

### Implementation
- **`AddAzureForwardedHeaders()`** — trusts `X-Forwarded-For` and `X-Forwarded-Proto` (clears known networks for App Service).
- **`Program.cs`:**
  - `app.UseForwardedHeaders()` (before HTTPS redirect)
  - `app.UseHsts()` in non-Development environments
  - `app.UseHttpsRedirection()`

### Production validation
- `Demo:Enabled` must be **false** in Production (new check).

---

## Deployment Steps

1. **Restart API** — runs embedded SQL scripts including **`018_ProductionHardening.sql`**.
2. **Set environment variables** (Production):
   ```bash
   JWT_SECRET=<32+ chars>
   DATABASE_CONNECTION=<sql connection>
   CORS_ALLOWED_ORIGINS=https://your-frontend.azurewebsites.net
   FILE_STORAGE_URL_SIGNING_SECRET=<optional, defaults to JWT_SECRET>
   ```
3. **Re-login** all users (existing file URLs in DB are stale — re-upload or refresh metadata).
4. **Super Admin UI:** pass `gymId` when viewing audit/revenue for a specific gym.

---

## Security Test Checklist

Use this checklist to verify Sprint 1 hardening before production release.

### File download security
- [ ] Upload a member profile photo; confirm `publicUrl` includes `g`, `exp`, and `sig` query params.
- [ ] Open signed URL in browser **without** Authorization header — image loads.
- [ ] Request `/api/files/{id}/content` **without** query params — returns **401**.
- [ ] Tamper with `sig` parameter — returns **401**.
- [ ] Use expired `exp` timestamp — returns **401**.
- [ ] Attempt download with valid sig but wrong `g` (gymId) — returns **404**.
- [ ] Increment `fileId` with stolen sig from another file — returns **401** or **404**.
- [ ] Authenticated user with gym access can download via signed URL only (img tags).

### Tenant isolation
- [ ] Gym Admin A cannot access Gym B member by ID (404/403).
- [ ] Call member list SP/API without gym context as Gym Admin — scoped to own gym only.
- [ ] Super Admin **without** `gymId` on revenue dashboard — returns **400** validation error.
- [ ] Super Admin **with** valid `gymId` — returns only that gym's revenue.
- [ ] Audit search without `gymId` as Super Admin — fails with gym required error.
- [ ] Verify `sp_File_GetById` with `@GymId = NULL` throws SQL error 50001.

### Rate limiting
- [ ] Send 11+ login attempts within 60s from same IP — 11th returns **429**.
- [ ] Repeat for `/api/auth/refresh`, `/forgot-password`, `/reset-password`.
- [ ] After window expires, requests succeed again.
- [ ] Legitimate login still works under limit.

### Trainer attendance bug
- [ ] Trainer check-in succeeds for active trainer (no SQL error about `IsDeleted`).
- [ ] Inactive trainer check-in fails with trainer not found.

### CORS
- [ ] Frontend on configured origin — API calls succeed.
- [ ] Request from unlisted origin — browser blocks (CORS error).
- [ ] Change `CORS_ALLOWED_ORIGINS` env var; restart API; new origin works.

### HSTS / Forwarded headers
- [ ] In Production/staging, response includes `Strict-Transport-Security` header.
- [ ] Behind Azure App Service, audit logs show client IP (not always `127.0.0.1`).
- [ ] HTTPS redirect works correctly behind TLS-terminated proxy.

### Production config guards
- [ ] Start API in Production with `Demo:Enabled=true` — startup fails.
- [ ] Start with `Jwt:ReturnResetTokenInDevelopment=true` — startup fails.

---

## Known Follow-ups (Sprint 2)

- Move JWT from `localStorage` to httpOnly cookies (frontend).
- Health checks + Dockerfile for Azure deployment.
- CI/CD migrations (remove startup migrate from app).
- Structured logging / Application Insights.
- Rotate/regenerate stored `PublicUrl` values for existing files after deploy.

---

## Files Changed (Summary)

| Area | Files |
|------|-------|
| SQL | `005–017` (bypass removal), `018_ProductionHardening.sql`, `013` (trainer fix), `017` (file get) |
| Security | `FileDownloadUrlSigner.cs`, `GymScopeResolver.cs`, `RateLimitingExtensions.cs`, `ForwardedHeadersExtensions.cs` |
| Services | All `*Service.cs` with gym scope + `FileService` |
| API | `Program.cs`, `AuthController.cs`, `FilesController.cs`, `AuditLogsController.cs`, `PaymentsController.cs` |
| Config | `appsettings.json`, `ConfigurationExtensions.cs`, `CorsSettings.cs`, `RateLimitSettings.cs`, `FileStorageSettings.cs` |
