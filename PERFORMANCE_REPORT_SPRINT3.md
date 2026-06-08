# Performance Report — Production Hardening Sprint 3

**Date:** Sprint 3  
**Scope:** Database indexes, stored procedure optimization, query plan review

---

## Executive Summary

Sprint 3 adds **11 new nonclustered indexes** and refactors **5 high-traffic stored procedures** to reduce tempdb usage and align predicates with index keys. Expected improvements are most significant for attendance date-range queries, audit search, and payment history at scale (10k+ rows per gym).

---

## 1. Indexes Added (`021_PerformanceIndexes.sql`)

### Attendance

| Index | Columns | Purpose |
|-------|---------|---------|
| `IX_MemberAttendance_Gym_Date` | `(GymId, AttendanceDate DESC)` + INCLUDE | Date-range reports, dashboard |
| `IX_MemberAttendance_Gym_Member_Date` | `(GymId, MemberId, AttendanceDate DESC)` | Member history |
| `IX_MemberAttendance_OpenSession` | `(GymId, MemberId)` WHERE `CheckOutAt IS NULL` | Check-in / open session lookup |
| `IX_TrainerAttendance_Gym_Trainer_Open` | `(GymId, TrainerId)` WHERE `CheckOutAt IS NULL` | Trainer check-in |

### Membership

| Index | Columns | Purpose |
|-------|---------|---------|
| `IX_Memberships_Gym_Status_EndDate` | `(GymId, Status, EndDate DESC)` | Active/expired lists |
| `IX_Memberships_Gym_Member_Status` | `(GymId, MemberId, Status)` | Member membership lookup |

### Payment

| Index | Columns | Purpose |
|-------|---------|---------|
| `IX_Payments_Gym_PaymentDate` | `(GymId, PaymentDate DESC)` | Payment history, revenue |
| `IX_Payments_Gym_Member` | `(GymId, MemberId, PaymentDate DESC)` | Member payment history |
| `IX_Payments_Gym_Membership` | `(GymId, MembershipId)` | Invoice / membership payment join |

### Audit (supplemental)

| Index | Columns | Purpose |
|-------|---------|---------|
| `IX_AuditLogs_Gym_Action_CreatedAt` | `(GymId, Action, CreatedAt DESC)` | Action-filtered audit search |
| `IX_AuditLogs_Gym_Entity_CreatedAt` | `(GymId, EntityName, CreatedAt DESC)` | Entity-filtered audit search |

**Pre-existing:** `IX_AuditLogs_GymId_CreatedAt`, `IX_Memberships_GymId`, `IX_Payments_GymId`, `IX_TrainerAttendance_Gym_Date`

---

## 2. Stored Procedure Optimizations (`022_StoredProcedureOptimization.sql`)

| Procedure | Change | Benefit |
|-----------|--------|---------|
| `sp_GetMemberAttendanceByDateRange` | Removed `#Filtered` temp table; separate COUNT + indexed SELECT | Less tempdb, better plan reuse |
| `sp_SearchAuditLogs` | CTE filter on `AuditLogs` before user join; precomputed `@ToExclusive` | Index seek on `GymId + CreatedAt` |
| `sp_GetAllMemberships` | `SYSUTCDATETIME()` for date; index hint on gym/status | Consistent UTC, index-friendly filter |
| `sp_GetExpiredMemberships` | Index-aligned WHERE on `GymId`, `Status`, `EndDate` | Faster expired list |
| `sp_GetPaymentHistory` | INNER JOIN members; index on `(GymId, PaymentDate)` | Avoid orphan rows, sort from index |

---

## 3. Query Plan Review (Recommended Checks)

Run after deploying `021` and `022` on staging with production-like volume:

```sql
SET STATISTICS IO, TIME ON;

DECLARE @GymId UNIQUEIDENTIFIER = '<demo-gym-id>';
DECLARE @Total INT;

EXEC dbo.sp_GetMemberAttendanceByDateRange
    @GymId = @GymId, @FromDate = '2025-01-01', @ToDate = '2026-05-29',
    @PageNumber = 1, @PageSize = 20, @TotalCount = @Total OUTPUT;

EXEC dbo.sp_SearchAuditLogs
    @GymId = @GymId, @PageNumber = 1, @PageSize = 20, @TotalCount = @Total OUTPUT;

EXEC dbo.sp_GetPaymentHistory @GymId = @GymId, @Search = NULL;
```

**Expected plan shapes:**

- **Attendance:** Index Seek on `IX_MemberAttendance_Gym_Date` → Nested Loops to Members/Users
- **Audit:** Index Seek on `IX_AuditLogs_GymId_CreatedAt` or `IX_AuditLogs_Gym_Entity_CreatedAt`
- **Payments:** Index Seek on `IX_Payments_Gym_PaymentDate` → Sort avoided if ORDER BY matches index

**Red flags:** Table Scan on `MemberAttendance`, `AuditLogs`, or `Payments`; Sort operations on large row counts

Capture plans: `Include Actual Execution Plan` in SSMS or `SET SHOWPLAN_XML ON`

---

## 4. Estimated Impact (Illustrative)

| Workload | Before (est.) | After (est.) | Notes |
|----------|---------------|--------------|-------|
| Attendance paged list (90 days, 5k rows) | 200–800 ms | 30–120 ms | Depends on gym size |
| Audit search (30 days, 20k logs) | 500 ms–2 s | 80–300 ms | Search text still applies LIKE |
| Payment history (1k payments) | 100–400 ms | 20–80 ms | Ordered by PaymentDate |
| Expired memberships | 150–500 ms | 40–100 ms | Status + EndDate index |

*Measure on your hardware with `STATISTICS TIME` — numbers are planning guides, not SLAs.*

---

## 5. Operational Notes

- Deploy via `dotnet run --project Backend/Gym.API -- migrate` (scripts recorded in `SchemaVersions`)
- Index creation is **online** for Enterprise; Standard may lock briefly on large tables
- Update statistics after deploy: `UPDATE STATISTICS dbo.MemberAttendance;` (and Payments, AuditLogs, Memberships)
- Monitor: `sys.dm_db_index_usage_stats` after 1 week to confirm index adoption

---

## Related

- `PRODUCTION_HARDENING_SPRINT3.md`
- `022_StoredProcedureOptimization.sql`
- `021_PerformanceIndexes.sql`
