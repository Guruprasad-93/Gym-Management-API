/*
  Mobile App API Optimization & Push Notification Module
*/

/* ========== DEVICE TOKENS ========== */
IF OBJECT_ID(N'dbo.DeviceTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DeviceTokens
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        DeviceType NVARCHAR(20) NOT NULL,
        DeviceToken NVARCHAR(500) NOT NULL,
        AppVersion NVARCHAR(50) NULL,
        LastActiveDate DATETIME2 NOT NULL CONSTRAINT DF_DeviceTokens_LastActive DEFAULT (SYSUTCDATETIME()),
        IsActive BIT NOT NULL CONSTRAINT DF_DeviceTokens_IsActive DEFAULT (1),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_DeviceTokens_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_DeviceTokens_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_DeviceTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id),
        CONSTRAINT UX_DeviceTokens_Token UNIQUE (DeviceToken)
    );
    CREATE INDEX IX_DeviceTokens_User ON dbo.DeviceTokens (GymId, UserId) WHERE IsActive = 1;
END
GO

/* ========== PUSH NOTIFICATIONS ========== */
IF OBJECT_ID(N'dbo.PushNotifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PushNotifications
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(1000) NOT NULL,
        NotificationType NVARCHAR(50) NOT NULL,
        DataJson NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_PushNotifications_Status DEFAULT (N'Pending'),
        IsRead BIT NOT NULL CONSTRAINT DF_PushNotifications_IsRead DEFAULT (0),
        SentDate DATETIME2 NULL,
        DeliveredDate DATETIME2 NULL,
        ReadDate DATETIME2 NULL,
        OpenedDate DATETIME2 NULL,
        ClickedDate DATETIME2 NULL,
        FailureReason NVARCHAR(500) NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_PushNotifications_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_PushNotifications_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_PushNotifications_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
    );
    CREATE INDEX IX_PushNotifications_User ON dbo.PushNotifications (GymId, UserId, CreatedDate DESC);
    CREATE INDEX IX_PushNotifications_Status ON dbo.PushNotifications (GymId, [Status]) WHERE [Status] = N'Pending';
END
GO

/* ========== NOTIFICATION PREFERENCES ========== */
IF OBJECT_ID(N'dbo.NotificationPreferences', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NotificationPreferences
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        PushEnabled BIT NOT NULL CONSTRAINT DF_NotificationPreferences_Push DEFAULT (1),
        MembershipReminders BIT NOT NULL CONSTRAINT DF_NotificationPreferences_Membership DEFAULT (1),
        WorkoutReminders BIT NOT NULL CONSTRAINT DF_NotificationPreferences_Workout DEFAULT (1),
        DietReminders BIT NOT NULL CONSTRAINT DF_NotificationPreferences_Diet DEFAULT (1),
        AttendanceReminders BIT NOT NULL CONSTRAINT DF_NotificationPreferences_Attendance DEFAULT (1),
        PromotionalNotifications BIT NOT NULL CONSTRAINT DF_NotificationPreferences_Promo DEFAULT (1),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT FK_NotificationPreferences_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id),
        CONSTRAINT UX_NotificationPreferences_User UNIQUE (UserId)
    );
END
GO

/* ========== DEVICE REGISTRATION ========== */
CREATE OR ALTER PROCEDURE dbo.sp_RegisterDeviceToken
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @DeviceType NVARCHAR(20),
    @DeviceToken NVARCHAR(500),
    @AppVersion NVARCHAR(50) = NULL,
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.DeviceTokens SET IsActive = 0
    WHERE GymId = @GymId AND UserId = @UserId AND DeviceToken <> @DeviceToken AND IsActive = 1;

    IF EXISTS (SELECT 1 FROM dbo.DeviceTokens WHERE DeviceToken = @DeviceToken)
    BEGIN
        UPDATE dbo.DeviceTokens
        SET GymId = @GymId, UserId = @UserId, DeviceType = @DeviceType, AppVersion = @AppVersion,
            LastActiveDate = SYSUTCDATETIME(), IsActive = 1
        WHERE DeviceToken = @DeviceToken;
        SELECT @Id = Id FROM dbo.DeviceTokens WHERE DeviceToken = @DeviceToken;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.DeviceTokens (GymId, UserId, DeviceType, DeviceToken, AppVersion)
        VALUES (@GymId, @UserId, @DeviceType, @DeviceToken, @AppVersion);
        SET @Id = SCOPE_IDENTITY();
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UnregisterDeviceToken
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @DeviceToken NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.DeviceTokens SET IsActive = 0, LastActiveDate = SYSUTCDATETIME()
    WHERE GymId = @GymId AND UserId = @UserId AND DeviceToken = @DeviceToken;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetActiveDeviceTokensForUser
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, GymId, UserId, DeviceType, DeviceToken, AppVersion, LastActiveDate, IsActive, CreatedDate
    FROM dbo.DeviceTokens
    WHERE GymId = @GymId AND UserId = @UserId AND IsActive = 1;
END
GO

/* ========== PUSH NOTIFICATIONS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreatePushNotification
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @Title NVARCHAR(200),
    @Message NVARCHAR(1000),
    @NotificationType NVARCHAR(50),
    @DataJson NVARCHAR(MAX) = NULL,
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.PushNotifications (GymId, UserId, Title, Message, NotificationType, DataJson)
    VALUES (@GymId, @UserId, @Title, @Message, @NotificationType, @DataJson);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdatePushNotificationStatus
    @Id INT,
    @GymId UNIQUEIDENTIFIER,
    @Status NVARCHAR(30),
    @FailureReason NVARCHAR(500) = NULL,
    @SentDate DATETIME2 = NULL,
    @DeliveredDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.PushNotifications
    SET [Status] = @Status,
        FailureReason = @FailureReason,
        SentDate = COALESCE(@SentDate, SentDate),
        DeliveredDate = COALESCE(@DeliveredDate, DeliveredDate)
    WHERE Id = @Id AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MarkPushNotificationsRead
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @NotificationIds NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @NotificationIds IS NULL OR LTRIM(RTRIM(@NotificationIds)) = N''
        UPDATE dbo.PushNotifications
        SET IsRead = 1, ReadDate = SYSUTCDATETIME()
        WHERE GymId = @GymId AND UserId = @UserId AND IsRead = 0;
    ELSE
        UPDATE pn SET IsRead = 1, ReadDate = SYSUTCDATETIME()
        FROM dbo.PushNotifications pn
        INNER JOIN STRING_SPLIT(@NotificationIds, N',') s ON TRY_CAST(s.[value] AS INT) = pn.Id
        WHERE pn.GymId = @GymId AND pn.UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RecordPushNotificationEngagement
    @Id INT,
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @EngagementType NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    IF @EngagementType = N'Opened'
        UPDATE dbo.PushNotifications SET OpenedDate = SYSUTCDATETIME(), [Status] = N'Opened'
        WHERE Id = @Id AND GymId = @GymId AND UserId = @UserId;
    ELSE IF @EngagementType = N'Clicked'
        UPDATE dbo.PushNotifications SET ClickedDate = SYSUTCDATETIME(), [Status] = N'Clicked'
        WHERE Id = @Id AND GymId = @GymId AND UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPushNotificationsPaged
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @UnreadOnly BIT = 0,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 20;
    IF @PageSize > 100 SET @PageSize = 100;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.PushNotifications
    WHERE GymId = @GymId AND UserId = @UserId
      AND (@UnreadOnly = 0 OR IsRead = 0);

    SELECT Id, GymId, UserId, Title, Message, NotificationType, DataJson, [Status], IsRead,
           SentDate, DeliveredDate, ReadDate, OpenedDate, ClickedDate, FailureReason, CreatedDate
    FROM dbo.PushNotifications
    WHERE GymId = @GymId AND UserId = @UserId
      AND (@UnreadOnly = 0 OR IsRead = 0)
    ORDER BY CreatedDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPendingPushNotifications
    @BatchSize INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@BatchSize)
        pn.Id, pn.GymId, pn.UserId, pn.Title, pn.Message, pn.NotificationType, pn.DataJson,
        dt.DeviceToken, dt.DeviceType
    FROM dbo.PushNotifications pn
    INNER JOIN dbo.DeviceTokens dt ON dt.UserId = pn.UserId AND dt.GymId = pn.GymId AND dt.IsActive = 1
    WHERE pn.[Status] = N'Pending'
    ORDER BY pn.CreatedDate;
END
GO

/* ========== PREFERENCES ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetOrCreateNotificationPreferences
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.NotificationPreferences WHERE UserId = @UserId)
        INSERT INTO dbo.NotificationPreferences (UserId) VALUES (@UserId);

    SELECT Id, UserId, PushEnabled, MembershipReminders, WorkoutReminders, DietReminders,
           AttendanceReminders, PromotionalNotifications, UpdatedDate
    FROM dbo.NotificationPreferences WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateNotificationPreferences
    @UserId UNIQUEIDENTIFIER,
    @PushEnabled BIT,
    @MembershipReminders BIT,
    @WorkoutReminders BIT,
    @DietReminders BIT,
    @AttendanceReminders BIT,
    @PromotionalNotifications BIT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.NotificationPreferences WHERE UserId = @UserId)
        INSERT INTO dbo.NotificationPreferences (UserId) VALUES (@UserId);

    UPDATE dbo.NotificationPreferences
    SET PushEnabled = @PushEnabled, MembershipReminders = @MembershipReminders,
        WorkoutReminders = @WorkoutReminders, DietReminders = @DietReminders,
        AttendanceReminders = @AttendanceReminders, PromotionalNotifications = @PromotionalNotifications,
        UpdatedDate = SYSUTCDATETIME()
    WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_IsPushCategoryEnabled
    @UserId UNIQUEIDENTIFIER,
    @NotificationType NVARCHAR(50),
    @IsEnabled BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @IsEnabled = 1;
    DECLARE @PushEnabled BIT = 1;
    SELECT @PushEnabled = PushEnabled FROM dbo.NotificationPreferences WHERE UserId = @UserId;
    IF @PushEnabled = 0 BEGIN SET @IsEnabled = 0; RETURN; END
    IF NOT EXISTS (SELECT 1 FROM dbo.NotificationPreferences WHERE UserId = @UserId) RETURN;

    SELECT @IsEnabled = CASE @NotificationType
        WHEN N'MembershipExpiry7Days' THEN MembershipReminders
        WHEN N'MembershipExpiry3Days' THEN MembershipReminders
        WHEN N'MembershipExpiryToday' THEN MembershipReminders
        WHEN N'MembershipRenewal' THEN MembershipReminders
        WHEN N'WorkoutPlanAssigned' THEN WorkoutReminders
        WHEN N'WorkoutReminder' THEN WorkoutReminders
        WHEN N'DietPlanAssigned' THEN DietReminders
        WHEN N'DietReminder' THEN DietReminders
        WHEN N'AttendanceReminder' THEN AttendanceReminders
        WHEN N'GoalCompleted' THEN WorkoutReminders
        WHEN N'GoalReminder' THEN WorkoutReminders
        WHEN N'BranchAnnouncement' THEN PromotionalNotifications
        WHEN N'ReferralRewardEarned' THEN PromotionalNotifications
        WHEN N'LeadAssigned' THEN PromotionalNotifications
        WHEN N'PayrollNotification' THEN PromotionalNotifications
        ELSE PromotionalNotifications
    END
    FROM dbo.NotificationPreferences WHERE UserId = @UserId;
END
GO

/* ========== MOBILE DASHBOARD (single call) ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMobileDashboard
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);

    -- Membership
    SELECT TOP 1 ms.MembershipId, mp.PlanName, ms.StartDate, ms.EndDate, ms.[Status],
           DATEDIFF(DAY, @Today, ms.EndDate) AS RemainingDays
    FROM dbo.Memberships ms
    INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
    WHERE ms.GymId = @GymId AND ms.MemberId = @MemberId AND ms.[Status] = N'Active'
    ORDER BY ms.EndDate DESC;

    -- Attendance summary
    SELECT
        TotalDays = DAY(EOMONTH(@Today)),
        PresentDays = (
            SELECT COUNT(DISTINCT AttendanceDate) FROM dbo.MemberAttendance
            WHERE GymId = @GymId AND MemberId = @MemberId AND AttendanceDate >= @MonthStart
        );

    -- Goal summary
    SELECT TOP 1 GoalId, GoalType, TargetValue, CurrentValue, TargetDate, [Status],
           CASE WHEN TargetValue = 0 THEN 0 ELSE (CurrentValue / TargetValue) * 100 END AS ProgressPercent
    FROM dbo.MemberGoals
    WHERE GymId = @GymId AND MemberId = @MemberId AND [Status] = N'Active' AND IsDeleted = 0
    ORDER BY CreatedDate DESC;

    -- Today's workout
    SELECT TOP 1 wt.*, wp.PlanName
    FROM dbo.WorkoutTracking wt
    LEFT JOIN dbo.WorkoutPlans wp ON wp.WorkoutPlanId = wt.WorkoutPlanId
    WHERE wt.GymId = @GymId AND wt.MemberId = @MemberId AND wt.WorkoutDate = @Today;

    -- Today's diet
    SELECT TOP 1 dt.*, dp.PlanName
    FROM dbo.DietTracking dt
    LEFT JOIN dbo.DietPlans dp ON dp.DietPlanId = dt.DietPlanId
    WHERE dt.GymId = @GymId AND dt.MemberId = @MemberId AND dt.TrackingDate = @Today;

    -- Water today
    SELECT * FROM dbo.WaterIntakeLogs
    WHERE GymId = @GymId AND MemberId = @MemberId AND LogDate = @Today;

    -- Unread notification count
    SELECT UnreadCount = COUNT(*)
    FROM dbo.PushNotifications
    WHERE GymId = @GymId AND UserId = @UserId AND IsRead = 0;

    -- Recent notifications (top 5)
    SELECT TOP 5 Id, Title, Message, NotificationType, IsRead, CreatedDate, [Status]
    FROM dbo.PushNotifications
    WHERE GymId = @GymId AND UserId = @UserId
    ORDER BY CreatedDate DESC;
END
GO

/* ========== MOBILE SYNC ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMobileSyncProfile
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.Id AS UserId, u.Name, u.Email, u.GymId, g.Name AS GymName, g.LogoUrl, g.PrimaryColor, g.SecondaryColor,
           m.MemberId, m.Phone, m.BranchId, b.BranchName
    FROM dbo.Users u
    INNER JOIN dbo.Gyms g ON g.GymId = u.GymId
    LEFT JOIN dbo.Members m ON m.UserId = u.Id AND m.GymId = @GymId AND m.IsDeleted = 0
    LEFT JOIN dbo.Branches b ON b.BranchId = m.BranchId
    WHERE u.Id = @UserId AND u.GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMobileSyncDelta
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @MemberId INT,
    @LastSyncDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Title, Message, NotificationType, IsRead, CreatedDate, [Status]
    FROM dbo.PushNotifications
    WHERE GymId = @GymId AND UserId = @UserId AND CreatedDate > @LastSyncDate
    ORDER BY CreatedDate DESC;

    IF @MemberId > 0
    BEGIN
        SELECT * FROM dbo.MemberGoals
        WHERE GymId = @GymId AND MemberId = @MemberId
          AND COALESCE(UpdatedDate, CreatedDate) > @LastSyncDate;

        SELECT * FROM dbo.WaterIntakeLogs
        WHERE GymId = @GymId AND MemberId = @MemberId
          AND COALESCE(UpdatedDate, CreatedDate) > @LastSyncDate;
    END
END
GO

/* ========== ANALYTICS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetPushNotificationAnalytics
    @GymId UNIQUEIDENTIFIER,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @From DATE = ISNULL(@FromDate, DATEADD(DAY, -30, CAST(SYSUTCDATETIME() AS DATE)));
    DECLARE @To DATE = ISNULL(@ToDate, CAST(SYSUTCDATETIME() AS DATE));

    SELECT
        TotalSent = SUM(CASE WHEN [Status] IN (N'Sent', N'Delivered', N'Opened', N'Clicked') THEN 1 ELSE 0 END),
        TotalDelivered = SUM(CASE WHEN DeliveredDate IS NOT NULL OR [Status] IN (N'Delivered', N'Opened', N'Clicked') THEN 1 ELSE 0 END),
        TotalFailed = SUM(CASE WHEN [Status] = N'Failed' THEN 1 ELSE 0 END),
        TotalOpened = SUM(CASE WHEN OpenedDate IS NOT NULL OR [Status] IN (N'Opened', N'Clicked') THEN 1 ELSE 0 END),
        TotalClicked = SUM(CASE WHEN ClickedDate IS NOT NULL OR [Status] = N'Clicked' THEN 1 ELSE 0 END),
        TotalPending = SUM(CASE WHEN [Status] = N'Pending' THEN 1 ELSE 0 END),
        ActiveDevices = (SELECT COUNT(*) FROM dbo.DeviceTokens WHERE GymId = @GymId AND IsActive = 1)
    FROM dbo.PushNotifications
    WHERE GymId = @GymId AND CAST(CreatedDate AS DATE) BETWEEN @From AND @To;

    SELECT NotificationType, COUNT(*) AS [Count],
        SUM(CASE WHEN [Status] = N'Failed' THEN 1 ELSE 0 END) AS FailedCount
    FROM dbo.PushNotifications
    WHERE GymId = @GymId AND CAST(CreatedDate AS DATE) BETWEEN @From AND @To
    GROUP BY NotificationType
    ORDER BY [Count] DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SearchPushNotificationCampaigns
    @GymId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH Campaigns AS (
        SELECT NotificationType, Title, Message, MIN(CreatedDate) AS SentDate, COUNT(*) AS RecipientCount,
               SUM(CASE WHEN [Status] = N'Failed' THEN 1 ELSE 0 END) AS FailedCount,
               SUM(CASE WHEN [Status] IN (N'Sent', N'Delivered', N'Opened', N'Clicked') THEN 1 ELSE 0 END) AS SentCount
        FROM dbo.PushNotifications
        WHERE GymId = @GymId AND NotificationType = N'ManualCampaign'
        GROUP BY NotificationType, Title, Message, CAST(CreatedDate AS DATE)
    )
    SELECT @TotalCount = COUNT(*) FROM Campaigns;
    SELECT * FROM Campaigns ORDER BY SentDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

/* ========== REMINDER CANDIDATES ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMembershipsExpiringForPush
    @DaysUntilExpiry INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Target DATE = DATEADD(DAY, @DaysUntilExpiry, CAST(SYSUTCDATETIME() AS DATE));
    SELECT ms.GymId, ms.MemberId, m.UserId, ms.MembershipId, ms.EndDate, mp.PlanName, u.Name AS MemberName
    FROM dbo.Memberships ms
    INNER JOIN dbo.Members m ON m.MemberId = ms.MemberId AND m.IsDeleted = 0 AND m.IsActive = 1
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
    WHERE ms.[Status] = N'Active' AND CAST(ms.EndDate AS DATE) = @Target
      AND EXISTS (SELECT 1 FROM dbo.DeviceTokens dt WHERE dt.UserId = m.UserId AND dt.GymId = ms.GymId AND dt.IsActive = 1);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMembersForAttendancePushReminder
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    SELECT m.GymId, m.MemberId, m.UserId, u.Name AS MemberName
    FROM dbo.Members m
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE m.IsDeleted = 0 AND m.IsActive = 1
      AND NOT EXISTS (
          SELECT 1 FROM dbo.MemberAttendance ma
          WHERE ma.MemberId = m.MemberId AND ma.GymId = m.GymId AND ma.AttendanceDate = @Today)
      AND EXISTS (SELECT 1 FROM dbo.DeviceTokens dt WHERE dt.UserId = m.UserId AND dt.GymId = m.GymId AND dt.IsActive = 1);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMembersForWorkoutPushReminder
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    SELECT DISTINCT m.GymId, m.MemberId, m.UserId, u.Name AS MemberName
    FROM dbo.Members m
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.AssignedWorkoutPlans awp ON awp.MemberId = m.MemberId AND awp.GymId = m.GymId
    WHERE m.IsDeleted = 0 AND m.IsActive = 1 AND awp.IsActive = 1
      AND NOT EXISTS (
          SELECT 1 FROM dbo.WorkoutTracking wt
          WHERE wt.MemberId = m.MemberId AND wt.GymId = m.GymId AND wt.WorkoutDate = @Today)
      AND EXISTS (SELECT 1 FROM dbo.DeviceTokens dt WHERE dt.UserId = m.UserId AND dt.GymId = m.GymId AND dt.IsActive = 1);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMembersForDietPushReminder
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    SELECT DISTINCT m.GymId, m.MemberId, m.UserId, u.Name AS MemberName
    FROM dbo.Members m
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.AssignedDietPlans adp ON adp.MemberId = m.MemberId AND adp.GymId = m.GymId
    WHERE m.IsDeleted = 0 AND m.IsActive = 1 AND adp.IsActive = 1
      AND NOT EXISTS (
          SELECT 1 FROM dbo.DietTracking dt
          WHERE dt.MemberId = m.MemberId AND dt.GymId = m.GymId AND dt.TrackingDate = @Today)
      AND EXISTS (SELECT 1 FROM dbo.DeviceTokens dt2 WHERE dt2.UserId = m.UserId AND dt2.GymId = m.GymId AND dt2.IsActive = 1);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMembersForGoalPushReminder
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    SELECT mg.GymId, mg.MemberId, m.UserId, mg.GoalId, mg.GoalType, mg.TargetDate, u.Name AS MemberName
    FROM dbo.MemberGoals mg
    INNER JOIN dbo.Members m ON m.MemberId = mg.MemberId AND m.IsDeleted = 0
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE mg.IsDeleted = 0 AND mg.[Status] = N'Active'
      AND mg.TargetDate <= DATEADD(DAY, 3, @Today)
      AND EXISTS (SELECT 1 FROM dbo.DeviceTokens dt WHERE dt.UserId = m.UserId AND dt.GymId = mg.GymId AND dt.IsActive = 1);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMobilePushRecipientUserIds
    @GymId UNIQUEIDENTIFIER,
    @BranchId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT m.UserId
    FROM dbo.Members m
    WHERE m.GymId = @GymId AND m.IsDeleted = 0 AND m.IsActive = 1
      AND (@BranchId IS NULL OR m.BranchId = @BranchId)
      AND EXISTS (
          SELECT 1 FROM dbo.DeviceTokens dt
          WHERE dt.UserId = m.UserId AND dt.GymId = @GymId AND dt.IsActive = 1);
END
GO
