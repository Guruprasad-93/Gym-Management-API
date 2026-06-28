# Gym Management API

ASP.NET Core 8 Web API for the Gym Management System (Clean Architecture, MediatR, Dapper, SQL Server stored procedures).

**Frontend UI repository:** [Gym-Management-UI](https://github.com/Guruprasad-93/Gym-Management-UI)

## Prerequisites

- .NET 8 SDK
- SQL Server (local or Docker)
- [Gym Management UI](https://github.com/Guruprasad-93/Gym-Management-UI) for the Angular SPA (optional for API-only work)

## Quick start

```powershell
cd Backend
dotnet build GymManagementSaaS.sln
dotnet run --project Gym.API\Gym.API.csproj --launch-profile http
```

- **API:** http://localhost:5088
- **Swagger:** http://localhost:5088/swagger

Copy `Backend/Gym.API/appsettings.Development.Example.json` to `appsettings.Development.json` and adjust connection strings as needed.

### Docker (API + SQL Server)

```powershell
docker compose up --build
```

## Project layout

| Path | Description |
|------|-------------|
| `Backend/Gym.API/` | API host, controllers, middleware |
| `Backend/Gym.Application/` | Commands, queries, DTOs |
| `Backend/Gym.Domain/` | Domain entities and interfaces |
| `Backend/Gym.Infrastructure/` | Dapper, SQL scripts, integrations |
| `Backend/Gym.IntegrationTests/` | Integration tests |
| `docs/` | Manual test cases, project summary |

## Demo credentials

| Role | Email | Password |
|------|--------|----------|
| Super Admin | `superadmin@gym.com` | `SuperAdmin@123` |
| Gym Admin (demo) | `admin` or `admin@fitzone-demo.com` | `Demo@123` |
| Demo member (phone login) | `9876543210` | `Demo@123` |
| Demo trainer (employee ID) | `EMP001` | `Demo@123` |
| Demo member (member code) | `MEM000123` | `Demo@123` |

## Related documentation

- `IMPLEMENTATION_SUMMARY.md` — feature overview and run instructions
- `docs/PROJECT_SUMMARY.md` — per-page UI behavior (UI lives in the UI repo)
- `docs/REPO_SPLIT_PLAN.md` — API vs UI repository split
