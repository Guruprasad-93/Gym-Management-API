-- 061_MigrateLegacySubscriptionData.sql
-- Convert legacy plans, billing cycles, and subscriptions to feature-driven model.
-- Idempotent: safe to re-run on databases partially migrated.

SET NOCOUNT ON;
SET XACT_ABORT ON;

/* ========== 1. NORMALIZE LEGACY PLAN METADATA ========== */
UPDATE dbo.SaasSubscriptionPlans
SET
    IsTrialPlan = CASE WHEN PlanCode = N'Trial' THEN 1 ELSE 0 END,
    IsPublic = CASE WHEN PlanCode = N'Trial' THEN 0 ELSE 1 END,
    Description = CASE PlanCode
        WHEN N'Trial'      THEN N'Starter trial — core features only'
        WHEN N'Basic'      THEN N'Essential gym management for growing studios'
        WHEN N'Premium'    THEN N'Advanced operations with CRM and analytics'
        WHEN N'Enterprise' THEN N'Full platform access including pro features'
        ELSE Description
    END,
    PlanName = CASE WHEN PlanCode = N'Trial' THEN N'Starter Trial' ELSE PlanName END,
    UpdatedAt = SYSUTCDATETIME()
WHERE PlanCode IN (N'Trial', N'Basic', N'Premium', N'Enterprise');
GO

/* ========== 2. MIGRATE PLAN QUOTAS FROM LEGACY COLUMNS ========== */
MERGE dbo.PlanQuotas AS t
USING (
    SELECT SaasPlanId, MaxMembers, MaxTrainers, StorageLimitMb, WhatsAppNotificationLimit
    FROM dbo.SaasSubscriptionPlans
) AS s ON t.SaasPlanId = s.SaasPlanId
WHEN MATCHED THEN
    UPDATE SET
        MaxMembers = s.MaxMembers,
        MaxTrainers = s.MaxTrainers,
        StorageLimitMb = s.StorageLimitMb,
        WhatsAppNotificationLimit = s.WhatsAppNotificationLimit,
        UpdatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (SaasPlanId, MaxMembers, MaxTrainers, StorageLimitMb, WhatsAppNotificationLimit)
    VALUES (s.SaasPlanId, s.MaxMembers, s.MaxTrainers, s.StorageLimitMb, s.WhatsAppNotificationLimit);
GO

/* ========== 3. MIGRATE LEGACY BILLING CYCLES → PRICING OPTIONS ========== */
;WITH LegacyPricing AS (
    SELECT p.SaasPlanId, 1 AS DurationValue, N'Month' AS DurationUnit, p.MonthlyPrice AS Price, N'1 Month' AS DisplayLabel, 1 AS SortOrder
    FROM dbo.SaasSubscriptionPlans p WHERE p.IsTrialPlan = 0 AND p.MonthlyPrice >= 0
    UNION ALL
    SELECT p.SaasPlanId, 3, N'Month', p.QuarterlyPrice, N'3 Months', 2
    FROM dbo.SaasSubscriptionPlans p WHERE p.IsTrialPlan = 0 AND p.QuarterlyPrice >= 0
    UNION ALL
    SELECT p.SaasPlanId, 6, N'Month', p.HalfYearlyPrice, N'6 Months', 3
    FROM dbo.SaasSubscriptionPlans p WHERE p.IsTrialPlan = 0 AND p.HalfYearlyPrice >= 0
    UNION ALL
    SELECT p.SaasPlanId, 12, N'Month', p.YearlyPrice, N'12 Months', 4
    FROM dbo.SaasSubscriptionPlans p WHERE p.IsTrialPlan = 0 AND p.YearlyPrice >= 0
)
MERGE dbo.PlanPricingOptions AS t
USING LegacyPricing AS s
ON t.SaasPlanId = s.SaasPlanId
   AND t.DurationValue = s.DurationValue
   AND t.DurationUnit = s.DurationUnit
WHEN MATCHED THEN
    UPDATE SET
        Price = s.Price,
        Currency = N'INR',
        DisplayLabel = s.DisplayLabel,
        SortOrder = s.SortOrder,
        IsActive = 1,
        UpdatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (SaasPlanId, DurationValue, DurationUnit, Price, Currency, DisplayLabel, SortOrder, IsActive)
    VALUES (s.SaasPlanId, s.DurationValue, s.DurationUnit, s.Price, N'INR', s.DisplayLabel, s.SortOrder, 1);
GO

/* Trial plan: single 15-day ₹0 pricing option */
DECLARE @TrialPlanId INT = (SELECT SaasPlanId FROM dbo.SaasSubscriptionPlans WHERE IsTrialPlan = 1);
DECLARE @TrialDays INT = ISNULL((SELECT TrialDays FROM dbo.SaasSubscriptionPlans WHERE SaasPlanId = @TrialPlanId), 15);

IF @TrialPlanId IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM dbo.PlanPricingOptions
        WHERE SaasPlanId = @TrialPlanId AND DurationValue = @TrialDays AND DurationUnit = N'Day')
    BEGIN
        INSERT INTO dbo.PlanPricingOptions
            (SaasPlanId, DurationValue, DurationUnit, Price, Currency, DisplayLabel, SortOrder, IsActive)
        VALUES
            (@TrialPlanId, @TrialDays, N'Day', 0, N'INR', CAST(@TrialDays AS NVARCHAR(10)) + N' Days Trial', 1, 1);
    END
    ELSE
    BEGIN
        UPDATE dbo.PlanPricingOptions
        SET Price = 0, Currency = N'INR', DisplayLabel = CAST(@TrialDays AS NVARCHAR(10)) + N' Days Trial', IsActive = 1, UpdatedAt = SYSUTCDATETIME()
        WHERE SaasPlanId = @TrialPlanId AND DurationValue = @TrialDays AND DurationUnit = N'Day';
    END
END
GO

/* ========== 4. ASSIGN FEATURES TO LEGACY PLANS ========== */
;WITH FeatureSets AS (
    SELECT PlanCode, FeatureCode FROM (VALUES
        /* Starter Trial — Core only */
        (N'Trial', N'DASHBOARD'),
        (N'Trial', N'MEMBERS'),
        (N'Trial', N'TRAINERS'),
        (N'Trial', N'ATTENDANCE'),
        (N'Trial', N'MEMBERSHIPS'),
        (N'Trial', N'PAYMENTS'),
        (N'Trial', N'SUBSCRIPTIONS'),
        /* Basic — Core + selected Premium */
        (N'Basic', N'DASHBOARD'),
        (N'Basic', N'MEMBERS'),
        (N'Basic', N'TRAINERS'),
        (N'Basic', N'ATTENDANCE'),
        (N'Basic', N'MEMBERSHIPS'),
        (N'Basic', N'PAYMENTS'),
        (N'Basic', N'SUBSCRIPTIONS'),
        (N'Basic', N'DIET_PLANS'),
        (N'Basic', N'WORKOUT_PLANS'),
        (N'Basic', N'NOTIFICATIONS'),
        /* Premium — Core + all Premium + Multi Branch */
        (N'Premium', N'DASHBOARD'),
        (N'Premium', N'MEMBERS'),
        (N'Premium', N'TRAINERS'),
        (N'Premium', N'ATTENDANCE'),
        (N'Premium', N'MEMBERSHIPS'),
        (N'Premium', N'PAYMENTS'),
        (N'Premium', N'SUBSCRIPTIONS'),
        (N'Premium', N'REPORTS'),
        (N'Premium', N'CRM'),
        (N'Premium', N'NOTIFICATIONS'),
        (N'Premium', N'DIET_PLANS'),
        (N'Premium', N'WORKOUT_PLANS'),
        (N'Premium', N'MULTI_BRANCH'),
        /* Enterprise — all features */
        (N'Enterprise', N'DASHBOARD'),
        (N'Enterprise', N'MEMBERS'),
        (N'Enterprise', N'TRAINERS'),
        (N'Enterprise', N'ATTENDANCE'),
        (N'Enterprise', N'MEMBERSHIPS'),
        (N'Enterprise', N'PAYMENTS'),
        (N'Enterprise', N'SUBSCRIPTIONS'),
        (N'Enterprise', N'REPORTS'),
        (N'Enterprise', N'CRM'),
        (N'Enterprise', N'NOTIFICATIONS'),
        (N'Enterprise', N'DIET_PLANS'),
        (N'Enterprise', N'WORKOUT_PLANS'),
        (N'Enterprise', N'WHITE_LABEL'),
        (N'Enterprise', N'WEBSITE_BUILDER'),
        (N'Enterprise', N'MULTI_BRANCH'),
        (N'Enterprise', N'PUBLIC_WEBSITE'),
        (N'Enterprise', N'AI_INSIGHTS'),
        (N'Enterprise', N'CUSTOM_BRANDING')
    ) AS v(PlanCode, FeatureCode)
)
INSERT INTO dbo.PlanFeatures (SaasPlanId, FeatureId, IsIncluded)
SELECT p.SaasPlanId, f.FeatureId, 1
FROM FeatureSets fs
INNER JOIN dbo.SaasSubscriptionPlans p ON p.PlanCode = fs.PlanCode
INNER JOIN dbo.SystemFeatures f ON f.FeatureCode = fs.FeatureCode
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.PlanFeatures pf
    WHERE pf.SaasPlanId = p.SaasPlanId AND pf.FeatureId = f.FeatureId);
GO

/* ========== 5. BACKFILL GYM SUBSCRIPTIONS → PRICING OPTIONS ========== */
UPDATE gs
SET
    PricingOptionId = po.PricingOptionId,
    DurationValue = po.DurationValue,
    DurationUnit = po.DurationUnit
FROM dbo.GymSubscriptions gs
INNER JOIN dbo.PlanPricingOptions po ON po.SaasPlanId = gs.SaasPlanId
WHERE gs.PricingOptionId IS NULL
  AND gs.SaasPlanId IS NOT NULL
  AND (
        (UPPER(ISNULL(gs.BillingCycle, N'')) IN (N'MONTHLY', N'') AND po.DurationValue = 1 AND po.DurationUnit = N'Month')
     OR (UPPER(gs.BillingCycle) IN (N'QUARTERLY') AND po.DurationValue = 3 AND po.DurationUnit = N'Month')
     OR (UPPER(gs.BillingCycle) IN (N'HALFYEARLY', N'HALF-YEARLY', N'HALF YEARLY') AND po.DurationValue = 6 AND po.DurationUnit = N'Month')
     OR (UPPER(gs.BillingCycle) IN (N'YEARLY') AND po.DurationValue = 12 AND po.DurationUnit = N'Month')
     OR (UPPER(gs.BillingCycle) IN (N'TRIAL') AND po.DurationUnit = N'Day' AND po.Price = 0)
  );
GO

/* Trial subscriptions without BillingCycle: use trial day pricing option */
UPDATE gs
SET
    PricingOptionId = po.PricingOptionId,
    DurationValue = po.DurationValue,
    DurationUnit = po.DurationUnit
FROM dbo.GymSubscriptions gs
INNER JOIN dbo.SaasSubscriptionPlans p ON p.SaasPlanId = gs.SaasPlanId AND p.IsTrialPlan = 1
INNER JOIN dbo.PlanPricingOptions po ON po.SaasPlanId = p.SaasPlanId AND po.DurationUnit = N'Day' AND po.Price = 0
WHERE gs.PricingOptionId IS NULL;
GO

/* ========== 6. BACKFILL PAYMENT RECORDS ========== */
UPDATE pay
SET PricingOptionId = gs.PricingOptionId
FROM dbo.SaasSubscriptionPayments pay
INNER JOIN dbo.GymSubscriptions gs ON gs.GymSubscriptionId = pay.GymSubscriptionId
WHERE pay.PricingOptionId IS NULL
  AND gs.PricingOptionId IS NOT NULL;
GO

UPDATE pay
SET PricingOptionId = po.PricingOptionId
FROM dbo.SaasSubscriptionPayments pay
INNER JOIN dbo.PlanPricingOptions po ON po.SaasPlanId = pay.SaasPlanId
WHERE pay.PricingOptionId IS NULL
  AND (
        (UPPER(pay.BillingCycle) = N'MONTHLY' AND po.DurationValue = 1 AND po.DurationUnit = N'Month')
     OR (UPPER(pay.BillingCycle) = N'QUARTERLY' AND po.DurationValue = 3 AND po.DurationUnit = N'Month')
     OR (UPPER(pay.BillingCycle) IN (N'HALFYEARLY', N'HALF-YEARLY') AND po.DurationValue = 6 AND po.DurationUnit = N'Month')
     OR (UPPER(pay.BillingCycle) = N'YEARLY' AND po.DurationValue = 12 AND po.DurationUnit = N'Month')
  );
GO

/* ========== 7. VALIDATION (throws on critical gaps) ========== */
IF EXISTS (
    SELECT 1 FROM dbo.SaasSubscriptionPlans p
    WHERE p.IsActive = 1
      AND NOT EXISTS (SELECT 1 FROM dbo.PlanFeatures pf WHERE pf.SaasPlanId = p.SaasPlanId AND pf.IsIncluded = 1))
    THROW 51071, 'Migration validation failed: active plan without features.', 1;

IF EXISTS (
    SELECT 1 FROM dbo.SaasSubscriptionPlans p
    WHERE p.IsActive = 1 AND p.IsTrialPlan = 0
      AND NOT EXISTS (SELECT 1 FROM dbo.PlanPricingOptions po WHERE po.SaasPlanId = p.SaasPlanId AND po.IsActive = 1))
    THROW 51072, 'Migration validation failed: paid plan without pricing options.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.SystemFeatures WHERE IsActive = 1)
    THROW 51073, 'Migration validation failed: SystemFeatures catalog is empty.', 1;
GO
