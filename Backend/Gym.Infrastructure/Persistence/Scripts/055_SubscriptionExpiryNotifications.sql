-- 055_SubscriptionExpiryNotifications.sql

IF OBJECT_ID(N'dbo.UserInAppNotifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserInAppNotifications
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        NotificationKey NVARCHAR(200) NOT NULL,
        NotificationType NVARCHAR(80) NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(1000) NOT NULL,
        Severity NVARCHAR(20) NOT NULL CONSTRAINT DF_UserInAppNotifications_Severity DEFAULT (N'Info'),
        ActionRoute NVARCHAR(300) NULL,
        ShowLoginPopup BIT NOT NULL CONSTRAINT DF_UserInAppNotifications_ShowPopup DEFAULT (0),
        IsRead BIT NOT NULL CONSTRAINT DF_UserInAppNotifications_IsRead DEFAULT (0),
        ReadDate DATETIME2 NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_UserInAppNotifications_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_UserInAppNotifications_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_UserInAppNotifications_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id),
        CONSTRAINT UX_UserInAppNotifications_Key UNIQUE (GymId, UserId, NotificationKey)
    );
    CREATE INDEX IX_UserInAppNotifications_UserUnread
        ON dbo.UserInAppNotifications (GymId, UserId, IsRead, CreatedDate DESC);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UserInAppNotification_Create
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @NotificationKey NVARCHAR(200),
    @NotificationType NVARCHAR(80),
    @Title NVARCHAR(200),
    @Message NVARCHAR(1000),
    @Severity NVARCHAR(20),
    @ActionRoute NVARCHAR(300) = NULL,
    @ShowLoginPopup BIT = 0,
    @Created BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Created = 0;

    IF EXISTS (
        SELECT 1 FROM dbo.UserInAppNotifications
        WHERE GymId = @GymId AND UserId = @UserId AND NotificationKey = @NotificationKey)
        RETURN;

    INSERT INTO dbo.UserInAppNotifications
        (GymId, UserId, NotificationKey, NotificationType, Title, Message, Severity, ActionRoute, ShowLoginPopup)
    VALUES
        (@GymId, @UserId, @NotificationKey, @NotificationType, @Title, @Message, @Severity, @ActionRoute, @ShowLoginPopup);

    SET @Created = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UserInAppNotifications_GetForUser
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @UnreadOnly BIT = 0,
    @Top INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Top)
        Id, GymId, UserId, NotificationKey, NotificationType, Title, Message, Severity,
        ActionRoute, ShowLoginPopup, IsRead, ReadDate, CreatedDate
    FROM dbo.UserInAppNotifications
    WHERE GymId = @GymId AND UserId = @UserId
      AND (@UnreadOnly = 0 OR IsRead = 0)
    ORDER BY CreatedDate DESC, Id DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UserInAppNotifications_GetUnreadCount
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @UnreadCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @UnreadCount = COUNT(*)
    FROM dbo.UserInAppNotifications
    WHERE GymId = @GymId AND UserId = @UserId AND IsRead = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UserInAppNotifications_MarkRead
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @NotificationIds NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @NotificationIds IS NULL OR LTRIM(RTRIM(@NotificationIds)) = N''
    BEGIN
        UPDATE dbo.UserInAppNotifications
        SET IsRead = 1, ReadDate = SYSUTCDATETIME()
        WHERE GymId = @GymId AND UserId = @UserId AND IsRead = 0;
        RETURN;
    END

    UPDATE n
    SET IsRead = 1, ReadDate = SYSUTCDATETIME()
    FROM dbo.UserInAppNotifications n
    INNER JOIN STRING_SPLIT(@NotificationIds, N',') s ON TRY_CAST(s.value AS INT) = n.Id
    WHERE n.GymId = @GymId AND n.UserId = @UserId AND n.IsRead = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SubscriptionExpiry_GetGymTenantUsers
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT u.Id AS UserId, r.RoleName AS RoleName
    FROM dbo.Users u
    INNER JOIN dbo.UserRoles ur ON ur.UserId = u.Id
    INNER JOIN dbo.Roles r ON r.RoleId = ur.RoleId
    WHERE u.GymId = @GymId
      AND u.IsActive = 1
      AND r.RoleName IN (N'GymAdmin', N'Trainer', N'Member');
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SubscriptionExpiry_GetActiveSubscriptions
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH Latest AS (
        SELECT gs.*,
            ROW_NUMBER() OVER (PARTITION BY gs.GymId ORDER BY gs.GymSubscriptionId DESC) AS rn
        FROM dbo.GymSubscriptions gs
    )
    SELECT
        l.GymSubscriptionId, l.GymId, l.SaasPlanId, l.PlanName, l.Status, l.BillingCycle,
        l.Amount, l.StartDate, l.EndDate, l.TrialEndsAt, l.CurrentPeriodStart, l.CurrentPeriodEnd,
        l.GraceEndsAt, l.CancelAtPeriodEnd,
        sp.PlanCode, sp.MaxMembers, sp.MaxTrainers, sp.StorageLimitMb, sp.WhatsAppNotificationLimit,
        sp.MonthlyPrice, sp.YearlyPrice,
        CASE
            WHEN l.Status IN (N'Active', N'Trial') AND (l.CurrentPeriodEnd IS NULL OR l.CurrentPeriodEnd >= SYSUTCDATETIME()) THEN 1
            WHEN l.GraceEndsAt IS NOT NULL AND l.GraceEndsAt >= SYSUTCDATETIME() THEN 1
            ELSE 0
        END AS HasAccess
    FROM Latest l
    INNER JOIN dbo.SaasSubscriptionPlans sp ON sp.SaasPlanId = l.SaasPlanId
    WHERE l.rn = 1
      AND l.Status IN (N'Trial', N'Active', N'PastDue', N'Expired');
END
GO
