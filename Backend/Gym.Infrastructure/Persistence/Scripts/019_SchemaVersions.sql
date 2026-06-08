/*
  Schema version tracking for incremental SQL script deployment.
*/

IF OBJECT_ID(N'dbo.SchemaVersions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchemaVersions
    (
        VersionId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        ScriptName NVARCHAR(200) NOT NULL,
        AppliedAt DATETIME2 NOT NULL CONSTRAINT DF_SchemaVersions_AppliedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_SchemaVersions_ScriptName UNIQUE (ScriptName)
    );
END
GO
