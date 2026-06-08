/*
  WhatsApp Notification Module
  Templates, gym settings, delivery logs, expiry reminder queue
*/

IF OBJECT_ID(N'dbo.NotificationTemplates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NotificationTemplates
    (
        TemplateId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        NotificationType NVARCHAR(50) NOT NULL,
        TemplateName NVARCHAR(100) NOT NULL,
        BodyTemplate NVARCHAR(MAX) NULL,
        VariablesJson NVARCHAR(MAX) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_NotificationTemplates_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_NotificationTemplates_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_NotificationTemplates_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE
    );
    CREATE INDEX IX_NotificationTemplates_GymId ON dbo.NotificationTemplates (GymId);
    CREATE UNIQUE INDEX UX_NotificationTemplates_Gym_Type ON dbo.NotificationTemplates (GymId, NotificationType);
END
GO

IF OBJECT_ID(N'dbo.NotificationSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NotificationSettings
    (
        SettingId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        NotificationType NVARCHAR(50) NOT NULL,
        IsEnabled BIT NOT NULL CONSTRAINT DF_NotificationSettings_IsEnabled DEFAULT (1),
        ProviderTemplateName NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_NotificationSettings_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_NotificationSettings_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE,
        CONSTRAINT UX_NotificationSettings_Gym_Type UNIQUE (GymId, NotificationType)
    );
END
GO

IF OBJECT_ID(N'dbo.NotificationLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NotificationLogs
    (
        LogId BIGINT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        NotificationType NVARCHAR(50) NOT NULL,
        TemplateId INT NULL,
        RecipientPhone NVARCHAR(20) NOT NULL,
        RecipientUserId UNIQUEIDENTIFIER NULL,
        MemberId INT NULL,
        WhatsAppTemplateName NVARCHAR(100) NOT NULL,
        VariablesJson NVARCHAR(MAX) NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_NotificationLogs_Status DEFAULT (N'Pending'),
        ErrorMessage NVARCHAR(500) NULL,
        ProviderMessageId NVARCHAR(100) NULL,
        ScheduledFor DATETIME2 NULL,
        SentAt DATETIME2 NULL,
        RelatedEntityType NVARCHAR(50) NULL,
        RelatedEntityId NVARCHAR(50) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_NotificationLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_NotificationLogs_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_NotificationLogs_Templates FOREIGN KEY (TemplateId) REFERENCES dbo.NotificationTemplates (TemplateId)
    );
    CREATE INDEX IX_NotificationLogs_GymId_CreatedAt ON dbo.NotificationLogs (GymId, CreatedAt DESC);
    CREATE INDEX IX_NotificationLogs_Status_Scheduled ON dbo.NotificationLogs (Status, ScheduledFor);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateNotificationTemplate
    @GymId UNIQUEIDENTIFIER,
    @NotificationType NVARCHAR(50),
    @TemplateName NVARCHAR(100),
    @BodyTemplate NVARCHAR(MAX) = NULL,
    @VariablesJson NVARCHAR(MAX) = NULL,
    @IsActive BIT = 1,
    @TemplateId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM dbo.NotificationTemplates WHERE GymId = @GymId AND NotificationType = @NotificationType)
            THROW 50070, 'A template already exists for this notification type.', 1;

        INSERT INTO dbo.NotificationTemplates (GymId, NotificationType, TemplateName, BodyTemplate, VariablesJson, IsActive, CreatedAt)
        VALUES (@GymId, @NotificationType, @TemplateName, @BodyTemplate, @VariablesJson, @IsActive, SYSUTCDATETIME());

        SET @TemplateId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateNotificationTemplate
    @TemplateId INT,
    @GymId UNIQUEIDENTIFIER,
    @NotificationType NVARCHAR(50),
    @TemplateName NVARCHAR(100),
    @BodyTemplate NVARCHAR(MAX) = NULL,
    @VariablesJson NVARCHAR(MAX) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.NotificationTemplates
        SET NotificationType = @NotificationType,
            TemplateName = @TemplateName,
            BodyTemplate = @BodyTemplate,
            VariablesJson = @VariablesJson,
            IsActive = @IsActive,
            UpdatedAt = SYSUTCDATETIME()
        WHERE TemplateId = @TemplateId AND GymId = @GymId;

        IF @@ROWCOUNT = 0
            THROW 50071, 'Notification template not found.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteNotificationTemplate
    @TemplateId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM dbo.NotificationTemplates WHERE TemplateId = @TemplateId AND GymId = @GymId;
        IF @@ROWCOUNT = 0
            THROW 50071, 'Notification template not found.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetNotificationTemplates
    @GymId UNIQUEIDENTIFIER,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TemplateId, GymId, NotificationType, TemplateName, BodyTemplate, VariablesJson, IsActive, CreatedAt, UpdatedAt
    FROM dbo.NotificationTemplates
    WHERE GymId = @GymId AND (@IncludeInactive = 1 OR IsActive = 1)
    ORDER BY NotificationType;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpsertNotificationSetting
    @GymId UNIQUEIDENTIFIER,
    @NotificationType NVARCHAR(50),
    @IsEnabled BIT,
    @ProviderTemplateName NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.NotificationSettings WHERE GymId = @GymId AND NotificationType = @NotificationType)
    BEGIN
        UPDATE dbo.NotificationSettings
        SET IsEnabled = @IsEnabled,
            ProviderTemplateName = @ProviderTemplateName,
            UpdatedAt = SYSUTCDATETIME()
        WHERE GymId = @GymId AND NotificationType = @NotificationType;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.NotificationSettings (GymId, NotificationType, IsEnabled, ProviderTemplateName, CreatedAt)
        VALUES (@GymId, @NotificationType, @IsEnabled, @ProviderTemplateName, SYSUTCDATETIME());
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetNotificationSettings
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SettingId, GymId, NotificationType, IsEnabled, ProviderTemplateName, CreatedAt, UpdatedAt
    FROM dbo.NotificationSettings
    WHERE GymId = @GymId
    ORDER BY NotificationType;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_LogNotification
    @GymId UNIQUEIDENTIFIER,
    @NotificationType NVARCHAR(50),
    @TemplateId INT = NULL,
    @RecipientPhone NVARCHAR(20),
    @RecipientUserId UNIQUEIDENTIFIER = NULL,
    @MemberId INT = NULL,
    @WhatsAppTemplateName NVARCHAR(100),
    @VariablesJson NVARCHAR(MAX) = NULL,
    @Status NVARCHAR(20) = N'Pending',
    @ErrorMessage NVARCHAR(500) = NULL,
    @ProviderMessageId NVARCHAR(100) = NULL,
    @ScheduledFor DATETIME2 = NULL,
    @SentAt DATETIME2 = NULL,
    @RelatedEntityType NVARCHAR(50) = NULL,
    @RelatedEntityId NVARCHAR(50) = NULL,
    @LogId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.NotificationLogs (
        GymId, NotificationType, TemplateId, RecipientPhone, RecipientUserId, MemberId,
        WhatsAppTemplateName, VariablesJson, Status, ErrorMessage, ProviderMessageId,
        ScheduledFor, SentAt, RelatedEntityType, RelatedEntityId, CreatedAt)
    VALUES (
        @GymId, @NotificationType, @TemplateId, @RecipientPhone, @RecipientUserId, @MemberId,
        @WhatsAppTemplateName, @VariablesJson, @Status, @ErrorMessage, @ProviderMessageId,
        @ScheduledFor, @SentAt, @RelatedEntityType, @RelatedEntityId, SYSUTCDATETIME());

    SET @LogId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateNotificationLogStatus
    @LogId BIGINT,
    @GymId UNIQUEIDENTIFIER,
    @Status NVARCHAR(20),
    @ErrorMessage NVARCHAR(500) = NULL,
    @ProviderMessageId NVARCHAR(100) = NULL,
    @SentAt DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.NotificationLogs
    SET Status = @Status,
        ErrorMessage = @ErrorMessage,
        ProviderMessageId = @ProviderMessageId,
        SentAt = COALESCE(@SentAt, SentAt)
    WHERE LogId = @LogId AND GymId = @GymId;

    IF @@ROWCOUNT = 0
        THROW 50072, 'Notification log not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SearchNotificationLogs
    @GymId UNIQUEIDENTIFIER,
    @Search NVARCHAR(200) = NULL,
    @NotificationType NVARCHAR(50) = NULL,
    @Status NVARCHAR(20) = NULL,
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

    SELECT @TotalCount = COUNT(*)
    FROM dbo.NotificationLogs nl
    WHERE nl.GymId = @GymId
      AND (@NotificationType IS NULL OR nl.NotificationType = @NotificationType)
      AND (@Status IS NULL OR nl.Status = @Status)
      AND (@FromDate IS NULL OR nl.CreatedAt >= @FromDate)
      AND (@ToDate IS NULL OR nl.CreatedAt < DATEADD(DAY, 1, CAST(@ToDate AS DATE)))
      AND (@SearchPattern IS NULL OR nl.RecipientPhone LIKE @SearchPattern OR nl.WhatsAppTemplateName LIKE @SearchPattern OR nl.ErrorMessage LIKE @SearchPattern);

    SELECT
        nl.LogId,
        nl.GymId,
        nl.NotificationType,
        nl.TemplateId,
        nl.RecipientPhone,
        nl.RecipientUserId,
        nl.MemberId,
        nl.WhatsAppTemplateName,
        nl.VariablesJson,
        nl.Status,
        nl.ErrorMessage,
        nl.ProviderMessageId,
        nl.ScheduledFor,
        nl.SentAt,
        nl.RelatedEntityType,
        nl.RelatedEntityId,
        nl.CreatedAt,
        u.Name AS RecipientName
    FROM dbo.NotificationLogs nl
    LEFT JOIN dbo.Users u ON u.Id = nl.RecipientUserId
    WHERE nl.GymId = @GymId
      AND (@NotificationType IS NULL OR nl.NotificationType = @NotificationType)
      AND (@Status IS NULL OR nl.Status = @Status)
      AND (@FromDate IS NULL OR nl.CreatedAt >= @FromDate)
      AND (@ToDate IS NULL OR nl.CreatedAt < DATEADD(DAY, 1, CAST(@ToDate AS DATE)))
      AND (@SearchPattern IS NULL OR nl.RecipientPhone LIKE @SearchPattern OR nl.WhatsAppTemplateName LIKE @SearchPattern OR nl.ErrorMessage LIKE @SearchPattern)
    ORDER BY nl.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPendingNotifications
    @BatchSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@BatchSize)
        nl.LogId,
        nl.GymId,
        nl.NotificationType,
        nl.TemplateId,
        nl.RecipientPhone,
        nl.RecipientUserId,
        nl.MemberId,
        nl.WhatsAppTemplateName,
        nl.VariablesJson,
        nl.Status,
        nl.ScheduledFor,
        nl.RelatedEntityType,
        nl.RelatedEntityId,
        nl.CreatedAt
    FROM dbo.NotificationLogs nl
    WHERE nl.Status = N'Pending'
      AND (nl.ScheduledFor IS NULL OR nl.ScheduledFor <= SYSUTCDATETIME())
    ORDER BY nl.ScheduledFor, nl.CreatedAt;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetNotificationDashboard
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.NotificationLogs WHERE GymId = @GymId) AS TotalLogs,
        (SELECT COUNT(*) FROM dbo.NotificationLogs WHERE GymId = @GymId AND Status = N'Sent') AS SentCount,
        (SELECT COUNT(*) FROM dbo.NotificationLogs WHERE GymId = @GymId AND Status = N'Failed') AS FailedCount,
        (SELECT COUNT(*) FROM dbo.NotificationLogs WHERE GymId = @GymId AND Status = N'Pending') AS PendingCount,
        (SELECT COUNT(*) FROM dbo.NotificationTemplates WHERE GymId = @GymId AND IsActive = 1) AS ActiveTemplates,
        (SELECT COUNT(*) FROM dbo.NotificationLogs WHERE GymId = @GymId AND CAST(CreatedAt AS DATE) = CAST(SYSUTCDATETIME() AS DATE)) AS SentToday;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMembershipsExpiringForNotification
    @GymId UNIQUEIDENTIFIER,
    @DaysBeforeExpiry INT,
    @NotificationType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TargetDate DATE = DATEADD(DAY, @DaysBeforeExpiry, CAST(SYSUTCDATETIME() AS DATE));

    SELECT
        ms.MembershipId,
        ms.GymId,
        ms.MemberId,
        ms.EndDate,
        m.UserId AS RecipientUserId,
        u.Name AS MemberName,
        u.Email AS MemberEmail,
        m.Phone AS MemberPhone,
        mp.PlanName
    FROM dbo.Memberships ms
    INNER JOIN dbo.Members m ON m.MemberId = ms.MemberId AND m.IsDeleted = 0
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
    WHERE ms.GymId = @GymId
      AND ms.Status <> N'Cancelled'
      AND CAST(ms.EndDate AS DATE) = @TargetDate
      AND m.Phone IS NOT NULL AND LEN(LTRIM(RTRIM(m.Phone))) > 0
      AND NOT EXISTS (
          SELECT 1 FROM dbo.NotificationLogs nl
          WHERE nl.GymId = @GymId
            AND nl.NotificationType = @NotificationType
            AND nl.MemberId = ms.MemberId
            AND nl.RelatedEntityId = CAST(ms.MembershipId AS NVARCHAR(50))
            AND CAST(nl.CreatedAt AS DATE) = CAST(SYSUTCDATETIME() AS DATE)
      );
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllActiveGymIds
AS
BEGIN
    SET NOCOUNT ON;
    SELECT GymId FROM dbo.Gyms WHERE IsActive = 1;
END
GO

-- Seed default settings for existing gyms (all types enabled)
INSERT INTO dbo.NotificationSettings (GymId, NotificationType, IsEnabled, CreatedAt)
SELECT g.GymId, t.NotificationType, 1, SYSUTCDATETIME()
FROM dbo.Gyms g
CROSS JOIN (VALUES
    (N'MembershipExpiry7Days'),
    (N'MembershipExpiry3Days'),
    (N'MembershipExpiryToday'),
    (N'PaymentSuccess'),
    (N'MembershipRenewal'),
    (N'NewMemberRegistration'),
    (N'WorkoutPlanAssigned'),
    (N'DietPlanAssigned')
) AS t(NotificationType)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.NotificationSettings ns
    WHERE ns.GymId = g.GymId AND ns.NotificationType = t.NotificationType
);
GO
