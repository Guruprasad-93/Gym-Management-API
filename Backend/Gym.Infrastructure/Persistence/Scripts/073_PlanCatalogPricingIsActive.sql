-- 073_PlanCatalogPricingIsActive.sql
-- sp_Saas_GetPlanCatalog omitted IsActive on pricing rows; Dapper defaulted bool to false
-- and the Angular catalog hid all Razorpay purchase buttons.

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetPlanCatalog
    @PublicOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        sp.SaasPlanId,
        sp.PlanCode,
        sp.PlanName,
        sp.Description,
        sp.IsTrialPlan,
        sp.IsPublic,
        sp.TrialDays,
        sp.SortOrder,
        COALESCE(pq.MaxMembers, sp.MaxMembers) AS MaxMembers,
        COALESCE(pq.MaxTrainers, sp.MaxTrainers) AS MaxTrainers,
        COALESCE(pq.MaxBranches, 1) AS MaxBranches,
        COALESCE(pq.MaxStorageGB, 0) AS MaxStorageGB,
        COALESCE(pq.MaxSmsPerMonth, 0) AS MaxSmsPerMonth,
        COALESCE(pq.MaxWhatsappMessages, 0) AS MaxWhatsappMessages
    FROM dbo.SaasSubscriptionPlans sp
    LEFT JOIN dbo.PlanQuotas pq ON pq.SaasPlanId = sp.SaasPlanId
    WHERE sp.IsActive = 1
      AND (@PublicOnly = 0 OR sp.IsPublic = 1)
      AND sp.IsTrialPlan = 0
    ORDER BY sp.SortOrder, sp.PlanName;

    SELECT
        po.PricingOptionId,
        po.SaasPlanId,
        po.DurationValue,
        po.DurationUnit,
        po.Price,
        po.Currency,
        po.DisplayLabel,
        po.IsActive,
        po.SortOrder
    FROM dbo.PlanPricingOptions po
    INNER JOIN dbo.SaasSubscriptionPlans sp ON sp.SaasPlanId = po.SaasPlanId
    WHERE po.IsActive = 1
      AND sp.IsActive = 1
      AND (@PublicOnly = 0 OR sp.IsPublic = 1)
      AND sp.IsTrialPlan = 0
    ORDER BY po.SaasPlanId, po.SortOrder, po.DurationUnit, po.DurationValue;

    SELECT
        pf.SaasPlanId,
        f.FeatureId,
        f.FeatureCode,
        f.FeatureName,
        f.Category
    FROM dbo.PlanFeatures pf
    INNER JOIN dbo.SystemFeatures f ON f.FeatureId = pf.FeatureId
    INNER JOIN dbo.SaasSubscriptionPlans sp ON sp.SaasPlanId = pf.SaasPlanId
    WHERE pf.IsIncluded = 1
      AND f.IsActive = 1
      AND sp.IsActive = 1
      AND (@PublicOnly = 0 OR sp.IsPublic = 1)
    ORDER BY sp.SaasPlanId, f.Category, f.SortOrder;
END
GO
