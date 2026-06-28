# Attendance

## Module Overview
Member and trainer attendance: check-in/out, manual mark, QR scan, dashboards, reports, exports.

## Navigation
| Page | Route |
|------|-------|
| Dashboard | `/gym-admin/attendance` |
| List | `/gym-admin/attendance/list` |
| Check-in | `/gym-admin/attendance/check-in` |
| Check-out | `/gym-admin/attendance/check-out` |
| Reports | `/gym-admin/attendance/reports` |
| Trainer attendance | `/gym-admin/attendance/trainers` |
| Member history | `/gym-admin/attendance/members/:id/history` |

## Buttons
| Button | API |
|--------|-----|
| Check in member | `POST /api/attendance/check-in` |
| Check out | `POST /api/attendance/check-out` |
| Mark attendance | `POST /api/attendance/mark` |
| Trainer check-in/out | `POST /api/attendance/trainers/check-in|check-out` |
| Export PDF/Excel | Report export endpoints |

## Validation
- Member ID required (min 1) on check-in forms
- Open session rules: only `Checked In` status blocks new check-in

## Tables
`MemberAttendance`, `TrainerAttendance`, `AttendanceStatuses`

## Stored Procedures
`sp_MemberAttendance_*`, `sp_TrainerAttendance_*`, `sp_GetAttendanceDashboard`, report SPs

## APIs
`AttendanceController` — `/api/attendance/*`

## Components
`attendance-dashboard`, `attendance-list`, `attendance-check-in`, `attendance-check-out`, `attendance-reports`, `trainer-attendance`, `member-attendance-history`

## Roles
**Gym Admin**, **Trainer** — view/manage per permissions

## SaaS Feature
`ATTENDANCE`

## Notes
Calendar date uses India Standard Time for “today” filters (`075_AttendanceLocalCalendarDate.sql`)
