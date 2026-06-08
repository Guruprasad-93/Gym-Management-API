/*
  Sprint 3 — Stored procedure optimizations (index-friendly predicates, reduced tempdb).
*/

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberAttendanceByDateRange
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL,
    @MemberId INT = NULL,
    @FromDate DATE,
    @ToDate DATE,
    @StatusId INT = NULL,
    @Search NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = 'CheckInAt',
    @SortDirection NVARCHAR(4) = 'DESC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SearchPattern NVARCHAR(202) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @SearchPattern = N'%' + LTRIM(RTRIM(@Search)) + N'%';

    SELECT @TotalCount = COUNT(*)
    FROM dbo.MemberAttendance ma WITH (INDEX(IX_MemberAttendance_Gym_Date))
    INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId AND m.IsDeleted = 0
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE ma.GymId = @GymId
      AND ma.AttendanceDate >= @FromDate
      AND ma.AttendanceDate <= @ToDate
      AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)
      AND (@MemberId IS NULL OR ma.MemberId = @MemberId)
      AND (@StatusId IS NULL OR ma.AttendanceStatusId = @StatusId)
      AND (@SearchPattern IS NULL OR u.Name LIKE @SearchPattern OR u.Email LIKE @SearchPattern);

    SELECT
        ma.MemberAttendanceId,
        ma.GymId,
        ma.MemberId,
        u.Name AS MemberName,
        u.Email AS MemberEmail,
        ma.TrainerId,
        tu.Name AS TrainerName,
        ma.AttendanceStatusId,
        st.Code AS StatusCode,
        st.Name AS StatusName,
        ma.AttendanceDate,
        ma.CheckInAt,
        ma.CheckOutAt,
        ma.Notes,
        ma.CreatedAt
    FROM dbo.MemberAttendance ma
    INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId AND m.IsDeleted = 0
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.AttendanceStatuses st ON st.AttendanceStatusId = ma.AttendanceStatusId
    LEFT JOIN dbo.Trainers t ON t.TrainerId = ma.TrainerId
    LEFT JOIN dbo.Users tu ON tu.Id = t.UserId
    WHERE ma.GymId = @GymId
      AND ma.AttendanceDate >= @FromDate
      AND ma.AttendanceDate <= @ToDate
      AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)
      AND (@MemberId IS NULL OR ma.MemberId = @MemberId)
      AND (@StatusId IS NULL OR ma.AttendanceStatusId = @StatusId)
      AND (@SearchPattern IS NULL OR u.Name LIKE @SearchPattern OR u.Email LIKE @SearchPattern)
    ORDER BY
        CASE WHEN @SortColumn = 'MemberName' AND @SortDirection = 'ASC' THEN u.Name END ASC,
        CASE WHEN @SortColumn = 'MemberName' AND @SortDirection = 'DESC' THEN u.Name END DESC,
        CASE WHEN @SortColumn = 'AttendanceDate' AND @SortDirection = 'ASC' THEN ma.AttendanceDate END ASC,
        CASE WHEN @SortColumn = 'AttendanceDate' AND @SortDirection = 'DESC' THEN ma.AttendanceDate END DESC,
        CASE WHEN @SortColumn = 'StatusName' AND @SortDirection = 'DESC' THEN st.Name END DESC,
        ma.CheckInAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
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
        FROM dbo.AuditLogs a WITH (INDEX(IX_AuditLogs_GymId_CreatedAt))
        WHERE a.GymId = @GymId
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
        WHERE a.GymId = @GymId
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

CREATE OR ALTER PROCEDURE dbo.sp_GetAllMemberships
    @GymId UNIQUEIDENTIFIER = NULL,
    @MemberId INT = NULL,
    @Search NVARCHAR(200) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    DECLARE @SearchPattern NVARCHAR(202) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @SearchPattern = N'%' + LTRIM(RTRIM(@Search)) + N'%';

    SELECT
        ms.MembershipId,
        ms.GymId,
        ms.MemberId,
        ms.MembershipPlanId,
        ms.StartDate,
        ms.EndDate,
        ms.Amount,
        ms.Notes,
        ms.CreatedAt,
        ms.UpdatedAt,
        mp.PlanName,
        mp.Price AS PlanPrice,
        ISNULL(mp.DurationInMonths, mp.DurationDays / 30) AS DurationInMonths,
        u.Name AS MemberName,
        u.Email AS MemberEmail,
        CASE
            WHEN ms.Status = N'Cancelled' THEN N'Cancelled'
            WHEN ms.EndDate >= @Today THEN N'Active'
            ELSE N'Expired'
        END AS Status
    FROM dbo.Memberships ms WITH (INDEX(IX_Memberships_Gym_Status_EndDate))
    INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
    INNER JOIN dbo.Members m ON m.MemberId = ms.MemberId AND m.IsDeleted = 0
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE ms.GymId = @GymId
      AND (@MemberId IS NULL OR ms.MemberId = @MemberId)
      AND (@SearchPattern IS NULL OR u.Name LIKE @SearchPattern OR mp.PlanName LIKE @SearchPattern)
      AND (
          @IncludeInactive = 1
          OR (ms.Status <> N'Cancelled' AND ms.EndDate >= @Today)
      )
    ORDER BY ms.StartDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPaymentHistory
    @GymId UNIQUEIDENTIFIER = NULL,
    @Search NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @SearchPattern NVARCHAR(202) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @SearchPattern = N'%' + LTRIM(RTRIM(@Search)) + N'%';

    SELECT
        p.PaymentId,
        p.GymId,
        p.MemberId,
        p.MembershipId,
        p.Amount,
        p.PaymentDate,
        p.PaymentMethod,
        p.TransactionReference,
        p.Status,
        p.Notes,
        p.CreatedAt,
        p.UpdatedAt,
        u.Name AS MemberName,
        u.Email AS MemberEmail,
        mp.PlanName AS MembershipPlanName
    FROM dbo.Payments p WITH (INDEX(IX_Payments_Gym_PaymentDate))
    INNER JOIN dbo.Members m ON m.MemberId = p.MemberId AND m.IsDeleted = 0
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    LEFT JOIN dbo.Memberships ms ON ms.MembershipId = p.MembershipId
    LEFT JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
    WHERE p.GymId = @GymId
      AND (
          @SearchPattern IS NULL
          OR u.Name LIKE @SearchPattern
          OR p.TransactionReference LIKE @SearchPattern
          OR p.PaymentMethod LIKE @SearchPattern
      )
    ORDER BY p.PaymentDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetExpiredMemberships
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);

    SELECT
        ms.MembershipId,
        ms.GymId,
        ms.MemberId,
        ms.MembershipPlanId,
        ms.StartDate,
        ms.EndDate,
        ms.Amount,
        ms.Notes,
        ms.CreatedAt,
        ms.UpdatedAt,
        mp.PlanName,
        mp.Price AS PlanPrice,
        ISNULL(mp.DurationInMonths, mp.DurationDays / 30) AS DurationInMonths,
        u.Name AS MemberName,
        u.Email AS MemberEmail,
        N'Expired' AS Status
    FROM dbo.Memberships ms WITH (INDEX(IX_Memberships_Gym_Status_EndDate))
    INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
    INNER JOIN dbo.Members m ON m.MemberId = ms.MemberId AND m.IsDeleted = 0
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE ms.GymId = @GymId
      AND ms.Status <> N'Cancelled'
      AND ms.EndDate < @Today
    ORDER BY ms.EndDate DESC;
END
GO
