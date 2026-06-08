/*
  Audit Logging Module — schema extensions + search SPs
*/

IF COL_LENGTH('dbo.AuditLogs', 'IpAddress') IS NULL
    ALTER TABLE dbo.AuditLogs ADD IpAddress NVARCHAR(50) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_GymId_CreatedAt' AND object_id = OBJECT_ID(N'dbo.AuditLogs'))
    CREATE INDEX IX_AuditLogs_GymId_CreatedAt ON dbo.AuditLogs (GymId, CreatedAt DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_UserId' AND object_id = OBJECT_ID(N'dbo.AuditLogs'))
    CREATE INDEX IX_AuditLogs_UserId ON dbo.AuditLogs (UserId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_EntityName' AND object_id = OBJECT_ID(N'dbo.AuditLogs'))
    CREATE INDEX IX_AuditLogs_EntityName ON dbo.AuditLogs (EntityName);
GO

CREATE OR ALTER PROCEDURE dbo.sp_AuditLog_Insert
    @GymId UNIQUEIDENTIFIER = NULL,
    @UserId UNIQUEIDENTIFIER = NULL,
    @EntityName NVARCHAR(100),
    @EntityId NVARCHAR(50),
    @ActionType NVARCHAR(50),
    @OldValueJson NVARCHAR(MAX) = NULL,
    @NewValueJson NVARCHAR(MAX) = NULL,
    @IpAddress NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AuditLogs (GymId, UserId, EntityName, EntityId, Action, OldValues, NewValues, IpAddress, CreatedAt)
    VALUES (@GymId, @UserId, @EntityName, @EntityId, @ActionType, @OldValueJson, @NewValueJson, @IpAddress, SYSUTCDATETIME());
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS AuditLogId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SearchAuditLogs
    @GymId UNIQUEIDENTIFIER = NULL,
    @UserId UNIQUEIDENTIFIER = NULL,
    @EntityName NVARCHAR(100) = NULL,
    @ActionType NVARCHAR(50) = NULL,
    @EntityId NVARCHAR(50) = NULL,
    @Search NVARCHAR(200) = NULL,
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF OBJECT_ID('tempdb..#AuditFiltered') IS NOT NULL DROP TABLE #AuditFiltered;

    SELECT
        a.AuditLogId,
        a.GymId,
        g.Name AS GymName,
        a.UserId,
        u.Name AS UserName,
        u.Email AS UserEmail,
        a.EntityName,
        a.EntityId,
        a.Action AS ActionType,
        a.OldValues AS OldValueJson,
        a.NewValues AS NewValueJson,
        a.IpAddress,
        a.CreatedAt AS CreatedDate
    INTO #AuditFiltered
    FROM dbo.AuditLogs a
    LEFT JOIN dbo.Users u ON u.Id = a.UserId
    LEFT JOIN dbo.Gyms g ON g.GymId = a.GymId
    WHERE (a.GymId = @GymId)
      AND (@UserId IS NULL OR a.UserId = @UserId)
      AND (@EntityName IS NULL OR a.EntityName = @EntityName)
      AND (@ActionType IS NULL OR a.Action = @ActionType)
      AND (@EntityId IS NULL OR a.EntityId = @EntityId)
      AND (@FromDate IS NULL OR a.CreatedAt >= @FromDate)
      AND (@ToDate IS NULL OR a.CreatedAt < DATEADD(DAY, 1, CAST(@ToDate AS DATE)))
      AND (@Search IS NULL OR u.Name LIKE N'%' + @Search + N'%' OR u.Email LIKE N'%' + @Search + N'%'
           OR a.EntityName LIKE N'%' + @Search + N'%' OR a.EntityId LIKE N'%' + @Search + N'%'
           OR a.Action LIKE N'%' + @Search + N'%');

    SET @TotalCount = (SELECT COUNT(*) FROM #AuditFiltered);

    SELECT *
    FROM #AuditFiltered
    ORDER BY CreatedDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAuditLogSummary
    @GymId UNIQUEIDENTIFIER = NULL,
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @From DATETIME2 = COALESCE(@FromDate, DATEADD(DAY, -30, SYSUTCDATETIME()));
    DECLARE @To DATETIME2 = COALESCE(@ToDate, SYSUTCDATETIME());

    SELECT COUNT(*) AS TotalLogs
    FROM dbo.AuditLogs a
    WHERE (a.GymId = @GymId)
      AND a.CreatedAt >= @From AND a.CreatedAt < DATEADD(DAY, 1, CAST(@To AS DATE));

    SELECT a.EntityName, COUNT(*) AS LogCount
    FROM dbo.AuditLogs a
    WHERE (a.GymId = @GymId)
      AND a.CreatedAt >= @From AND a.CreatedAt < DATEADD(DAY, 1, CAST(@To AS DATE))
    GROUP BY a.EntityName
    ORDER BY LogCount DESC;

    SELECT a.Action AS ActionType, COUNT(*) AS LogCount
    FROM dbo.AuditLogs a
    WHERE (a.GymId = @GymId)
      AND a.CreatedAt >= @From AND a.CreatedAt < DATEADD(DAY, 1, CAST(@To AS DATE))
    GROUP BY a.Action
    ORDER BY LogCount DESC;
END
GO
