# Stored procedures

Canonical procedure names live in `StoredProcedureNames.cs`.

SQL definitions are in `../Persistence/Scripts/`:

| Script | Purpose |
|--------|---------|
| 001 | Authorization schema |
| 002 | Auth CRUD SPs |
| 003 | MVP business tables |
| 004 | Future tables |
| 005 | MVP API SPs |
| 006 | User auth columns |
| 007 | Auth session / refresh token SPs |
| 008 | Gym admin module |
| 009 | Standard names (`sp_CreateGym`, `sp_LoginUser`, …) |

Legacy names (`sp_Gym_Insert`, etc.) remain for backward compatibility; new code should use standard names in `StoredProcedureNames`.
