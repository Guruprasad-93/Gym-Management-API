# Class Scheduling

## Module Overview
Recurring weekly class schedule management for gym branches (class name, trainer, day, time, capacity).

## Purpose
Define bookable class slots that members see in the booking module.

## Navigation Paths
| Page | Route | Permission |
|------|-------|------------|
| Schedule list | `/gym-admin/schedules` | MANAGE_SCHEDULES |
| New schedule | `/gym-admin/schedules/new` | MANAGE_SCHEDULES |
| Edit schedule | `/gym-admin/schedules/:id/edit` | MANAGE_SCHEDULES |
| Booking settings | `/gym-admin/schedules/settings` | MANAGE_SCHEDULES |

## Screen Description

### Schedule List (`ScheduleListComponent`)
- Card grid per schedule: class name, branch, trainer, day, time, capacity, status
- Header actions: Add schedule, Settings, Bookings, Analytics

### Schedule Editor (`ScheduleEditorComponent`)
- Form: class name, branch, trainer, day of week, start/end time, capacity, description
- Create or update mode

### Booking Settings (`BookingSettingsComponent`)
- Max bookings per member per day, cancellation window, reminder minutes, allow waitlist toggle

## Buttons & Functionality
| Button | Action |
|--------|--------|
| Add schedule | Navigate to `/schedules/new` |
| Settings | Navigate to `/schedules/settings` |
| Bookings / Analytics | Cross-links |
| Edit | Navigate to `/:id/edit` |
| Delete schedule | Confirm dialog → `DELETE /api/schedules/{id}` (permanent delete) |
| Create / Update schedule | `POST` or `PUT /api/schedules` |
| Save settings | `PUT /api/bookings/settings` |
| Back to schedules | Router link |

## Validation Rules

### Frontend (reactive forms)
- Branch ID ≥ 1, trainer ID ≥ 1
- Class name required
- Day of week 0–6
- Capacity 1–500
- End time must be after start time

### Backend (`BookingDtoValidators`)
- `CreateClassScheduleDtoValidator`, `UpdateClassScheduleDtoValidator`
- Update status must be `Active`

## Business Rules
- Schedules are weekly recurring (day of week + time)
- **Delete is permanent:** removes `ClassSchedules`, all `SlotBookings`, and `BookingWaitlist` for that schedule (transactional SP)
- Available slots for booking only include `Status = Active` schedules
- Tenant isolated by `GymId`

## Workflow
1. Admin creates schedule for branch + trainer
2. Members see generated slots via `sp_GetAvailableSlots`
3. Admin may edit schedule fields
4. Admin deletes schedule → cascade delete related bookings

## Database Tables
`ClassSchedules`, `BookingSettings` (settings page)

## Stored Procedures
`sp_CreateClassSchedule`, `sp_UpdateClassSchedule`, `sp_GetClassScheduleById`, `sp_GetClassSchedulesPaged`, `sp_DeleteClassSchedule`, `sp_GetBookingSettings`, `sp_UpsertBookingSettings`

## APIs
| Method | Endpoint | Permission |
|--------|----------|------------|
| GET | `/api/schedules` | VIEW_BOOKINGS |
| GET | `/api/schedules/{id}` | VIEW_BOOKINGS |
| POST | `/api/schedules` | MANAGE_SCHEDULES |
| PUT | `/api/schedules/{id}` | MANAGE_SCHEDULES |
| DELETE | `/api/schedules/{id}` | MANAGE_SCHEDULES |
| GET/PUT | `/api/bookings/settings` | MANAGE_SCHEDULES |

## Angular Components
`schedule-list`, `schedule-editor`, `booking-settings`, `booking.service.ts`

## Roles
**Gym Admin** with `MANAGE_SCHEDULES` (and `BOOKINGS` SaaS feature)

## Messages
- Success: "Schedule created/updated/deleted", "Booking settings saved"
- Error: load/save failures, CSRF 403, not found on edit

## Dependencies
`BranchService`, `TrainerService`, `BookingService`, `featureGuard('BOOKINGS')`

## Limitations
**Partial:** `TrainerAvailability` table/SPs exist but no UI or API for trainer availability windows
