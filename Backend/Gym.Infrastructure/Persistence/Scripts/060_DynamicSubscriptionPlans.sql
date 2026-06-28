-- 060_DynamicSubscriptionPlans.sql
-- Dynamic pricing options, plan-feature assignments, and plan quotas (Phase 1).
-- Extends SaasSubscriptionPlans; legacy price columns retained for rollback.

IF COL_LENGTH('dbo.SaasSubscriptionPlans', 'Description') IS NULL
    ALTER TABLE dbo.SaasSubscriptionPlans ADD Description NVARCHAR(1000) NULL;
GO

IF COL_LENGTH('dbo.SaasSubscriptionPlans', 'IsTrialPlan') IS NULL
    ALTER TABLE dbo.SaasSubscriptionPlans ADD IsTrialPlan BIT NOT NULL
        CONSTRAINT DF_SaasSubscriptionPlans_IsTrialPlan DEFAULT (0) WITH VALUES;
GO

IF COL_LENGTH('dbo.SaasSubscriptionPlans', 'IsPublic') IS NULL
    ALTER TABLE dbo.SaasSubscriptionPlans ADD IsPublic BIT NOT NULL
        CONSTRAINT DF_SaasSubscriptionPlans_IsPublic DEFAULT (1) WITH VALUES;
GO

IF OBJECT_ID(N'dbo.PlanPricingOptions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PlanPricingOptions
    (
        PricingOptionId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        SaasPlanId      INT NOT NULL,
        DurationValue   INT NOT NULL,
        DurationUnit    NVARCHAR(20) NOT NULL,
        Price           DECIMAL(18, 2) NOT NULL,
        Currency        NVARCHAR(3) NOT NULL CONSTRAINT DF_PlanPricingOptions_Currency DEFAULT (N'INR'),
        DisplayLabel    NVARCHAR(100) NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_PlanPricingOptions_IsActive DEFAULT (1),
        SortOrder       INT NOT NULL CONSTRAINT DF_PlanPricingOptions_SortOrder DEFAULT (0),
        CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_PlanPricingOptions_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2 NULL,
        CONSTRAINT FK_PlanPricingOptions_Plan FOREIGN KEY (SaasPlanId) REFERENCES dbo.SaasSubscriptionPlans (SaasPlanId),
        CONSTRAINT CK_PlanPricingOptions_DurationValue CHECK (DurationValue > 0),
        CONSTRAINT CK_PlanPricingOptions_DurationUnit CHECK (DurationUnit IN (N'Day', N'Month', N'Year')),
        CONSTRAINT CK_PlanPricingOptions_Price CHECK (Price >= 0),
        CONSTRAINT UQ_PlanPricingOptions_Plan_Duration UNIQUE (SaasPlanId, DurationValue, DurationUnit)
    );
    CREATE INDEX IX_PlanPricingOptions_PlanId ON dbo.PlanPricingOptions (SaasPlanId, SortOrder) WHERE IsActive = 1;
END
GO

IF OBJECT_ID(N'dbo.PlanFeatures', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PlanFeatures
    (
        PlanFeatureId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        SaasPlanId    INT NOT NULL,
        FeatureId     INT NOT NULL,
        IsIncluded    BIT NOT NULL CONSTRAINT DF_PlanFeatures_IsIncluded DEFAULT (1),
        CreatedAt     DATETIME2 NOT NULL CONSTRAINT DF_PlanFeatures_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_PlanFeatures_Plan FOREIGN KEY (SaasPlanId) REFERENCES dbo.SaasSubscriptionPlans (SaasPlanId),
        CONSTRAINT FK_PlanFeatures_Feature FOREIGN KEY (FeatureId) REFERENCES dbo.SystemFeatures (FeatureId),
        CONSTRAINT UQ_PlanFeatures_Plan_Feature UNIQUE (SaasPlanId, FeatureId)
    );
    CREATE INDEX IX_PlanFeatures_PlanId ON dbo.PlanFeatures (SaasPlanId) WHERE IsIncluded = 1;
END
GO

IF OBJECT_ID(N'dbo.PlanQuotas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PlanQuotas
    (
        PlanQuotaId               INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        SaasPlanId                INT NOT NULL,
        MaxMembers                INT NOT NULL,
        MaxTrainers               INT NOT NULL,
        StorageLimitMb            INT NOT NULL,
        WhatsAppNotificationLimit INT NOT NULL,
        UpdatedAt                 DATETIME2 NULL,
        CONSTRAINT FK_PlanQuotas_Plan FOREIGN KEY (SaasPlanId) REFERENCES dbo.SaasSubscriptionPlans (SaasPlanId),
        CONSTRAINT UQ_PlanQuotas_Plan UNIQUE (SaasPlanId)
    );
END
GO

IF COL_LENGTH('dbo.GymSubscriptions', 'PricingOptionId') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD PricingOptionId INT NULL;
GO

IF COL_LENGTH('dbo.GymSubscriptions', 'DurationValue') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD DurationValue INT NULL;
GO

IF COL_LENGTH('dbo.GymSubscriptions', 'DurationUnit') IS NULL
    ALTER TABLE dbo.GymSubscriptions ADD DurationUnit NVARCHAR(20) NULL;
GO

IF OBJECT_ID(N'dbo.FK_GymSubscriptions_PricingOption', N'F') IS NULL
   AND COL_LENGTH('dbo.GymSubscriptions', 'PricingOptionId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.GymSubscriptions ADD CONSTRAINT FK_GymSubscriptions_PricingOption
        FOREIGN KEY (PricingOptionId) REFERENCES dbo.PlanPricingOptions (PricingOptionId);
END
GO

IF COL_LENGTH('dbo.SaasSubscriptionPayments', 'PricingOptionId') IS NULL
    ALTER TABLE dbo.SaasSubscriptionPayments ADD PricingOptionId INT NULL;
GO

IF OBJECT_ID(N'dbo.FK_SaasSubscriptionPayments_PricingOption', N'F') IS NULL
   AND COL_LENGTH('dbo.SaasSubscriptionPayments', 'PricingOptionId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.SaasSubscriptionPayments ADD CONSTRAINT FK_SaasSubscriptionPayments_PricingOption
        FOREIGN KEY (PricingOptionId) REFERENCES dbo.PlanPricingOptions (PricingOptionId);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PlanPricing_GetByPlanId
    @SaasPlanId INT,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        PricingOptionId, SaasPlanId, DurationValue, DurationUnit, Price, Currency,
        DisplayLabel, IsActive, SortOrder
    FROM dbo.PlanPricingOptions
    WHERE SaasPlanId = @SaasPlanId
      AND (@IncludeInactive = 1 OR IsActive = 1)
    ORDER BY SortOrder, DurationUnit, DurationValue;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PlanFeature_GetByPlanId
    @SaasPlanId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        pf.PlanFeatureId,
        pf.SaasPlanId,
        f.FeatureId,
        f.FeatureCode,
        f.FeatureName,
        f.Category,
        pf.IsIncluded
    FROM dbo.PlanFeatures pf
    INNER JOIN dbo.SystemFeatures f ON f.FeatureId = pf.FeatureId
    WHERE pf.SaasPlanId = @SaasPlanId
    ORDER BY f.Category, f.SortOrder;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PlanQuota_GetByPlanId
    @SaasPlanId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PlanQuotaId, SaasPlanId, MaxMembers, MaxTrainers, StorageLimitMb, WhatsAppNotificationLimit
    FROM dbo.PlanQuotas
    WHERE SaasPlanId = @SaasPlanId;
END
GO

/* Effective features for a gym: plan entitlements (active subscription).
   Menu visibility in Phase 2 = feature enabled AND GymMenus.IsEnabled. */
CREATE OR ALTER PROCEDURE dbo.sp_Gym_GetEnabledFeatureCodes
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SaasPlanId INT;

    SELECT TOP 1 @SaasPlanId = gs.SaasPlanId
    FROM dbo.GymSubscriptions gs
    WHERE gs.GymId = @GymId
      AND gs.Status IN (N'Trial', N'Active', N'PastDue')
      AND gs.CancelledAt IS NULL
    ORDER BY gs.GymSubscriptionId DESC;

    IF @SaasPlanId IS NULL
    BEGIN
        SELECT CAST(NULL AS NVARCHAR(50)) AS FeatureCode WHERE 1 = 0;
        RETURN;
    END

    SELECT DISTINCT f.FeatureCode
    FROM dbo.PlanFeatures pf
    INNER JOIN dbo.SystemFeatures f ON f.FeatureId = pf.FeatureId
    WHERE pf.SaasPlanId = @SaasPlanId
      AND pf.IsIncluded = 1
      AND f.IsActive = 1
    ORDER BY f.FeatureCode;
END
GO

/* Visible menus = plan feature menu codes INTERSECT gym-enabled menus. */
CREATE OR ALTER PROCEDURE dbo.sp_Gym_GetVisibleMenuCodes
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH PlanMenus AS (
        SELECT DISTINCT m.MenuCode
        FROM dbo.GymSubscriptions gs
        INNER JOIN dbo.PlanFeatures pf ON pf.SaasPlanId = gs.SaasPlanId AND pf.IsIncluded = 1
        INNER JOIN dbo.SystemFeatures f ON f.FeatureId = pf.FeatureId AND f.IsActive = 1
        INNER JOIN dbo.FeatureMenus fm ON fm.FeatureId = f.FeatureId
        INNER JOIN dbo.Menus m ON m.MenuId = fm.MenuId AND m.IsActive = 1
        WHERE gs.GymId = @GymId
          AND gs.Status IN (N'Trial', N'Active', N'PastDue')
          AND gs.CancelledAt IS NULL
    )
    SELECT pm.MenuCode
    FROM PlanMenus pm
    WHERE EXISTS (
        SELECT 1
        FROM dbo.GymMenus gm
        INNER JOIN dbo.Menus m ON m.MenuId = gm.MenuId
        WHERE gm.GymId = @GymId
          AND gm.IsEnabled = 1
          AND m.MenuCode = pm.MenuCode
          AND m.IsActive = 1)
    ORDER BY pm.MenuCode;
END
GO
