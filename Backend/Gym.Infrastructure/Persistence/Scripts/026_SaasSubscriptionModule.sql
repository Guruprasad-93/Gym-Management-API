/*
  SaaS Subscription & Gym Onboarding Module
  Plans, subscriptions, billing, branding, tenant limits, platform metrics
*/

/* ========== GYM BRANDING COLUMNS ========== */
IF COL_LENGTH('dbo.Gyms', 'PrimaryColor') IS NULL
    ALTER TABLE dbo.Gyms ADD PrimaryColor NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.Gyms', 'SecondaryColor') IS NULL
    ALTER TABLE dbo.Gyms ADD SecondaryColor NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.Gyms', 'BannerFileId') IS NULL
    ALTER TABLE dbo.Gyms ADD BannerFileId BIGINT NULL;
IF COL_LENGTH('dbo.Gyms', 'ReceiptHeaderText') IS NULL
    ALTER TABLE dbo.Gyms ADD ReceiptHeaderText NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.Gyms', 'InvoiceFooterText') IS NULL
    ALTER TABLE dbo.Gyms ADD InvoiceFooterText NVARCHAR(500) NULL;
GO

IF OBJECT_ID(N'dbo.FK_Gyms_BannerFile', N'F') IS NULL AND COL_LENGTH('dbo.Gyms', 'BannerFileId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Gyms ADD CONSTRAINT FK_Gyms_BannerFile FOREIGN KEY (BannerFileId) REFERENCES dbo.Files (FileId);
END
GO

/* ========== SAAS PLANS ========== */
IF OBJECT_ID(N'dbo.SaasSubscriptionPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SaasSubscriptionPlans
    (
        SaasPlanId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        PlanCode NVARCHAR(50) NOT NULL,
        PlanName NVARCHAR(100) NOT NULL,
        MaxMembers INT NOT NULL,
        MaxTrainers INT NOT NULL,
        StorageLimitMb INT NOT NULL,
        WhatsAppNotificationLimit INT NOT NULL,
        MonthlyPrice DECIMAL(18, 2) NOT NULL,
        YearlyPrice DECIMAL(18, 2) NOT NULL,
        TrialDays INT NOT NULL CONSTRAINT DF_SaasSubscriptionPlans_TrialDays DEFAULT (0),
        IsActive BIT NOT NULL CONSTRAINT DF_SaasSubscriptionPlans_IsActive DEFAULT (1),
        SortOrder INT NOT NULL CONSTRAINT DF_SaasSubscriptionPlans_SortOrder DEFAULT (0),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_SaasSubscriptionPlans_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT UX_SaasSubscriptionPlans_PlanCode UNIQUE (PlanCode)
    );
END
GO

/* ========== EXTEND GYM SUBSCRIPTIONS ========== */
IF COL_LENGTH('dbo.GymSubscriptions', 'SaasPlanId') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD SaasPlanId INT NULL;
IF COL_LENGTH('dbo.GymSubscriptions', 'BillingCycle') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD BillingCycle NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.GymSubscriptions', 'TrialEndsAt') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD TrialEndsAt DATETIME2 NULL;
IF COL_LENGTH('dbo.GymSubscriptions', 'CurrentPeriodStart') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD CurrentPeriodStart DATETIME2 NULL;
IF COL_LENGTH('dbo.GymSubscriptions', 'CurrentPeriodEnd') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD CurrentPeriodEnd DATETIME2 NULL;
IF COL_LENGTH('dbo.GymSubscriptions', 'GraceEndsAt') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD GraceEndsAt DATETIME2 NULL;
IF COL_LENGTH('dbo.GymSubscriptions', 'RazorpayOrderId') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD RazorpayOrderId NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.GymSubscriptions', 'RazorpayPaymentId') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD RazorpayPaymentId NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.GymSubscriptions', 'RazorpaySubscriptionId') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD RazorpaySubscriptionId NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.GymSubscriptions', 'CancelledAt') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD CancelledAt DATETIME2 NULL;
IF COL_LENGTH('dbo.GymSubscriptions', 'CancelAtPeriodEnd') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD CancelAtPeriodEnd BIT NOT NULL CONSTRAINT DF_GymSubscriptions_CancelAtPeriodEnd DEFAULT (0);
GO

IF OBJECT_ID(N'dbo.FK_GymSubscriptions_SaasPlan', N'F') IS NULL AND COL_LENGTH('dbo.GymSubscriptions', 'SaasPlanId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.GymSubscriptions ADD CONSTRAINT FK_GymSubscriptions_SaasPlan
        FOREIGN KEY (SaasPlanId) REFERENCES dbo.SaasSubscriptionPlans (SaasPlanId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GymSubscriptions_GymId_Status' AND object_id = OBJECT_ID(N'dbo.GymSubscriptions'))
    CREATE INDEX IX_GymSubscriptions_GymId_Status ON dbo.GymSubscriptions (GymId, Status);
GO

/* ========== SAAS BILLING PAYMENTS ========== */
IF OBJECT_ID(N'dbo.SaasSubscriptionPayments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SaasSubscriptionPayments
    (
        SaasPaymentId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        GymSubscriptionId INT NOT NULL,
        SaasPlanId INT NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        BillingCycle NVARCHAR(20) NOT NULL,
        RazorpayOrderId NVARCHAR(100) NULL,
        RazorpayPaymentId NVARCHAR(100) NULL,
        Status NVARCHAR(20) NOT NULL,
        PaidAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_SaasSubscriptionPayments_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_SaasSubscriptionPayments_Gym FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_SaasSubscriptionPayments_Sub FOREIGN KEY (GymSubscriptionId) REFERENCES dbo.GymSubscriptions (GymSubscriptionId),
        CONSTRAINT FK_SaasSubscriptionPayments_Plan FOREIGN KEY (SaasPlanId) REFERENCES dbo.SaasSubscriptionPlans (SaasPlanId)
    );
    CREATE INDEX IX_SaasSubscriptionPayments_GymId ON dbo.SaasSubscriptionPayments (GymId, CreatedAt DESC);
END
GO

/* ========== SEED PLANS ========== */
MERGE dbo.SaasSubscriptionPlans AS t
USING (VALUES
    (N'Trial',      N'Trial Plan',      50,  5,  512,  100,   0.00,    0.00,   15, 1),
    (N'Basic',      N'Basic Plan',     200, 10, 2048,  500, 999.00, 9990.00,  0, 2),
    (N'Premium',    N'Premium Plan',   500, 25, 5120, 2000, 2499.00,24990.00, 0, 3),
    (N'Enterprise', N'Enterprise Plan',-1, -1,10240,10000, 4999.00,49990.00, 0, 4)
) AS s (PlanCode, PlanName, MaxMembers, MaxTrainers, StorageLimitMb, WhatsAppNotificationLimit, MonthlyPrice, YearlyPrice, TrialDays, SortOrder)
ON t.PlanCode = s.PlanCode
WHEN NOT MATCHED THEN
    INSERT (PlanCode, PlanName, MaxMembers, MaxTrainers, StorageLimitMb, WhatsAppNotificationLimit, MonthlyPrice, YearlyPrice, TrialDays, SortOrder)
    VALUES (s.PlanCode, s.PlanName, s.MaxMembers, s.MaxTrainers, s.StorageLimitMb, s.WhatsAppNotificationLimit, s.MonthlyPrice, s.YearlyPrice, s.TrialDays, s.SortOrder);
GO

/* ========== GYM SP UPDATES ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetGymById
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT GymId, Name, Address, Phone, Email, LogoUrl, IsActive, CreatedAt, UpdatedAt,
           PrimaryColor, SecondaryColor, BannerFileId, ReceiptHeaderText, InvoiceFooterText
    FROM dbo.Gyms WHERE GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllGyms
AS
BEGIN
    SET NOCOUNT ON;
    SELECT GymId, Name, Address, Phone, Email, LogoUrl, IsActive, CreatedAt, UpdatedAt,
           PrimaryColor, SecondaryColor, BannerFileId, ReceiptHeaderText, InvoiceFooterText
    FROM dbo.Gyms ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Gym_UpdateBranding
    @GymId UNIQUEIDENTIFIER,
    @PrimaryColor NVARCHAR(20) = NULL,
    @SecondaryColor NVARCHAR(20) = NULL,
    @BannerFileId BIGINT = NULL,
    @ReceiptHeaderText NVARCHAR(500) = NULL,
    @InvoiceFooterText NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Gyms WHERE GymId = @GymId)
        THROW 50011, 'Gym not found.', 1;

    UPDATE dbo.Gyms
    SET PrimaryColor = @PrimaryColor,
        SecondaryColor = @SecondaryColor,
        BannerFileId = @BannerFileId,
        ReceiptHeaderText = @ReceiptHeaderText,
        InvoiceFooterText = @InvoiceFooterText,
        UpdatedAt = SYSUTCDATETIME()
    WHERE GymId = @GymId;
END
GO

/* ========== SAAS PLANS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetAllPlans
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SaasPlanId, PlanCode, PlanName, MaxMembers, MaxTrainers, StorageLimitMb,
           WhatsAppNotificationLimit, MonthlyPrice, YearlyPrice, TrialDays, IsActive, SortOrder
    FROM dbo.SaasSubscriptionPlans
    WHERE IsActive = 1
    ORDER BY SortOrder;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetPlanById
    @SaasPlanId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SaasPlanId, PlanCode, PlanName, MaxMembers, MaxTrainers, StorageLimitMb,
           WhatsAppNotificationLimit, MonthlyPrice, YearlyPrice, TrialDays, IsActive, SortOrder
    FROM dbo.SaasSubscriptionPlans WHERE SaasPlanId = @SaasPlanId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetPlanByCode
    @PlanCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SaasPlanId, PlanCode, PlanName, MaxMembers, MaxTrainers, StorageLimitMb,
           WhatsAppNotificationLimit, MonthlyPrice, YearlyPrice, TrialDays, IsActive, SortOrder
    FROM dbo.SaasSubscriptionPlans WHERE PlanCode = @PlanCode AND IsActive = 1;
END
GO

/* ========== SUBSCRIPTION CRUD ========== */
CREATE OR ALTER PROCEDURE dbo.sp_Saas_CreateTrialSubscription
    @GymId UNIQUEIDENTIFIER,
    @GracePeriodDays INT = 3,
    @GymSubscriptionId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @PlanId INT, @TrialDays INT;
    SELECT @PlanId = SaasPlanId, @TrialDays = TrialDays FROM dbo.SaasSubscriptionPlans WHERE PlanCode = N'Trial';

    IF @PlanId IS NULL
        THROW 50050, 'Trial plan not configured.', 1;

    IF @TrialDays < 1 SET @TrialDays = 15;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    DECLARE @TrialEnd DATETIME2 = DATEADD(DAY, @TrialDays, @Now);

    INSERT INTO dbo.GymSubscriptions
        (GymId, SaasPlanId, PlanName, StartDate, EndDate, Amount, Status, BillingCycle,
         TrialEndsAt, CurrentPeriodStart, CurrentPeriodEnd, GraceEndsAt, CreatedAt)
    VALUES
        (@GymId, @PlanId, N'Trial Plan', CAST(@Now AS DATE), CAST(@TrialEnd AS DATE), 0,
         N'Trial', N'Trial', @TrialEnd, @Now, @TrialEnd, DATEADD(DAY, @GracePeriodDays, @TrialEnd), @Now);

    SET @GymSubscriptionId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetGymSubscription
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1
        gs.GymSubscriptionId, gs.GymId, gs.SaasPlanId, gs.PlanName, gs.StartDate, gs.EndDate,
        gs.Amount, gs.Status, gs.BillingCycle, gs.TrialEndsAt, gs.CurrentPeriodStart, gs.CurrentPeriodEnd,
        gs.GraceEndsAt, gs.RazorpayOrderId, gs.RazorpayPaymentId, gs.RazorpaySubscriptionId,
        gs.CancelledAt, gs.CancelAtPeriodEnd, gs.CreatedAt,
        sp.PlanCode, sp.MaxMembers, sp.MaxTrainers, sp.StorageLimitMb, sp.WhatsAppNotificationLimit,
        sp.MonthlyPrice, sp.YearlyPrice,
        CASE WHEN gs.Status = N'Trial' AND gs.TrialEndsAt IS NOT NULL
             THEN CASE WHEN gs.TrialEndsAt > SYSUTCDATETIME() THEN DATEDIFF(DAY, SYSUTCDATETIME(), gs.TrialEndsAt) + 1 ELSE 0 END
             ELSE NULL END AS RemainingTrialDays,
        CASE
            WHEN gs.Status IN (N'Active', N'Trial') AND (gs.CurrentPeriodEnd IS NULL OR gs.CurrentPeriodEnd >= SYSUTCDATETIME()) THEN 1
            WHEN gs.GraceEndsAt IS NOT NULL AND gs.GraceEndsAt >= SYSUTCDATETIME() THEN 1
            ELSE 0
        END AS HasAccess
    FROM dbo.GymSubscriptions gs
    INNER JOIN dbo.SaasSubscriptionPlans sp ON sp.SaasPlanId = gs.SaasPlanId
    WHERE gs.GymId = @GymId
    ORDER BY gs.GymSubscriptionId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetGymUsage
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.Members m INNER JOIN dbo.Users u ON u.Id = m.UserId WHERE m.GymId = @GymId AND u.IsActive = 1) AS MemberCount,
        (SELECT COUNT(*) FROM dbo.Trainers t INNER JOIN dbo.Users u ON u.Id = t.UserId WHERE t.GymId = @GymId AND u.IsActive = 1) AS TrainerCount,
        ISNULL((SELECT SUM(FileSizeBytes) FROM dbo.Files WHERE GymId = @GymId AND IsDeleted = 0), 0) AS StorageUsedBytes,
        (SELECT COUNT(*) FROM dbo.NotificationLogs WHERE GymId = @GymId AND Status = N'Sent'
            AND CreatedAt >= DATEFROMPARTS(YEAR(SYSUTCDATETIME()), MONTH(SYSUTCDATETIME()), 1)) AS WhatsAppSentThisMonth;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_CheckTenantLimit
    @GymId UNIQUEIDENTIFIER,
    @ResourceType NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @MaxMembers INT, @MaxTrainers INT, @MemberCount INT, @TrainerCount INT, @HasAccess BIT = 0, @PlanName NVARCHAR(100);

    SELECT TOP 1 @MaxMembers = sp.MaxMembers, @MaxTrainers = sp.MaxTrainers, @PlanName = sp.PlanName,
        @HasAccess = CASE
            WHEN gs.Status IN (N'Active', N'Trial') AND (gs.CurrentPeriodEnd IS NULL OR gs.CurrentPeriodEnd >= SYSUTCDATETIME()) THEN 1
            WHEN gs.GraceEndsAt IS NOT NULL AND gs.GraceEndsAt >= SYSUTCDATETIME() THEN 1
            ELSE 0 END
    FROM dbo.GymSubscriptions gs
    INNER JOIN dbo.SaasSubscriptionPlans sp ON sp.SaasPlanId = gs.SaasPlanId
    WHERE gs.GymId = @GymId
    ORDER BY gs.GymSubscriptionId DESC;

    SELECT @MemberCount = COUNT(*) FROM dbo.Members m INNER JOIN dbo.Users u ON u.Id = m.UserId WHERE m.GymId = @GymId AND u.IsActive = 1;
    SELECT @TrainerCount = COUNT(*) FROM dbo.Trainers t INNER JOIN dbo.Users u ON u.Id = t.UserId WHERE t.GymId = @GymId AND u.IsActive = 1;

    SELECT
        @HasAccess AS HasAccess,
        @PlanName AS PlanName,
        @MaxMembers AS MaxMembers,
        @MaxTrainers AS MaxTrainers,
        @MemberCount AS CurrentMembers,
        @TrainerCount AS CurrentTrainers,
        CASE WHEN @ResourceType = N'Member' AND @MaxMembers >= 0 AND @MemberCount >= @MaxMembers THEN 1 ELSE 0 END AS MemberLimitReached,
        CASE WHEN @ResourceType = N'Trainer' AND @MaxTrainers >= 0 AND @TrainerCount >= @MaxTrainers THEN 1 ELSE 0 END AS TrainerLimitReached;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_UpdateSubscriptionPlan
    @GymId UNIQUEIDENTIFIER,
    @SaasPlanId INT,
    @BillingCycle NVARCHAR(20),
    @Amount DECIMAL(18, 2),
    @RazorpayOrderId NVARCHAR(100) = NULL,
    @RazorpayPaymentId NVARCHAR(100) = NULL,
    @RazorpaySubscriptionId NVARCHAR(100) = NULL,
    @GracePeriodDays INT = 3
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @PlanName NVARCHAR(100), @Now DATETIME2 = SYSUTCDATETIME();
    SELECT @PlanName = PlanName FROM dbo.SaasSubscriptionPlans WHERE SaasPlanId = @SaasPlanId;
    IF @PlanName IS NULL THROW 50051, 'Plan not found.', 1;

    DECLARE @PeriodEnd DATETIME2 = CASE WHEN @BillingCycle = N'Yearly' THEN DATEADD(YEAR, 1, @Now) ELSE DATEADD(MONTH, 1, @Now) END;

    UPDATE dbo.GymSubscriptions
    SET Status = N'Cancelled', CancelledAt = @Now, UpdatedAt = @Now
    WHERE GymId = @GymId AND Status IN (N'Trial', N'Active', N'PastDue') AND CancelledAt IS NULL;

    INSERT INTO dbo.GymSubscriptions
        (GymId, SaasPlanId, PlanName, StartDate, EndDate, Amount, Status, BillingCycle,
         TrialEndsAt, CurrentPeriodStart, CurrentPeriodEnd, GraceEndsAt,
         RazorpayOrderId, RazorpayPaymentId, RazorpaySubscriptionId, CreatedAt)
    VALUES
        (@GymId, @SaasPlanId, @PlanName, CAST(@Now AS DATE), CAST(@PeriodEnd AS DATE), @Amount, N'Active', @BillingCycle,
         NULL, @Now, @PeriodEnd, DATEADD(DAY, @GracePeriodDays, @PeriodEnd),
         @RazorpayOrderId, @RazorpayPaymentId, @RazorpaySubscriptionId, @Now);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_CancelSubscription
    @GymId UNIQUEIDENTIFIER,
    @CancelAtPeriodEnd BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.GymSubscriptions
    SET CancelAtPeriodEnd = @CancelAtPeriodEnd,
        CancelledAt = CASE WHEN @CancelAtPeriodEnd = 0 THEN SYSUTCDATETIME() ELSE CancelledAt END,
        Status = CASE WHEN @CancelAtPeriodEnd = 0 THEN N'Cancelled' ELSE Status END,
        UpdatedAt = SYSUTCDATETIME()
    WHERE GymSubscriptionId = (
        SELECT TOP 1 GymSubscriptionId FROM dbo.GymSubscriptions
        WHERE GymId = @GymId AND Status IN (N'Trial', N'Active', N'PastDue')
        ORDER BY GymSubscriptionId DESC);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_CreatePendingPayment
    @GymId UNIQUEIDENTIFIER,
    @GymSubscriptionId INT,
    @SaasPlanId INT,
    @Amount DECIMAL(18, 2),
    @BillingCycle NVARCHAR(20),
    @RazorpayOrderId NVARCHAR(100),
    @SaasPaymentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.SaasSubscriptionPayments
        (GymId, GymSubscriptionId, SaasPlanId, Amount, BillingCycle, RazorpayOrderId, Status)
    VALUES (@GymId, @GymSubscriptionId, @SaasPlanId, @Amount, @BillingCycle, @RazorpayOrderId, N'Pending');
    SET @SaasPaymentId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_CompletePayment
    @SaasPaymentId INT,
    @RazorpayPaymentId NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.SaasSubscriptionPayments
    SET Status = N'Completed', RazorpayPaymentId = @RazorpayPaymentId, PaidAt = SYSUTCDATETIME()
    WHERE SaasPaymentId = @SaasPaymentId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetPlatformDashboard
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH LatestSubs AS (
        SELECT gs.*, sp.MonthlyPrice, sp.YearlyPrice,
            ROW_NUMBER() OVER (PARTITION BY gs.GymId ORDER BY gs.GymSubscriptionId DESC) AS rn
        FROM dbo.GymSubscriptions gs
        INNER JOIN dbo.SaasSubscriptionPlans sp ON sp.SaasPlanId = gs.SaasPlanId
    )
    SELECT
        (SELECT COUNT(*) FROM dbo.Gyms) AS TotalGyms,
        (SELECT COUNT(*) FROM dbo.Gyms WHERE IsActive = 1) AS ActiveGyms,
        (SELECT COUNT(*) FROM LatestSubs WHERE rn = 1 AND Status = N'Active') AS ActiveSubscriptions,
        (SELECT COUNT(*) FROM LatestSubs WHERE rn = 1 AND Status IN (N'Cancelled', N'Expired')
            OR (CurrentPeriodEnd < SYSUTCDATETIME() AND GraceEndsAt < SYSUTCDATETIME())) AS ExpiredSubscriptions,
        (SELECT COUNT(*) FROM LatestSubs WHERE rn = 1 AND Status = N'Trial') AS TrialSubscriptions,
        ISNULL((SELECT SUM(CASE WHEN BillingCycle = N'Yearly' THEN YearlyPrice / 12.0 ELSE MonthlyPrice END)
            FROM LatestSubs WHERE rn = 1 AND Status = N'Active'), 0) AS MonthlyRecurringRevenue,
        ISNULL((SELECT SUM(CASE WHEN BillingCycle = N'Yearly' THEN YearlyPrice ELSE MonthlyPrice * 12 END)
            FROM LatestSubs WHERE rn = 1 AND Status = N'Active'), 0) AS AnnualRecurringRevenue;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_ExpireSubscriptions
    @GracePeriodDays INT = 3
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE gs SET Status = N'Expired', UpdatedAt = SYSUTCDATETIME()
    FROM dbo.GymSubscriptions gs
    WHERE gs.Status IN (N'Trial', N'Active', N'PastDue')
      AND gs.GraceEndsAt IS NOT NULL AND gs.GraceEndsAt < SYSUTCDATETIME();

    UPDATE g SET IsActive = 0, UpdatedAt = SYSUTCDATETIME()
    FROM dbo.Gyms g
    WHERE EXISTS (
        SELECT 1 FROM dbo.GymSubscriptions gs
        WHERE gs.GymId = g.GymId
          AND gs.GymSubscriptionId = (SELECT TOP 1 GymSubscriptionId FROM dbo.GymSubscriptions WHERE GymId = g.GymId ORDER BY GymSubscriptionId DESC)
          AND gs.Status = N'Expired'
          AND (gs.GraceEndsAt IS NULL OR gs.GraceEndsAt < SYSUTCDATETIME())
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetPendingPayment
    @SaasPaymentId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.SaasPaymentId, p.GymId, p.GymSubscriptionId, p.SaasPlanId, p.Amount, p.BillingCycle,
           p.RazorpayOrderId, p.Status, sp.PlanName
    FROM dbo.SaasSubscriptionPayments p
    INNER JOIN dbo.SaasSubscriptionPlans sp ON sp.SaasPlanId = p.SaasPlanId
    WHERE p.SaasPaymentId = @SaasPaymentId;
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
        (N'WorkoutPlanAssigned'), (N'DietPlanAssigned'), (N'GymOwnerWelcome')
    ) AS nt(NotificationType)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.NotificationSettings ns
        WHERE ns.GymId = @GymId AND ns.NotificationType = nt.NotificationType);
END
GO
