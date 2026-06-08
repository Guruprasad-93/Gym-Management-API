# Mobile Push Notification Module — Implementation Report

## Overview

Mobile-optimized API layer and Firebase Cloud Messaging push notifications for the gym SaaS platform. Includes device registration, notification center, preferences, single-call mobile dashboard, sync/delta APIs, hourly reminder background job, and gym-admin campaign/analytics UI.

## Database — `031_MobileAppPushNotificationModule.sql`

| Table | Purpose |
|-------|---------|
| `DeviceTokens` | FCM device registration per user |
| `PushNotifications` | In-app notification center + delivery tracking |
| `NotificationPreferences` | Per-user category toggles |

## Backend

| Layer | Files |
|-------|---------|
| DTOs | `Gym.Application/DTOs/Mobile/MobileDtos.cs` |
| Constants | `PushNotificationTypes.cs`, permissions, audit entity |
| Service | `MobilePushService.cs`, `IFirebasePushService` |
| Firebase | `FirebasePushService.cs` (Mock + FCM HTTP v1) |
| Repository | `MobilePushRepository.cs` |
| Controllers | `MobileController.cs`, `MobileAdminController.cs` |
| Background job | `PushNotificationBackgroundJob.cs` (hourly) |

## API Endpoints

**Member / mobile**
- `POST /api/mobile/device/register`, `POST /api/mobile/device/unregister`
- `GET /api/mobile/dashboard` — single home-screen payload
- `GET /api/mobile/sync` — full sync; `?lastSyncDate=` returns delta
- `GET /api/mobile/notifications`, `PUT /api/mobile/notifications/read`
- `GET/PUT /api/mobile/preferences`

**Gym admin**
- `POST /api/mobile/admin/send`
- `GET /api/mobile/admin/analytics`
- `GET /api/mobile/admin/campaigns`

## Permissions (Member role)

- `VIEW_MOBILE_NOTIFICATIONS`
- `MANAGE_NOTIFICATION_PREFERENCES`

## Event push integration

- Goal completed → `MemberSelfService` (WhatsApp + push)
- Background job: membership, attendance, workout, diet, goal reminders

## Frontend

- `/gym-admin/mobile-notifications` — send campaign + templates
- `/gym-admin/mobile-analytics` — delivery stats + campaign history

## Tests

`MobileNotificationTests.cs` — device registration, dashboard, sync, preferences, tenant auth

## Deployment

See `MOBILE_PUSH_DEPLOYMENT.md`
