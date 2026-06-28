/* Ensure PremiumPro includes BOOKINGS (top demo plan). */
SET NOCOUNT ON;

;WITH PlanMap AS (
    SELECT PlanCode, FeatureCode FROM (VALUES
        (N'PremiumPro', N'BOOKINGS')
    ) AS v (PlanCode, FeatureCode)
)
INSERT INTO dbo.PlanFeatures (SaasPlanId, FeatureId, IsIncluded)
SELECT p.SaasPlanId, f.FeatureId, 1
FROM PlanMap pm
INNER JOIN dbo.SaasSubscriptionPlans p ON p.PlanCode = pm.PlanCode
INNER JOIN dbo.SystemFeatures f ON f.FeatureCode = pm.FeatureCode
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.PlanFeatures pf
    WHERE pf.SaasPlanId = p.SaasPlanId AND pf.FeatureId = f.FeatureId);
GO
