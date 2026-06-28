-- 064_Phase3PlanManagement.sql
-- Phase 3: plan clone, subscriber counts, pricing reorder, feature dependencies, privilege seed.

IF OBJECT_ID(N'dbo.FeatureDependencies', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FeatureDependencies
    (
        FeatureDependencyId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        FeatureCode         NVARCHAR(50) NOT NULL,
        RequiresFeatureCode NVARCHAR(50) NOT NULL,
        CONSTRAINT UQ_FeatureDependencies UNIQUE (FeatureCode, RequiresFeatureCode)
    );
END
GO

MERGE dbo.FeatureDependencies AS t
USING (VALUES
    (N'WEBSITE_BUILDER', N'PUBLIC_WEBSITE'),
    (N'AI_INSIGHTS',     N'REPORTS'),
    (N'MULTI_BRANCH',    N'MEMBERS'),
    (N'MULTI_BRANCH',    N'TRAINERS')
) AS s (FeatureCode, RequiresFeatureCode)
ON t.FeatureCode = s.FeatureCode AND t.RequiresFeatureCode = s.RequiresFeatureCode
WHEN NOT MATCHED THEN
    INSERT (FeatureCode, RequiresFeatureCode) VALUES (s.FeatureCode, s.RequiresFeatureCode);
GO

CREATE OR ALTER PROCEDURE dbo.sp_Feature_GetDependencies
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FeatureCode, RequiresFeatureCode
    FROM dbo.FeatureDependencies
    ORDER BY FeatureCode, RequiresFeatureCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_Platform_ListPlans
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH LatestSubs AS (
        SELECT
            gs.GymId,
            gs.SaasPlanId,
            gs.Status,
            ROW_NUMBER() OVER (PARTITION BY gs.GymId ORDER BY gs.GymSubscriptionId DESC) AS rn
        FROM dbo.GymSubscriptions gs
        WHERE gs.CancelledAt IS NULL
    ),
    ActiveByPlan AS (
        SELECT
            ls.SaasPlanId,
            COUNT(DISTINCT ls.GymId) AS ActiveSubscriberCount
        FROM LatestSubs ls
        WHERE ls.rn = 1
          AND ls.Status IN (N'Trial', N'Active', N'PastDue')
        GROUP BY ls.SaasPlanId
    ),
    FeatureCounts AS (
        SELECT SaasPlanId, COUNT(*) AS FeatureCount
        FROM dbo.PlanFeatures
        WHERE IsIncluded = 1
        GROUP BY SaasPlanId
    ),
    PricingCounts AS (
        SELECT SaasPlanId, COUNT(*) AS PricingOptionCount
        FROM dbo.PlanPricingOptions
        WHERE IsActive = 1
        GROUP BY SaasPlanId
    )
    SELECT
        sp.SaasPlanId,
        sp.PlanCode,
        sp.PlanName,
        sp.Description,
        sp.IsTrialPlan,
        sp.IsPublic,
        sp.TrialDays,
        sp.IsActive,
        sp.SortOrder,
        COALESCE(abp.ActiveSubscriberCount, 0) AS ActiveSubscriberCount,
        COALESCE(fc.FeatureCount, 0) AS FeatureCount,
        COALESCE(pc.PricingOptionCount, 0) AS PricingOptionCount,
        COALESCE(pq.MaxMembers, sp.MaxMembers) AS MaxMembers,
        COALESCE(pq.MaxTrainers, sp.MaxTrainers) AS MaxTrainers,
        COALESCE(pq.MaxBranches, 1) AS MaxBranches,
        COALESCE(pq.MaxStorageGB, 0) AS MaxStorageGB,
        COALESCE(pq.MaxSmsPerMonth, 0) AS MaxSmsPerMonth,
        COALESCE(pq.MaxWhatsappMessages, 0) AS MaxWhatsappMessages
    FROM dbo.SaasSubscriptionPlans sp
    LEFT JOIN ActiveByPlan abp ON abp.SaasPlanId = sp.SaasPlanId
    LEFT JOIN FeatureCounts fc ON fc.SaasPlanId = sp.SaasPlanId
    LEFT JOIN PricingCounts pc ON pc.SaasPlanId = sp.SaasPlanId
    LEFT JOIN dbo.PlanQuotas pq ON pq.SaasPlanId = sp.SaasPlanId
    ORDER BY sp.SortOrder, sp.PlanName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_Platform_ClonePlan
    @SourceSaasPlanId INT,
    @PlanCode NVARCHAR(50),
    @PlanName NVARCHAR(100),
    @Description NVARCHAR(1000) = NULL,
    @IsTrialPlan BIT = NULL,
    @IsPublic BIT = NULL,
    @SortOrder INT = NULL,
    @NewSaasPlanId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.SaasSubscriptionPlans WHERE SaasPlanId = @SourceSaasPlanId)
        THROW 50051, 'Source plan not found.', 1;

    IF EXISTS (SELECT 1 FROM dbo.SaasSubscriptionPlans WHERE PlanCode = @PlanCode)
        THROW 50053, 'Plan code already exists.', 1;

    DECLARE @SrcDescription NVARCHAR(1000);
    DECLARE @SrcIsTrialPlan BIT;
    DECLARE @SrcIsPublic BIT;
    DECLARE @SrcSortOrder INT;

    SELECT
        @SrcDescription = Description,
        @SrcIsTrialPlan = IsTrialPlan,
        @SrcIsPublic = IsPublic,
        @SrcSortOrder = SortOrder
    FROM dbo.SaasSubscriptionPlans
    WHERE SaasPlanId = @SourceSaasPlanId;

    INSERT INTO dbo.SaasSubscriptionPlans
        (PlanCode, PlanName, Description, IsTrialPlan, IsPublic, TrialDays, SortOrder, IsActive,
         MaxMembers, MaxTrainers, StorageLimitMb, WhatsAppNotificationLimit,
         MonthlyPrice, QuarterlyPrice, HalfYearlyPrice, YearlyPrice, CreatedAt)
    SELECT
        @PlanCode,
        @PlanName,
        COALESCE(@Description, Description),
        COALESCE(@IsTrialPlan, IsTrialPlan),
        COALESCE(@IsPublic, IsPublic),
        TrialDays,
        COALESCE(@SortOrder, SortOrder + 1),
        1,
        MaxMembers, MaxTrainers, StorageLimitMb, WhatsAppNotificationLimit,
        MonthlyPrice, QuarterlyPrice, HalfYearlyPrice, YearlyPrice,
        SYSUTCDATETIME()
    FROM dbo.SaasSubscriptionPlans
    WHERE SaasPlanId = @SourceSaasPlanId;

    SET @NewSaasPlanId = SCOPE_IDENTITY();

    INSERT INTO dbo.PlanQuotas
        (SaasPlanId, MaxMembers, MaxTrainers, MaxBranches, MaxStorageGB, MaxSmsPerMonth, MaxWhatsappMessages, StorageLimitMb, WhatsAppNotificationLimit)
    SELECT
        @NewSaasPlanId, MaxMembers, MaxTrainers, MaxBranches, MaxStorageGB, MaxSmsPerMonth, MaxWhatsappMessages, StorageLimitMb, WhatsAppNotificationLimit
    FROM dbo.PlanQuotas
    WHERE SaasPlanId = @SourceSaasPlanId;

    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO dbo.PlanQuotas (SaasPlanId, MaxMembers, MaxTrainers, MaxBranches, MaxStorageGB, MaxSmsPerMonth, MaxWhatsappMessages, StorageLimitMb, WhatsAppNotificationLimit)
        SELECT @NewSaasPlanId, MaxMembers, MaxTrainers, 1, 1, 0, 0, StorageLimitMb, WhatsAppNotificationLimit
        FROM dbo.SaasSubscriptionPlans WHERE SaasPlanId = @SourceSaasPlanId;
    END

    INSERT INTO dbo.PlanFeatures (SaasPlanId, FeatureId, IsIncluded)
    SELECT @NewSaasPlanId, FeatureId, IsIncluded
    FROM dbo.PlanFeatures
    WHERE SaasPlanId = @SourceSaasPlanId AND IsIncluded = 1;

    INSERT INTO dbo.PlanPricingOptions
        (SaasPlanId, DurationValue, DurationUnit, Price, Currency, DisplayLabel, IsActive, SortOrder)
    SELECT
        @NewSaasPlanId, DurationValue, DurationUnit, Price, Currency, DisplayLabel, IsActive, SortOrder
    FROM dbo.PlanPricingOptions
    WHERE SaasPlanId = @SourceSaasPlanId AND IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PlanPricing_Reorder
    @SaasPlanId INT,
    @PricingOptionOrders NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    ;WITH Orders AS (
        SELECT
            CAST(LEFT(value, CHARINDEX(':', value + ':') - 1) AS INT) AS PricingOptionId,
            CAST(SUBSTRING(value, CHARINDEX(':', value) + 1, 50) AS INT) AS SortOrder
        FROM STRING_SPLIT(@PricingOptionOrders, N',')
        WHERE LTRIM(RTRIM(value)) <> N'' AND CHARINDEX(':', value) > 0
    )
    UPDATE po
    SET po.SortOrder = o.SortOrder,
        po.UpdatedAt = SYSUTCDATETIME()
    FROM dbo.PlanPricingOptions po
    INNER JOIN Orders o ON o.PricingOptionId = po.PricingOptionId
    WHERE po.SaasPlanId = @SaasPlanId;
END
GO

/* Privilege seed: MANAGE_SUBSCRIPTION_PLANS for Super Admin */
IF NOT EXISTS (SELECT 1 FROM dbo.Privileges WHERE PrivilegeName = N'MANAGE_SUBSCRIPTION_PLANS')
BEGIN
    INSERT INTO dbo.Privileges (PrivilegeName, Description, Category, CreatedDate, CreatedAt)
    VALUES (N'MANAGE_SUBSCRIPTION_PLANS', N'Manage dynamic SaaS subscription plans', N'SaaS', CAST(SYSUTCDATETIME() AS DATE), SYSUTCDATETIME());
END
GO

DECLARE @ManagePlansPrivilegeId INT = (SELECT PrivilegeId FROM dbo.Privileges WHERE PrivilegeName = N'MANAGE_SUBSCRIPTION_PLANS');
DECLARE @SuperAdminRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = N'SuperAdmin');

IF @ManagePlansPrivilegeId IS NOT NULL AND @SuperAdminRoleId IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM dbo.RolePrivileges
       WHERE RoleId = @SuperAdminRoleId AND PrivilegeId = @ManagePlansPrivilegeId)
BEGIN
    INSERT INTO dbo.RolePrivileges (RoleId, PrivilegeId, CreatedAt)
    VALUES (@SuperAdminRoleId, @ManagePlansPrivilegeId, SYSUTCDATETIME());
END
GO
