/*
  Tenant Menu & Feature Management
  - Menus catalog (all application modules)
  - GymMenus per-tenant enable/disable
  - Default: all menus enabled for existing gyms (no breaking changes)
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Menus', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Menus
        (
            MenuId        INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
            MenuCode      NVARCHAR(50)  NOT NULL,
            MenuName      NVARCHAR(100) NOT NULL,
            ParentMenuId  INT NULL,
            Route         NVARCHAR(200) NULL,
            Icon          NVARCHAR(50)  NULL,
            SortOrder     INT NOT NULL CONSTRAINT DF_Menus_SortOrder DEFAULT (0),
            IsActive      BIT NOT NULL CONSTRAINT DF_Menus_IsActive DEFAULT (1),
            CONSTRAINT UQ_Menus_MenuCode UNIQUE (MenuCode),
            CONSTRAINT FK_Menus_Parent FOREIGN KEY (ParentMenuId) REFERENCES dbo.Menus(MenuId)
        );
    END

    IF OBJECT_ID(N'dbo.GymMenus', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.GymMenus
        (
            GymMenuId  INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
            GymId      UNIQUEIDENTIFIER NOT NULL,
            MenuId     INT NOT NULL,
            IsEnabled  BIT NOT NULL CONSTRAINT DF_GymMenus_IsEnabled DEFAULT (1),
            EnabledOn  DATETIME2 NULL,
            EnabledBy  UNIQUEIDENTIFIER NULL,
            CONSTRAINT UQ_GymMenus_GymId_MenuId UNIQUE (GymId, MenuId),
            CONSTRAINT FK_GymMenus_Gym FOREIGN KEY (GymId) REFERENCES dbo.Gyms(GymId),
            CONSTRAINT FK_GymMenus_Menu FOREIGN KEY (MenuId) REFERENCES dbo.Menus(MenuId)
        );
        CREATE INDEX IX_GymMenus_GymId ON dbo.GymMenus(GymId);
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO

/* ========== SEED MENUS (idempotent) ========== */
MERGE dbo.Menus AS target
USING (VALUES
    (N'DASHBOARD',           N'Dashboard',            NULL, N'/gym-admin/dashboard',              N'dashboard',           10),
    (N'CRM',                 N'CRM',                  NULL, NULL,                                 N'contact_page',        20),
    (N'LEADS',               N'Leads',                N'CRM', N'/gym-admin/leads',                N'contact_page',        21),
    (N'MEMBERS',             N'Members',              NULL, N'/gym-admin/members',                N'groups',              30),
    (N'MEMBERSHIPS',         N'Memberships',          NULL, N'/gym-admin/memberships',            N'event_note',          40),
    (N'MEMBERSHIP_PLANS',    N'Membership Plans',     N'MEMBERSHIPS', N'/gym-admin/membership-plans', N'card_membership', 41),
    (N'ATTENDANCE',          N'Attendance',           NULL, N'/gym-admin/attendance',             N'event_available',     50),
    (N'ATTENDANCE_REPORTS',  N'Attendance Reports',   N'ATTENDANCE', N'/gym-admin/attendance/reports', N'assessment',      51),
    (N'PAYMENTS',            N'Payments',             NULL, N'/gym-admin/payments',               N'payments',            60),
    (N'REVENUE',             N'Revenue',              NULL, N'/gym-admin/revenue',                N'trending_up',         70),
    (N'DIET_PLANS',          N'Diet Plans',           NULL, N'/gym-admin/diet-plans',             N'restaurant_menu',     80),
    (N'WORKOUT_PLANS',       N'Workout Plans',        NULL, N'/gym-admin/workout-plans',          N'fitness_center',      90),
    (N'BOOKINGS',            N'Bookings',             NULL, N'/gym-admin/bookings',               N'event_available',    100),
    (N'CLASS_SCHEDULES',     N'Class Schedules',      N'BOOKINGS', N'/gym-admin/schedules',         N'calendar_month',     101),
    (N'BOOKING_ANALYTICS',   N'Booking Analytics',    N'BOOKINGS', N'/gym-admin/booking-analytics',  N'insights',           102),
    (N'NOTIFICATIONS',       N'Notifications',        NULL, N'/gym-admin/notifications',          N'notifications',      110),
    (N'MOBILE_PUSH',         N'Mobile Push',          N'NOTIFICATIONS', N'/gym-admin/mobile-notifications', N'phonelink_ring', 111),
    (N'MOBILE_ANALYTICS',    N'Mobile Analytics',     N'NOTIFICATIONS', N'/gym-admin/mobile-analytics', N'insights',         112),
    (N'REPORTS',             N'Reports',              NULL, NULL,                                 N'assessment',         120),
    (N'REVENUE_ANALYTICS',   N'Revenue Analytics',    N'REPORTS', N'/gym-admin/analytics/revenue',  N'trending_up',        121),
    (N'MEMBER_ANALYTICS',    N'Member Analytics',     N'REPORTS', N'/gym-admin/analytics/members',  N'groups',             122),
    (N'ATTENDANCE_ANALYTICS',N'Attendance Analytics', N'REPORTS', N'/gym-admin/analytics/attendance', N'bar_chart',        123),
    (N'TRAINER_ANALYTICS',   N'Trainer Analytics',    N'REPORTS', N'/gym-admin/analytics/trainers', N'sports',             124),
    (N'FINANCIAL',           N'Financial',            N'REPORTS', N'/gym-admin/financial',           N'account_balance',    125),
    (N'ANALYTICS',           N'Analytics',            NULL, NULL,                                 N'insights',           130),
    (N'TRAINERS',            N'Trainers',             NULL, N'/gym-admin/trainers',               N'sports',             140),
    (N'STAFF',               N'Staff',                NULL, N'/gym-admin/gym-admins',             N'badge',              150),
    (N'BRANCHES',            N'Branches',             NULL, N'/gym-admin/branches',               N'store',              160),
    (N'BRANCH_DASHBOARD',    N'Branch Dashboard',     N'BRANCHES', N'/gym-admin/branches/dashboard', N'dashboard',        161),
    (N'BRANCH_ANALYTICS',    N'Branch Analytics',     N'BRANCHES', N'/gym-admin/branches/analytics', N'leaderboard',      162),
    (N'BRANCH_TRANSFERS',    N'Branch Transfers',     N'BRANCHES', N'/gym-admin/branches/transfers', N'swap_horiz',       163),
    (N'BRANCH_TARGETS',      N'Branch Targets',       N'BRANCHES', N'/gym-admin/branches/targets',   N'flag',             164),
    (N'INVENTORY',           N'Inventory',            NULL, N'/gym-admin/inventory',              N'inventory_2',        170),
    (N'POS',                 N'POS',                  NULL, N'/gym-admin/pos',                    N'point_of_sale',      180),
    (N'AI_INSIGHTS',         N'AI Insights',          NULL, N'/gym-admin/ai',                    N'psychology',         190),
    (N'AI_DASHBOARD',        N'AI Dashboard',         N'AI_INSIGHTS', N'/gym-admin/ai',           N'psychology',         191),
    (N'AI_RECOMMENDATIONS',  N'AI Recommendations',   N'AI_INSIGHTS', N'/trainer/ai-recommendations', N'lightbulb',      192),
    (N'PUBLIC_WEBSITE',      N'Public Website',       NULL, NULL,                                 N'language',           200),
    (N'WEBSITE_BUILDER',     N'Website Builder',      N'PUBLIC_WEBSITE', N'/gym-admin/website-builder', N'language',       201),
    (N'WEBSITE_ANALYTICS',   N'Website Analytics',    N'PUBLIC_WEBSITE', N'/gym-admin/website-builder/analytics', N'public', 202),
    (N'WHITE_LABEL',         N'White Label',          NULL, N'/gym-admin/white-label',            N'branding_watermark', 210),
    (N'SUBSCRIPTIONS',       N'Subscriptions',        NULL, N'/gym-admin/subscription',           N'subscriptions',      220),
    (N'AUDIT_LOGS',          N'Audit Logs',           NULL, N'/gym-admin/audit',                  N'history',            230),
    (N'SETTINGS',            N'Settings',             NULL, NULL,                                 N'settings',           240),
    (N'EXPENSES',            N'Expenses',             N'SETTINGS', N'/gym-admin/expenses',          N'receipt_long',       241),
    (N'PAYROLL',             N'Payroll',              N'SETTINGS', N'/gym-admin/payroll',           N'payments',           242),
    (N'GYM_BRANDING',        N'Gym Branding',         N'SETTINGS', N'/gym-admin/settings/branding',   N'palette',            243)
) AS source (MenuCode, MenuName, ParentMenuCode, Route, Icon, SortOrder)
ON target.MenuCode = source.MenuCode
WHEN MATCHED THEN
    UPDATE SET
        MenuName = source.MenuName,
        Route = source.Route,
        Icon = source.Icon,
        SortOrder = source.SortOrder,
        IsActive = 1
WHEN NOT MATCHED THEN
    INSERT (MenuCode, MenuName, ParentMenuId, Route, Icon, SortOrder, IsActive)
    VALUES (source.MenuCode, source.MenuName, NULL, source.Route, source.Icon, source.SortOrder, 1);
GO

UPDATE child
SET ParentMenuId = parent.MenuId
FROM dbo.Menus child
INNER JOIN (
    SELECT MenuCode, ParentMenuCode FROM (VALUES
        (N'LEADS', N'CRM'),
        (N'MEMBERSHIP_PLANS', N'MEMBERSHIPS'),
        (N'ATTENDANCE_REPORTS', N'ATTENDANCE'),
        (N'CLASS_SCHEDULES', N'BOOKINGS'),
        (N'BOOKING_ANALYTICS', N'BOOKINGS'),
        (N'MOBILE_PUSH', N'NOTIFICATIONS'),
        (N'MOBILE_ANALYTICS', N'NOTIFICATIONS'),
        (N'REVENUE_ANALYTICS', N'REPORTS'),
        (N'MEMBER_ANALYTICS', N'REPORTS'),
        (N'ATTENDANCE_ANALYTICS', N'REPORTS'),
        (N'TRAINER_ANALYTICS', N'REPORTS'),
        (N'FINANCIAL', N'REPORTS'),
        (N'BRANCH_DASHBOARD', N'BRANCHES'),
        (N'BRANCH_ANALYTICS', N'BRANCHES'),
        (N'BRANCH_TRANSFERS', N'BRANCHES'),
        (N'BRANCH_TARGETS', N'BRANCHES'),
        (N'AI_DASHBOARD', N'AI_INSIGHTS'),
        (N'AI_RECOMMENDATIONS', N'AI_INSIGHTS'),
        (N'WEBSITE_BUILDER', N'PUBLIC_WEBSITE'),
        (N'WEBSITE_ANALYTICS', N'PUBLIC_WEBSITE'),
        (N'EXPENSES', N'SETTINGS'),
        (N'PAYROLL', N'SETTINGS'),
        (N'GYM_BRANDING', N'SETTINGS')
    ) AS map(MenuCode, ParentMenuCode)
) m ON child.MenuCode = m.MenuCode
INNER JOIN dbo.Menus parent ON parent.MenuCode = m.ParentMenuCode;
GO

/* Seed GymMenus for all existing gyms (all enabled) */
INSERT INTO dbo.GymMenus (GymId, MenuId, IsEnabled, EnabledOn)
SELECT g.GymId, m.MenuId, 1, SYSUTCDATETIME()
FROM dbo.Gyms g
CROSS JOIN dbo.Menus m
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.GymMenus gm
    WHERE gm.GymId = g.GymId AND gm.MenuId = m.MenuId
);
GO

/* ========== STORED PROCEDURES ========== */
CREATE OR ALTER PROCEDURE dbo.sp_Menu_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MenuId, MenuCode, MenuName, ParentMenuId, Route, Icon, SortOrder, IsActive
    FROM dbo.Menus
    WHERE IsActive = 1
    ORDER BY SortOrder, MenuName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymMenu_GetEnabledCodes
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.MenuCode
    FROM dbo.GymMenus gm
    INNER JOIN dbo.Menus m ON m.MenuId = gm.MenuId
    WHERE gm.GymId = @GymId
      AND gm.IsEnabled = 1
      AND m.IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymMenu_IsEnabled
    @GymId UNIQUEIDENTIFIER,
    @MenuCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM dbo.GymMenus gm
        INNER JOIN dbo.Menus m ON m.MenuId = gm.MenuId
        WHERE gm.GymId = @GymId
          AND m.MenuCode = @MenuCode
          AND gm.IsEnabled = 1
          AND m.IsActive = 1
    ) THEN 1 ELSE 0 END AS BIT) AS IsEnabled;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymMenu_GetByGymId
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        gm.GymMenuId,
        gm.GymId,
        m.MenuId,
        m.MenuCode,
        m.MenuName,
        m.ParentMenuId,
        m.Route,
        m.Icon,
        m.SortOrder,
        ISNULL(gm.IsEnabled, 0) AS IsEnabled,
        gm.EnabledOn,
        gm.EnabledBy
    FROM dbo.Menus m
    LEFT JOIN dbo.GymMenus gm ON gm.MenuId = m.MenuId AND gm.GymId = @GymId
    WHERE m.IsActive = 1
    ORDER BY m.SortOrder, m.MenuName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymMenu_SetEnabled
    @GymId UNIQUEIDENTIFIER,
    @MenuId INT,
    @IsEnabled BIT,
    @EnabledBy UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Gyms WHERE GymId = @GymId)
        THROW 50060, 'Gym not found.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Menus WHERE MenuId = @MenuId AND IsActive = 1)
        THROW 50061, 'Menu not found.', 1;

    IF EXISTS (SELECT 1 FROM dbo.GymMenus WHERE GymId = @GymId AND MenuId = @MenuId)
    BEGIN
        UPDATE dbo.GymMenus
        SET IsEnabled = @IsEnabled,
            EnabledOn = CASE WHEN @IsEnabled = 1 THEN SYSUTCDATETIME() ELSE EnabledOn END,
            EnabledBy = CASE WHEN @IsEnabled = 1 THEN @EnabledBy ELSE EnabledBy END
        WHERE GymId = @GymId AND MenuId = @MenuId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.GymMenus (GymId, MenuId, IsEnabled, EnabledOn, EnabledBy)
        VALUES (@GymId, @MenuId, @IsEnabled, CASE WHEN @IsEnabled = 1 THEN SYSUTCDATETIME() ELSE NULL END, @EnabledBy);
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymMenu_BulkSetEnabled
    @GymId UNIQUEIDENTIFIER,
    @MenuIds NVARCHAR(MAX),
    @IsEnabled BIT,
    @EnabledBy UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Gyms WHERE GymId = @GymId)
        THROW 50060, 'Gym not found.', 1;

    DECLARE @Ids TABLE (MenuId INT PRIMARY KEY);
    INSERT INTO @Ids (MenuId)
    SELECT TRY_CAST(value AS INT)
    FROM STRING_SPLIT(@MenuIds, ',')
    WHERE TRY_CAST(value AS INT) IS NOT NULL;

    MERGE dbo.GymMenus AS target
    USING (
        SELECT @GymId AS GymId, i.MenuId
        FROM @Ids i
        INNER JOIN dbo.Menus m ON m.MenuId = i.MenuId AND m.IsActive = 1
    ) AS source
    ON target.GymId = source.GymId AND target.MenuId = source.MenuId
    WHEN MATCHED THEN
        UPDATE SET
            IsEnabled = @IsEnabled,
            EnabledOn = CASE WHEN @IsEnabled = 1 THEN SYSUTCDATETIME() ELSE target.EnabledOn END,
            EnabledBy = CASE WHEN @IsEnabled = 1 THEN @EnabledBy ELSE target.EnabledBy END
    WHEN NOT MATCHED THEN
        INSERT (GymId, MenuId, IsEnabled, EnabledOn, EnabledBy)
        VALUES (source.GymId, source.MenuId, @IsEnabled,
                CASE WHEN @IsEnabled = 1 THEN SYSUTCDATETIME() ELSE NULL END,
                CASE WHEN @IsEnabled = 1 THEN @EnabledBy ELSE NULL END);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymMenu_SeedForGym
    @GymId UNIQUEIDENTIFIER,
    @EnabledBy UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.GymMenus (GymId, MenuId, IsEnabled, EnabledOn, EnabledBy)
    SELECT @GymId, m.MenuId, 1, SYSUTCDATETIME(), @EnabledBy
    FROM dbo.Menus m
    WHERE m.IsActive = 1
      AND NOT EXISTS (
          SELECT 1 FROM dbo.GymMenus gm
          WHERE gm.GymId = @GymId AND gm.MenuId = m.MenuId
      );
END
GO
