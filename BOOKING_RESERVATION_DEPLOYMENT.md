# Booking & Slot Reservation — Deployment Guide

## 1. Run Migration

```bash
cd Backend/Gym.API
dotnet run -- migrate
```

Applies `033_BookingAndSlotReservationModule.sql`.

## 2. Permissions

Re-run seed or enable `Database:RunSeedOnStartup` to assign booking permissions to GymAdmin, Trainer, and Member roles. Demo users must re-login after seed.

## 3. Configure Booking Settings

Gym admins can configure via `GET/PUT /api/bookings/settings`:

- Max bookings per day (default 3)
- Allow waitlist (default true)
- Cancellation window hours (default 2)
- Reminder minutes before class (default 60)

## 4. Create Class Schedules

Use `/gym-admin/schedules` or `POST /api/schedules` with branch, trainer, day of week, time window, and capacity.

## 5. QR Check-In

Reuses member QR tokens (`GMS:{gymId}:{memberId}:{token}`). Trainers/admins with `MANAGE_BOOKINGS` scan via `POST /api/booking-checkin`.

## 6. Background Job

`BookingReminderBackgroundJob` runs automatically when the API is hosted. Processes no-shows and sends reminders based on `BookingSettings.ReminderMinutesBefore`.

## 7. Exports

- PDF/Excel booking report: `GET /api/booking-analytics/export/pdf?reportType=bookings`
- Occupancy report: `reportType=occupancy`

## 8. Production Checklist

- [ ] Migration 033 applied
- [ ] Permissions seeded for all roles
- [ ] At least one branch and trainer per gym
- [ ] Class schedules created per branch
- [ ] WhatsApp + Firebase configured for notifications
- [ ] Integration tests passing: `dotnet test --filter BookingReservationTests`
