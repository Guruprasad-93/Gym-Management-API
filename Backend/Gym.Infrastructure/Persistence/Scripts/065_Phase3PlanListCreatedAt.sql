-- 065_Phase3PlanListCreatedAt.sql
-- Add CreatedAt to platform plan list summary.

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
        sp.CreatedAt,
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
