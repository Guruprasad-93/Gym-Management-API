# Gym Management SaaS – Data Access Architecture

## Stored procedure only (mandatory)

All business database operations use **SQL Server stored procedures** executed through **Dapper** via `IStoredProcedureExecutor`.

| Operation | Approach |
|-----------|----------|
| Create | `dbo.sp_Create{Entity}` |
| Update | `dbo.sp_Update{Entity}` |
| Delete (soft) | `dbo.sp_Delete{Entity}` |
| Get by id | `dbo.sp_Get{Entity}ById` |
| List / search | `dbo.sp_GetAll{Entities}` |
| Dashboard | `dbo.sp_GetDashboardStatistics`, `dbo.sp_GetGymDashboardStatistics` |
| Login | `dbo.sp_LoginUser` |
| Permissions / roles | `dbo.sp_User_GetPermissions`, `dbo.sp_User_GetRoles` |
| Payments | `dbo.sp_CreatePayment` (explicit transaction + TRY/CATCH) |

**Entity Framework Core** (`ApplicationDbContext`) is limited to:

- Database migrations (`MigrateAsync`)
- Fluent API entity configuration
- **Not** for runtime CRUD in services or repositories

## Project layout

```
Gym.Infrastructure/
  StoredProcedures/
    StoredProcedureNames.cs      # Canonical procedure names
    StoredProcedureExecutor.cs # Dapper wrapper (async, parameterized)
  Persistence/
    Scripts/                     # Embedded SQL (001–009), deployed on startup
    DapperContext.cs             # Internal Dapper helpers
    ApplicationDbContext.cs      # Migrations only
  Repositories/                  # One repository per area; calls SPs only
```

## Execution flow

```
API Controller → MediatR Handler → Application Service → Repository → IStoredProcedureExecutor → SQL Server SP
```

## Conventions

- **Parameterized only** – never concatenate SQL in C#
- **async/await** on all data access
- **TRY/CATCH** in procedures (see `009_StandardStoredProcedureNames.sql`)
- **Transactions** for payments and multi-step writes
- **Soft delete** where applicable (`IsActive = 0`, not physical delete for gyms/members/trainers)
- **GymId isolation** – gym-scoped SPs accept `@GymId` from `ICurrentUserService`

## Deploying SQL scripts

Scripts in `Persistence/Scripts/*.sql` are embedded resources and run automatically by `StoredProcedureDeployer` on application startup (after EF migrations).

## Adding a new feature

1. Add SQL script with `CREATE OR ALTER PROCEDURE dbo.sp_...` + TRY/CATCH
2. Add name to `StoredProcedureNames.cs`
3. Add repository methods using `IStoredProcedureExecutor`
4. Expose via Application service + MediatR + FluentValidation
5. Do **not** add EF `DbSet` queries in services
