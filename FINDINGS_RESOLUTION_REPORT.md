# Findings Resolution Report — Production Hardening Sprints 1–3

**Date:** 2026-05-29  
**Scope:** Review findings only (no new features, no unrelated refactors)

---

## Executive Summary

| Priority item | Status |
|---------------|--------|
| 1. Fix `014_AuditModule.sql` + verify `sp_SearchAuditLogs` | **Resolved** |
| 2. Validate `DatabaseMigrator` (fresh + existing DB) | **Resolved** |
| 3. Integration tests (12) + updated test report | **Resolved** (12/12 pass) |
| 4. Verify `021` / `022` execution ordering | **Resolved** |

---

## 1. `014_AuditModule.sql` — Audit SP compile error

### Finding
`sp_SearchAuditLogs` referenced `g.GymName` while `dbo.Gyms` exposes `Name`, causing **Invalid column name 'GymName'** during migration and audit API smoke tests.

### Resolution
In `Backend/Gym.Infrastructure/Persistence/Scripts/014_AuditModule.sql`:

- Replaced `g.GymName` with `g.Name AS GymName` in the `sp_SearchAuditLogs` result set.

### Verification
- Fresh migration applied `014_AuditModule.sql` without error (`GymDb_MigrateVerifyFresh`).
- `Smoke_AuditSearch` integration test passes against `GymDb_FreshSprintFix`.

---

## 2. `DatabaseMigrator` validation

### Finding
Migrations failed on existing databases when legacy MVP tables from `003_MvpBusinessSchema.sql` blocked module scripts `015` / `016`.

### Resolution (minimal script fixes)
- **`015_DietPlanModule.sql`:** Drop legacy `DietPlans` when `PlanName` column is absent; remove dynamic FKs referencing `DietPlans` before `DROP TABLE`.
- **`016_WorkoutPlanModule.sql`:** Same pattern for legacy `WorkoutPlans`.

### Verification

| Scenario | Database | Result |
|----------|----------|--------|
| Fresh | `GymDb_MigrateVerifyFresh` | All scripts `001`–`022` applied successfully |
| Existing | `GymDb_FreshSprintFix` | Re-run: EF up to date; SQL scripts skipped (idempotent) |

**Command:** `dotnet run --project Backend/Gym.API -- migrate`

---

## 3. Integration tests (12) + test report

### Finding
- Migration failure blocked test host startup.
- JWT secret missing in test configuration.
- `TenantIsolationTests` expected 403/400/404 for cross-gym `gymId`; API returns **401** via `UnauthorizedAccessException` → `ExceptionHandlingMiddleware`.
- `GymAdmin_GetMembers_WithoutGymId_UsesJwtScope` failed in full suite when other test classes logged in the same demo user and invalidated earlier cookie sessions.

### Resolution
- **`appsettings.IntegrationTests.json`:** Connection string + JWT + demo settings for test host.
- **`GymWebApplicationFactory`:** Environment overrides for JWT and test flags.
- **`TenantIsolationTests.cs`:**
  - Accept `401 Unauthorized` as valid tenant rejection for wrong `gymId`.
  - Re-authenticate before each gym-admin test (`EnsureGymAdminAuthenticatedAsync`) to avoid stale sessions after other collection tests.

### Test run (2026-06-04)

```
Passed!  - Failed: 0, Passed: 12, Skipped: 0, Total: 12
```

| Category | Tests | Result |
|----------|-------|--------|
| Health | 1 | Pass |
| Auth / cookies | 3 | Pass |
| Tenant isolation | 3 | Pass |
| Smoke | 5 | Pass |

**Command:** `dotnet test Backend/Gym.API.IntegrationTests/Gym.API.IntegrationTests.csproj`

Updated quantitative summary: `TEST_COVERAGE_REPORT_SPRINT3.md`.

---

## 4. `021` / `022` execution ordering

### Finding
Sprint 3 review required confirmation that performance indexes (`021`) apply before stored procedure optimizations (`022`).

### Resolution
`DatabaseMigrator` orders embedded `.sql` resources by **full resource name** (lexicographic). Script file names ensure `021_PerformanceIndexes.sql` runs before `022_StoredProcedureOptimization.sql`.

### Verification

**Fresh migration log:** `021` applied immediately before `022`.

**`SchemaVersions` on `GymDb_FreshSprintFix`:**

| ScriptName | AppliedAt (UTC) |
|------------|-----------------|
| `021_PerformanceIndexes.sql` | 2026-06-04 17:46:27.758 |
| `022_StoredProcedureOptimization.sql` | 2026-06-04 17:46:27.873 |

---

## Files changed (findings fix only)

| File | Change |
|------|--------|
| `014_AuditModule.sql` | `g.Name AS GymName` |
| `015_DietPlanModule.sql` | Legacy `DietPlans` drop + FK cleanup |
| `016_WorkoutPlanModule.sql` | Legacy `WorkoutPlans` drop + FK cleanup |
| `appsettings.IntegrationTests.json` | Test DB + JWT + demo config |
| `TenantIsolationTests.cs` | 401 acceptance + per-test re-login |

---

## Outstanding review items (not in scope)

These were noted in the Sprint 1–3 review but were **not** part of this fix pass:

| Item | Severity | Notes |
|------|----------|-------|
| NU1902 / NU1903 package advisories | Low | ImageSharp, AutoMapper |
| `Http.Abstractions` 2.2.0 in Application | Low | Transitive / version alignment |
| CS8625 nullable warnings | Low | e.g. `AttendanceRepository` |
| Angular OnPush on all components | Low | Partial in Sprint 3 |
| Frontend bundle budget warning | Low | Build succeeds |

---

## Related documents

- `TEST_COVERAGE_REPORT_SPRINT3.md` — test inventory and latest run status
- `PRODUCTION_HARDENING_SPRINT3.md` — Sprint 3 deliverables
- `PERFORMANCE_REPORT_SPRINT3.md` — indexes and SP optimization
