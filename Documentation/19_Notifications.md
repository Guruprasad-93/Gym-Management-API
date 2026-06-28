# Notifications

## Module Overview
WhatsApp-oriented notification templates, settings, send history, test send; mobile push campaigns (separate mobile module).

## Navigation
| Page | Route |
|------|-------|
| Notification dashboard | `/gym-admin/notifications` |
| Templates | `/gym-admin/notifications/templates` |
| History | `/gym-admin/notifications/history` |
| Test send | `/gym-admin/notifications/test` |
| Mobile push | `/gym-admin/mobile-notifications` |
| Mobile analytics | `/gym-admin/mobile-analytics` |

## Buttons
- CRUD templates
- Update notification settings
- Send test / bulk send
- Mobile push campaign send

## Tables
`NotificationTemplates`, `NotificationSettings`, `NotificationLogs`, `DeviceTokens`, `PushNotifications`, `NotificationPreferences`

## APIs
`NotificationsController` — `/api/notifications/*`, `MobileAdminController` — `/api/mobile/admin/*`, `MobileController` — device registration

## Limitations
**Partial:** WhatsApp and Firebase providers use **Mock** mode in development when not configured

## SaaS Feature
`NOTIFICATIONS`
