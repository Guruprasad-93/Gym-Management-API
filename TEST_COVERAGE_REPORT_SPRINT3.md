# Test Coverage Report — Production Hardening Sprint 3

**Date:** Sprint 3 (updated after findings resolution — 2026-05-29)  
**Project:** `Gym.API.IntegrationTests` (xUnit + `WebApplicationFactory`)

---

## Summary

| Metric | Value |
|--------|-------|
| Test project | `Backend/Gym.API.IntegrationTests` |
| Total integration tests | **12** |
| Last run | **12 passed**, 0 failed, 0 skipped |
| Test DB | `GymDb_FreshSprintFix` (`appsettings.IntegrationTests.json`) |
| Test categories | Health (1), Auth/Cookies (3), Tenant isolation (3), Smoke (5) |
| Frontend unit tests | 1 scaffold (`app.component.spec.ts`) — unchanged |
| Line coverage (backend) | Not instrumented yet — see **Coverage gaps** |

---

## Test Inventory

### Health
| Test | File | Validates |
|------|------|-----------|
| `Health_ReturnsHealthy_WithSqlCheck` | `HealthEndpointTests.cs` | `GET /health`, SQL check present |

### Auth cookie + CSRF
| Test | File | Validates |
|------|------|-----------|
| `Login_SetsHttpOnlyCookies_AndValidateSucceeds` | `AuthCookieTests.cs` | CSRF + login cookies + validate |
| `MutatingRequest_WithoutCsrf_ReturnsForbidden` | `AuthCookieTests.cs` | CSRF enforcement on change-password |
| `Session_Endpoint_RefreshesPermissions` | `AuthCookieTests.cs` | `GET /api/auth/session` |

### Multi-tenant isolation
| Test | File | Validates |
|------|------|-----------|
| `GymAdmin_CannotAccessMembers_WithWrongGymId` | `TenantIsolationTests.cs` | Cross-gym `gymId` rejected (401/403/400/404) |
| `GymAdmin_GetMembers_WithoutGymId_UsesJwtScope` | `TenantIsolationTests.cs` | JWT-scoped list OK |
| `Anonymous_CannotAccessMembers` | `TenantIsolationTests.cs` | 401 without auth |

### Smoke (core flows)
| Test | File | Validates |
|------|------|-----------|
| `Smoke_Login_And_ListMembers` | `SmokeTests.cs` | Members API |
| `Smoke_AttendanceStatuses` | `SmokeTests.cs` | Attendance module |
| `Smoke_MembershipsList` | `SmokeTests.cs` | Memberships API |
| `Smoke_PaymentsHistory` | `SmokeTests.cs` | Payments API |
| `Smoke_AuditSearch` | `SmokeTests.cs` | Audit logs API |

---

## How to Run

**Prerequisites:** SQL Server; connection string in `appsettings.IntegrationTests.json` (default `GymDb_FreshSprintFix`). Tests call `DatabaseMigrator` + `DatabaseSeeder` on first use.

```bash
dotnet test Backend/Gym.API.IntegrationTests/Gym.API.IntegrationTests.csproj
```

With coverage (optional):

```bash
dotnet test Backend/Gym.API.IntegrationTests/Gym.API.IntegrationTests.csproj \
  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

---

## Coverage by Layer (Qualitative)

| Layer | Coverage | Notes |
|-------|----------|-------|
| API auth pipeline | **High** | Cookies, CSRF, session refresh |
| Tenant isolation | **Medium** | Members endpoint; extend to files/payments |
| Domain services | **Low** | No isolated unit tests yet |
| Repositories / SPs | **Low** | Exercised indirectly via API |
| Angular UI | **Minimal** | No component tests added in Sprint 3 |

---

## Coverage Gaps (Sprint 4 Candidates)

- [ ] Unit tests for `GymScopeResolver`, `AuthService`, `FileService`
- [ ] File signed-URL download authorization tests
- [ ] Super Admin `gymId` required tests (audit, revenue)
- [ ] Rate limiting (429) tests
- [ ] Angular tests for `httpErrorInterceptor`, `GlobalErrorHandler`, `AuthService.refreshPermissions`
- [ ] E2E (Playwright/Cypress) for login → dashboard flow

---

## CI Recommendation

```yaml
integration_tests:
  services:
    sqlserver:
      image: mcr.microsoft.com/mssql/server:2022-latest
  steps:
    - dotnet run --project Backend/Gym.API -- migrate
    - dotnet test Backend/Gym.API.IntegrationTests --logger trx
```

---

## Related

- `FINDINGS_RESOLUTION_REPORT.md` — Sprint 1–3 review fixes and verification
- `PRODUCTION_HARDENING_SPRINT3.md`
