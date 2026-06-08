# Attendance Management Module

**Script:** `Backend/Gym.Infrastructure/Persistence/Scripts/013_AttendanceModule.sql` (auto-deployed on API startup)  
**API base:** `GET/POST /api/attendance`  
**Build:** succeeded

---

## Database

| Object | Purpose |
|--------|---------|
| `AttendanceStatuses` | Lookup: Checked In, Checked Out, Present, Absent, Late, Excused |
| `MemberAttendance` | Member sessions + manual marks (enhanced from `004`) |
| `TrainerAttendance` | Trainer check-in/out |
| `AuditLogs` | Written via `sp_AuditLog_Insert` on mutations |

**Multi-tenant:** All SPs filter by `@GymId`; trainers see assigned members via `@TrainerId`.

---

## API endpoints (Swagger: Development `/swagger`)

| Method | Path | Permission |
|--------|------|------------|
| GET | `/api/attendance/statuses` | VIEW_ATTENDANCE |
| GET | `/api/attendance/dashboard` | VIEW_ATTENDANCE |
| GET | `/api/attendance/today` | VIEW_ATTENDANCE |
| GET | `/api/attendance` | VIEW_ATTENDANCE (paged, date range) |
| GET | `/api/attendance/members/{id}/history` | VIEW_ATTENDANCE |
| POST | `/api/attendance/check-in` | MANAGE_ATTENDANCE |
| POST | `/api/attendance/check-out` | MANAGE_ATTENDANCE |
| POST | `/api/attendance/mark` | MANAGE_ATTENDANCE |
| GET | `/api/attendance/reports/daily` | VIEW_ATTENDANCE |
| GET | `/api/attendance/reports/monthly` | VIEW_ATTENDANCE |
| GET | `/api/attendance/reports/daily/export/pdf` | EXPORT_ATTENDANCE_REPORTS |
| GET | `/api/attendance/reports/daily/export/excel` | EXPORT_ATTENDANCE_REPORTS |
| GET | `/api/attendance/reports/monthly/export/pdf` | EXPORT_ATTENDANCE_REPORTS |
| GET | `/api/attendance/reports/monthly/export/excel` | EXPORT_ATTENDANCE_REPORTS |
| GET | `/api/attendance/members/{id}/history/export/excel` | EXPORT_ATTENDANCE_REPORTS |
| POST | `/api/attendance/trainers/check-in` | MANAGE_TRAINER_ATTENDANCE |
| POST | `/api/attendance/trainers/check-out` | MANAGE_TRAINER_ATTENDANCE |
| GET | `/api/attendance/trainers` | VIEW_TRAINER_ATTENDANCE |

---

## Permissions (seeded)

- `VIEW_ATTENDANCE`, `MANAGE_ATTENDANCE`
- `VIEW_TRAINER_ATTENDANCE`, `MANAGE_TRAINER_ATTENDANCE`
- `EXPORT_ATTENDANCE_REPORTS`

Assigned to **GymAdmin** and **Trainer** (via `DatabaseSeeder` on startup). **Re-login** after first deploy to refresh JWT claims.

---

## Frontend routes

### Gym Admin (`/gym-admin/attendance/...`)

| Route | Screen |
|-------|--------|
| `/gym-admin/attendance` | Dashboard + today's list |
| `/gym-admin/attendance/list` | Paged list, filters |
| `/gym-admin/attendance/check-in` | Check-in screen |
| `/gym-admin/attendance/check-out` | Check-out screen |
| `/gym-admin/attendance/members/:id/history` | Member history + Excel export |
| `/gym-admin/attendance/reports` | Daily/monthly reports + PDF/Excel |
| `/gym-admin/attendance/trainers` | Trainer attendance |

### Trainer (`/trainer/attendance/...`)

Dashboard, check-in, check-out (shared components).

---

## Test checklist

### Database
- [ ] Restart API — log shows `Deployed SQL script: ...013_AttendanceModule.sql`
- [ ] `SELECT * FROM AttendanceStatuses` — 6 rows
- [ ] `MemberAttendance` has `AttendanceStatusId`, `AttendanceDate` columns

### API — member
- [ ] `GET /api/attendance/statuses` — 200
- [ ] `POST /api/attendance/check-in` `{ "memberId": 1 }` — 200
- [ ] Duplicate check-in same member — 400
- [ ] `POST /api/attendance/check-out` `{ "memberId": 1 }` — 200
- [ ] `POST /api/attendance/mark` with statusId 3 (Present) — 200
- [ ] `GET /api/attendance/today` — includes records
- [ ] `GET /api/attendance?fromDate=...&toDate=...&pageNumber=1&pageSize=10` — paged 200
- [ ] `GET /api/attendance/dashboard` — KPI counts
- [ ] `GET /api/attendance/reports/daily?date=2026-06-03` — summary + details
- [ ] `GET /api/attendance/reports/monthly?year=2026&month=6` — per-member stats

### API — exports
- [ ] Daily PDF export opens valid PDF
- [ ] Daily Excel export opens in Excel
- [ ] Monthly PDF / Excel
- [ ] Member history Excel

### API — trainer
- [ ] `POST /api/attendance/trainers/check-in` — 200
- [ ] `GET /api/attendance/trainers` — paged list

### Authorization
- [ ] User without `VIEW_ATTENDANCE` — 403 on GET
- [ ] Trainer sees only assigned members in lists
- [ ] Gym admin cannot see other gym's attendance

### Audit
- [ ] After check-in, `SELECT TOP 5 * FROM AuditLogs WHERE EntityName = 'MemberAttendance'` — new row

### UI
- [ ] Gym Admin menu shows Attendance + Reports
- [ ] Dashboard loads stats
- [ ] Check-in / check-out flows
- [ ] List pagination and filters
- [ ] Reports tab export buttons

---

## Key files

**Backend:** `013_AttendanceModule.sql`, `AttendanceController.cs`, `AttendanceService.cs`, `AttendanceRepository.cs`, `AttendanceReportExporter.cs`  
**Frontend:** `features/gym-admin/attendance/*`, `core/services/attendance.service.ts`, `shared/models/attendance.models.ts`
