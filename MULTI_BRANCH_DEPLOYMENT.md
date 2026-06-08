# Multi-Branch Module — Deployment Notes

## Migration

```bash
cd Backend/Gym.API
dotnet run -- migrate
```

Or set `Database:RunMigrationsOnStartup = true`.

Verify: `SELECT * FROM dbo.SchemaVersions WHERE ScriptName LIKE '%030%'`

## Post-Migration

- Each existing gym receives a **Main Branch** automatically
- All members, trainers, payments, etc. are backfilled with that branch's `BranchId`
- Gym admins must **re-login** to receive new branch permissions

## Verify

1. Login as `admin@fitzone-demo.com` / `Demo@123`
2. Navigate to `/gym-admin/branches`
3. Create a second branch
4. Transfer a member via `/gym-admin/branches/transfers`
5. View dashboard and analytics

## WhatsApp Announcements

Enable `BranchAnnouncement` in Notifications → Settings per gym.

Create announcement with `sendWhatsApp: true` in API or extend UI.

## Integration Tests

```bash
dotnet test Backend/Gym.API.IntegrationTests --filter BranchManagementTests
```
