# Demo Login Credentials

Default passwords for local development when `Demo:Enabled=true` and database seeding is enabled.

| Setting | Value |
|---------|--------|
| **API** | `http://localhost:5088` |
| **Angular UI** | `http://localhost:4200` |
| **Login page** | `/auth/login` |
| **Demo gym** | FitZone Demo Gym |
| **Demo gym ID** | `b2edbb38-ee01-4d17-94b6-1b3303807b91` |
| **Demo password** | `Demo@123` (config: `Demo:Password`) |
| **Super Admin password** | `SuperAdmin@123` (config: `Bootstrap:SuperAdminPassword`) |

Use the **Login identifier** field on the login form (not always the email).

---

## Primary accounts (recommended for testing)

| Role | Login identifier | Password | Gym ID required? | Notes |
|------|------------------|----------|------------------|-------|
| Super Admin | `superadmin` | `SuperAdmin@123` | No | Platform-wide access |
| Gym Admin | `fitzone_admin` | `Demo@123` | Optional* | Priya Sharma — FitZone Demo Gym |
| Trainer 1 | `fitzone_trainer1` | `Demo@123` | Optional* | |
| Trainer 2 | `fitzone_trainer2` | `Demo@123` | Optional* | |
| Trainer 3 | `fitzone_trainer3` | `Demo@123` | Optional* | |
| Trainer 4 | `fitzone_trainer4` | `Demo@123` | Optional* | |
| Trainer 5 | `fitzone_trainer5` | `Demo@123` | Optional* | |
| Member 1 | `fitzone_member001` | `Demo@123` | Optional* | |
| Member 2 | `fitzone_member002` | `Demo@123` | Optional* | |
| Member 100 | `fitzone_member100` | `Demo@123` | Optional* | |

\*Gym ID is resolved automatically when login identifier is globally unique. You can pass `gymId` in the login API or use `/auth/login?gymId=b2edbb38-ee01-4d17-94b6-1b3303807b91` if needed.

---

## Super Admin

| Field | Value |
|-------|--------|
| Login identifier | `superadmin` |
| Email | `superadmin@gym.com` |
| Password | `SuperAdmin@123` |
| Display name | Super Admin |
| Role | SuperAdmin |

---

## Gym Admin (FitZone Demo Gym)

| Field | Value |
|-------|--------|
| Login identifier | `fitzone_admin` |
| Email | `priya.sharma@fitzonegym.in` |
| Password | `Demo@123` |
| Display name | Priya Sharma |
| Role | GymAdmin |
| Gym ID | `b2edbb38-ee01-4d17-94b6-1b3303807b91` |

---

## Trainers (all use password `Demo@123`)

| # | Login identifier | Email pattern |
|---|------------------|---------------|
| 1 | `fitzone_trainer1` | `*.@fitzonegym.in` (seeded name) |
| 2 | `fitzone_trainer2` | |
| 3 | `fitzone_trainer3` | |
| 4 | `fitzone_trainer4` | |
| 5 | `fitzone_trainer5` | |

---

## Members (all use password `Demo@123`)

Login pattern: `fitzone_member###` where `###` is 001–100 (zero-padded).

| Login identifier | Example |
|------------------|---------|
| `fitzone_member001` | Member 1 |
| `fitzone_member002` | Member 2 |
| `fitzone_member003` | Member 3 |
| … | … |
| `fitzone_member099` | Member 99 |
| `fitzone_member100` | Member 100 |

Full list:

```
fitzone_member001
fitzone_member002
fitzone_member003
fitzone_member004
fitzone_member005
fitzone_member006
fitzone_member007
fitzone_member008
fitzone_member009
fitzone_member010
fitzone_member011
fitzone_member012
fitzone_member013
fitzone_member014
fitzone_member015
fitzone_member016
fitzone_member017
fitzone_member018
fitzone_member019
fitzone_member020
fitzone_member021
fitzone_member022
fitzone_member023
fitzone_member024
fitzone_member025
fitzone_member026
fitzone_member027
fitzone_member028
fitzone_member029
fitzone_member030
fitzone_member031
fitzone_member032
fitzone_member033
fitzone_member034
fitzone_member035
fitzone_member036
fitzone_member037
fitzone_member038
fitzone_member039
fitzone_member040
fitzone_member041
fitzone_member042
fitzone_member043
fitzone_member044
fitzone_member045
fitzone_member046
fitzone_member047
fitzone_member048
fitzone_member049
fitzone_member050
fitzone_member051
fitzone_member052
fitzone_member053
fitzone_member054
fitzone_member055
fitzone_member056
fitzone_member057
fitzone_member058
fitzone_member059
fitzone_member060
fitzone_member061
fitzone_member062
fitzone_member063
fitzone_member064
fitzone_member065
fitzone_member066
fitzone_member067
fitzone_member068
fitzone_member069
fitzone_member070
fitzone_member071
fitzone_member072
fitzone_member073
fitzone_member074
fitzone_member075
fitzone_member076
fitzone_member077
fitzone_member078
fitzone_member079
fitzone_member080
fitzone_member081
fitzone_member082
fitzone_member083
fitzone_member084
fitzone_member085
fitzone_member086
fitzone_member087
fitzone_member088
fitzone_member089
fitzone_member090
fitzone_member091
fitzone_member092
fitzone_member093
fitzone_member094
fitzone_member095
fitzone_member096
fitzone_member097
fitzone_member098
fitzone_member099
fitzone_member100
```

---

## Legacy login identifiers (migrated → use new ID)

If an old database still has legacy IDs, migration renames them to the `fitzone_*` identifiers above.

| Legacy ID | Current ID |
|-----------|------------|
| `admin` | `fitzone_admin` |
| `admin@fitzone-demo.com` | `fitzone_admin` |
| `trainer1` | `fitzone_trainer1` |
| `EMP001` | `fitzone_trainer1` |
| `trainer2` | `fitzone_trainer2` |
| `member1` | `fitzone_member001` |
| `fitzone_member1` | `fitzone_member001` |
| `MEM000123` | `fitzone_member001` |
| `member2` | `fitzone_member002` |
| `fitzone_member2` | `fitzone_member002` |
| `9876543210` | `fitzone_member002` |

---

## API login example

```http
POST /api/auth/login
Content-Type: application/json

{
  "loginIdentifier": "fitzone_admin",
  "password": "Demo@123"
}
```

Super Admin:

```json
{
  "loginIdentifier": "superadmin",
  "password": "SuperAdmin@123"
}
```

---

## CSV export (copy-friendly)

```csv
role,login_identifier,password,gym_id,notes
Super Admin,superadmin,SuperAdmin@123,,Platform admin
Gym Admin,fitzone_admin,Demo@123,b2edbb38-ee01-4d17-94b6-1b3303807b91,FitZone Demo Gym
Trainer,fitzone_trainer1,Demo@123,b2edbb38-ee01-4d17-94b6-1b3303807b91,Trainer 1
Trainer,fitzone_trainer2,Demo@123,b2edbb38-ee01-4d17-94b6-1b3303807b91,Trainer 2
Trainer,fitzone_trainer3,Demo@123,b2edbb38-ee01-4d17-94b6-1b3303807b91,Trainer 3
Trainer,fitzone_trainer4,Demo@123,b2edbb38-ee01-4d17-94b6-1b3303807b91,Trainer 4
Trainer,fitzone_trainer5,Demo@123,b2edbb38-ee01-4d17-94b6-1b3303807b91,Trainer 5
Member,fitzone_member001,Demo@123,b2edbb38-ee01-4d17-94b6-1b3303807b91,Member 1
Member,fitzone_member002,Demo@123,b2edbb38-ee01-4d17-94b6-1b3303807b91,Member 2
Member,fitzone_member003,Demo@123,b2edbb38-ee01-4d17-94b6-1b3303807b91,Member 3
```

For members 004–100, use login `fitzone_member` + zero-padded number (e.g. `fitzone_member050`) with password `Demo@123`.

---

## Security note

**Do not use these credentials in production.** Set `Demo:Enabled=false` and `Database:RunSeedOnStartup=false` in production configuration.
