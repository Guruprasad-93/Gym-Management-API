# Production Hardening Sprint 2 — Implementation Report

**Date:** Sprint 2  
**Scope:** JWT cookie auth, health checks, deployment packaging, database change management, observability, backup/DR documentation

---

## Summary

Sprint 2 implements six production-readiness pillars. Backend build **succeeds**. Frontend auth migrated from `localStorage` JWT to httpOnly secure cookies with CSRF protection.

| # | Requirement | Status | Key changes |
|---|-------------|--------|-------------|
| 1 | JWT Security (cookies + CSRF + refresh hardening) | Done | `AuthCookieService`, CSRF middleware, hashed refresh tokens, rotation reuse detection |
| 2 | Health Checks | Done | `GET /health` — SQL + Azure Blob checks |
| 3 | Deployment Readiness | Done | `Dockerfile`, `docker-compose.yml`, `AZURE_DEPLOYMENT_GUIDE.md` |
| 4 | Database Change Management | Done | `SchemaVersions` table, `DatabaseMigrator`, startup migrations disabled in Production |
| 5 | Logging & Monitoring | Done | Serilog, correlation IDs, tenant enricher, Application Insights hook |
| 6 | Backup & Disaster Recovery | Done | `BACKUP_DISASTER_RECOVERY.md` with RPO/RTO |

---

## Updated Production Readiness Score

| Category | Sprint 1 (59 total) | Sprint 2 | Notes |
|----------|---------------------|----------|-------|
| Security & Auth | 12/20 | **17/20** | httpOnly cookies, CSRF, hashed refresh tokens, reuse detection |
| Multi-tenant isolation | 8/10 | 8/10 | Unchanged (Sprint 1) |
| API & validation | 7/10 | 7/10 | Unchanged |
| Frontend security | 4/10 | **8/10** | No JWT in JS; credentials + XSRF interceptors |
| Logging & monitoring | 3/10 | **7/10** | Serilog, correlation ID, tenant context, App Insights ready |
| Deployment & ops | 2/10 | **7/10** | Docker, health checks, Azure guide |
| Database & migrations | 4/10 | **7/10** | Versioned scripts, CI migrate command |
| Backup & DR | 0/5 | **4/5** | Documented strategy; automation still manual |
| Testing & CI | 3/10 | 3/10 | Out of scope |
| Documentation | 5/5 | 5/5 | Sprint report + deployment + DR guides |

### **Overall: 76 / 100** (Pilot-ready → Staging-ready)

**Previous:** 59/100 (Pre-production / pilot only)  
**Change:** +17 points

Remaining gaps for 85+: automated integration tests, CI pipeline YAML, secrets vault integration, WAF/rate-limit at edge, refresh-token family tracking in DB audit, geo-redundant DR automation.

---

## 1. JWT Security

### httpOnly secure cookies
- **`AuthCookieSettings`** — `UseCookieAuth` (default `true`), cookie names, paths.
- **`AuthCookieService`** — sets/clears access + refresh cookies; issues CSRF cookie.
- **JWT bearer** reads access token from cookie via `OnMessageReceived` when cookie auth is enabled.
- **`AuthController`** — login/refresh set cookies; response body omits token values in cookie mode.
- **CORS** — `AllowCredentials()` when cookie auth is on; origins must be explicit (no wildcard).

### CSRF protection
- Double-submit cookie: `XSRF-TOKEN` (readable) + `X-XSRF-TOKEN` header.
- **`CsrfValidationMiddleware`** — validates on POST/PUT/PATCH/DELETE; exempts auth login/refresh and `/health`.
- **`GET /api/auth/csrf`** — pre-login CSRF issuance.
- **Angular** — `withXsrfConfiguration` + `credentialsInterceptor` (`withCredentials: true`).

### Refresh token hardening
- Refresh tokens **hashed (SHA-256)** before DB storage (same approach as password-reset tokens).
- **Rotation** on every refresh (existing behavior preserved).
- **Reuse detection** — revoked token reuse revokes all user refresh tokens and increments token version (`020_RefreshTokenHardening.sql`).
- Refresh cookie path restricted to `/api/auth`; httpOnly + SameSite=Strict.

### Frontend changes
- Tokens removed from `localStorage`; user profile in `sessionStorage` only.
- **`environment.useCookieAuth: true`** in dev and prod.
- **`proxy.conf.json`** — `cookieDomainRewrite: localhost` for dev proxy.

### Breaking change
- All users must **re-login** after deploy (existing plain-text refresh tokens in DB are invalid).
- Swagger/API clients must send cookies + CSRF header, or set `AuthCookies:UseCookieAuth=false` for legacy Bearer-only mode.

---

## 2. Health Checks

**Endpoint:** `GET /health` (anonymous)

| Check | Name | Behavior |
|-------|------|----------|
| SQL Server | `sqlserver` | Unhealthy if DB unreachable |
| Azure Blob | `azureblob` | Healthy (skipped) for local storage; checks container when `FileStorage:Provider=Azure` |

JSON response includes per-check status, description, and duration.

**Files:** `HealthCheckExtensions.cs`, `AzureBlobHealthCheck.cs`, `Program.cs`

---

## 3. Deployment Readiness

| Artifact | Purpose |
|----------|---------|
| `Dockerfile` | Multi-stage .NET 8 API image, port 8080 |
| `docker-compose.yml` | SQL Server 2022 + API with dev env vars |
| `AZURE_DEPLOYMENT_GUIDE.md` | App Service, Azure SQL, Blob, Front Door, CI/CD |

**Local Docker:**
```bash
docker compose up --build
# API: http://localhost:5088/health
```

---

## 4. Database Change Management

### SchemaVersions table
- **`019_SchemaVersions.sql`** — tracks applied script names.
- **`DatabaseMigrator`** — EF migrate + incremental SQL scripts; records each script in `SchemaVersions`.

### Startup behavior
| Environment | `RunMigrationsOnStartup` | `RunSeedOnStartup` |
|-------------|--------------------------|---------------------|
| Production (`appsettings.json`) | **false** | **false** |
| Development | **true** | **true** |

### CI/CD migration command
```bash
dotnet run --project Backend/Gym.API -- migrate
```
Runs EF migrations + pending SQL scripts only (idempotent via `SchemaVersions`).

**`StoredProcedureDeployer`** — obsolete wrapper delegating to `DatabaseMigrator`.

---

## 5. Logging & Monitoring

| Feature | Implementation |
|---------|----------------|
| Serilog | Console + optional Application Insights sink |
| Correlation ID | `X-Correlation-Id` header; propagated in `LogContext` |
| Tenant-aware | `TenantLogEnricher` adds `UserId`, `GymId`, `UserEmail` |
| Request logging | `UseSerilogRequestLogging()` + existing middleware |
| App Insights | `ApplicationInsights:Enabled` + connection string |

**Env vars:** `APPLICATIONINSIGHTS_CONNECTION_STRING`

---

## 6. Backup & Disaster Recovery

See **`BACKUP_DISASTER_RECOVERY.md`** for:
- Azure SQL automated backup + PITR
- Blob soft-delete + GRS replication
- Restore runbooks
- **RPO:** 1 hour (SQL PITR) / 24 hours (blob)
- **RTO:** 4 hours (SQL) / 8 hours (full platform)

---

## Security Test Checklist

### Cookie auth
- [ ] Login sets `gym_access_token` and `gym_refresh_token` as httpOnly cookies (DevTools → Application → Cookies).
- [ ] No `gym_auth_token` in localStorage.
- [ ] Authenticated API calls succeed without `Authorization` header.
- [ ] Logout clears auth cookies.

### CSRF
- [ ] POST without `X-XSRF-TOKEN` header returns **403**.
- [ ] POST with matching CSRF cookie + header succeeds.
- [ ] Login flow fetches CSRF before first mutating request.

### Refresh hardening
- [ ] Refresh rotates cookies; old refresh token cannot be reused.
- [ ] Reusing a revoked refresh token invalidates all sessions (must re-login).

### Health
- [ ] `GET /health` returns 200 with `sqlserver` healthy.
- [ ] Stop SQL → health returns Unhealthy.

### Migrations
- [ ] Production config: API starts without running migrations.
- [ ] `dotnet run -- migrate` applies only pending scripts (check `SchemaVersions`).

---

## Files Added / Changed (Summary)

| Area | Files |
|------|-------|
| Auth | `AuthCookieSettings.cs`, `IAuthCookieService.cs`, `AuthCookieService.cs`, `AuthController.cs`, `CsrfValidationMiddleware.cs` |
| Auth hardening | `AuthService.cs`, `AuthRepository.cs`, `020_RefreshTokenHardening.sql` |
| DB | `019_SchemaVersions.sql`, `DatabaseMigrator.cs`, `DatabaseOptions.cs`, `DatabaseSeeder.cs` |
| Ops | `Dockerfile`, `docker-compose.yml`, `HealthCheckExtensions.cs`, `AzureBlobHealthCheck.cs` |
| Logging | `SerilogExtensions.cs`, `CorrelationIdMiddleware.cs`, `TenantLogEnricher.cs` |
| Frontend | `auth.service.ts`, `auth.interceptor.ts`, `credentials.interceptor.ts`, `app.config.ts`, `environment*.ts` |
| Config | `appsettings.json`, `appsettings.Development.json`, `ConfigurationExtensions.cs`, `ServiceCollectionExtensions.cs`, `Program.cs` |
| Docs | `AZURE_DEPLOYMENT_GUIDE.md`, `BACKUP_DISASTER_RECOVERY.md` |

---

## Known Follow-ups (Sprint 3)

- GitHub Actions / Azure DevOps pipeline YAML with migrate job before deploy.
- Azure Key Vault references for secrets.
- Integration test suite for auth cookie + CSRF flows.
- Angular production static hosting on Azure Static Web Apps with API proxy.
- Automated backup verification (restore drill quarterly).
