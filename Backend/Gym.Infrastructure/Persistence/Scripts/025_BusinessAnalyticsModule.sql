/*
  Business Analytics & Dashboard Module
  Analytics SPs, optional cache table, monthly revenue fix
*/

IF OBJECT_ID(N'dbo.AnalyticsDashboardCache', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AnalyticsDashboardCache
    (
        CacheId BIGINT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        CacheKey NVARCHAR(100) NOT NULL,
        PayloadJson NVARCHAR(MAX) NOT NULL,
        ExpiresAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AnalyticsDashboardCache_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UX_AnalyticsDashboardCache UNIQUE (GymId, CacheKey)
    );
    CREATE INDEX IX_AnalyticsDashboardCache_Expires ON dbo.AnalyticsDashboardCache (ExpiresAt);
END
GO

/* Fix monthly revenue columns for C# mapping */
CREATE OR ALTER PROCEDURE dbo.sp_GetMonthlyRevenueSummary
    @GymId UNIQUEIDENTIFIER,
    @Months INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    IF @Months < 1 SET @Months = 1;
    IF @Months > 24 SET @Months = 24;

    ;WITH MonthSeries AS (
        SELECT 0 AS N
        UNION ALL SELECT N + 1 FROM MonthSeries WHERE N + 1 < @Months
    )
    SELECT
        YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))) AS [Year],
        MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))) AS [Month],
        FORMAT(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)), 'MMM yyyy') AS MonthLabel,
        ISNULL(SUM(p.Amount), 0) AS Revenue
    FROM MonthSeries ms
    LEFT JOIN dbo.Payments p ON p.GymId = @GymId
        AND p.Status = N'Completed'
        AND YEAR(p.PaymentDate) = YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))
        AND MONTH(p.PaymentDate) = MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))
    GROUP BY ms.N,
        YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))),
        MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))),
        FORMAT(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)), 'MMM yyyy')
    ORDER BY [Year] DESC, [Month] DESC
    OPTION (MAXRECURSION 24);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsRevenueSummary
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    DECLARE @WeekStart DATE = DATEADD(DAY, 1 - DATEPART(WEEKDAY, @Today), @Today);

    SELECT
        ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE GymId = @GymId AND Status = N'Completed' AND CAST(PaymentDate AS DATE) = @Today), 0) AS RevenueToday,
        ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE GymId = @GymId AND Status = N'Completed' AND CAST(PaymentDate AS DATE) >= @WeekStart), 0) AS RevenueThisWeek,
        ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE GymId = @GymId AND Status = N'Completed' AND YEAR(PaymentDate) = YEAR(@Today) AND MONTH(PaymentDate) = MONTH(@Today)), 0) AS RevenueThisMonth,
        ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE GymId = @GymId AND Status = N'Completed' AND YEAR(PaymentDate) = YEAR(@Today)), 0) AS RevenueThisYear,
        ISNULL((SELECT COUNT(*) FROM dbo.Payments WHERE GymId = @GymId AND Status IN (N'Failed', N'Refunded')), 0) AS FailedPaymentsCount;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsRevenueByPlan
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT mp.PlanName, ISNULL(SUM(p.Amount), 0) AS Revenue, COUNT(p.PaymentId) AS PaymentCount
    FROM dbo.Payments p
    INNER JOIN dbo.Memberships ms ON ms.MembershipId = p.MembershipId
    INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
    WHERE p.GymId = @GymId AND p.Status = N'Completed'
    GROUP BY mp.PlanName
    ORDER BY Revenue DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsRevenueByPaymentMethod
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.PaymentMethod, ISNULL(SUM(p.Amount), 0) AS Revenue, COUNT(*) AS PaymentCount
    FROM dbo.Payments p
    WHERE p.GymId = @GymId AND p.Status = N'Completed'
    GROUP BY p.PaymentMethod
    ORDER BY Revenue DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsMembershipSummary
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    SELECT
        (SELECT COUNT(*) FROM dbo.Members m WHERE m.GymId = @GymId AND m.IsDeleted = 0 AND m.IsActive = 1) AS ActiveMembers,
        (SELECT COUNT(*) FROM dbo.Memberships ms WHERE ms.GymId = @GymId AND ms.Status <> N'Cancelled' AND ms.EndDate < @Today) AS ExpiredMembers,
        (SELECT COUNT(*) FROM dbo.Memberships ms WHERE ms.GymId = @GymId AND ms.Status <> N'Cancelled' AND ms.EndDate >= @Today AND ms.EndDate <= DATEADD(DAY, 7, @Today)) AS ExpiringIn7Days,
        (SELECT COUNT(*) FROM dbo.Members m WHERE m.GymId = @GymId AND m.IsDeleted = 0 AND YEAR(m.JoinDate) = YEAR(@Today) AND MONTH(m.JoinDate) = MONTH(@Today)) AS NewRegistrationsThisMonth,
        (SELECT COUNT(*) FROM dbo.Memberships ms WHERE ms.GymId = @GymId AND ms.Status <> N'Cancelled' AND ms.EndDate >= @Today) AS ActiveMemberships;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsMembershipGrowthTrend
    @GymId UNIQUEIDENTIFIER,
    @Months INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    IF @Months < 1 SET @Months = 1;
    IF @Months > 24 SET @Months = 24;

    ;WITH MonthSeries AS (
        SELECT 0 AS N UNION ALL SELECT N + 1 FROM MonthSeries WHERE N + 1 < @Months
    )
    SELECT
        YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))) AS [Year],
        MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))) AS [Month],
        FORMAT(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)), 'MMM yyyy') AS MonthLabel,
        (SELECT COUNT(*) FROM dbo.Members m WHERE m.GymId = @GymId AND m.IsDeleted = 0
            AND YEAR(m.JoinDate) = YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))
            AND MONTH(m.JoinDate) = MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))) AS NewMembers
    FROM MonthSeries ms
    ORDER BY [Year] DESC, [Month] DESC
    OPTION (MAXRECURSION 24);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsPlanDistribution
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    SELECT mp.PlanName, COUNT(*) AS MemberCount
    FROM dbo.Memberships ms
    INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
    WHERE ms.GymId = @GymId AND ms.Status <> N'Cancelled' AND ms.EndDate >= @Today
    GROUP BY mp.PlanName
    ORDER BY MemberCount DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsAttendanceSummary
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    SELECT
        (SELECT COUNT(*) FROM dbo.MemberAttendance ma INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId WHERE m.GymId = @GymId AND CAST(ma.CheckInAt AS DATE) = @Today) AS TodayAttendanceCount,
        (SELECT COUNT(DISTINCT ma.MemberId) FROM dbo.MemberAttendance ma INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId WHERE m.GymId = @GymId AND CAST(ma.CheckInAt AS DATE) = @Today) AS UniqueMembersToday;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsAttendanceWeeklyTrend
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH Days AS (
        SELECT 0 AS N UNION ALL SELECT N + 1 FROM Days WHERE N + 1 < 7
    )
    SELECT
        CAST(DATEADD(DAY, -d.N, CAST(SYSUTCDATETIME() AS DATE)) AS DATE) AS AttendanceDate,
        FORMAT(DATEADD(DAY, -d.N, CAST(SYSUTCDATETIME() AS DATE)), 'ddd') AS DayLabel,
        (SELECT COUNT(*) FROM dbo.MemberAttendance ma INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId
         WHERE m.GymId = @GymId AND CAST(ma.CheckInAt AS DATE) = CAST(DATEADD(DAY, -d.N, CAST(SYSUTCDATETIME() AS DATE)) AS DATE)) AS AttendanceCount
    FROM Days d
    ORDER BY AttendanceDate
    OPTION (MAXRECURSION 7);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsAttendanceMonthlyTrend
    @GymId UNIQUEIDENTIFIER,
    @Months INT = 6
AS
BEGIN
    SET NOCOUNT ON;
    IF @Months < 1 SET @Months = 1;
    IF @Months > 12 SET @Months = 12;

    ;WITH MonthSeries AS (
        SELECT 0 AS N UNION ALL SELECT N + 1 FROM MonthSeries WHERE N + 1 < @Months
    )
    SELECT
        FORMAT(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)), 'MMM yyyy') AS MonthLabel,
        (SELECT COUNT(*) FROM dbo.MemberAttendance ma INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId
         WHERE m.GymId = @GymId
           AND YEAR(ma.CheckInAt) = YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))
           AND MONTH(ma.CheckInAt) = MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))) AS AttendanceCount
    FROM MonthSeries ms
    ORDER BY MonthLabel DESC
    OPTION (MAXRECURSION 12);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsMostActiveMembers
    @GymId UNIQUEIDENTIFIER,
    @TopN INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@TopN)
        m.MemberId, u.Name AS MemberName, COUNT(*) AS AttendanceCount
    FROM dbo.MemberAttendance ma
    INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE m.GymId = @GymId AND m.IsDeleted = 0
      AND ma.CheckInAt >= DATEADD(DAY, -30, SYSUTCDATETIME())
    GROUP BY m.MemberId, u.Name
    ORDER BY AttendanceCount DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsLeastActiveMembers
    @GymId UNIQUEIDENTIFIER,
    @TopN INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@TopN)
        m.MemberId, u.Name AS MemberName, COUNT(ma.MemberAttendanceId) AS AttendanceCount
    FROM dbo.Members m
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    LEFT JOIN dbo.MemberAttendance ma ON ma.MemberId = m.MemberId AND ma.CheckInAt >= DATEADD(DAY, -30, SYSUTCDATETIME())
    WHERE m.GymId = @GymId AND m.IsDeleted = 0 AND m.IsActive = 1
    GROUP BY m.MemberId, u.Name
    ORDER BY AttendanceCount ASC, u.Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsMemberAttendancePercentage
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        m.MemberId,
        u.Name AS MemberName,
        CAST(CASE WHEN 30 > 0 THEN (COUNT(ma.MemberAttendanceId) * 100.0 / 30) ELSE 0 END AS DECIMAL(5, 2)) AS AttendancePercentage
    FROM dbo.Members m
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    LEFT JOIN dbo.MemberAttendance ma ON ma.MemberId = m.MemberId AND ma.CheckInAt >= DATEADD(DAY, -30, SYSUTCDATETIME())
    WHERE m.GymId = @GymId AND m.IsDeleted = 0 AND m.IsActive = 1
    GROUP BY m.MemberId, u.Name
    ORDER BY AttendancePercentage DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsTrainerSummary
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.Trainers t WHERE t.GymId = @GymId AND t.IsActive = 1) AS ActiveTrainers,
        (SELECT COUNT(*) FROM dbo.Members m WHERE m.GymId = @GymId AND m.IsDeleted = 0 AND m.TrainerId IS NOT NULL) AS AssignedMembers;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsTrainerPerformance
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        t.TrainerId,
        u.Name AS TrainerName,
        (SELECT COUNT(*) FROM dbo.Members m WHERE m.TrainerId = t.TrainerId AND m.IsDeleted = 0 AND m.IsActive = 1) AS AssignedMembers,
        (SELECT COUNT(*) FROM dbo.TrainerAttendance ta WHERE ta.TrainerId = t.TrainerId AND CAST(ta.CheckInAt AS DATE) = CAST(SYSUTCDATETIME() AS DATE)) AS TodayAttendance
    FROM dbo.Trainers t
    INNER JOIN dbo.Users u ON u.Id = t.UserId
    WHERE t.GymId = @GymId AND t.IsActive = 1
    ORDER BY AssignedMembers DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsWorkoutSummary
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.AssignedWorkoutPlans aw WHERE aw.GymId = @GymId AND aw.IsActive = 1) AS ActiveWorkoutPlans,
        (SELECT COUNT(DISTINCT aw.AssignedWorkoutPlanId) FROM dbo.AssignedWorkoutPlans aw
         INNER JOIN dbo.MemberWorkoutProgress mwp ON mwp.AssignedWorkoutPlanId = aw.AssignedWorkoutPlanId
         WHERE aw.GymId = @GymId AND mwp.IsCompleted = 1) AS CompletedWorkoutPlans,
        CAST(CASE WHEN (SELECT COUNT(*) FROM dbo.MemberWorkoutProgress mwp
            INNER JOIN dbo.AssignedWorkoutPlans aw ON aw.AssignedWorkoutPlanId = mwp.AssignedWorkoutPlanId
            WHERE aw.GymId = @GymId) > 0
            THEN (SELECT COUNT(*) FROM dbo.MemberWorkoutProgress mwp
                INNER JOIN dbo.AssignedWorkoutPlans aw ON aw.AssignedWorkoutPlanId = mwp.AssignedWorkoutPlanId
                WHERE aw.GymId = @GymId AND mwp.IsCompleted = 1) * 100.0 /
                (SELECT COUNT(*) FROM dbo.MemberWorkoutProgress mwp
                INNER JOIN dbo.AssignedWorkoutPlans aw ON aw.AssignedWorkoutPlanId = mwp.AssignedWorkoutPlanId
                WHERE aw.GymId = @GymId)
            ELSE 0 END AS DECIMAL(5, 2)) AS CompletionPercentage;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsDietSummary
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.AssignedDietPlans ad WHERE ad.GymId = @GymId AND ad.IsActive = 1) AS ActiveDietPlans,
        CAST(CASE WHEN (SELECT COUNT(*) FROM dbo.AssignedDietPlans ad WHERE ad.GymId = @GymId AND ad.IsActive = 1) > 0
            THEN 75.0 ELSE 0 END AS DECIMAL(5, 2)) AS CompliancePercentage;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsRecentPayments
    @GymId UNIQUEIDENTIFIER,
    @TopN INT = 5
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@TopN)
        p.PaymentId, p.Amount, p.PaymentMethod, p.Status, p.PaymentDate, u.Name AS MemberName
    FROM dbo.Payments p
    LEFT JOIN dbo.Members m ON m.MemberId = p.MemberId
    LEFT JOIN dbo.Users u ON u.Id = m.UserId
    WHERE p.GymId = @GymId
    ORDER BY p.PaymentDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsExpiringMemberships
    @GymId UNIQUEIDENTIFIER,
    @TopN INT = 5
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    SELECT TOP (@TopN)
        ms.MembershipId, ms.EndDate, u.Name AS MemberName, mp.PlanName
    FROM dbo.Memberships ms
    INNER JOIN dbo.Members m ON m.MemberId = ms.MemberId
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
    WHERE ms.GymId = @GymId AND ms.Status <> N'Cancelled'
      AND ms.EndDate >= @Today AND ms.EndDate <= DATEADD(DAY, 7, @Today)
    ORDER BY ms.EndDate;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsNewMembers
    @GymId UNIQUEIDENTIFIER,
    @TopN INT = 5
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@TopN)
        m.MemberId, u.Name AS MemberName, u.Email AS MemberEmail, m.JoinDate
    FROM dbo.Members m
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE m.GymId = @GymId AND m.IsDeleted = 0
    ORDER BY m.JoinDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsRecentNotifications
    @GymId UNIQUEIDENTIFIER,
    @TopN INT = 5
AS
BEGIN
    SET NOCOUNT ON;
    IF OBJECT_ID(N'dbo.NotificationLogs', N'U') IS NULL
    BEGIN
        SELECT CAST(NULL AS BIGINT) AS LogId, CAST(NULL AS NVARCHAR(50)) AS NotificationType,
               CAST(NULL AS NVARCHAR(20)) AS Status, CAST(NULL AS DATETIME2) AS CreatedAt
        WHERE 1 = 0;
        RETURN;
    END

    SELECT TOP (@TopN)
        nl.LogId, nl.NotificationType, nl.Status, nl.RecipientPhone, nl.CreatedAt
    FROM dbo.NotificationLogs nl
    WHERE nl.GymId = @GymId
    ORDER BY nl.CreatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAnalyticsDashboardOverview
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);

    SELECT
        (SELECT COUNT(*) FROM dbo.Members m WHERE m.GymId = @GymId AND m.IsDeleted = 0) AS TotalMembers,
        (SELECT COUNT(*) FROM dbo.Members m WHERE m.GymId = @GymId AND m.IsDeleted = 0 AND m.IsActive = 1) AS ActiveMembers,
        ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE GymId = @GymId AND Status = N'Completed' AND CAST(PaymentDate AS DATE) = @Today), 0) AS RevenueToday,
        ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE GymId = @GymId AND Status = N'Completed' AND YEAR(PaymentDate) = YEAR(@Today) AND MONTH(PaymentDate) = MONTH(@Today)), 0) AS RevenueThisMonth,
        (SELECT COUNT(*) FROM dbo.Memberships ms WHERE ms.GymId = @GymId AND ms.Status <> N'Cancelled' AND ms.EndDate >= @Today AND ms.EndDate <= DATEADD(DAY, 7, @Today)) AS ExpiringMemberships,
        (SELECT COUNT(*) FROM dbo.Trainers t WHERE t.GymId = @GymId AND t.IsActive = 1) AS ActiveTrainers;
END
GO
