/*
  AI Trainer & Recommendation Engine Module
*/

/* ========== AI RECOMMENDATIONS ========== */
IF OBJECT_ID(N'dbo.AiRecommendations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AiRecommendations
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        RecommendationType NVARCHAR(50) NOT NULL,
        RecommendationText NVARCHAR(2000) NOT NULL,
        ConfidenceScore DECIMAL(5, 2) NOT NULL CONSTRAINT DF_AiRecommendations_Confidence DEFAULT (0),
        IsAccepted BIT NOT NULL CONSTRAINT DF_AiRecommendations_IsAccepted DEFAULT (0),
        AcceptedDate DATETIME2 NULL,
        GeneratedDate DATETIME2 NOT NULL CONSTRAINT DF_AiRecommendations_Generated DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_AiRecommendations_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_AiRecommendations_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId)
    );
    CREATE INDEX IX_AiRecommendations_Member ON dbo.AiRecommendations (GymId, MemberId, GeneratedDate DESC);
    CREATE INDEX IX_AiRecommendations_Type ON dbo.AiRecommendations (GymId, RecommendationType, GeneratedDate DESC);
END
GO

/* ========== AI INSIGHTS ========== */
IF OBJECT_ID(N'dbo.AiInsights', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AiInsights
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        InsightType NVARCHAR(50) NOT NULL,
        InsightText NVARCHAR(2000) NOT NULL,
        Severity NVARCHAR(20) NOT NULL CONSTRAINT DF_AiInsights_Severity DEFAULT (N'Info'),
        GeneratedDate DATETIME2 NOT NULL CONSTRAINT DF_AiInsights_Generated DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_AiInsights_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId)
    );
    CREATE INDEX IX_AiInsights_Gym ON dbo.AiInsights (GymId, GeneratedDate DESC);
END
GO

/* ========== MEMBER RISK SCORES ========== */
IF OBJECT_ID(N'dbo.MemberRiskScores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberRiskScores
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        ChurnRisk NVARCHAR(20) NOT NULL,
        AttendanceRisk NVARCHAR(20) NOT NULL,
        RenewalProbability DECIMAL(5, 2) NOT NULL,
        HealthScore DECIMAL(5, 2) NOT NULL CONSTRAINT DF_MemberRiskScores_Health DEFAULT (0),
        LastCalculatedDate DATETIME2 NOT NULL CONSTRAINT DF_MemberRiskScores_Calc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_MemberRiskScores_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_MemberRiskScores_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT UX_MemberRiskScores_Member UNIQUE (GymId, MemberId)
    );
    CREATE INDEX IX_MemberRiskScores_Churn ON dbo.MemberRiskScores (GymId, ChurnRisk);
END
GO

/* ========== AI GENERATION LOGS ========== */
IF OBJECT_ID(N'dbo.AiGenerationLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AiGenerationLogs
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        EntityType NVARCHAR(50) NOT NULL,
        EntityId NVARCHAR(100) NULL,
        TokensUsed INT NOT NULL CONSTRAINT DF_AiGenerationLogs_Tokens DEFAULT (0),
        Provider NVARCHAR(30) NOT NULL,
        GeneratedDate DATETIME2 NOT NULL CONSTRAINT DF_AiGenerationLogs_Generated DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_AiGenerationLogs_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId)
    );
    CREATE INDEX IX_AiGenerationLogs_Gym ON dbo.AiGenerationLogs (GymId, GeneratedDate DESC);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateAiRecommendation
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @RecommendationType NVARCHAR(50),
    @RecommendationText NVARCHAR(2000),
    @ConfidenceScore DECIMAL(5, 2),
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AiRecommendations (GymId, MemberId, RecommendationType, RecommendationText, ConfidenceScore)
    VALUES (@GymId, @MemberId, @RecommendationType, @RecommendationText, @ConfidenceScore);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAiRecommendationsPaged
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT = NULL,
    @RecommendationType NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @TotalCount = COUNT(*)
    FROM dbo.AiRecommendations r
    INNER JOIN dbo.Members m ON m.MemberId = r.MemberId AND m.GymId = r.GymId
    WHERE r.GymId = @GymId
      AND (@MemberId IS NULL OR r.MemberId = @MemberId)
      AND (@RecommendationType IS NULL OR r.RecommendationType = @RecommendationType)
      AND m.IsDeleted = 0;

    SELECT r.Id, r.GymId, r.MemberId, u.Name AS MemberName, r.RecommendationType,
           r.RecommendationText, r.ConfidenceScore, r.IsAccepted, r.AcceptedDate, r.GeneratedDate
    FROM dbo.AiRecommendations r
    INNER JOIN dbo.Members m ON m.MemberId = r.MemberId AND m.GymId = r.GymId
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE r.GymId = @GymId
      AND (@MemberId IS NULL OR r.MemberId = @MemberId)
      AND (@RecommendationType IS NULL OR r.RecommendationType = @RecommendationType)
      AND m.IsDeleted = 0
    ORDER BY r.GeneratedDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MarkAiRecommendationAccepted
    @GymId UNIQUEIDENTIFIER,
    @RecommendationId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.AiRecommendations
    SET IsAccepted = 1, AcceptedDate = SYSUTCDATETIME()
    WHERE Id = @RecommendationId AND GymId = @GymId AND IsAccepted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateAiInsight
    @GymId UNIQUEIDENTIFIER,
    @InsightType NVARCHAR(50),
    @InsightText NVARCHAR(2000),
    @Severity NVARCHAR(20),
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AiInsights (GymId, InsightType, InsightText, Severity)
    VALUES (@GymId, @InsightType, @InsightText, @Severity);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAiInsightsPaged
    @GymId UNIQUEIDENTIFIER,
    @Severity NVARCHAR(20) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @TotalCount = COUNT(*)
    FROM dbo.AiInsights
    WHERE GymId = @GymId AND (@Severity IS NULL OR Severity = @Severity);

    SELECT Id, GymId, InsightType, InsightText, Severity, GeneratedDate
    FROM dbo.AiInsights
    WHERE GymId = @GymId AND (@Severity IS NULL OR Severity = @Severity)
    ORDER BY GeneratedDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpsertMemberRiskScore
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @ChurnRisk NVARCHAR(20),
    @AttendanceRisk NVARCHAR(20),
    @RenewalProbability DECIMAL(5, 2),
    @HealthScore DECIMAL(5, 2),
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.MemberRiskScores WHERE GymId = @GymId AND MemberId = @MemberId)
    BEGIN
        UPDATE dbo.MemberRiskScores
        SET ChurnRisk = @ChurnRisk, AttendanceRisk = @AttendanceRisk,
            RenewalProbability = @RenewalProbability, HealthScore = @HealthScore,
            LastCalculatedDate = SYSUTCDATETIME()
        WHERE GymId = @GymId AND MemberId = @MemberId;
        SELECT @Id = Id FROM dbo.MemberRiskScores WHERE GymId = @GymId AND MemberId = @MemberId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MemberRiskScores (GymId, MemberId, ChurnRisk, AttendanceRisk, RenewalProbability, HealthScore)
        VALUES (@GymId, @MemberId, @ChurnRisk, @AttendanceRisk, @RenewalProbability, @HealthScore);
        SET @Id = SCOPE_IDENTITY();
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberRiskScoresPaged
    @GymId UNIQUEIDENTIFIER,
    @ChurnRisk NVARCHAR(20) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @TotalCount = COUNT(*)
    FROM dbo.MemberRiskScores rs
    INNER JOIN dbo.Members m ON m.MemberId = rs.MemberId AND m.GymId = rs.GymId
    WHERE rs.GymId = @GymId
      AND (@ChurnRisk IS NULL OR rs.ChurnRisk = @ChurnRisk)
      AND m.IsDeleted = 0 AND m.IsActive = 1;

    SELECT rs.Id, rs.GymId, rs.MemberId, u.Name AS MemberName, rs.ChurnRisk, rs.AttendanceRisk,
           rs.RenewalProbability, rs.HealthScore, rs.LastCalculatedDate
    FROM dbo.MemberRiskScores rs
    INNER JOIN dbo.Members m ON m.MemberId = rs.MemberId AND m.GymId = rs.GymId
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE rs.GymId = @GymId
      AND (@ChurnRisk IS NULL OR rs.ChurnRisk = @ChurnRisk)
      AND m.IsDeleted = 0 AND m.IsActive = 1
    ORDER BY CASE rs.ChurnRisk WHEN N'High' THEN 1 WHEN N'Medium' THEN 2 ELSE 3 END, rs.RenewalProbability ASC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberRiskScoreByMemberId
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT rs.Id, rs.GymId, rs.MemberId, u.Name AS MemberName, rs.ChurnRisk, rs.AttendanceRisk,
           rs.RenewalProbability, rs.HealthScore, rs.LastCalculatedDate
    FROM dbo.MemberRiskScores rs
    INNER JOIN dbo.Members m ON m.MemberId = rs.MemberId AND m.GymId = rs.GymId
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE rs.GymId = @GymId AND rs.MemberId = @MemberId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateAiGenerationLog
    @GymId UNIQUEIDENTIFIER,
    @EntityType NVARCHAR(50),
    @EntityId NVARCHAR(100) = NULL,
    @TokensUsed INT,
    @Provider NVARCHAR(30),
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AiGenerationLogs (GymId, EntityType, EntityId, TokensUsed, Provider)
    VALUES (@GymId, @EntityType, @EntityId, @TokensUsed, @Provider);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAiAnalytics
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.AiRecommendations WHERE GymId = @GymId) AS TotalRecommendations,
        (SELECT COUNT(*) FROM dbo.AiRecommendations WHERE GymId = @GymId AND IsAccepted = 1) AS AcceptedRecommendations,
        CASE WHEN (SELECT COUNT(*) FROM dbo.AiRecommendations WHERE GymId = @GymId) = 0 THEN 0
             ELSE CAST((SELECT COUNT(*) FROM dbo.AiRecommendations WHERE GymId = @GymId AND IsAccepted = 1) AS DECIMAL(18,2))
                  / (SELECT COUNT(*) FROM dbo.AiRecommendations WHERE GymId = @GymId) * 100 END AS AcceptanceRate,
        (SELECT COUNT(*) FROM dbo.MemberRiskScores WHERE GymId = @GymId AND ChurnRisk = N'High') AS HighChurnPredictions,
        (SELECT COUNT(*) FROM dbo.MemberRiskScores WHERE GymId = @GymId AND ChurnRisk = N'High'
            AND LastCalculatedDate >= DATEADD(DAY, -30, SYSUTCDATETIME())) AS RecentHighChurnPredictions,
        (SELECT ISNULL(SUM(TokensUsed), 0) FROM dbo.AiGenerationLogs WHERE GymId = @GymId) AS TotalTokensUsed,
        (SELECT COUNT(*) FROM dbo.AiGenerationLogs WHERE GymId = @GymId) AS TotalGenerations,
        (SELECT COUNT(*) FROM dbo.AiInsights WHERE GymId = @GymId) AS TotalInsights;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAiDashboard
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.MemberRiskScores WHERE GymId = @GymId AND ChurnRisk = N'High') AS HighRiskMembers,
        (SELECT COUNT(*) FROM dbo.MemberRiskScores WHERE GymId = @GymId AND RenewalProbability >= 70) AS PredictedRenewals,
        (SELECT COUNT(*) FROM dbo.Leads l
            WHERE l.GymId = @GymId AND l.IsDeleted = 0 AND l.Status NOT IN (N'Converted', N'Lost')
              AND (
                (SELECT COUNT(*) FROM dbo.LeadFollowUps f WHERE f.LeadId = l.LeadId AND f.GymId = l.GymId) >= 3
                OR EXISTS (SELECT 1 FROM dbo.LeadTrials t WHERE t.LeadId = l.LeadId AND t.GymId = l.GymId AND t.AttendanceStatus <> N'Scheduled')
              )) AS HotLeads,
        (SELECT COUNT(*) FROM dbo.AiRecommendations WHERE GymId = @GymId AND GeneratedDate >= DATEADD(DAY, -7, SYSUTCDATETIME())) AS RecentRecommendations,
        (SELECT COUNT(*) FROM dbo.AiInsights WHERE GymId = @GymId AND Severity IN (N'Warning', N'Critical')) AS ActionableInsights;

    SELECT ChurnRisk AS Label, COUNT(*) AS [Count]
    FROM dbo.MemberRiskScores WHERE GymId = @GymId
    GROUP BY ChurnRisk;

    SELECT
        CASE WHEN RenewalProbability >= 80 THEN N'80-100%'
             WHEN RenewalProbability >= 60 THEN N'60-79%'
             WHEN RenewalProbability >= 40 THEN N'40-59%'
             ELSE N'0-39%' END AS Label,
        COUNT(*) AS [Count]
    FROM dbo.MemberRiskScores WHERE GymId = @GymId
    GROUP BY CASE WHEN RenewalProbability >= 80 THEN N'80-100%'
                  WHEN RenewalProbability >= 60 THEN N'60-79%'
                  WHEN RenewalProbability >= 40 THEN N'40-59%'
                  ELSE N'0-39%' END;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetLeadScoringPaged
    @GymId UNIQUEIDENTIFIER,
    @ScoreCategory NVARCHAR(20) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH Scored AS (
        SELECT l.LeadId, l.GymId, l.FullName, l.MobileNumber, l.Email, l.Status, l.LeadSource, l.CreatedDate,
               (SELECT COUNT(*) FROM dbo.LeadFollowUps f WHERE f.LeadId = l.LeadId AND f.GymId = l.GymId) AS FollowUpCount,
               (SELECT COUNT(*) FROM dbo.LeadTrials t WHERE t.LeadId = l.LeadId AND t.GymId = l.GymId AND t.AttendanceStatus <> N'Scheduled') AS CompletedTrials,
               CASE
                   WHEN l.Status IN (N'Converted', N'Lost') THEN N'Cold'
                   WHEN (SELECT COUNT(*) FROM dbo.LeadFollowUps f WHERE f.LeadId = l.LeadId AND f.GymId = l.GymId) >= 3
                        OR EXISTS (SELECT 1 FROM dbo.LeadTrials t WHERE t.LeadId = l.LeadId AND t.GymId = l.GymId AND t.AttendanceStatus <> N'Scheduled')
                   THEN N'Hot'
                   WHEN (SELECT COUNT(*) FROM dbo.LeadFollowUps f WHERE f.LeadId = l.LeadId AND f.GymId = l.GymId) >= 1
                        OR EXISTS (SELECT 1 FROM dbo.LeadTrials t WHERE t.LeadId = l.LeadId AND t.GymId = l.GymId)
                   THEN N'Warm'
                   ELSE N'Cold'
               END AS ScoreCategory,
               CASE
                   WHEN l.Status IN (N'Converted', N'Lost') THEN 10
                   WHEN (SELECT COUNT(*) FROM dbo.LeadFollowUps f WHERE f.LeadId = l.LeadId AND f.GymId = l.GymId) >= 3
                        OR EXISTS (SELECT 1 FROM dbo.LeadTrials t WHERE t.LeadId = l.LeadId AND t.GymId = l.GymId AND t.AttendanceStatus <> N'Scheduled')
                   THEN 90
                   WHEN (SELECT COUNT(*) FROM dbo.LeadFollowUps f WHERE f.LeadId = l.LeadId AND f.GymId = l.GymId) >= 1
                        OR EXISTS (SELECT 1 FROM dbo.LeadTrials t WHERE t.LeadId = l.LeadId AND t.GymId = l.GymId)
                   THEN 55
                   ELSE 20
               END AS EngagementScore
        FROM dbo.Leads l
        WHERE l.GymId = @GymId AND l.IsDeleted = 0
    )
    SELECT @TotalCount = COUNT(*) FROM Scored WHERE @ScoreCategory IS NULL OR ScoreCategory = @ScoreCategory;

    ;WITH Scored AS (
        SELECT l.LeadId, l.GymId, l.FullName, l.MobileNumber, l.Email, l.Status, l.LeadSource, l.CreatedDate,
               (SELECT COUNT(*) FROM dbo.LeadFollowUps f WHERE f.LeadId = l.LeadId AND f.GymId = l.GymId) AS FollowUpCount,
               (SELECT COUNT(*) FROM dbo.LeadTrials t WHERE t.LeadId = l.LeadId AND t.GymId = l.GymId AND t.AttendanceStatus <> N'Scheduled') AS CompletedTrials,
               CASE
                   WHEN l.Status IN (N'Converted', N'Lost') THEN N'Cold'
                   WHEN (SELECT COUNT(*) FROM dbo.LeadFollowUps f WHERE f.LeadId = l.LeadId AND f.GymId = l.GymId) >= 3
                        OR EXISTS (SELECT 1 FROM dbo.LeadTrials t WHERE t.LeadId = l.LeadId AND t.GymId = l.GymId AND t.AttendanceStatus <> N'Scheduled')
                   THEN N'Hot'
                   WHEN (SELECT COUNT(*) FROM dbo.LeadFollowUps f WHERE f.LeadId = l.LeadId AND f.GymId = l.GymId) >= 1
                        OR EXISTS (SELECT 1 FROM dbo.LeadTrials t WHERE t.LeadId = l.LeadId AND t.GymId = l.GymId)
                   THEN N'Warm'
                   ELSE N'Cold'
               END AS ScoreCategory,
               CASE
                   WHEN l.Status IN (N'Converted', N'Lost') THEN 10
                   WHEN (SELECT COUNT(*) FROM dbo.LeadFollowUps f WHERE f.LeadId = l.LeadId AND f.GymId = l.GymId) >= 3
                        OR EXISTS (SELECT 1 FROM dbo.LeadTrials t WHERE t.LeadId = l.LeadId AND t.GymId = l.GymId AND t.AttendanceStatus <> N'Scheduled')
                   THEN 90
                   WHEN (SELECT COUNT(*) FROM dbo.LeadFollowUps f WHERE f.LeadId = l.LeadId AND f.GymId = l.GymId) >= 1
                        OR EXISTS (SELECT 1 FROM dbo.LeadTrials t WHERE t.LeadId = l.LeadId AND t.GymId = l.GymId)
                   THEN 55
                   ELSE 20
               END AS EngagementScore
        FROM dbo.Leads l
        WHERE l.GymId = @GymId AND l.IsDeleted = 0
    )
    SELECT LeadId, GymId, FullName, MobileNumber, Email, Status, LeadSource, CreatedDate,
           FollowUpCount, CompletedTrials, ScoreCategory, EngagementScore
    FROM Scored
    WHERE @ScoreCategory IS NULL OR ScoreCategory = @ScoreCategory
    ORDER BY EngagementScore DESC, CreatedDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberAiAnalysisContext
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.MemberId, m.GymId, u.Name AS FullName, m.Weight, m.Height, m.JoinDate,
           (SELECT TOP 1 g.GoalType FROM dbo.MemberGoals g WHERE g.GymId = @GymId AND g.MemberId = @MemberId AND g.IsDeleted = 0 ORDER BY g.CreatedDate DESC) AS PrimaryGoal,
           (SELECT COUNT(*) FROM dbo.MemberAttendance a WHERE a.GymId = @GymId AND a.MemberId = @MemberId AND a.AttendanceDate >= DATEADD(DAY, -30, CAST(SYSUTCDATETIME() AS DATE))) AS AttendanceLast30Days,
           (SELECT COUNT(*) FROM dbo.MemberAttendance a WHERE a.GymId = @GymId AND a.MemberId = @MemberId AND a.AttendanceDate >= DATEADD(DAY, -60, CAST(SYSUTCDATETIME() AS DATE)) AND a.AttendanceDate < DATEADD(DAY, -30, CAST(SYSUTCDATETIME() AS DATE))) AS AttendancePrev30Days,
           (SELECT AVG(CAST(wt.CompletionPercentage AS FLOAT)) FROM dbo.WorkoutTracking wt WHERE wt.GymId = @GymId AND wt.MemberId = @MemberId AND wt.WorkoutDate >= DATEADD(DAY, -30, CAST(SYSUTCDATETIME() AS DATE))) AS AvgWorkoutCompletion,
           (SELECT AVG(CAST(dt.CompliancePercentage AS FLOAT)) FROM dbo.DietTracking dt WHERE dt.GymId = @GymId AND dt.MemberId = @MemberId AND dt.TrackingDate >= DATEADD(DAY, -30, CAST(SYSUTCDATETIME() AS DATE))) AS AvgDietCompliance,
           (SELECT TOP 1 mp.Weight FROM dbo.MemberProgress mp WHERE mp.GymId = @GymId AND mp.MemberId = @MemberId ORDER BY mp.ProgressDate DESC) AS LatestWeight,
           (SELECT TOP 1 mp.Weight FROM dbo.MemberProgress mp WHERE mp.GymId = @GymId AND mp.MemberId = @MemberId AND mp.ProgressDate <= DATEADD(DAY, -30, CAST(SYSUTCDATETIME() AS DATE)) ORDER BY mp.ProgressDate DESC) AS Weight30DaysAgo,
           (SELECT COUNT(*) FROM dbo.MemberGoals g WHERE g.GymId = @GymId AND g.MemberId = @MemberId AND g.Status = N'Completed' AND g.IsDeleted = 0) AS CompletedGoals,
           (SELECT COUNT(*) FROM dbo.MemberGoals g WHERE g.GymId = @GymId AND g.MemberId = @MemberId AND g.IsDeleted = 0) AS TotalGoals,
           (SELECT TOP 1 ms.EndDate FROM dbo.Memberships ms WHERE ms.GymId = @GymId AND ms.MemberId = @MemberId AND ms.Status = N'Active' ORDER BY ms.EndDate DESC) AS MembershipEndDate,
           (SELECT COUNT(*) FROM dbo.Payments p WHERE p.GymId = @GymId AND p.MemberId = @MemberId AND p.Status = N'Completed' AND p.PaymentDate >= DATEADD(MONTH, -6, SYSUTCDATETIME())) AS PaymentsLast6Months,
           (SELECT MAX(wt.WorkoutDate) FROM dbo.WorkoutTracking wt WHERE wt.GymId = @GymId AND wt.MemberId = @MemberId) AS LastWorkoutDate
    FROM dbo.Members m
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE m.GymId = @GymId AND m.MemberId = @MemberId AND m.IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetActiveMembersForAiJob
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.MemberId, m.GymId, m.UserId, u.Name AS FullName
    FROM dbo.Members m
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE m.GymId = @GymId AND m.IsActive = 1 AND m.IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetGymsForAiJob
AS
BEGIN
    SET NOCOUNT ON;
    SELECT g.GymId, g.Name AS GymName
    FROM dbo.Gyms g
    WHERE g.IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBusinessAiContext
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(SYSUTCDATETIME()), MONTH(SYSUTCDATETIME()), 1);
    DECLARE @PrevMonthStart DATE = DATEADD(MONTH, -1, @MonthStart);
    DECLARE @PrevMonthEnd DATE = DATEADD(DAY, -1, @MonthStart);

    SELECT
        (SELECT ISNULL(SUM(p.Amount), 0) FROM dbo.Payments p WHERE p.GymId = @GymId AND p.Status = N'Completed' AND CAST(p.PaymentDate AS DATE) >= @MonthStart) AS RevenueThisMonth,
        (SELECT ISNULL(SUM(p.Amount), 0) FROM dbo.Payments p WHERE p.GymId = @GymId AND p.Status = N'Completed' AND CAST(p.PaymentDate AS DATE) BETWEEN @PrevMonthStart AND @PrevMonthEnd) AS RevenuePrevMonth,
        (SELECT COUNT(DISTINCT a.MemberId) FROM dbo.MemberAttendance a WHERE a.GymId = @GymId AND a.AttendanceDate >= DATEADD(DAY, -30, CAST(SYSUTCDATETIME() AS DATE))) AS ActiveMembersLast30Days,
        (SELECT COUNT(DISTINCT a.MemberId) FROM dbo.MemberAttendance a WHERE a.GymId = @GymId AND a.AttendanceDate >= DATEADD(DAY, -60, CAST(SYSUTCDATETIME() AS DATE)) AND a.AttendanceDate < DATEADD(DAY, -30, CAST(SYSUTCDATETIME() AS DATE))) AS ActiveMembersPrev30Days,
        (SELECT COUNT(*) FROM dbo.MemberRiskScores WHERE GymId = @GymId AND ChurnRisk = N'High') AS HighChurnMembers,
        (SELECT COUNT(*) FROM dbo.Trainers t WHERE t.GymId = @GymId AND t.IsActive = 1) AS ActiveTrainers,
        (SELECT COUNT(DISTINCT m.MemberId) FROM dbo.Members m WHERE m.GymId = @GymId AND m.IsActive = 1 AND m.IsDeleted = 0 AND m.TrainerId IS NOT NULL) AS MembersWithTrainer;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBranchAttendanceForAi
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT b.BranchId, b.BranchName,
           (SELECT COUNT(*) FROM dbo.MemberAttendance a
            INNER JOIN dbo.Members m ON m.MemberId = a.MemberId AND m.GymId = a.GymId
            WHERE a.GymId = @GymId AND m.BranchId = b.BranchId AND a.AttendanceDate >= DATEADD(DAY, -30, CAST(SYSUTCDATETIME() AS DATE))) AS AttendanceLast30Days,
           (SELECT COUNT(*) FROM dbo.MemberAttendance a
            INNER JOIN dbo.Members m ON m.MemberId = a.MemberId AND m.GymId = a.GymId
            WHERE a.GymId = @GymId AND m.BranchId = b.BranchId AND a.AttendanceDate >= DATEADD(DAY, -60, CAST(SYSUTCDATETIME() AS DATE)) AND a.AttendanceDate < DATEADD(DAY, -30, CAST(SYSUTCDATETIME() AS DATE))) AS AttendancePrev30Days
    FROM dbo.Branches b
    WHERE b.GymId = @GymId AND b.IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetHighRiskMembersForNotification
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT rs.MemberId, m.UserId, u.Name AS FullName, m.Phone AS PhoneNumber, rs.ChurnRisk, rs.RenewalProbability
    FROM dbo.MemberRiskScores rs
    INNER JOIN dbo.Members m ON m.MemberId = rs.MemberId AND m.GymId = rs.GymId
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE rs.GymId = @GymId AND rs.ChurnRisk = N'High' AND m.IsActive = 1 AND m.IsDeleted = 0;
END
GO
