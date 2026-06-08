# Mobile Push Module — Deployment Notes

## Migration

```bash
cd Backend/Gym.API
dotnet run -- migrate
```

Verify: `SELECT * FROM dbo.SchemaVersions WHERE ScriptName LIKE '%031%'`

## Firebase Configuration

Add to `appsettings.Production.json` or environment variables:

```json
"Firebase": {
  "Enabled": true,
  "Provider": "Firebase",
  "ProjectId": "your-project-id",
  "ClientEmail": "firebase-adminsdk@your-project.iam.gserviceaccount.com",
  "PrivateKey": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n",
  "BackgroundJobIntervalMinutes": 60
}
```

Environment variable mapping (via `AddEnvironmentConfiguration`):
- Use appsettings section `Firebase:*` or configure in JSON directly.

**Development:** leave `Provider: "Mock"` (default) — pushes are logged, not sent.

## Android Setup

1. Create Firebase project → add Android app with package name
2. Download `google-services.json` into the mobile app
3. Register FCM token via `POST /api/mobile/device/register` after login

## iOS Setup

1. Add iOS app in Firebase console
2. Upload APNs key to Firebase Cloud Messaging
3. Register device token from the iOS app via the same register API with `DeviceType: "iOS"`

## Post-Deploy Checklist

- [ ] Run migration 031
- [ ] Configure Firebase credentials (production)
- [ ] Re-login members to receive new permissions
- [ ] Register test device and send campaign from `/gym-admin/mobile-notifications`
- [ ] Verify analytics at `/gym-admin/mobile-analytics`
- [ ] Confirm background job runs (hourly reminders)

## Integration Tests

```bash
dotnet test Backend/Gym.API.IntegrationTests --filter MobileNotificationTests
```

## Seeder

Members receive `VIEW_MOBILE_NOTIFICATIONS` and `MANAGE_NOTIFICATION_PREFERENCES` on next seed run. Gym admins use existing `SEND_NOTIFICATIONS` / `VIEW_NOTIFICATIONS` for admin APIs.
