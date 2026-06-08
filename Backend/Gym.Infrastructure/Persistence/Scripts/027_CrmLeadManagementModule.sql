/*
  CRM & Lead Management Module
  Leads, follow-ups, trials, activities, analytics SPs
*/

IF OBJECT_ID(N'dbo.Leads', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Leads
    (
        LeadId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        FullName NVARCHAR(200) NOT NULL,
        MobileNumber NVARCHAR(20) NOT NULL,
        Email NVARCHAR(256) NULL,
        Gender NVARCHAR(20) NULL,
        Age INT NULL,
        [Address] NVARCHAR(500) NULL,
        LeadSource NVARCHAR(50) NOT NULL,
        InterestedPlanId INT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_Leads_Status DEFAULT (N'New'),
        AssignedTrainerId INT NULL,
        Notes NVARCHAR(MAX) NULL,
        ConvertedMemberId INT NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_Leads_CreatedDate DEFAULT (SYSUTCDATETIME()),
        CreatedBy UNIQUEIDENTIFIER NULL,
        UpdatedDate DATETIME2 NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Leads_IsDeleted DEFAULT (0),
        CONSTRAINT FK_Leads_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_Leads_MembershipPlans FOREIGN KEY (InterestedPlanId) REFERENCES dbo.MembershipPlans (MembershipPlanId),
        CONSTRAINT FK_Leads_Trainers FOREIGN KEY (AssignedTrainerId) REFERENCES dbo.Trainers (TrainerId),
        CONSTRAINT FK_Leads_Members FOREIGN KEY (ConvertedMemberId) REFERENCES dbo.Members (MemberId)
    );
    CREATE INDEX IX_Leads_GymId_Status ON dbo.Leads (GymId, [Status]) WHERE IsDeleted = 0;
    CREATE INDEX IX_Leads_GymId_CreatedDate ON dbo.Leads (GymId, CreatedDate DESC) WHERE IsDeleted = 0;
    CREATE INDEX IX_Leads_AssignedTrainer ON dbo.Leads (GymId, AssignedTrainerId) WHERE IsDeleted = 0;
END
GO

IF OBJECT_ID(N'dbo.LeadFollowUps', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeadFollowUps
    (
        FollowUpId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        LeadId INT NOT NULL,
        GymId UNIQUEIDENTIFIER NOT NULL,
        FollowUpDate DATETIME2 NOT NULL,
        FollowUpType NVARCHAR(50) NOT NULL,
        Remarks NVARCHAR(1000) NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_LeadFollowUps_Status DEFAULT (N'Pending'),
        NextFollowUpDate DATETIME2 NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_LeadFollowUps_CreatedDate DEFAULT (SYSUTCDATETIME()),
        CreatedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_LeadFollowUps_Leads FOREIGN KEY (LeadId) REFERENCES dbo.Leads (LeadId),
        CONSTRAINT FK_LeadFollowUps_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId)
    );
    CREATE INDEX IX_LeadFollowUps_LeadId ON dbo.LeadFollowUps (LeadId, FollowUpDate DESC);
    CREATE INDEX IX_LeadFollowUps_Pending ON dbo.LeadFollowUps (GymId, [Status], FollowUpDate);
END
GO

IF OBJECT_ID(N'dbo.LeadTrials', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeadTrials
    (
        TrialId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        LeadId INT NOT NULL,
        GymId UNIQUEIDENTIFIER NOT NULL,
        TrainerId INT NULL,
        TrialDate DATETIME2 NOT NULL,
        AttendanceStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_LeadTrials_AttendanceStatus DEFAULT (N'Scheduled'),
        Feedback NVARCHAR(1000) NULL,
        Rating INT NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_LeadTrials_CreatedDate DEFAULT (SYSUTCDATETIME()),
        CreatedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_LeadTrials_Leads FOREIGN KEY (LeadId) REFERENCES dbo.Leads (LeadId),
        CONSTRAINT FK_LeadTrials_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_LeadTrials_Trainers FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers (TrainerId)
    );
    CREATE INDEX IX_LeadTrials_GymId_TrialDate ON dbo.LeadTrials (GymId, TrialDate);
    CREATE INDEX IX_LeadTrials_LeadId ON dbo.LeadTrials (LeadId, TrialDate DESC);
END
GO

IF OBJECT_ID(N'dbo.LeadActivities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeadActivities
    (
        ActivityId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        LeadId INT NOT NULL,
        GymId UNIQUEIDENTIFIER NOT NULL,
        ActivityType NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_LeadActivities_CreatedDate DEFAULT (SYSUTCDATETIME()),
        CreatedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_LeadActivities_Leads FOREIGN KEY (LeadId) REFERENCES dbo.Leads (LeadId),
        CONSTRAINT FK_LeadActivities_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId)
    );
    CREATE INDEX IX_LeadActivities_LeadId ON dbo.LeadActivities (LeadId, CreatedDate DESC);
END
GO

/* ========== CREATE LEAD ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateLead
    @GymId UNIQUEIDENTIFIER,
    @FullName NVARCHAR(200),
    @MobileNumber NVARCHAR(20),
    @Email NVARCHAR(256) = NULL,
    @Gender NVARCHAR(20) = NULL,
    @Age INT = NULL,
    @Address NVARCHAR(500) = NULL,
    @LeadSource NVARCHAR(50),
    @InterestedPlanId INT = NULL,
    @Status NVARCHAR(30) = N'New',
    @AssignedTrainerId INT = NULL,
    @Notes NVARCHAR(MAX) = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @LeadId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        INSERT INTO dbo.Leads (GymId, FullName, MobileNumber, Email, Gender, Age, [Address], LeadSource,
            InterestedPlanId, [Status], AssignedTrainerId, Notes, CreatedBy)
        VALUES (@GymId, @FullName, @MobileNumber, @Email, @Gender, @Age, @Address, @LeadSource,
            @InterestedPlanId, @Status, @AssignedTrainerId, @Notes, @CreatedBy);
        SET @LeadId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateLead
    @LeadId INT,
    @GymId UNIQUEIDENTIFIER,
    @FullName NVARCHAR(200),
    @MobileNumber NVARCHAR(20),
    @Email NVARCHAR(256) = NULL,
    @Gender NVARCHAR(20) = NULL,
    @Age INT = NULL,
    @Address NVARCHAR(500) = NULL,
    @LeadSource NVARCHAR(50),
    @InterestedPlanId INT = NULL,
    @Status NVARCHAR(30),
    @AssignedTrainerId INT = NULL,
    @Notes NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Leads WHERE LeadId = @LeadId AND GymId = @GymId AND IsDeleted = 0)
        THROW 50060, 'Lead not found.', 1;
    UPDATE dbo.Leads SET
        FullName = @FullName, MobileNumber = @MobileNumber, Email = @Email, Gender = @Gender, Age = @Age,
        [Address] = @Address, LeadSource = @LeadSource, InterestedPlanId = @InterestedPlanId,
        [Status] = @Status, AssignedTrainerId = @AssignedTrainerId, Notes = @Notes, UpdatedDate = SYSUTCDATETIME()
    WHERE LeadId = @LeadId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateLeadStatus
    @LeadId INT,
    @GymId UNIQUEIDENTIFIER,
    @Status NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Leads SET [Status] = @Status, UpdatedDate = SYSUTCDATETIME()
    WHERE LeadId = @LeadId AND GymId = @GymId AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteLead
    @LeadId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Leads SET IsDeleted = 1, UpdatedDate = SYSUTCDATETIME()
    WHERE LeadId = @LeadId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetLeadById
    @LeadId INT,
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT l.LeadId, l.GymId, l.FullName, l.MobileNumber, l.Email, l.Gender, l.Age, l.[Address],
           l.LeadSource, l.InterestedPlanId, mp.PlanName AS InterestedPlanName,
           l.[Status], l.AssignedTrainerId, t.Name AS AssignedTrainerName,
           l.Notes, l.ConvertedMemberId, l.CreatedDate, l.CreatedBy, l.UpdatedDate
    FROM dbo.Leads l
    LEFT JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = l.InterestedPlanId
    LEFT JOIN dbo.Trainers tr ON tr.TrainerId = l.AssignedTrainerId
    LEFT JOIN dbo.Users t ON t.Id = tr.UserId
    WHERE l.LeadId = @LeadId AND l.IsDeleted = 0
      AND (@GymId IS NULL OR l.GymId = @GymId)
      AND (@TrainerId IS NULL OR l.AssignedTrainerId = @TrainerId);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetLeadsPaged
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL,
    @Search NVARCHAR(200) = NULL,
    @Status NVARCHAR(30) = NULL,
    @LeadSource NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = N'CreatedDate',
    @SortDirection NVARCHAR(4) = N'DESC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 10;
    IF @PageSize > 100 SET @PageSize = 100;

    DECLARE @SearchPattern NVARCHAR(202) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @SearchPattern = N'%' + LTRIM(RTRIM(@Search)) + N'%';

    ;WITH Filtered AS (
        SELECT l.LeadId, l.GymId, l.FullName, l.MobileNumber, l.Email, l.Gender, l.Age, l.[Address],
               l.LeadSource, l.InterestedPlanId, mp.PlanName AS InterestedPlanName,
               l.[Status], l.AssignedTrainerId, tu.Name AS AssignedTrainerName,
               l.Notes, l.ConvertedMemberId, l.CreatedDate, l.CreatedBy, l.UpdatedDate
        FROM dbo.Leads l
        LEFT JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = l.InterestedPlanId
        LEFT JOIN dbo.Trainers tr ON tr.TrainerId = l.AssignedTrainerId
        LEFT JOIN dbo.Users tu ON tu.Id = tr.UserId
        WHERE l.IsDeleted = 0
          AND (@GymId IS NULL OR l.GymId = @GymId)
          AND (@TrainerId IS NULL OR l.AssignedTrainerId = @TrainerId)
          AND (@Status IS NULL OR l.[Status] = @Status)
          AND (@LeadSource IS NULL OR l.LeadSource = @LeadSource)
          AND (@SearchPattern IS NULL OR l.FullName LIKE @SearchPattern OR l.MobileNumber LIKE @SearchPattern
               OR l.Email LIKE @SearchPattern OR tu.Name LIKE @SearchPattern)
    )
    SELECT @TotalCount = COUNT(*) FROM Filtered;

    SELECT * FROM Filtered
    ORDER BY
        CASE WHEN @SortColumn = N'FullName' AND @SortDirection = N'ASC' THEN FullName END ASC,
        CASE WHEN @SortColumn = N'FullName' AND @SortDirection = N'DESC' THEN FullName END DESC,
        CASE WHEN @SortColumn = N'Status' AND @SortDirection = N'ASC' THEN [Status] END ASC,
        CASE WHEN @SortColumn = N'Status' AND @SortDirection = N'DESC' THEN [Status] END DESC,
        CASE WHEN @SortColumn = N'LeadSource' AND @SortDirection = N'ASC' THEN LeadSource END ASC,
        CASE WHEN @SortColumn = N'LeadSource' AND @SortDirection = N'DESC' THEN LeadSource END DESC,
        CASE WHEN @SortColumn = N'CreatedDate' AND @SortDirection = N'ASC' THEN CreatedDate END ASC,
        CASE WHEN @SortColumn = N'CreatedDate' AND @SortDirection = N'DESC' THEN CreatedDate END DESC,
        LeadId DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SearchLeads
    @GymId UNIQUEIDENTIFIER,
    @TrainerId INT = NULL,
    @Search NVARCHAR(200) = NULL,
    @Status NVARCHAR(30) = NULL,
    @LeadSource NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Total INT;
    EXEC dbo.sp_GetLeadsPaged @GymId, @TrainerId, @Search, @Status, @LeadSource,
        1, 5000, N'CreatedDate', N'DESC', @Total OUTPUT;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_AssignTrainerToLead
    @LeadId INT,
    @GymId UNIQUEIDENTIFIER,
    @TrainerId INT,
    @Status NVARCHAR(30) = N'Contacted'
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Leads SET AssignedTrainerId = @TrainerId, [Status] = @Status, UpdatedDate = SYSUTCDATETIME()
    WHERE LeadId = @LeadId AND GymId = @GymId AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ConvertLeadToMember
    @LeadId INT,
    @GymId UNIQUEIDENTIFIER,
    @ConvertedMemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Leads SET [Status] = N'Converted', ConvertedMemberId = @ConvertedMemberId, UpdatedDate = SYSUTCDATETIME()
    WHERE LeadId = @LeadId AND GymId = @GymId AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ScheduleTrialSession
    @LeadId INT,
    @GymId UNIQUEIDENTIFIER,
    @TrainerId INT = NULL,
    @TrialDate DATETIME2,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @TrialId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.LeadTrials (LeadId, GymId, TrainerId, TrialDate, AttendanceStatus, CreatedBy)
    VALUES (@LeadId, @GymId, @TrainerId, @TrialDate, N'Scheduled', @CreatedBy);
    SET @TrialId = SCOPE_IDENTITY();
    UPDATE dbo.Leads SET [Status] = N'TrialScheduled', UpdatedDate = SYSUTCDATETIME()
    WHERE LeadId = @LeadId AND GymId = @GymId AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RecordTrialFeedback
    @TrialId INT,
    @GymId UNIQUEIDENTIFIER,
    @AttendanceStatus NVARCHAR(30),
    @Feedback NVARCHAR(1000) = NULL,
    @Rating INT = NULL,
    @LeadStatus NVARCHAR(30) = N'TrialCompleted'
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE lt SET AttendanceStatus = @AttendanceStatus, Feedback = @Feedback, Rating = @Rating
    FROM dbo.LeadTrials lt
    WHERE lt.TrialId = @TrialId AND lt.GymId = @GymId;

    UPDATE l SET [Status] = @LeadStatus, UpdatedDate = SYSUTCDATETIME()
    FROM dbo.Leads l
    INNER JOIN dbo.LeadTrials lt ON lt.LeadId = l.LeadId
    WHERE lt.TrialId = @TrialId AND l.GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateLeadFollowUp
    @LeadId INT,
    @GymId UNIQUEIDENTIFIER,
    @FollowUpDate DATETIME2,
    @FollowUpType NVARCHAR(50),
    @Remarks NVARCHAR(1000) = NULL,
    @Status NVARCHAR(30) = N'Pending',
    @NextFollowUpDate DATETIME2 = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @FollowUpId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.LeadFollowUps (LeadId, GymId, FollowUpDate, FollowUpType, Remarks, [Status], NextFollowUpDate, CreatedBy)
    VALUES (@LeadId, @GymId, @FollowUpDate, @FollowUpType, @Remarks, @Status, @NextFollowUpDate, @CreatedBy);
    SET @FollowUpId = SCOPE_IDENTITY();
    UPDATE dbo.Leads SET [Status] = N'FollowUpPending', UpdatedDate = SYSUTCDATETIME()
    WHERE LeadId = @LeadId AND GymId = @GymId AND IsDeleted = 0 AND [Status] NOT IN (N'Converted', N'Lost');
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateLeadFollowUp
    @FollowUpId INT,
    @GymId UNIQUEIDENTIFIER,
    @FollowUpDate DATETIME2,
    @FollowUpType NVARCHAR(50),
    @Remarks NVARCHAR(1000) = NULL,
    @Status NVARCHAR(30),
    @NextFollowUpDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.LeadFollowUps SET
        FollowUpDate = @FollowUpDate, FollowUpType = @FollowUpType, Remarks = @Remarks,
        [Status] = @Status, NextFollowUpDate = @NextFollowUpDate
    WHERE FollowUpId = @FollowUpId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateLeadActivity
    @LeadId INT,
    @GymId UNIQUEIDENTIFIER,
    @ActivityType NVARCHAR(50),
    @Description NVARCHAR(1000),
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @ActivityId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.LeadActivities (LeadId, GymId, ActivityType, [Description], CreatedBy)
    VALUES (@LeadId, @GymId, @ActivityType, @Description, @CreatedBy);
    SET @ActivityId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetLeadActivities
    @LeadId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ActivityId, LeadId, GymId, ActivityType, [Description], CreatedDate, CreatedBy
    FROM dbo.LeadActivities WHERE LeadId = @LeadId AND GymId = @GymId ORDER BY CreatedDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetLeadFollowUps
    @LeadId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FollowUpId, LeadId, GymId, FollowUpDate, FollowUpType, Remarks, [Status], NextFollowUpDate, CreatedDate, CreatedBy
    FROM dbo.LeadFollowUps WHERE LeadId = @LeadId AND GymId = @GymId ORDER BY FollowUpDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetLeadTrials
    @LeadId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT lt.TrialId, lt.LeadId, lt.GymId, lt.TrainerId, tu.Name AS TrainerName,
           lt.TrialDate, lt.AttendanceStatus, lt.Feedback, lt.Rating, lt.CreatedDate
    FROM dbo.LeadTrials lt
    LEFT JOIN dbo.Trainers tr ON tr.TrainerId = lt.TrainerId
    LEFT JOIN dbo.Users tu ON tu.Id = tr.UserId
    WHERE lt.LeadId = @LeadId AND lt.GymId = @GymId ORDER BY lt.TrialDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetLeadDashboard
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);

    SELECT
        (SELECT COUNT(*) FROM dbo.Leads l WHERE l.IsDeleted = 0 AND (@GymId IS NULL OR l.GymId = @GymId)) AS TotalLeads,
        (SELECT COUNT(*) FROM dbo.Leads l WHERE l.IsDeleted = 0 AND CAST(l.CreatedDate AS DATE) = @Today
            AND (@GymId IS NULL OR l.GymId = @GymId)) AS NewLeadsToday,
        (SELECT COUNT(*) FROM dbo.Leads l WHERE l.IsDeleted = 0 AND l.[Status] = N'Converted'
            AND (@GymId IS NULL OR l.GymId = @GymId)) AS ConvertedLeads,
        (SELECT COUNT(*) FROM dbo.Leads l WHERE l.IsDeleted = 0 AND l.[Status] = N'Lost'
            AND (@GymId IS NULL OR l.GymId = @GymId)) AS LostLeads,
        (SELECT COUNT(*) FROM dbo.LeadFollowUps f WHERE f.[Status] = N'Pending'
            AND (@GymId IS NULL OR f.GymId = @GymId)) AS PendingFollowUps,
        (SELECT COUNT(*) FROM dbo.LeadTrials t WHERE CAST(t.TrialDate AS DATE) = @Today
            AND (@GymId IS NULL OR t.GymId = @GymId)) AS TodaysTrials,
        CASE WHEN (SELECT COUNT(*) FROM dbo.Leads l WHERE l.IsDeleted = 0 AND (@GymId IS NULL OR l.GymId = @GymId)) = 0 THEN 0
             ELSE CAST((SELECT COUNT(*) FROM dbo.Leads l WHERE l.IsDeleted = 0 AND l.[Status] = N'Converted'
                AND (@GymId IS NULL OR l.GymId = @GymId)) AS DECIMAL(18,4))
                / (SELECT COUNT(*) FROM dbo.Leads l WHERE l.IsDeleted = 0 AND (@GymId IS NULL OR l.GymId = @GymId)) * 100 END AS ConversionRate,
        CASE WHEN (SELECT COUNT(*) FROM dbo.LeadTrials t WHERE (@GymId IS NULL OR t.GymId = @GymId)) = 0 THEN 0
             ELSE CAST((SELECT COUNT(*) FROM dbo.Leads l WHERE l.IsDeleted = 0 AND l.[Status] = N'Converted'
                AND (@GymId IS NULL OR l.GymId = @GymId)) AS DECIMAL(18,4))
                / NULLIF((SELECT COUNT(*) FROM dbo.LeadTrials t WHERE (@GymId IS NULL OR t.GymId = @GymId)), 0) * 100 END AS TrialConversionRate;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetLeadSourceAnalytics
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT LeadSource AS [Name], COUNT(*) AS [Count]
    FROM dbo.Leads WHERE IsDeleted = 0 AND (@GymId IS NULL OR GymId = @GymId)
    GROUP BY LeadSource ORDER BY COUNT(*) DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetLeadStatusAnalytics
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Status] AS [Name], COUNT(*) AS [Count]
    FROM dbo.Leads WHERE IsDeleted = 0 AND (@GymId IS NULL OR GymId = @GymId)
    GROUP BY [Status] ORDER BY COUNT(*) DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetLeadConversionReport
    @GymId UNIQUEIDENTIFIER = NULL,
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
        ISNULL(SUM(CASE WHEN l.[Status] = N'Converted' THEN 1 ELSE 0 END), 0) AS Conversions,
        ISNULL(COUNT(l.LeadId), 0) AS NewLeads
    FROM MonthSeries ms
    LEFT JOIN dbo.Leads l ON l.IsDeleted = 0
        AND (@GymId IS NULL OR l.GymId = @GymId)
        AND YEAR(l.CreatedDate) = YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))
        AND MONTH(l.CreatedDate) = MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)))
    GROUP BY ms.N,
        YEAR(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))),
        MONTH(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE))),
        FORMAT(DATEADD(MONTH, -ms.N, CAST(SYSUTCDATETIME() AS DATE)), 'MMM yyyy')
    ORDER BY [Year] DESC, [Month] DESC
    OPTION (MAXRECURSION 24);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetTrainerLeadConversion
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT tu.Name AS TrainerName, l.AssignedTrainerId AS TrainerId,
           COUNT(*) AS TotalLeads,
           SUM(CASE WHEN l.[Status] = N'Converted' THEN 1 ELSE 0 END) AS ConvertedLeads
    FROM dbo.Leads l
    LEFT JOIN dbo.Trainers tr ON tr.TrainerId = l.AssignedTrainerId
    LEFT JOIN dbo.Users tu ON tu.Id = tr.UserId
    WHERE l.IsDeleted = 0 AND l.AssignedTrainerId IS NOT NULL
      AND (@GymId IS NULL OR l.GymId = @GymId)
    GROUP BY tu.Name, l.AssignedTrainerId
    ORDER BY ConvertedLeads DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPendingFollowUps
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL,
    @TopN INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@TopN) f.FollowUpId, f.LeadId, l.FullName AS LeadName, l.MobileNumber,
           f.FollowUpDate, f.FollowUpType, f.Remarks, f.[Status], f.NextFollowUpDate
    FROM dbo.LeadFollowUps f
    INNER JOIN dbo.Leads l ON l.LeadId = f.LeadId AND l.IsDeleted = 0
    WHERE f.[Status] = N'Pending'
      AND (@GymId IS NULL OR f.GymId = @GymId)
      AND (@TrainerId IS NULL OR l.AssignedTrainerId = @TrainerId)
    ORDER BY f.FollowUpDate ASC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetTodaysTrials
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    SELECT lt.TrialId, lt.LeadId, l.FullName AS LeadName, l.MobileNumber,
           lt.TrainerId, tu.Name AS TrainerName, lt.TrialDate, lt.AttendanceStatus
    FROM dbo.LeadTrials lt
    INNER JOIN dbo.Leads l ON l.LeadId = lt.LeadId AND l.IsDeleted = 0
    LEFT JOIN dbo.Trainers tr ON tr.TrainerId = lt.TrainerId
    LEFT JOIN dbo.Users tu ON tu.Id = tr.UserId
    WHERE CAST(lt.TrialDate AS DATE) = @Today
      AND (@GymId IS NULL OR lt.GymId = @GymId)
      AND (@TrainerId IS NULL OR lt.TrainerId = @TrainerId OR l.AssignedTrainerId = @TrainerId)
    ORDER BY lt.TrialDate ASC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetLeadReminderCandidates
    @HoursAhead INT = 24
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    DECLARE @Until DATETIME2 = DATEADD(HOUR, @HoursAhead, @Now);

    SELECT N'TrialReminder' AS ReminderType, lt.TrialId AS EntityId, l.GymId, l.LeadId,
           l.FullName AS LeadName, l.MobileNumber, lt.TrialDate AS ScheduledAt
    FROM dbo.LeadTrials lt
    INNER JOIN dbo.Leads l ON l.LeadId = lt.LeadId AND l.IsDeleted = 0
    WHERE lt.AttendanceStatus = N'Scheduled'
      AND lt.TrialDate BETWEEN @Now AND @Until
      AND l.[Status] NOT IN (N'Converted', N'Lost')

    UNION ALL

    SELECT N'FollowUpReminder', f.FollowUpId, f.GymId, f.LeadId,
           l.FullName, l.MobileNumber, f.FollowUpDate
    FROM dbo.LeadFollowUps f
    INNER JOIN dbo.Leads l ON l.LeadId = f.LeadId AND l.IsDeleted = 0
    WHERE f.[Status] = N'Pending'
      AND f.FollowUpDate BETWEEN @Now AND @Until
      AND l.[Status] NOT IN (N'Converted', N'Lost');
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_SeedNotificationSettings
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.NotificationSettings (GymId, NotificationType, IsEnabled)
    SELECT @GymId, nt.NotificationType, 1
    FROM (VALUES
        (N'MembershipExpiry7Days'), (N'MembershipExpiry3Days'), (N'MembershipExpiryToday'),
        (N'PaymentSuccess'), (N'MembershipRenewal'), (N'NewMemberRegistration'),
        (N'WorkoutPlanAssigned'), (N'DietPlanAssigned'), (N'GymOwnerWelcome'),
        (N'LeadCreated'), (N'TrialScheduled'), (N'TrialReminder'), (N'FollowUpReminder'), (N'LeadConverted')
    ) AS nt(NotificationType)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.NotificationSettings ns
        WHERE ns.GymId = @GymId AND ns.NotificationType = nt.NotificationType);
END
GO

INSERT INTO dbo.NotificationSettings (GymId, NotificationType, IsEnabled)
SELECT g.GymId, nt.NotificationType, 1
FROM dbo.Gyms g
CROSS JOIN (VALUES
    (N'LeadCreated'), (N'TrialScheduled'), (N'TrialReminder'), (N'FollowUpReminder'), (N'LeadConverted')
) AS nt(NotificationType)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.NotificationSettings ns
    WHERE ns.GymId = g.GymId AND ns.NotificationType = nt.NotificationType);
GO
