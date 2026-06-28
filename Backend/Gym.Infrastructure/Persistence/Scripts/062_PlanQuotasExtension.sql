-- 062_PlanQuotasExtension.sql
-- Extend PlanQuotas with MaxBranches, MaxStorageGB, MaxSmsPerMonth, MaxWhatsappMessages.
-- Legacy StorageLimitMb and WhatsAppNotificationLimit columns are retained.

IF COL_LENGTH('dbo.PlanQuotas', 'MaxBranches') IS NULL
    ALTER TABLE dbo.PlanQuotas ADD MaxBranches INT NOT NULL
        CONSTRAINT DF_PlanQuotas_MaxBranches DEFAULT (1) WITH VALUES;
GO

IF COL_LENGTH('dbo.PlanQuotas', 'MaxStorageGB') IS NULL
    ALTER TABLE dbo.PlanQuotas ADD MaxStorageGB INT NOT NULL
        CONSTRAINT DF_PlanQuotas_MaxStorageGB DEFAULT (0) WITH VALUES;
GO

IF COL_LENGTH('dbo.PlanQuotas', 'MaxSmsPerMonth') IS NULL
    ALTER TABLE dbo.PlanQuotas ADD MaxSmsPerMonth INT NOT NULL
        CONSTRAINT DF_PlanQuotas_MaxSmsPerMonth DEFAULT (0) WITH VALUES;
GO

IF COL_LENGTH('dbo.PlanQuotas', 'MaxWhatsappMessages') IS NULL
    ALTER TABLE dbo.PlanQuotas ADD MaxWhatsappMessages INT NOT NULL
        CONSTRAINT DF_PlanQuotas_MaxWhatsappMessages DEFAULT (0) WITH VALUES;
GO

UPDATE pq
SET
    MaxStorageGB = CASE
        WHEN pq.MaxStorageGB > 0 THEN pq.MaxStorageGB
        WHEN pq.StorageLimitMb <= 0 THEN 0
        ELSE (pq.StorageLimitMb + 1023) / 1024
    END,
    MaxWhatsappMessages = CASE
        WHEN pq.MaxWhatsappMessages > 0 THEN pq.MaxWhatsappMessages
        ELSE pq.WhatsAppNotificationLimit
    END,
    MaxBranches = CASE WHEN pq.MaxBranches <= 0 THEN 1 ELSE pq.MaxBranches END,
    UpdatedAt = SYSUTCDATETIME()
FROM dbo.PlanQuotas pq;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PlanQuota_GetByPlanId
    @SaasPlanId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        PlanQuotaId,
        SaasPlanId,
        MaxMembers,
        MaxTrainers,
        MaxBranches,
        MaxStorageGB,
        MaxSmsPerMonth,
        MaxWhatsappMessages,
        StorageLimitMb,
        WhatsAppNotificationLimit
    FROM dbo.PlanQuotas
    WHERE SaasPlanId = @SaasPlanId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_CheckTenantLimit
    @GymId UNIQUEIDENTIFIER,
    @ResourceType NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MaxMembers INT = 0;
    DECLARE @MaxTrainers INT = 0;
    DECLARE @MaxBranches INT = 0;
    DECLARE @MaxStorageGB INT = 0;
    DECLARE @MaxSmsPerMonth INT = 0;
    DECLARE @MaxWhatsappMessages INT = 0;
    DECLARE @MemberCount INT = 0;
    DECLARE @TrainerCount INT = 0;
    DECLARE @BranchCount INT = 0;
    DECLARE @StorageUsedBytes BIGINT = 0;
    DECLARE @SmsSentThisMonth INT = 0;
    DECLARE @WhatsAppSentThisMonth INT = 0;
    DECLARE @HasAccess BIT = 0;
    DECLARE @PlanName NVARCHAR(100) = N'';

    SELECT TOP 1
        @PlanName = sp.PlanName,
        @MaxMembers = COALESCE(pq.MaxMembers, sp.MaxMembers),
        @MaxTrainers = COALESCE(pq.MaxTrainers, sp.MaxTrainers),
        @MaxBranches = COALESCE(pq.MaxBranches, 1),
        @MaxStorageGB = COALESCE(pq.MaxStorageGB, CASE WHEN COALESCE(pq.StorageLimitMb, sp.StorageLimitMb) <= 0 THEN 0 ELSE (COALESCE(pq.StorageLimitMb, sp.StorageLimitMb) + 1023) / 1024 END),
        @MaxSmsPerMonth = COALESCE(pq.MaxSmsPerMonth, 0),
        @MaxWhatsappMessages = COALESCE(pq.MaxWhatsappMessages, COALESCE(pq.WhatsAppNotificationLimit, sp.WhatsAppNotificationLimit)),
        @HasAccess = CASE
            WHEN gs.Status IN (N'Active', N'Trial') AND (gs.CurrentPeriodEnd IS NULL OR gs.CurrentPeriodEnd >= SYSUTCDATETIME()) THEN 1
            WHEN gs.GraceEndsAt IS NOT NULL AND gs.GraceEndsAt >= SYSUTCDATETIME() THEN 1
            ELSE 0
        END
    FROM dbo.GymSubscriptions gs
    INNER JOIN dbo.SaasSubscriptionPlans sp ON sp.SaasPlanId = gs.SaasPlanId
    LEFT JOIN dbo.PlanQuotas pq ON pq.SaasPlanId = sp.SaasPlanId
    WHERE gs.GymId = @GymId
    ORDER BY gs.GymSubscriptionId DESC;

    SELECT @MemberCount = COUNT(*)
    FROM dbo.Members m
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE m.GymId = @GymId AND u.IsActive = 1;

    SELECT @TrainerCount = COUNT(*)
    FROM dbo.Trainers t
    INNER JOIN dbo.Users u ON u.Id = t.UserId
    WHERE t.GymId = @GymId AND u.IsActive = 1;

    SELECT @BranchCount = COUNT(*)
    FROM dbo.Branches b
    WHERE b.GymId = @GymId AND b.IsActive = 1;

    SELECT @StorageUsedBytes = ISNULL(SUM(FileSizeBytes), 0)
    FROM dbo.Files
    WHERE GymId = @GymId AND IsDeleted = 0;

    SELECT @WhatsAppSentThisMonth = COUNT(*)
    FROM dbo.NotificationLogs
    WHERE GymId = @GymId
      AND Status = N'Sent'
      AND CreatedAt >= DATEFROMPARTS(YEAR(SYSUTCDATETIME()), MONTH(SYSUTCDATETIME()), 1);

    SET @SmsSentThisMonth = 0;
    IF COL_LENGTH('dbo.NotificationLogs', 'Channel') IS NOT NULL
    BEGIN
        DECLARE @SmsSql NVARCHAR(500) = N'
            SELECT @SmsSentThisMonthOut = COUNT(*)
            FROM dbo.NotificationLogs
            WHERE GymId = @GymId
              AND Status = N''Sent''
              AND Channel = N''SMS''
              AND CreatedAt >= DATEFROMPARTS(YEAR(SYSUTCDATETIME()), MONTH(SYSUTCDATETIME()), 1);';
        EXEC sp_executesql
            @SmsSql,
            N'@GymId UNIQUEIDENTIFIER, @SmsSentThisMonthOut INT OUTPUT',
            @GymId = @GymId,
            @SmsSentThisMonthOut = @SmsSentThisMonth OUTPUT;
    END

    SELECT
        @HasAccess AS HasAccess,
        @PlanName AS PlanName,
        @MaxMembers AS MaxMembers,
        @MaxTrainers AS MaxTrainers,
        @MaxBranches AS MaxBranches,
        @MaxStorageGB AS MaxStorageGB,
        @MaxSmsPerMonth AS MaxSmsPerMonth,
        @MaxWhatsappMessages AS MaxWhatsappMessages,
        @MemberCount AS CurrentMembers,
        @TrainerCount AS CurrentTrainers,
        @BranchCount AS CurrentBranches,
        @StorageUsedBytes AS StorageUsedBytes,
        @SmsSentThisMonth AS SmsSentThisMonth,
        @WhatsAppSentThisMonth AS WhatsAppSentThisMonth,
        CASE WHEN @ResourceType = N'Member' AND @MaxMembers >= 0 AND @MemberCount >= @MaxMembers THEN 1 ELSE 0 END AS MemberLimitReached,
        CASE WHEN @ResourceType = N'Trainer' AND @MaxTrainers >= 0 AND @TrainerCount >= @MaxTrainers THEN 1 ELSE 0 END AS TrainerLimitReached,
        CASE WHEN @ResourceType = N'Branch' AND @MaxBranches >= 0 AND @BranchCount >= @MaxBranches THEN 1 ELSE 0 END AS BranchLimitReached,
        CASE WHEN @ResourceType = N'Storage' AND @MaxStorageGB > 0 AND @StorageUsedBytes >= CAST(@MaxStorageGB AS BIGINT) * 1073741824 THEN 1 ELSE 0 END AS StorageLimitReached,
        CASE WHEN @ResourceType = N'Sms' AND @MaxSmsPerMonth > 0 AND @SmsSentThisMonth >= @MaxSmsPerMonth THEN 1 ELSE 0 END AS SmsLimitReached,
        CASE WHEN @ResourceType = N'WhatsApp' AND @MaxWhatsappMessages > 0 AND @WhatsAppSentThisMonth >= @MaxWhatsappMessages THEN 1 ELSE 0 END AS WhatsAppLimitReached;
END
GO
