# Member Self-Service Module — Deployment Notes

## Prerequisites

- SQL Server with existing Gym Management schema (scripts 001–028 applied)
- .NET 8 SDK
- Node.js for Angular frontend build

## Database Migration

### Option A — Automatic (recommended for dev/staging)

Set in `appsettings` or environment:

```json
"Database": {
  "RunMigrationsOnStartup": true
}
```

Script `029_MemberSelfServiceModule.sql` runs automatically after EF migrations.

### Option B — CLI

```bash
cd Backend/Gym.API
dotnet run -- migrate
```

Verify in SQL:

```sql
SELECT * FROM dbo.SchemaVersions WHERE ScriptName LIKE '%029%';
```

## Backend Deployment

1. Restore packages (includes new `QRCoder` dependency):

```bash
cd Backend
dotnet restore GymManagementSystem.sln
dotnet build GymManagementSystem.sln
dotnet publish Gym.API/Gym.API.csproj -c Release -o ./publish
```

2. Restart API service after deploy.

3. On first startup after deploy, `DatabaseSeeder` adds new privileges and assigns them to the **Member** role for existing installations.

## Frontend Deployment

```bash
cd Frontend/gym-app
npm install
npm run build
```

Deploy `dist/gym-app` to your static host / reverse proxy.

## Post-Deploy Verification

1. Log in as demo member: `member1@fitzone-demo.com` / `Demo@123`
2. Navigate to `/member/dashboard`
3. Confirm stat cards, membership info, QR code display
4. Create a goal at `/member/goals`
5. Log progress at `/member/progress`
6. As gym admin, scan member QR via `POST /api/member/attendance/qr-scan` (or integrate scanner UI in attendance module)

## WhatsApp Notification Setup

Enable new notification types per gym:

1. Gym Admin → Notifications → Settings
2. Enable `GoalCompleted` and `ReferralRewardEarned`
3. Add matching WhatsApp templates (or use mock provider in dev)

Membership expiry, workout assigned, and diet assigned notifications use existing configuration.

## Permissions Rollout

Existing members receive new privileges automatically via `EnsureMemberRoleAndPrivilegesAsync`. Users must **log out and log back in** (or call `/api/auth/session`) to refresh JWT permission claims.

## QR Attendance

- Members display QR from dashboard (`GET /api/member/qr-code`)
- Trainers/admins with `MANAGE_ATTENDANCE` scan via `POST /api/member/attendance/qr-scan`
- Duplicate same-day check-in is blocked at database level

## Rollback

If needed, script 029 is additive (new tables/SPs). Rollback requires manual table/SP drops — not recommended in production without backup.

## Integration Tests

```bash
cd Backend
dotnet test Gym.API.IntegrationTests/Gym.API.IntegrationTests.csproj --filter MemberSelfServiceTests
```

Requires local SQL Server with integration test connection string from `appsettings.IntegrationTests.json`.
