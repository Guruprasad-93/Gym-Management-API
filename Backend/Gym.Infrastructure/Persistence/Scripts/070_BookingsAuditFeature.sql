/* Bookings system feature + audit menu linkage for plan entitlements */
SET NOCOUNT ON;

MERGE dbo.SystemFeatures AS t
USING (VALUES
    (N'BOOKINGS', N'Bookings', N'Class schedules, slot bookings, and waitlist', N'Premium Features', N'/gym-admin/bookings', N'event_available', 155)
) AS s (FeatureCode, FeatureName, Description, Category, MenuRoute, MenuIcon, SortOrder)
ON t.FeatureCode = s.FeatureCode
WHEN MATCHED THEN
    UPDATE SET FeatureName = s.FeatureName, Description = s.Description, Category = s.Category,
               MenuRoute = s.MenuRoute, MenuIcon = s.MenuIcon, SortOrder = s.SortOrder, IsActive = 1, UpdatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (FeatureCode, FeatureName, Description, Category, MenuRoute, MenuIcon, SortOrder, IsMenuFeature, IsApiFeature, IsQuotaFeature)
    VALUES (s.FeatureCode, s.FeatureName, s.Description, s.Category, s.MenuRoute, s.MenuIcon, s.SortOrder, 1, 1, 0);
GO

;WITH Map AS (
    SELECT FeatureCode, MenuCode FROM (VALUES
        (N'BOOKINGS', N'BOOKINGS'),
        (N'BOOKINGS', N'CLASS_SCHEDULES'),
        (N'BOOKINGS', N'BOOKING_ANALYTICS'),
        (N'REPORTS', N'AUDIT_LOGS'),
        (N'REPORTS', N'EXPENSES'),
        (N'REPORTS', N'PAYROLL'),
        (N'REPORTS', N'SETTINGS')
    ) AS v (FeatureCode, MenuCode)
)
INSERT INTO dbo.FeatureMenus (FeatureId, MenuId)
SELECT f.FeatureId, m.MenuId
FROM Map mp
INNER JOIN dbo.SystemFeatures f ON f.FeatureCode = mp.FeatureCode
INNER JOIN dbo.Menus m ON m.MenuCode = mp.MenuCode
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.FeatureMenus fm
    WHERE fm.FeatureId = f.FeatureId AND fm.MenuId = m.MenuId);
GO

;WITH Routes AS (
    SELECT FeatureCode, RoutePrefix FROM (VALUES
        (N'BOOKINGS', N'/api/bookings'),
        (N'BOOKINGS', N'/api/schedules'),
        (N'BOOKINGS', N'/api/booking-analytics')
    ) AS v (FeatureCode, RoutePrefix)
)
INSERT INTO dbo.FeatureApiRoutes (FeatureId, RoutePrefix)
SELECT f.FeatureId, r.RoutePrefix
FROM Routes r
INNER JOIN dbo.SystemFeatures f ON f.FeatureCode = r.FeatureCode
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.FeatureApiRoutes far
    WHERE far.FeatureId = f.FeatureId AND far.RoutePrefix = r.RoutePrefix);
GO

;WITH PlanMap AS (
    SELECT PlanCode, FeatureCode FROM (VALUES
        (N'Premium', N'BOOKINGS'),
        (N'PremiumPro', N'BOOKINGS'),
        (N'Enterprise', N'BOOKINGS')
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
