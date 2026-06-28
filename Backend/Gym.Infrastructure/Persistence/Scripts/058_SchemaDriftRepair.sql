-- 058_SchemaDriftRepair.sql
-- Idempotent repair for databases created before Members.IsDeleted was added in script 003.
-- Ensures soft-delete columns exist before dependent stored procedures are (re)applied.

IF OBJECT_ID(N'dbo.Members', N'U') IS NOT NULL AND COL_LENGTH('dbo.Members', 'IsDeleted') IS NULL
    ALTER TABLE dbo.Members ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Members_IsDeleted_058 DEFAULT (0) WITH VALUES;
GO

IF OBJECT_ID(N'dbo.Files', N'U') IS NOT NULL AND COL_LENGTH('dbo.Files', 'IsDeleted') IS NULL
    ALTER TABLE dbo.Files ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Files_IsDeleted_058 DEFAULT (0) WITH VALUES;
GO

IF OBJECT_ID(N'dbo.Leads', N'U') IS NOT NULL AND COL_LENGTH('dbo.Leads', 'IsDeleted') IS NULL
    ALTER TABLE dbo.Leads ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Leads_IsDeleted_058 DEFAULT (0) WITH VALUES;
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL AND COL_LENGTH('dbo.Expenses', 'IsDeleted') IS NULL
    ALTER TABLE dbo.Expenses ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Expenses_IsDeleted_058 DEFAULT (0) WITH VALUES;
GO

IF OBJECT_ID(N'dbo.ExerciseLibrary', N'U') IS NOT NULL AND COL_LENGTH('dbo.ExerciseLibrary', 'IsDeleted') IS NULL
    ALTER TABLE dbo.ExerciseLibrary ADD IsDeleted BIT NOT NULL CONSTRAINT DF_ExerciseLibrary_IsDeleted_058 DEFAULT (0) WITH VALUES;
GO

IF OBJECT_ID(N'dbo.WorkoutPlans', N'U') IS NOT NULL AND COL_LENGTH('dbo.WorkoutPlans', 'IsDeleted') IS NULL
    ALTER TABLE dbo.WorkoutPlans ADD IsDeleted BIT NOT NULL CONSTRAINT DF_WorkoutPlans_IsDeleted_058 DEFAULT (0) WITH VALUES;
GO

IF OBJECT_ID(N'dbo.DietPlans', N'U') IS NOT NULL AND COL_LENGTH('dbo.DietPlans', 'IsDeleted') IS NULL
    ALTER TABLE dbo.DietPlans ADD IsDeleted BIT NOT NULL CONSTRAINT DF_DietPlans_IsDeleted_058 DEFAULT (0) WITH VALUES;
GO
