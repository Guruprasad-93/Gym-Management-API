-- 059_SystemFeatures.sql
-- Master feature catalog for dynamic subscription entitlements (Phase 1).
-- Super Admin selects from seeded features only; no custom feature codes.

IF OBJECT_ID(N'dbo.SystemFeatures', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemFeatures
    (
        FeatureId     INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        FeatureCode   NVARCHAR(50)  NOT NULL,
        FeatureName   NVARCHAR(100) NOT NULL,
        Description   NVARCHAR(500) NULL,
        Category      NVARCHAR(50)  NOT NULL,
        MenuRoute     NVARCHAR(200) NULL,
        MenuIcon      NVARCHAR(50)  NULL,
        IsMenuFeature BIT NOT NULL CONSTRAINT DF_SystemFeatures_IsMenuFeature DEFAULT (1),
        IsApiFeature  BIT NOT NULL CONSTRAINT DF_SystemFeatures_IsApiFeature DEFAULT (1),
        IsQuotaFeature BIT NOT NULL CONSTRAINT DF_SystemFeatures_IsQuotaFeature DEFAULT (0),
        SortOrder     INT NOT NULL CONSTRAINT DF_SystemFeatures_SortOrder DEFAULT (0),
        IsActive      BIT NOT NULL CONSTRAINT DF_SystemFeatures_IsActive DEFAULT (1),
        CreatedAt     DATETIME2 NOT NULL CONSTRAINT DF_SystemFeatures_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt     DATETIME2 NULL,
        CONSTRAINT UQ_SystemFeatures_FeatureCode UNIQUE (FeatureCode)
    );
    CREATE INDEX IX_SystemFeatures_Category ON dbo.SystemFeatures (Category, SortOrder) WHERE IsActive = 1;
END
GO

IF OBJECT_ID(N'dbo.FeatureMenus', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FeatureMenus
    (
        FeatureMenuId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        FeatureId     INT NOT NULL,
        MenuId        INT NOT NULL,
        CONSTRAINT UQ_FeatureMenus_Feature_Menu UNIQUE (FeatureId, MenuId),
        CONSTRAINT FK_FeatureMenus_Feature FOREIGN KEY (FeatureId) REFERENCES dbo.SystemFeatures (FeatureId),
        CONSTRAINT FK_FeatureMenus_Menu FOREIGN KEY (MenuId) REFERENCES dbo.Menus (MenuId)
    );
    CREATE INDEX IX_FeatureMenus_MenuId ON dbo.FeatureMenus (MenuId);
END
GO

IF OBJECT_ID(N'dbo.FeatureApiRoutes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FeatureApiRoutes
    (
        FeatureApiRouteId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        FeatureId         INT NOT NULL,
        RoutePrefix       NVARCHAR(200) NOT NULL,
        HttpMethods       NVARCHAR(50) NULL,
        CONSTRAINT UQ_FeatureApiRoutes_Feature_Route UNIQUE (FeatureId, RoutePrefix),
        CONSTRAINT FK_FeatureApiRoutes_Feature FOREIGN KEY (FeatureId) REFERENCES dbo.SystemFeatures (FeatureId)
    );
END
GO

/* ========== SEED FEATURE CATALOG (idempotent) ========== */
MERGE dbo.SystemFeatures AS t
USING (VALUES
    (N'DASHBOARD',       N'Dashboard',        N'Core gym dashboard and KPIs',                    N'Core Features',    N'/gym-admin/dashboard',              N'dashboard',           10),
    (N'MEMBERS',         N'Members',          N'Member management and profiles',                 N'Core Features',    N'/gym-admin/members',                N'groups',              20),
    (N'TRAINERS',        N'Trainers',         N'Trainer management and assignments',             N'Core Features',    N'/gym-admin/trainers',               N'sports',              30),
    (N'ATTENDANCE',      N'Attendance',       N'Member and trainer attendance tracking',         N'Core Features',    N'/gym-admin/attendance',             N'event_available',     40),
    (N'MEMBERSHIPS',     N'Memberships',      N'Membership plans and member subscriptions',      N'Core Features',    N'/gym-admin/memberships',            N'event_note',          50),
    (N'PAYMENTS',        N'Payments',         N'Payment collection and revenue',                 N'Core Features',    N'/gym-admin/payments',               N'payments',            60),
    (N'SUBSCRIPTIONS',   N'Subscriptions',    N'Gym SaaS plan view, purchase, and renewal',      N'Core Features',    N'/gym-admin/subscription',           N'subscriptions',       70),
    (N'REPORTS',         N'Reports',          N'Analytics and business reports',                 N'Premium Features', NULL,                                 N'assessment',          110),
    (N'CRM',             N'CRM',              N'Lead and CRM pipeline management',               N'Premium Features', N'/gym-admin/leads',                  N'contact_page',        120),
    (N'NOTIFICATIONS',   N'Notifications',    N'WhatsApp, push, and in-app notifications',       N'Premium Features', N'/gym-admin/notifications',          N'notifications',       130),
    (N'DIET_PLANS',      N'Diet Plans',       N'Diet plan library and member assignments',       N'Premium Features', N'/gym-admin/diet-plans',             N'restaurant_menu',     140),
    (N'WORKOUT_PLANS',   N'Workout Plans',    N'Workout plan library and assignments',           N'Premium Features', N'/gym-admin/workout-plans',          N'fitness_center',        150),
    (N'WHITE_LABEL',     N'White Label',      N'White-label branding and tenant identity',       N'Pro Features',     N'/gym-admin/white-label',            N'branding_watermark',  210),
    (N'WEBSITE_BUILDER', N'Website Builder',  N'Gym website builder and page management',        N'Pro Features',     N'/gym-admin/website-builder',        N'language',            220),
    (N'MULTI_BRANCH',    N'Multi Branch',     N'Multi-branch operations and transfers',          N'Pro Features',     N'/gym-admin/branches',               N'store',               230),
    (N'PUBLIC_WEBSITE',  N'Public Website',   N'Public-facing gym website',                      N'Pro Features',     NULL,                                 N'language',            240),
    (N'AI_INSIGHTS',     N'AI Insights',      N'AI recommendations and insights',                N'Pro Features',     N'/gym-admin/ai',                     N'psychology',          250),
    (N'CUSTOM_BRANDING', N'Custom Branding',  N'Custom colors, logos, and gym branding',         N'Pro Features',     N'/gym-admin/settings/branding',      N'palette',             260)
) AS s (FeatureCode, FeatureName, Description, Category, MenuRoute, MenuIcon, SortOrder)
ON t.FeatureCode = s.FeatureCode
WHEN MATCHED THEN
    UPDATE SET
        FeatureName = s.FeatureName,
        Description = s.Description,
        Category = s.Category,
        MenuRoute = s.MenuRoute,
        MenuIcon = s.MenuIcon,
        SortOrder = s.SortOrder,
        IsActive = 1,
        UpdatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (FeatureCode, FeatureName, Description, Category, MenuRoute, MenuIcon, SortOrder, IsMenuFeature, IsApiFeature, IsQuotaFeature)
    VALUES (s.FeatureCode, s.FeatureName, s.Description, s.Category, s.MenuRoute, s.MenuIcon, s.SortOrder, 1, 1, 0);
GO

/* ========== FEATURE → MENU MAPPINGS ========== */
;WITH Map AS (
    SELECT FeatureCode, MenuCode FROM (VALUES
        (N'DASHBOARD',       N'DASHBOARD'),
        (N'MEMBERS',         N'MEMBERS'),
        (N'TRAINERS',        N'TRAINERS'),
        (N'ATTENDANCE',      N'ATTENDANCE'),
        (N'ATTENDANCE',      N'ATTENDANCE_REPORTS'),
        (N'MEMBERSHIPS',     N'MEMBERSHIPS'),
        (N'MEMBERSHIPS',     N'MEMBERSHIP_PLANS'),
        (N'PAYMENTS',        N'PAYMENTS'),
        (N'PAYMENTS',        N'REVENUE'),
        (N'SUBSCRIPTIONS',   N'SUBSCRIPTIONS'),
        (N'REPORTS',         N'REPORTS'),
        (N'REPORTS',         N'ANALYTICS'),
        (N'REPORTS',         N'REVENUE_ANALYTICS'),
        (N'REPORTS',         N'MEMBER_ANALYTICS'),
        (N'REPORTS',         N'ATTENDANCE_ANALYTICS'),
        (N'REPORTS',         N'TRAINER_ANALYTICS'),
        (N'REPORTS',         N'FINANCIAL'),
        (N'CRM',             N'CRM'),
        (N'CRM',             N'LEADS'),
        (N'NOTIFICATIONS',   N'NOTIFICATIONS'),
        (N'NOTIFICATIONS',   N'MOBILE_PUSH'),
        (N'NOTIFICATIONS',   N'MOBILE_ANALYTICS'),
        (N'DIET_PLANS',      N'DIET_PLANS'),
        (N'WORKOUT_PLANS',   N'WORKOUT_PLANS'),
        (N'WHITE_LABEL',     N'WHITE_LABEL'),
        (N'WEBSITE_BUILDER', N'WEBSITE_BUILDER'),
        (N'WEBSITE_BUILDER', N'WEBSITE_ANALYTICS'),
        (N'MULTI_BRANCH',    N'BRANCHES'),
        (N'MULTI_BRANCH',    N'BRANCH_DASHBOARD'),
        (N'MULTI_BRANCH',    N'BRANCH_ANALYTICS'),
        (N'MULTI_BRANCH',    N'BRANCH_TRANSFERS'),
        (N'MULTI_BRANCH',    N'BRANCH_TARGETS'),
        (N'PUBLIC_WEBSITE',  N'PUBLIC_WEBSITE'),
        (N'AI_INSIGHTS',     N'AI_INSIGHTS'),
        (N'AI_INSIGHTS',     N'AI_DASHBOARD'),
        (N'AI_INSIGHTS',     N'AI_RECOMMENDATIONS'),
        (N'CUSTOM_BRANDING', N'GYM_BRANDING')
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

/* ========== FEATURE → API ROUTE MAPPINGS ========== */
;WITH Routes AS (
    SELECT FeatureCode, RoutePrefix FROM (VALUES
        (N'DASHBOARD',       N'/api/dashboard'),
        (N'REPORTS',         N'/api/analytics'),
        (N'CRM',             N'/api/leads'),
        (N'MEMBERS',         N'/api/members'),
        (N'MEMBERSHIPS',     N'/api/memberships'),
        (N'MEMBERSHIPS',     N'/api/membership-plans'),
        (N'PAYMENTS',        N'/api/payments'),
        (N'PAYMENTS',        N'/api/revenue'),
        (N'ATTENDANCE',      N'/api/attendance'),
        (N'DIET_PLANS',      N'/api/diet-plans'),
        (N'WORKOUT_PLANS',   N'/api/workout-plans'),
        (N'NOTIFICATIONS',   N'/api/notifications'),
        (N'NOTIFICATIONS',   N'/api/mobile-notifications'),
        (N'NOTIFICATIONS',   N'/api/mobile'),
        (N'TRAINERS',        N'/api/trainers'),
        (N'MULTI_BRANCH',    N'/api/branches'),
        (N'AI_INSIGHTS',     N'/api/ai'),
        (N'WEBSITE_BUILDER', N'/api/website'),
        (N'WHITE_LABEL',     N'/api/white-label'),
        (N'CUSTOM_BRANDING', N'/api/white-label'),
        (N'SUBSCRIPTIONS',   N'/api/saas'),
        (N'REPORTS',         N'/api/financial'),
        (N'REPORTS',         N'/api/expenses'),
        (N'REPORTS',         N'/api/payroll')
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

CREATE OR ALTER FUNCTION dbo.fn_CalculateSubscriptionPeriodEnd
(
    @PeriodStart DATETIME2,
    @DurationValue INT,
    @DurationUnit NVARCHAR(20)
)
RETURNS DATETIME2
AS
BEGIN
    IF @DurationValue IS NULL OR @DurationValue <= 0
        RETURN DATEADD(MONTH, 1, @PeriodStart);

    DECLARE @Unit NVARCHAR(20) = UPPER(LTRIM(RTRIM(@DurationUnit)));

    RETURN CASE @Unit
        WHEN N'DAY'   THEN DATEADD(DAY,   @DurationValue, @PeriodStart)
        WHEN N'DAYS'  THEN DATEADD(DAY,   @DurationValue, @PeriodStart)
        WHEN N'MONTH' THEN DATEADD(MONTH, @DurationValue, @PeriodStart)
        WHEN N'MONTHS' THEN DATEADD(MONTH, @DurationValue, @PeriodStart)
        WHEN N'YEAR'  THEN DATEADD(YEAR,  @DurationValue, @PeriodStart)
        WHEN N'YEARS' THEN DATEADD(YEAR,  @DurationValue, @PeriodStart)
        ELSE DATEADD(MONTH, @DurationValue, @PeriodStart)
    END;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Feature_GetAll
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        FeatureId, FeatureCode, FeatureName, Description, Category,
        MenuRoute, MenuIcon, IsMenuFeature, IsApiFeature, IsQuotaFeature,
        SortOrder, IsActive
    FROM dbo.SystemFeatures
    WHERE @IncludeInactive = 1 OR IsActive = 1
    ORDER BY Category, SortOrder, FeatureName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Feature_GetMenuCodes
    @FeatureCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.MenuCode
    FROM dbo.SystemFeatures f
    INNER JOIN dbo.FeatureMenus fm ON fm.FeatureId = f.FeatureId
    INNER JOIN dbo.Menus m ON m.MenuId = fm.MenuId
    WHERE f.FeatureCode = @FeatureCode AND f.IsActive = 1 AND m.IsActive = 1
    ORDER BY m.SortOrder;
END
GO
