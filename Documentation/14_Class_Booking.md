# Class Booking

## Module Overview
Member slot reservation, waitlist, cancellation, admin booking management, trainer schedule view, and booking analytics.

## Purpose
Allow members to book class sessions; allow staff to view/manage bookings and analyze utilization.

## Navigation Paths
| Role | Page | Route |
|------|------|-------|
| Gym Admin | All bookings | `/gym-admin/bookings` |
| Gym Admin | Analytics | `/gym-admin/booking-analytics` |
| Trainer | My schedule | `/trainer/schedule` |
| Trainer | Class bookings | `/trainer/bookings` |
| Member | Book a class | `/member/bookings` |
| Member | History | `/member/bookings/history` |

## Screen Description

### Member Bookings (`MemberBookingsComponent`)
- Date range filter (from/to)
- Available slot cards with capacity and waitlist count
- Book or Join Waitlist per slot

### Member Booking History (`MemberBookingHistoryComponent`)
- List of member's bookings with status
- Cancel button for `Booked` status

### Gym Admin Bookings (`GymAdminBookingsComponent`)
- Table: member, class, date/time, trainer, status
- Cancel booking (requires `MANAGE_BOOKINGS`)

### Trainer Schedule / Bookings
- Weekly schedule with booking counts
- Filterable booking list for trainer's classes

### Booking Analytics (`BookingAnalyticsComponent`)
- KPIs: occupancy, no-show %, trends, popular classes, peak hours
- Export PDF/Excel

## Buttons & Functionality
| Button | Action |
|--------|--------|
| Book | `POST /api/bookings/book` |
| Join Waitlist | `POST /api/bookings/waitlist` |
| Cancel (member/admin) | `POST /api/bookings/cancel` |
| Export report | `GET /api/booking-analytics/export/{format}` |
| Booking History link | `/member/bookings/history` |

## Filters
- Member: date range on available slots
- Trainer bookings: search text, status filter (`Booked`, `CheckedIn`, `Completed`, `Cancelled`, `NoShow`)
- Analytics: days, optional branch

## Validation
- `BookSlotDto`: `classScheduleId` > 0, `bookingDate` required
- Frontend: member ID min 1 on related flows
- Backend SP checks: duplicate booking, max per day, capacity, active schedule

## Business Rules
- Booking statuses: `Booked`, `CheckedIn`, `Completed`, `Cancelled`, `NoShow`
- Waitlist is separate table (`BookingWaitlist`); promotion on cancellation via SP
- Settings: max bookings/day, cancellation window hours, reminders
- Deleting a **schedule** permanently removes all its bookings (see Class Scheduling doc)
- Individual booking cancel respects cancellation window

## Workflow
1. Member selects slot → booking created as `Booked`
2. If full → waitlist with position
3. Reminder background job uses `sp_GetBookingsForReminder`
4. Check-in via `POST /api/booking-checkin` (QR payload)
5. No-show processing via `sp_ProcessNoShowBookings` (job)

## Database Tables
`SlotBookings`, `BookingWaitlist`, `BookingSettings`, `ClassSchedules`

## Stored Procedures
`sp_GetAvailableSlots`, `sp_CreateSlotBooking`, `sp_CancelSlotBooking`, `sp_JoinBookingWaitlist`, `sp_GetSlotBookingsPaged`, `sp_BookingQrCheckIn`, `sp_GetTrainerSchedule`, `sp_GetBookingAnalytics`, `sp_GetBookingsForReminder`, `sp_ProcessNoShowBookings`

## APIs
| Method | Endpoint | Permission |
|--------|----------|------------|
| GET | `/api/bookings/available-slots` | VIEW_BOOKINGS |
| POST | `/api/bookings/book` | VIEW_BOOKINGS |
| POST | `/api/bookings/cancel` | VIEW_BOOKINGS / MANAGE_BOOKINGS |
| POST | `/api/bookings/waitlist` | VIEW_BOOKINGS |
| GET | `/api/bookings` | VIEW_BOOKINGS |
| GET | `/api/trainer-schedule` | VIEW_BOOKINGS |
| GET | `/api/booking-analytics` | VIEW_BOOKING_ANALYTICS |
| POST | `/api/booking-checkin` | MANAGE_BOOKINGS (typical) |

## Angular Components
`member-bookings`, `member-booking-history`, `gym-admin-bookings`, `trainer-schedule`, `trainer-bookings`, `booking-analytics`

## Roles
- **Member:** book, view/cancel own bookings
- **Trainer:** view schedule and bookings
- **Gym Admin:** view all; cancel with `MANAGE_BOOKINGS`

## Messages
- "Booking confirmed!", "Added to waitlist", "Booking cancelled"
- Errors from API: duplicate, full class, max per day, cancellation window

## Dependencies
`BOOKINGS` SaaS feature, `BookingService`, `csrfInterceptor`

## Limitations
**Partial:** Class booking QR check-in API exists; no dedicated scanner UI page in gym-admin
