/*
  Member Self-Service & Progress Tracking Module
*/

/* ========== MEMBER GOALS ========== */
IF OBJECT_ID(N'dbo.MemberGoals', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberGoals
    (
        GoalId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        GoalType NVARCHAR(30) NOT NULL,
        TargetValue DECIMAL(10, 2) NOT NULL,
        CurrentValue DECIMAL(10, 2) NOT NULL CONSTRAINT DF_MemberGoals_Current DEFAULT (0),
        TargetDate DATE NOT NULL,
        [Status] NVARCHAR(20) NOT NULL CONSTRAINT DF_MemberGoals_Status DEFAULT (N'Active'),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_MemberGoals_Created DEFAULT (SYSUTCDATETIME()),
        UpdatedDate DATETIME2 NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_MemberGoals_IsDeleted DEFAULT (0),
        CONSTRAINT FK_MemberGoals_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_MemberGoals_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId)
    );
    CREATE INDEX IX_MemberGoals_Gym_Member ON dbo.MemberGoals (GymId, MemberId, [Status]) WHERE IsDeleted = 0;
END
GO

/* Replace legacy MemberProgress (004) with self-service schema */
IF OBJECT_ID(N'dbo.MemberProgress', N'U') IS NOT NULL AND COL_LENGTH('dbo.MemberProgress', 'ProgressId') IS NULL
    DROP TABLE dbo.MemberProgress;
GO

/* ========== MEMBER PROGRESS ========== */
IF OBJECT_ID(N'dbo.MemberProgress', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberProgress
    (
        ProgressId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        Weight DECIMAL(6, 2) NULL,
        BMI DECIMAL(5, 2) NULL,
        BodyFatPercentage DECIMAL(5, 2) NULL,
        Chest DECIMAL(6, 2) NULL,
        Waist DECIMAL(6, 2) NULL,
        Arms DECIMAL(6, 2) NULL,
        Thighs DECIMAL(6, 2) NULL,
        Notes NVARCHAR(1000) NULL,
        ProgressDate DATE NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_MemberProgress_Created DEFAULT (SYSUTCDATETIME()),
        IsDeleted BIT NOT NULL CONSTRAINT DF_MemberProgress_IsDeleted DEFAULT (0),
        CONSTRAINT FK_MemberProgress_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_MemberGoals_Members_Progress FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId)
    );
    CREATE INDEX IX_MemberProgress_Gym_Member_Date ON dbo.MemberProgress (GymId, MemberId, ProgressDate DESC) WHERE IsDeleted = 0;
END
GO

/* ========== MEMBER PROGRESS PHOTOS ========== */
IF OBJECT_ID(N'dbo.MemberProgressPhotos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberProgressPhotos
    (
        ProgressPhotoId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        FileId BIGINT NOT NULL,
        PhotoType NVARCHAR(30) NOT NULL CONSTRAINT DF_MemberProgressPhotos_Type DEFAULT (N'Front'),
        UploadedDate DATETIME2 NOT NULL CONSTRAINT DF_MemberProgressPhotos_Uploaded DEFAULT (SYSUTCDATETIME()),
        IsDeleted BIT NOT NULL CONSTRAINT DF_MemberProgressPhotos_IsDeleted DEFAULT (0),
        CONSTRAINT FK_MemberProgressPhotos_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_MemberProgressPhotos_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT FK_MemberProgressPhotos_Files FOREIGN KEY (FileId) REFERENCES dbo.Files (FileId)
    );
    CREATE INDEX IX_MemberProgressPhotos_Member ON dbo.MemberProgressPhotos (GymId, MemberId, UploadedDate DESC) WHERE IsDeleted = 0;
END
GO

/* ========== WATER INTAKE LOGS ========== */
IF OBJECT_ID(N'dbo.WaterIntakeLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WaterIntakeLogs
    (
        WaterIntakeId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        TargetLitres DECIMAL(5, 2) NOT NULL CONSTRAINT DF_WaterIntake_Target DEFAULT (2.5),
        ConsumedLitres DECIMAL(5, 2) NOT NULL CONSTRAINT DF_WaterIntake_Consumed DEFAULT (0),
        LogDate DATE NOT NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_WaterIntake_Created DEFAULT (SYSUTCDATETIME()),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT FK_WaterIntakeLogs_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_WaterIntakeLogs_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT UX_WaterIntakeLogs_Member_Date UNIQUE (GymId, MemberId, LogDate)
    );
END
GO

/* ========== WORKOUT TRACKING ========== */
IF OBJECT_ID(N'dbo.WorkoutTracking', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkoutTracking
    (
        WorkoutTrackingId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        WorkoutPlanId INT NOT NULL,
        ExerciseCompleted NVARCHAR(500) NULL,
        CompletionPercentage DECIMAL(5, 2) NOT NULL CONSTRAINT DF_WorkoutTracking_Pct DEFAULT (0),
        WorkoutDate DATE NOT NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_WorkoutTracking_Created DEFAULT (SYSUTCDATETIME()),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT FK_WorkoutTracking_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_WorkoutTracking_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT FK_WorkoutTracking_Plans FOREIGN KEY (WorkoutPlanId) REFERENCES dbo.WorkoutPlans (WorkoutPlanId),
        CONSTRAINT UX_WorkoutTracking_Member_Date UNIQUE (GymId, MemberId, WorkoutDate)
    );
END
GO

/* ========== DIET TRACKING ========== */
IF OBJECT_ID(N'dbo.DietTracking', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DietTracking
    (
        DietTrackingId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        DietPlanId INT NOT NULL,
        CompliancePercentage DECIMAL(5, 2) NOT NULL CONSTRAINT DF_DietTracking_Pct DEFAULT (0),
        MealsCompleted INT NOT NULL CONSTRAINT DF_DietTracking_Meals DEFAULT (0),
        TrackingDate DATE NOT NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_DietTracking_Created DEFAULT (SYSUTCDATETIME()),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT FK_DietTracking_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_DietTracking_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT FK_DietTracking_Plans FOREIGN KEY (DietPlanId) REFERENCES dbo.DietPlans (DietPlanId),
        CONSTRAINT UX_DietTracking_Member_Date UNIQUE (GymId, MemberId, TrackingDate)
    );
END
GO

/* ========== MEMBER REFERRALS ========== */
IF OBJECT_ID(N'dbo.MemberReferrals', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberReferrals
    (
        ReferralId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        ReferralCode NVARCHAR(20) NOT NULL,
        ReferredMemberId INT NULL,
        RewardPoints INT NOT NULL CONSTRAINT DF_MemberReferrals_Points DEFAULT (0),
        [Status] NVARCHAR(20) NOT NULL CONSTRAINT DF_MemberReferrals_Status DEFAULT (N'Pending'),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_MemberReferrals_Created DEFAULT (SYSUTCDATETIME()),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT FK_MemberReferrals_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_MemberReferrals_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT FK_MemberReferrals_Referred FOREIGN KEY (ReferredMemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT UX_MemberReferrals_Code UNIQUE (GymId, ReferralCode)
    );
    CREATE INDEX IX_MemberReferrals_Referrer ON dbo.MemberReferrals (GymId, MemberId);
END
GO

/* ========== MEMBER FEEDBACK ========== */
IF OBJECT_ID(N'dbo.MemberFeedback', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberFeedback
    (
        FeedbackId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        Rating INT NOT NULL,
        Comments NVARCHAR(2000) NULL,
        TrainerId INT NULL,
        FeedbackType NVARCHAR(20) NOT NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_MemberFeedback_Created DEFAULT (SYSUTCDATETIME()),
        IsDeleted BIT NOT NULL CONSTRAINT DF_MemberFeedback_IsDeleted DEFAULT (0),
        CONSTRAINT FK_MemberFeedback_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_MemberFeedback_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT FK_MemberFeedback_Trainers FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers (TrainerId),
        CONSTRAINT CK_MemberFeedback_Rating CHECK (Rating BETWEEN 1 AND 5)
    );
    CREATE INDEX IX_MemberFeedback_Gym ON dbo.MemberFeedback (GymId, FeedbackType, CreatedDate DESC) WHERE IsDeleted = 0;
END
GO

/* ========== MEMBER QR TOKENS ========== */
IF OBJECT_ID(N'dbo.MemberQrTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberQrTokens
    (
        MemberId INT NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        QrToken NVARCHAR(64) NOT NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_MemberQrTokens_Created DEFAULT (SYSUTCDATETIME()),
        RotatedDate DATETIME2 NULL,
        CONSTRAINT FK_MemberQrTokens_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT FK_MemberQrTokens_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT UX_MemberQrTokens_Token UNIQUE (QrToken)
    );
END
GO

/* ========== GOALS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateMemberGoal
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @GoalType NVARCHAR(30),
    @TargetValue DECIMAL(10, 2),
    @CurrentValue DECIMAL(10, 2) = 0,
    @TargetDate DATE,
    @GoalId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Members WHERE MemberId = @MemberId AND GymId = @GymId AND IsDeleted = 0)
        THROW 50400, 'Member not found.', 1;
    INSERT INTO dbo.MemberGoals (GymId, MemberId, GoalType, TargetValue, CurrentValue, TargetDate, [Status])
    VALUES (@GymId, @MemberId, @GoalType, @TargetValue, @CurrentValue, @TargetDate, N'Active');
    SET @GoalId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateMemberGoal
    @GoalId INT,
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @GoalType NVARCHAR(30),
    @TargetValue DECIMAL(10, 2),
    @TargetDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.MemberGoals
    SET GoalType = @GoalType, TargetValue = @TargetValue, TargetDate = @TargetDate, UpdatedDate = SYSUTCDATETIME()
    WHERE GoalId = @GoalId AND GymId = @GymId AND MemberId = @MemberId AND IsDeleted = 0 AND [Status] = N'Active';
    IF @@ROWCOUNT = 0 THROW 50404, 'Goal not found or not editable.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateMemberGoalProgress
    @GoalId INT,
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @CurrentValue DECIMAL(10, 2)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.MemberGoals
    SET CurrentValue = @CurrentValue, UpdatedDate = SYSUTCDATETIME()
    WHERE GoalId = @GoalId AND GymId = @GymId AND MemberId = @MemberId AND IsDeleted = 0 AND [Status] = N'Active';
    IF @@ROWCOUNT = 0 THROW 50404, 'Goal not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CompleteMemberGoal
    @GoalId INT,
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.MemberGoals
    SET [Status] = N'Completed', UpdatedDate = SYSUTCDATETIME()
    WHERE GoalId = @GoalId AND GymId = @GymId AND MemberId = @MemberId AND IsDeleted = 0;
    IF @@ROWCOUNT = 0 THROW 50404, 'Goal not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberGoals
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @Status NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT GoalId, GymId, MemberId, GoalType, TargetValue, CurrentValue, TargetDate, [Status], CreatedDate, UpdatedDate
    FROM dbo.MemberGoals
    WHERE GymId = @GymId AND MemberId = @MemberId AND IsDeleted = 0
      AND (@Status IS NULL OR [Status] = @Status)
    ORDER BY CreatedDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberGoalById
    @GoalId INT,
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT GoalId, GymId, MemberId, GoalType, TargetValue, CurrentValue, TargetDate, [Status], CreatedDate, UpdatedDate
    FROM dbo.MemberGoals
    WHERE GoalId = @GoalId AND GymId = @GymId AND MemberId = @MemberId AND IsDeleted = 0;
END
GO

/* ========== PROGRESS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateMemberProgress
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @Weight DECIMAL(6, 2) = NULL,
    @BMI DECIMAL(5, 2) = NULL,
    @BodyFatPercentage DECIMAL(5, 2) = NULL,
    @Chest DECIMAL(6, 2) = NULL,
    @Waist DECIMAL(6, 2) = NULL,
    @Arms DECIMAL(6, 2) = NULL,
    @Thighs DECIMAL(6, 2) = NULL,
    @Notes NVARCHAR(1000) = NULL,
    @ProgressDate DATE,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @ProgressId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.MemberProgress (GymId, MemberId, Weight, BMI, BodyFatPercentage, Chest, Waist, Arms, Thighs, Notes, ProgressDate, CreatedBy)
    VALUES (@GymId, @MemberId, @Weight, @BMI, @BodyFatPercentage, @Chest, @Waist, @Arms, @Thighs, @Notes, @ProgressDate, @CreatedBy);
    SET @ProgressId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberProgressHistory
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProgressId, GymId, MemberId, Weight, BMI, BodyFatPercentage, Chest, Waist, Arms, Thighs, Notes, ProgressDate, CreatedDate
    FROM dbo.MemberProgress
    WHERE GymId = @GymId AND MemberId = @MemberId AND IsDeleted = 0
      AND (@FromDate IS NULL OR ProgressDate >= @FromDate)
      AND (@ToDate IS NULL OR ProgressDate <= @ToDate)
    ORDER BY ProgressDate ASC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateMemberProgressPhoto
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @FileId BIGINT,
    @PhotoType NVARCHAR(30),
    @ProgressPhotoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.MemberProgressPhotos (GymId, MemberId, FileId, PhotoType)
    VALUES (@GymId, @MemberId, @FileId, @PhotoType);
    SET @ProgressPhotoId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberProgressPhotos
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.ProgressPhotoId, p.GymId, p.MemberId, p.FileId, p.PhotoType, p.UploadedDate,
           f.OriginalFileName, f.ContentType
    FROM dbo.MemberProgressPhotos p
    INNER JOIN dbo.Files f ON f.FileId = p.FileId
    WHERE p.GymId = @GymId AND p.MemberId = @MemberId AND p.IsDeleted = 0
    ORDER BY p.UploadedDate DESC;
END
GO

/* ========== WATER ========== */
CREATE OR ALTER PROCEDURE dbo.sp_UpsertWaterIntake
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @TargetLitres DECIMAL(5, 2),
    @ConsumedLitres DECIMAL(5, 2),
    @LogDate DATE,
    @WaterIntakeId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    MERGE dbo.WaterIntakeLogs AS t
    USING (SELECT @GymId AS GymId, @MemberId AS MemberId, @LogDate AS LogDate) AS s
    ON t.GymId = s.GymId AND t.MemberId = s.MemberId AND t.LogDate = s.LogDate
    WHEN MATCHED THEN
        UPDATE SET TargetLitres = @TargetLitres, ConsumedLitres = @ConsumedLitres, UpdatedDate = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (GymId, MemberId, TargetLitres, ConsumedLitres, LogDate)
        VALUES (@GymId, @MemberId, @TargetLitres, @ConsumedLitres, @LogDate);
    SELECT @WaterIntakeId = WaterIntakeId FROM dbo.WaterIntakeLogs
    WHERE GymId = @GymId AND MemberId = @MemberId AND LogDate = @LogDate;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWaterIntake
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @LogDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT WaterIntakeId, GymId, MemberId, TargetLitres, ConsumedLitres, LogDate, CreatedDate, UpdatedDate
    FROM dbo.WaterIntakeLogs
    WHERE GymId = @GymId AND MemberId = @MemberId AND LogDate = @LogDate;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWaterIntakeHistory
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT WaterIntakeId, GymId, MemberId, TargetLitres, ConsumedLitres, LogDate
    FROM dbo.WaterIntakeLogs
    WHERE GymId = @GymId AND MemberId = @MemberId AND LogDate BETWEEN @FromDate AND @ToDate
    ORDER BY LogDate ASC;
END
GO

/* ========== WORKOUT TRACKING ========== */
CREATE OR ALTER PROCEDURE dbo.sp_UpsertWorkoutTracking
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @WorkoutPlanId INT,
    @ExerciseCompleted NVARCHAR(500) = NULL,
    @CompletionPercentage DECIMAL(5, 2),
    @WorkoutDate DATE,
    @WorkoutTrackingId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    MERGE dbo.WorkoutTracking AS t
    USING (SELECT @GymId AS GymId, @MemberId AS MemberId, @WorkoutDate AS WorkoutDate) AS s
    ON t.GymId = s.GymId AND t.MemberId = s.MemberId AND t.WorkoutDate = s.WorkoutDate
    WHEN MATCHED THEN
        UPDATE SET WorkoutPlanId = @WorkoutPlanId, ExerciseCompleted = @ExerciseCompleted,
                   CompletionPercentage = @CompletionPercentage, UpdatedDate = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (GymId, MemberId, WorkoutPlanId, ExerciseCompleted, CompletionPercentage, WorkoutDate)
        VALUES (@GymId, @MemberId, @WorkoutPlanId, @ExerciseCompleted, @CompletionPercentage, @WorkoutDate);
    SELECT @WorkoutTrackingId = WorkoutTrackingId FROM dbo.WorkoutTracking
    WHERE GymId = @GymId AND MemberId = @MemberId AND WorkoutDate = @WorkoutDate;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWorkoutTrackingHistory
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT wt.WorkoutTrackingId, wt.GymId, wt.MemberId, wt.WorkoutPlanId, wp.PlanName AS WorkoutPlanName,
           wt.ExerciseCompleted, wt.CompletionPercentage, wt.WorkoutDate
    FROM dbo.WorkoutTracking wt
    LEFT JOIN dbo.WorkoutPlans wp ON wp.WorkoutPlanId = wt.WorkoutPlanId
    WHERE wt.GymId = @GymId AND wt.MemberId = @MemberId AND wt.WorkoutDate BETWEEN @FromDate AND @ToDate
    ORDER BY wt.WorkoutDate ASC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWorkoutStreak
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @StreakDays INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @StreakDays = 0;
    DECLARE @CheckDate DATE = CAST(SYSUTCDATETIME() AS DATE);
    WHILE EXISTS (
        SELECT 1 FROM dbo.WorkoutTracking
        WHERE GymId = @GymId AND MemberId = @MemberId AND WorkoutDate = @CheckDate AND CompletionPercentage >= 50
    )
    BEGIN
        SET @StreakDays = @StreakDays + 1;
        SET @CheckDate = DATEADD(DAY, -1, @CheckDate);
    END
END
GO

/* ========== DIET TRACKING ========== */
CREATE OR ALTER PROCEDURE dbo.sp_UpsertDietTracking
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @DietPlanId INT,
    @CompliancePercentage DECIMAL(5, 2),
    @MealsCompleted INT,
    @TrackingDate DATE,
    @DietTrackingId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    MERGE dbo.DietTracking AS t
    USING (SELECT @GymId AS GymId, @MemberId AS MemberId, @TrackingDate AS TrackingDate) AS s
    ON t.GymId = s.GymId AND t.MemberId = s.MemberId AND t.TrackingDate = s.TrackingDate
    WHEN MATCHED THEN
        UPDATE SET DietPlanId = @DietPlanId, CompliancePercentage = @CompliancePercentage,
                   MealsCompleted = @MealsCompleted, UpdatedDate = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (GymId, MemberId, DietPlanId, CompliancePercentage, MealsCompleted, TrackingDate)
        VALUES (@GymId, @MemberId, @DietPlanId, @CompliancePercentage, @MealsCompleted, @TrackingDate);
    SELECT @DietTrackingId = DietTrackingId FROM dbo.DietTracking
    WHERE GymId = @GymId AND MemberId = @MemberId AND TrackingDate = @TrackingDate;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetDietTrackingHistory
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT dt.DietTrackingId, dt.GymId, dt.MemberId, dt.DietPlanId, dp.PlanName AS DietPlanName,
           dt.CompliancePercentage, dt.MealsCompleted, dt.TrackingDate
    FROM dbo.DietTracking dt
    LEFT JOIN dbo.DietPlans dp ON dp.DietPlanId = dt.DietPlanId
    WHERE dt.GymId = @GymId AND dt.MemberId = @MemberId AND dt.TrackingDate BETWEEN @FromDate AND @ToDate
    ORDER BY dt.TrackingDate ASC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetDietComplianceSummary
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @DailyCompliance DECIMAL(5, 2) OUTPUT,
    @WeeklyCompliance DECIMAL(5, 2) OUTPUT,
    @MonthlyCompliance DECIMAL(5, 2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    SELECT @DailyCompliance = ISNULL(CompliancePercentage, 0) FROM dbo.DietTracking
    WHERE GymId = @GymId AND MemberId = @MemberId AND TrackingDate = @Today;
    SELECT @WeeklyCompliance = ISNULL(AVG(CAST(CompliancePercentage AS FLOAT)), 0)
    FROM dbo.DietTracking WHERE GymId = @GymId AND MemberId = @MemberId
      AND TrackingDate >= DATEADD(DAY, -6, @Today);
    SELECT @MonthlyCompliance = ISNULL(AVG(CAST(CompliancePercentage AS FLOAT)), 0)
    FROM dbo.DietTracking WHERE GymId = @GymId AND MemberId = @MemberId
      AND TrackingDate >= DATEADD(DAY, -29, @Today);
    SET @DailyCompliance = ISNULL(@DailyCompliance, 0);
    SET @WeeklyCompliance = ISNULL(@WeeklyCompliance, 0);
    SET @MonthlyCompliance = ISNULL(@MonthlyCompliance, 0);
END
GO

/* ========== REFERRALS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetOrCreateReferralCode
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @ReferralCode NVARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 @ReferralCode = ReferralCode FROM dbo.MemberReferrals
    WHERE GymId = @GymId AND MemberId = @MemberId AND ReferredMemberId IS NULL
    ORDER BY CreatedDate;
    IF @ReferralCode IS NOT NULL RETURN;
    SET @ReferralCode = UPPER(LEFT(REPLACE(CAST(NEWID() AS NVARCHAR(36)), '-', ''), 8));
    INSERT INTO dbo.MemberReferrals (GymId, MemberId, ReferralCode, [Status])
    VALUES (@GymId, @MemberId, @ReferralCode, N'Active');
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RecordReferralConversion
    @GymId UNIQUEIDENTIFIER,
    @ReferralCode NVARCHAR(20),
    @ReferredMemberId INT,
    @RewardPoints INT = 100,
    @ReferralId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.MemberReferrals
    SET ReferredMemberId = @ReferredMemberId, RewardPoints = @RewardPoints,
        [Status] = N'Converted', UpdatedDate = SYSUTCDATETIME()
    WHERE GymId = @GymId AND ReferralCode = @ReferralCode AND ReferredMemberId IS NULL;
    IF @@ROWCOUNT = 0 THROW 50405, 'Invalid or already used referral code.', 1;
    SELECT @ReferralId = ReferralId FROM dbo.MemberReferrals
    WHERE GymId = @GymId AND ReferralCode = @ReferralCode AND ReferredMemberId = @ReferredMemberId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetReferralStats
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.ReferralId, r.ReferralCode, r.ReferredMemberId, r.RewardPoints, r.[Status], r.CreatedDate, r.UpdatedDate,
           ru.Name AS ReferredMemberName
    FROM dbo.MemberReferrals r
    LEFT JOIN dbo.Members rm ON rm.MemberId = r.ReferredMemberId
    LEFT JOIN dbo.Users ru ON ru.Id = rm.UserId
    WHERE r.GymId = @GymId AND r.MemberId = @MemberId
    ORDER BY r.CreatedDate DESC;
END
GO

/* ========== FEEDBACK ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateMemberFeedback
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @Rating INT,
    @Comments NVARCHAR(2000) = NULL,
    @TrainerId INT = NULL,
    @FeedbackType NVARCHAR(20),
    @FeedbackId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.MemberFeedback (GymId, MemberId, Rating, Comments, TrainerId, FeedbackType)
    VALUES (@GymId, @MemberId, @Rating, @Comments, @TrainerId, @FeedbackType);
    SET @FeedbackId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberFeedback
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FeedbackId, f.GymId, f.MemberId, f.Rating, f.Comments, f.TrainerId, f.FeedbackType, f.CreatedDate,
           tu.Name AS TrainerName
    FROM dbo.MemberFeedback f
    LEFT JOIN dbo.Trainers t ON t.TrainerId = f.TrainerId
    LEFT JOIN dbo.Users tu ON tu.Id = t.UserId
    WHERE f.GymId = @GymId AND f.MemberId = @MemberId AND f.IsDeleted = 0
    ORDER BY f.CreatedDate DESC;
END
GO

/* ========== QR CODE ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetOrCreateMemberQrToken
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @QrToken NVARCHAR(64) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @QrToken = QrToken FROM dbo.MemberQrTokens WHERE MemberId = @MemberId AND GymId = @GymId;
    IF @QrToken IS NOT NULL RETURN;
    SET @QrToken = REPLACE(CAST(NEWID() AS NVARCHAR(36)), '-', '') + REPLACE(CAST(NEWID() AS NVARCHAR(36)), '-', '');
    INSERT INTO dbo.MemberQrTokens (MemberId, GymId, QrToken) VALUES (@MemberId, @GymId, @QrToken);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MemberAttendance_QrCheckIn
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @QrToken NVARCHAR(64),
    @MarkedByUserId UNIQUEIDENTIFIER = NULL,
    @MemberAttendanceId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.MemberQrTokens q
        INNER JOIN dbo.Members m ON m.MemberId = q.MemberId
        WHERE q.MemberId = @MemberId AND q.GymId = @GymId AND q.QrToken = @QrToken
          AND m.IsDeleted = 0 AND m.IsActive = 1
    )
        THROW 50408, 'Invalid or expired QR code.', 1;

    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);

    IF EXISTS (
        SELECT 1 FROM dbo.MemberAttendance
        WHERE GymId = @GymId AND MemberId = @MemberId AND AttendanceDate = @Today
    )
        THROW 50409, 'Member already checked in today.', 1;

    IF EXISTS (
        SELECT 1 FROM dbo.MemberAttendance
        WHERE GymId = @GymId AND MemberId = @MemberId AND CheckOutAt IS NULL
    )
        THROW 50401, 'Member already has an open check-in session.', 1;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    INSERT INTO dbo.MemberAttendance (
        GymId, MemberId, AttendanceStatusId, AttendanceDate, CheckInAt, Notes, MarkedByUserId, CreatedAt)
    VALUES (@GymId, @MemberId, 1, @Today, @Now, N'QR check-in', @MarkedByUserId, @Now);
    SET @MemberAttendanceId = SCOPE_IDENTITY();
END
GO

/* ========== DASHBOARD ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMemberSelfServiceDashboard
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);

    -- Result set 1: Membership
    SELECT TOP 1 ms.MembershipId, mp.PlanName, ms.StartDate, ms.EndDate, ms.[Status],
           DATEDIFF(DAY, @Today, ms.EndDate) AS RemainingDays
    FROM dbo.Memberships ms
    INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
    WHERE ms.GymId = @GymId AND ms.MemberId = @MemberId AND ms.[Status] = N'Active'
    ORDER BY ms.EndDate DESC;

    -- Result set 2: Attendance percentage (current month)
    SELECT
        TotalDays = DAY(EOMONTH(@Today)),
        PresentDays = (
            SELECT COUNT(DISTINCT AttendanceDate) FROM dbo.MemberAttendance
            WHERE GymId = @GymId AND MemberId = @MemberId AND AttendanceDate >= @MonthStart
              AND AttendanceStatusId IN (1, 2, 3)
        );

    -- Result set 3: Active goal
    SELECT TOP 1 GoalId, GoalType, TargetValue, CurrentValue, TargetDate, [Status],
           CASE WHEN TargetValue = 0 THEN 0 ELSE (CurrentValue / TargetValue) * 100 END AS ProgressPercent
    FROM dbo.MemberGoals
    WHERE GymId = @GymId AND MemberId = @MemberId AND [Status] = N'Active' AND IsDeleted = 0
    ORDER BY CreatedDate DESC;

    -- Result set 4: Today's workout
    SELECT TOP 1 wt.*, wp.PlanName
    FROM dbo.WorkoutTracking wt
    LEFT JOIN dbo.WorkoutPlans wp ON wp.WorkoutPlanId = wt.WorkoutPlanId
    WHERE wt.GymId = @GymId AND wt.MemberId = @MemberId AND wt.WorkoutDate = @Today;

    -- Result set 5: Today's diet
    SELECT TOP 1 dt.*, dp.PlanName
    FROM dbo.DietTracking dt
    LEFT JOIN dbo.DietPlans dp ON dp.DietPlanId = dt.DietPlanId
    WHERE dt.GymId = @GymId AND dt.MemberId = @MemberId AND dt.TrackingDate = @Today;

    -- Result set 6: Water today
    SELECT * FROM dbo.WaterIntakeLogs
    WHERE GymId = @GymId AND MemberId = @MemberId AND LogDate = @Today;

    -- Result set 7: Recent payments
    SELECT TOP 5 p.PaymentId, p.Amount, p.PaymentDate, p.PaymentMethod, p.[Status], i.InvoiceNumber
    FROM dbo.Payments p
    LEFT JOIN dbo.Invoices i ON i.PaymentId = p.PaymentId
    WHERE p.GymId = @GymId AND p.MemberId = @MemberId
    ORDER BY p.PaymentDate DESC;

    -- Result set 8: Referral stats summary
    SELECT
        TotalReferrals = COUNT(*),
        ConvertedReferrals = SUM(CASE WHEN [Status] = N'Converted' THEN 1 ELSE 0 END),
        TotalRewardPoints = ISNULL(SUM(RewardPoints), 0)
    FROM dbo.MemberReferrals
    WHERE GymId = @GymId AND MemberId = @MemberId;
END
GO

/* ========== ANALYTICS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMemberSelfServiceAnalytics
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    DECLARE @DailyCompliance DECIMAL(5, 2);
    DECLARE @WeeklyCompliance DECIMAL(5, 2);
    DECLARE @MonthlyCompliance DECIMAL(5, 2);

    SELECT
        GoalCompletionRate = CASE WHEN COUNT(*) = 0 THEN 0
            ELSE CAST(SUM(CASE WHEN [Status] = N'Completed' THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100 END
    FROM dbo.MemberGoals WHERE GymId = @GymId AND MemberId = @MemberId AND IsDeleted = 0;

    SELECT WorkoutCompliance = ISNULL(AVG(CAST(CompletionPercentage AS FLOAT)), 0)
    FROM dbo.WorkoutTracking WHERE GymId = @GymId AND MemberId = @MemberId
      AND WorkoutDate >= DATEADD(DAY, -29, @Today);

    EXEC dbo.sp_GetDietComplianceSummary @GymId, @MemberId,
        @DailyCompliance OUTPUT, @WeeklyCompliance OUTPUT, @MonthlyCompliance OUTPUT;

    SELECT @DailyCompliance AS DailyCompliance, @WeeklyCompliance AS WeeklyCompliance, @MonthlyCompliance AS MonthlyCompliance;

    SELECT WaterCompliance = ISNULL(AVG(CASE WHEN TargetLitres = 0 THEN 0 ELSE (ConsumedLitres / TargetLitres) * 100 END), 0)
    FROM dbo.WaterIntakeLogs WHERE GymId = @GymId AND MemberId = @MemberId
      AND LogDate >= DATEADD(DAY, -29, @Today);

    SELECT ReferralConversion = CASE WHEN COUNT(*) = 0 THEN 0
        ELSE CAST(SUM(CASE WHEN [Status] = N'Converted' THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100 END
    FROM dbo.MemberReferrals WHERE GymId = @GymId AND MemberId = @MemberId;
END
GO
