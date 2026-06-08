/*
  Allow Super Admin to query all audit logs when @GymId is omitted.
*/

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

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 20;
    IF @PageSize > 10000 SET @PageSize = 10000;

    DECLARE @SearchPattern NVARCHAR(202) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @SearchPattern = N'%' + LTRIM(RTRIM(@Search)) + N'%';

    DECLARE @ToExclusive DATETIME2 = CASE
        WHEN @ToDate IS NULL THEN NULL
        ELSE DATEADD(DAY, 1, CAST(@ToDate AS DATE))
    END;

    ;WITH Filtered AS (
        SELECT
            a.AuditLogId,
            a.GymId,
            a.UserId,
            a.EntityName,
            a.EntityId,
            a.Action,
            a.OldValues,
            a.NewValues,
            a.IpAddress,
            a.CreatedAt
        FROM dbo.AuditLogs a
        WHERE (@GymId IS NULL OR a.GymId = @GymId)
          AND (@UserId IS NULL OR a.UserId = @UserId)
          AND (@EntityName IS NULL OR a.EntityName = @EntityName)
          AND (@ActionType IS NULL OR a.Action = @ActionType)
          AND (@EntityId IS NULL OR a.EntityId = @EntityId)
          AND (@FromDate IS NULL OR a.CreatedAt >= @FromDate)
          AND (@ToExclusive IS NULL OR a.CreatedAt < @ToExclusive)
    )
    SELECT @TotalCount = COUNT(*)
    FROM Filtered f
    LEFT JOIN dbo.Users u ON u.Id = f.UserId
    WHERE @SearchPattern IS NULL
       OR u.Name LIKE @SearchPattern
       OR u.Email LIKE @SearchPattern
       OR f.EntityName LIKE @SearchPattern
       OR f.EntityId LIKE @SearchPattern
       OR f.Action LIKE @SearchPattern;

    ;WITH Filtered AS (
        SELECT
            a.AuditLogId,
            a.GymId,
            a.UserId,
            a.EntityName,
            a.EntityId,
            a.Action,
            a.OldValues,
            a.NewValues,
            a.IpAddress,
            a.CreatedAt
        FROM dbo.AuditLogs a
        WHERE (@GymId IS NULL OR a.GymId = @GymId)
          AND (@UserId IS NULL OR a.UserId = @UserId)
          AND (@EntityName IS NULL OR a.EntityName = @EntityName)
          AND (@ActionType IS NULL OR a.Action = @ActionType)
          AND (@EntityId IS NULL OR a.EntityId = @EntityId)
          AND (@FromDate IS NULL OR a.CreatedAt >= @FromDate)
          AND (@ToExclusive IS NULL OR a.CreatedAt < @ToExclusive)
    )
    SELECT
        f.AuditLogId,
        f.GymId,
        g.Name AS GymName,
        f.UserId,
        u.Name AS UserName,
        u.Email AS UserEmail,
        f.EntityName,
        f.EntityId,
        f.Action AS ActionType,
        f.OldValues AS OldValueJson,
        f.NewValues AS NewValueJson,
        f.IpAddress,
        f.CreatedAt AS CreatedDate
    FROM Filtered f
    LEFT JOIN dbo.Users u ON u.Id = f.UserId
    LEFT JOIN dbo.Gyms g ON g.GymId = f.GymId
    WHERE @SearchPattern IS NULL
       OR u.Name LIKE @SearchPattern
       OR u.Email LIKE @SearchPattern
       OR f.EntityName LIKE @SearchPattern
       OR f.EntityId LIKE @SearchPattern
       OR f.Action LIKE @SearchPattern
    ORDER BY f.CreatedAt DESC
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
    DECLARE @ToExclusive DATETIME2 = DATEADD(DAY, 1, CAST(COALESCE(@ToDate, SYSUTCDATETIME()) AS DATE));

    SELECT COUNT(*) AS TotalLogs
    FROM dbo.AuditLogs a
    WHERE (@GymId IS NULL OR a.GymId = @GymId)
      AND a.CreatedAt >= @From
      AND a.CreatedAt < @ToExclusive;

    SELECT a.EntityName, COUNT(*) AS LogCount
    FROM dbo.AuditLogs a
    WHERE (@GymId IS NULL OR a.GymId = @GymId)
      AND a.CreatedAt >= @From
      AND a.CreatedAt < @ToExclusive
    GROUP BY a.EntityName
    ORDER BY LogCount DESC;

    SELECT a.Action AS ActionType, COUNT(*) AS LogCount
    FROM dbo.AuditLogs a
    WHERE (@GymId IS NULL OR a.GymId = @GymId)
      AND a.CreatedAt >= @From
      AND a.CreatedAt < @ToExclusive
    GROUP BY a.Action
    ORDER BY LogCount DESC;
END
GO
