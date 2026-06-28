# Production Checklist — Gym Management SaaS v1.0.0-RC1

Complete all sections before promoting RC1 to production. Sign off each item with date and owner.

**Release:** v1.0.0-RC1  
**Target environment:** Production  
**Sign-off required from:** Platform Lead, DBA, Security, QA

---

## Security Checklist

| # | Item | Status | Owner | Date |
|---|------|--------|-------|------|
| S1 | JWT secret is 32+ characters, stored in Key Vault (not appsettings) | ☐ | | |
| S2 | `Demo__Enabled=false` and demo passwords removed from config | ☐ | | |
| S3 | Bootstrap Super Admin password changed from default | ☐ | | |
| S4 | HTTPS enforced on API and UI (TLS 1.2+) | ☐ | | |
| S5 | HSTS enabled (automatic in Production environment) | ☐ | | |
| S6 | CORS restricted to production frontend origin only | ☐ | | |
| S7 | Cookie auth: HttpOnly, Secure, SameSite configured correctly | ☐ | | |
| S8 | CSRF protection verified on POST/PUT/DELETE | ☐ | | |
| S9 | Rate limiting active on `/api/auth/*` (10 req/min default) | ☐ | | |
| S10 | SQL Server firewall: deny public access; allow app subnet only | ☐ | | |
| S11 | SQL connection uses encrypted connection (`Encrypt=True`) | ☐ | | |
| S12 | File upload limits enforced (`MaxFileSizeBytes`, allowed extensions) | ☐ | | |
| S13 | Azure Blob containers are private; signed URLs for downloads | ☐ | | |
| S14 | Swagger/OpenAPI disabled in Production | ☐ | | |
| S15 | Secrets not committed to git (.env, User Secrets, Key Vault only) | ☐ | | |
| S16 | Razorpay/WhatsApp/Firebase/OpenAI keys in Key Vault if enabled | ☐ | | |
| S17 | Forwarded headers configured for reverse proxy | ☐ | | |
| S18 | Penetration test or OWASP top-10 review completed for auth flows | ☐ | | |

---

## Database Checklist

| # | Item | Status | Owner | Date |
|---|------|--------|-------|------|
| D1 | All migrations 001–052 applied; verified in `dbo.SchemaVersions` | ☐ | | |
| D2 | EF Core migrations applied (`dotnet run -- migrate`) | ☐ | | |
| D3 | `Database__RunMigrationsOnStartup=false` in production | ☐ | | |
| D4 | `Database__RunSeedOnStartup=false` after bootstrap | ☐ | | |
| D5 | Azure SQL tier ≥ General Purpose (PITR enabled) | ☐ | | |
| D6 | Backup retention ≥ 30 days; geo-redundancy configured | ☐ | | |
| D7 | LoginIdentifier column populated for all users (script 051) | ☐ | | |
| D8 | Tenant menus seeded for all active gyms (script 052) | ☐ | | |
| D9 | Performance indexes applied (script 021) | ☐ | | |
| D10 | Database user has least privilege (not `sa` for app connection) | ☐ | | |
| D11 | Monthly restore drill scheduled | ☐ | | |
| D12 | Connection pool and timeout settings reviewed | ☐ | | |

---

## API Checklist

| # | Item | Status | Owner | Date |
|---|------|--------|-------|------|
| A1 | `/health` returns 200 Healthy | ☐ | | |
| A2 | LoginIdentifier login works (Super Admin, Gym Admin, Trainer, Member) | ☐ | | |
| A3 | CSRF token flow: GET `/api/auth/csrf` → login → mutating request | ☐ | | |
| A4 | Session refresh: GET `/api/auth/session` returns permissions | ☐ | | |
| A5 | Tenant menu disable → API returns 403 for disabled module | ☐ | | |
| A6 | Cross-tenant access blocked (GymId isolation) | ☐ | | |
| A7 | Paging respects max pageSize (100 for members) | ☐ | | |
| A8 | File upload/download works (local or Azure) | ☐ | | |
| A9 | Razorpay webhook configured (if payments enabled) | ☐ | | |
| A10 | WhatsApp provider configured (if notifications enabled) | ☐ | | |
| A11 | Application Insights receiving telemetry | ☐ | | |
| A12 | Serilog logging to configured sink (App Insights / file) | ☐ | | |
| A13 | API QA runner: ≥95% pass on staging (`e2e-qa-runner.ps1`) | ☐ | | |
| A14 | Error responses do not leak stack traces in Production | ☐ | | |

---

## Frontend Checklist

| # | Item | Status | Owner | Date |
|---|------|--------|-------|------|
| F1 | Production build: `ng build --configuration production` succeeds | ☐ | | |
| F2 | API URL / proxy configured for production domain | ☐ | | |
| F3 | Login page uses LoginIdentifier field (not email-only) | ☐ | | |
| F4 | Gym-branded login loads white-label settings per gymId | ☐ | | |
| F5 | All role portals accessible: Super Admin, Gym Admin, Trainer, Member | ☐ | | |
| F6 | Tenant menu guard hides disabled modules | ☐ | | |
| F7 | Public website pages render for published gyms | ☐ | | |
| F8 | Global error handler and HTTP interceptor active | ☐ | | |
| F9 | Playwright E2E: ≥95% pass on staging (44/46 minimum) | ☐ | | |
| F10 | No console errors on critical paths (login, dashboard, members) | ☐ | | |
| F11 | SPA fallback routing configured (all routes → index.html) | ☐ | | |
| F12 | Static assets cached; index.html not aggressively cached | ☐ | | |

---

## Backup Checklist

| # | Item | Status | Owner | Date |
|---|------|--------|-------|------|
| B1 | Azure SQL automated backups verified | ☐ | | |
| B2 | PITR tested: restore to staging within RTO (4 hours) | ☐ | | |
| B3 | Blob soft delete enabled (14–30 days) | ☐ | | |
| B4 | Blob geo-redundant storage (GRS/GZRS) configured | ☐ | | |
| B5 | Configuration backup (Key Vault export / IaC) | ☐ | | |
| B6 | Runbook documented: [BACKUP_DISASTER_RECOVERY.md](./BACKUP_DISASTER_RECOVERY.md) | ☐ | | |
| B7 | On-call contact list for database restore | ☐ | | |
| B8 | RPO/RTO agreed with business: 1h SQL / 4h platform | ☐ | | |

---

## Monitoring Checklist

| # | Item | Status | Owner | Date |
|---|------|--------|-------|------|
| M1 | Application Insights connected to API | ☐ | | |
| M2 | Health check probed every 1–5 min (App Service / uptime monitor) | ☐ | | |
| M3 | Alert: API health check failure | ☐ | | |
| M4 | Alert: HTTP 5xx rate > threshold | ☐ | | |
| M5 | Alert: SQL DTU/CPU > 80% sustained | ☐ | | |
| M6 | Alert: SQL storage > 80% | ☐ | | |
| M7 | Alert: Auth rate limit spikes (possible brute force) | ☐ | | |
| M8 | Log retention policy configured (≥30 days) | ☐ | | |
| M9 | Dashboard: request rate, latency p95, error rate | ☐ | | |
| M10 | Dashboard: active gyms, failed logins, payment failures | ☐ | | |

---

## Release Sign-Off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Platform Lead | | | |
| DBA | | | |
| Security | | | |
| QA Lead | | | |
| Product Owner | | | |

**Git tag after sign-off:**

```bash
git tag -a v1.0.0-RC1 -m "Gym SaaS v1.0.0 Release Candidate 1"
git push origin v1.0.0-RC1
```

**Promote to GA (`v1.0.0`) only after:**
- All **blocker** checklist items complete
- Staging soak test ≥ 72 hours without P1 incidents
- Rollback procedure tested once

---

## RC1 Known Blockers (review before GA)

| Issue | Severity | Workaround |
|-------|----------|------------|
| Trainer subscription limit counts soft-deleted trainers | Medium | Upgrade plan or manual DB adjustment |
| Assign dialogs use pageSize=200 for members | Low | Assign from member detail page |
| Integration tests require clean DB (local dev) | Low | Use API QA runner for staging validation |

See [TESTING_SUMMARY.md](./TESTING_SUMMARY.md) for full test results.
