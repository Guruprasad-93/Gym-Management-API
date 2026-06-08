/*
  White Label SaaS Module
  Branding, domains, email templates, mobile settings, platform analytics
*/

IF OBJECT_ID(N'dbo.WhiteLabelSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WhiteLabelSettings
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        BrandName NVARCHAR(200) NOT NULL,
        LogoFileId BIGINT NULL,
        FaviconFileId BIGINT NULL,
        PrimaryColor NVARCHAR(20) NULL,
        SecondaryColor NVARCHAR(20) NULL,
        LoginBackgroundFileId BIGINT NULL,
        AppDisplayName NVARCHAR(200) NULL,
        SupportEmail NVARCHAR(256) NULL,
        SupportPhone NVARCHAR(20) NULL,
        CustomDomain NVARCHAR(253) NULL,
        SubDomain NVARCHAR(100) NULL,
        IsWhiteLabelEnabled BIT NOT NULL CONSTRAINT DF_WhiteLabelSettings_Enabled DEFAULT (0),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_WhiteLabelSettings_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_WhiteLabelSettings_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_WhiteLabelSettings_LogoFile FOREIGN KEY (LogoFileId) REFERENCES dbo.Files (FileId),
        CONSTRAINT FK_WhiteLabelSettings_FaviconFile FOREIGN KEY (FaviconFileId) REFERENCES dbo.Files (FileId),
        CONSTRAINT FK_WhiteLabelSettings_LoginBgFile FOREIGN KEY (LoginBackgroundFileId) REFERENCES dbo.Files (FileId)
    );
    CREATE UNIQUE INDEX UX_WhiteLabelSettings_GymId ON dbo.WhiteLabelSettings (GymId);
    CREATE UNIQUE INDEX UX_WhiteLabelSettings_SubDomain ON dbo.WhiteLabelSettings (SubDomain) WHERE SubDomain IS NOT NULL;
    CREATE UNIQUE INDEX UX_WhiteLabelSettings_CustomDomain ON dbo.WhiteLabelSettings (CustomDomain) WHERE CustomDomain IS NOT NULL;
END
GO

IF OBJECT_ID(N'dbo.WhiteLabelEmailTemplates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WhiteLabelEmailTemplates
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        TemplateName NVARCHAR(100) NOT NULL,
        Subject NVARCHAR(300) NOT NULL,
        Body NVARCHAR(MAX) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_WhiteLabelEmailTemplates_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_WhiteLabelEmailTemplates_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_WhiteLabelEmailTemplates_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId)
    );
    CREATE UNIQUE INDEX UX_WhiteLabelEmailTemplates_Gym_Name ON dbo.WhiteLabelEmailTemplates (GymId, TemplateName);
END
GO

IF OBJECT_ID(N'dbo.WhiteLabelMobileSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WhiteLabelMobileSettings
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        AppName NVARCHAR(200) NULL,
        SplashScreenFileId BIGINT NULL,
        AppIconFileId BIGINT NULL,
        AndroidPackageName NVARCHAR(200) NULL,
        IOSBundleId NVARCHAR(200) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_WhiteLabelMobileSettings_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_WhiteLabelMobileSettings_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_WhiteLabelMobileSettings_SplashFile FOREIGN KEY (SplashScreenFileId) REFERENCES dbo.Files (FileId),
        CONSTRAINT FK_WhiteLabelMobileSettings_AppIconFile FOREIGN KEY (AppIconFileId) REFERENCES dbo.Files (FileId)
    );
    CREATE UNIQUE INDEX UX_WhiteLabelMobileSettings_GymId ON dbo.WhiteLabelMobileSettings (GymId);
END
GO

/* ========== SETTINGS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_UpsertWhiteLabelSettings
    @GymId UNIQUEIDENTIFIER,
    @BrandName NVARCHAR(200),
    @LogoFileId BIGINT = NULL,
    @FaviconFileId BIGINT = NULL,
    @PrimaryColor NVARCHAR(20) = NULL,
    @SecondaryColor NVARCHAR(20) = NULL,
    @LoginBackgroundFileId BIGINT = NULL,
    @AppDisplayName NVARCHAR(200) = NULL,
    @SupportEmail NVARCHAR(256) = NULL,
    @SupportPhone NVARCHAR(20) = NULL,
    @CustomDomain NVARCHAR(253) = NULL,
    @SubDomain NVARCHAR(100) = NULL,
    @IsWhiteLabelEnabled BIT = 0,
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @SubDomain = NULLIF(LOWER(LTRIM(RTRIM(@SubDomain))), N'');
    SET @CustomDomain = NULLIF(LOWER(LTRIM(RTRIM(@CustomDomain))), N'');

    IF @SubDomain IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.WhiteLabelSettings WHERE SubDomain = @SubDomain AND GymId <> @GymId)
        THROW 50001, N'SubDomain is already in use.', 1;
    IF @CustomDomain IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.WhiteLabelSettings WHERE CustomDomain = @CustomDomain AND GymId <> @GymId)
        THROW 50001, N'CustomDomain is already in use.', 1;

    IF EXISTS (SELECT 1 FROM dbo.WhiteLabelSettings WHERE GymId = @GymId)
    BEGIN
        UPDATE dbo.WhiteLabelSettings SET
            BrandName = @BrandName, LogoFileId = @LogoFileId, FaviconFileId = @FaviconFileId,
            PrimaryColor = @PrimaryColor, SecondaryColor = @SecondaryColor,
            LoginBackgroundFileId = @LoginBackgroundFileId, AppDisplayName = @AppDisplayName,
            SupportEmail = @SupportEmail, SupportPhone = @SupportPhone,
            CustomDomain = @CustomDomain, SubDomain = @SubDomain,
            IsWhiteLabelEnabled = @IsWhiteLabelEnabled, UpdatedAt = SYSUTCDATETIME()
        WHERE GymId = @GymId;
        SELECT @Id = Id FROM dbo.WhiteLabelSettings WHERE GymId = @GymId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.WhiteLabelSettings (GymId, BrandName, LogoFileId, FaviconFileId, PrimaryColor, SecondaryColor,
            LoginBackgroundFileId, AppDisplayName, SupportEmail, SupportPhone, CustomDomain, SubDomain, IsWhiteLabelEnabled)
        VALUES (@GymId, @BrandName, @LogoFileId, @FaviconFileId, @PrimaryColor, @SecondaryColor,
            @LoginBackgroundFileId, @AppDisplayName, @SupportEmail, @SupportPhone, @CustomDomain, @SubDomain, @IsWhiteLabelEnabled);
        SET @Id = SCOPE_IDENTITY();
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWhiteLabelSettings
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT w.*,
           lf.PublicUrl AS LogoUrl, ff.PublicUrl AS FaviconUrl, bg.PublicUrl AS LoginBackgroundUrl
    FROM dbo.WhiteLabelSettings w
    LEFT JOIN dbo.Files lf ON lf.FileId = w.LogoFileId
    LEFT JOIN dbo.Files ff ON ff.FileId = w.FaviconFileId
    LEFT JOIN dbo.Files bg ON bg.FileId = w.LoginBackgroundFileId
    WHERE w.GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SetWhiteLabelEnabled
    @GymId UNIQUEIDENTIFIER,
    @IsWhiteLabelEnabled BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.WhiteLabelSettings SET IsWhiteLabelEnabled = @IsWhiteLabelEnabled, UpdatedAt = SYSUTCDATETIME()
    WHERE GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateWhiteLabelDomain
    @GymId UNIQUEIDENTIFIER,
    @SubDomain NVARCHAR(100) = NULL,
    @CustomDomain NVARCHAR(253) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @SubDomain = NULLIF(LOWER(LTRIM(RTRIM(@SubDomain))), N'');
    SET @CustomDomain = NULLIF(LOWER(LTRIM(RTRIM(@CustomDomain))), N'');

    IF @SubDomain IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.WhiteLabelSettings WHERE SubDomain = @SubDomain AND GymId <> @GymId)
        THROW 50001, N'SubDomain is already in use.', 1;
    IF @CustomDomain IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.WhiteLabelSettings WHERE CustomDomain = @CustomDomain AND GymId <> @GymId)
        THROW 50001, N'CustomDomain is already in use.', 1;

    UPDATE dbo.WhiteLabelSettings SET SubDomain = @SubDomain, CustomDomain = @CustomDomain, UpdatedAt = SYSUTCDATETIME()
    WHERE GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWhiteLabelBySubDomain
    @SubDomain NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT w.*, lf.PublicUrl AS LogoUrl, ff.PublicUrl AS FaviconUrl, bg.PublicUrl AS LoginBackgroundUrl
    FROM dbo.WhiteLabelSettings w
    LEFT JOIN dbo.Files lf ON lf.FileId = w.LogoFileId
    LEFT JOIN dbo.Files ff ON ff.FileId = w.FaviconFileId
    LEFT JOIN dbo.Files bg ON bg.FileId = w.LoginBackgroundFileId
    WHERE w.SubDomain = LOWER(LTRIM(RTRIM(@SubDomain))) AND w.IsWhiteLabelEnabled = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWhiteLabelByCustomDomain
    @CustomDomain NVARCHAR(253)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT w.*, lf.PublicUrl AS LogoUrl, ff.PublicUrl AS FaviconUrl, bg.PublicUrl AS LoginBackgroundUrl
    FROM dbo.WhiteLabelSettings w
    LEFT JOIN dbo.Files lf ON lf.FileId = w.LogoFileId
    LEFT JOIN dbo.Files ff ON ff.FileId = w.FaviconFileId
    LEFT JOIN dbo.Files bg ON bg.FileId = w.LoginBackgroundFileId
    WHERE w.CustomDomain = LOWER(LTRIM(RTRIM(@CustomDomain))) AND w.IsWhiteLabelEnabled = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWhiteLabelLoginBranding
    @GymId UNIQUEIDENTIFIER = NULL,
    @SubDomain NVARCHAR(100) = NULL,
    @CustomDomain NVARCHAR(253) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 w.GymId, w.BrandName, w.AppDisplayName, w.PrimaryColor, w.SecondaryColor,
           w.SupportEmail, w.SupportPhone, lf.PublicUrl AS LogoUrl, bg.PublicUrl AS LoginBackgroundUrl
    FROM dbo.WhiteLabelSettings w
    LEFT JOIN dbo.Files lf ON lf.FileId = w.LogoFileId
    LEFT JOIN dbo.Files bg ON bg.FileId = w.LoginBackgroundFileId
    WHERE w.IsWhiteLabelEnabled = 1
      AND ((@GymId IS NOT NULL AND w.GymId = @GymId)
        OR (@SubDomain IS NOT NULL AND w.SubDomain = LOWER(LTRIM(RTRIM(@SubDomain))))
        OR (@CustomDomain IS NOT NULL AND w.CustomDomain = LOWER(LTRIM(RTRIM(@CustomDomain)))));
END
GO

/* ========== EMAIL TEMPLATES ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateWhiteLabelEmailTemplate
    @GymId UNIQUEIDENTIFIER, @TemplateName NVARCHAR(100), @Subject NVARCHAR(300), @Body NVARCHAR(MAX), @IsActive BIT = 1, @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.WhiteLabelEmailTemplates (GymId, TemplateName, Subject, Body, IsActive)
    VALUES (@GymId, @TemplateName, @Subject, @Body, @IsActive);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateWhiteLabelEmailTemplate
    @GymId UNIQUEIDENTIFIER, @Id INT, @TemplateName NVARCHAR(100), @Subject NVARCHAR(300), @Body NVARCHAR(MAX), @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.WhiteLabelEmailTemplates SET TemplateName = @TemplateName, Subject = @Subject, Body = @Body,
        IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME() WHERE Id = @Id AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteWhiteLabelEmailTemplate
    @GymId UNIQUEIDENTIFIER, @Id INT
AS
BEGIN SET NOCOUNT ON; DELETE FROM dbo.WhiteLabelEmailTemplates WHERE Id = @Id AND GymId = @GymId; END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWhiteLabelEmailTemplates
    @GymId UNIQUEIDENTIFIER
AS
BEGIN SET NOCOUNT ON; SELECT * FROM dbo.WhiteLabelEmailTemplates WHERE GymId = @GymId ORDER BY TemplateName; END
GO

/* ========== MOBILE SETTINGS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_UpsertWhiteLabelMobileSettings
    @GymId UNIQUEIDENTIFIER,
    @AppName NVARCHAR(200) = NULL,
    @SplashScreenFileId BIGINT = NULL,
    @AppIconFileId BIGINT = NULL,
    @AndroidPackageName NVARCHAR(200) = NULL,
    @IOSBundleId NVARCHAR(200) = NULL,
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.WhiteLabelMobileSettings WHERE GymId = @GymId)
    BEGIN
        UPDATE dbo.WhiteLabelMobileSettings SET AppName = @AppName, SplashScreenFileId = @SplashScreenFileId,
            AppIconFileId = @AppIconFileId, AndroidPackageName = @AndroidPackageName, IOSBundleId = @IOSBundleId,
            UpdatedAt = SYSUTCDATETIME() WHERE GymId = @GymId;
        SELECT @Id = Id FROM dbo.WhiteLabelMobileSettings WHERE GymId = @GymId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.WhiteLabelMobileSettings (GymId, AppName, SplashScreenFileId, AppIconFileId, AndroidPackageName, IOSBundleId)
        VALUES (@GymId, @AppName, @SplashScreenFileId, @AppIconFileId, @AndroidPackageName, @IOSBundleId);
        SET @Id = SCOPE_IDENTITY();
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWhiteLabelMobileSettings
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.*, sf.PublicUrl AS SplashScreenUrl, ic.PublicUrl AS AppIconUrl
    FROM dbo.WhiteLabelMobileSettings m
    LEFT JOIN dbo.Files sf ON sf.FileId = m.SplashScreenFileId
    LEFT JOIN dbo.Files ic ON ic.FileId = m.AppIconFileId
    WHERE m.GymId = @GymId;
END
GO

/* ========== PLATFORM DASHBOARD ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetWhiteLabelPlatformDashboard
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.WhiteLabelSettings WHERE IsWhiteLabelEnabled = 1) AS TotalWhiteLabelCustomers,
        (SELECT COUNT(*) FROM dbo.WhiteLabelSettings WHERE IsWhiteLabelEnabled = 1 AND SubDomain IS NOT NULL) AS SubDomainCustomers,
        (SELECT COUNT(*) FROM dbo.WhiteLabelSettings WHERE IsWhiteLabelEnabled = 1 AND CustomDomain IS NOT NULL) AS CustomDomainCustomers,
        (SELECT ISNULL(SUM(sp.MonthlyPrice), 0) FROM dbo.GymSubscriptions gs
            INNER JOIN dbo.WhiteLabelSettings wl ON wl.GymId = gs.GymId AND wl.IsWhiteLabelEnabled = 1
            INNER JOIN dbo.SaasSubscriptionPlans sp ON sp.SaasPlanId = gs.SaasPlanId
            WHERE gs.Status = N'Active') AS WhiteLabelMonthlyRevenue,
        (SELECT COUNT(*) FROM dbo.GymSubscriptions gs
            INNER JOIN dbo.WhiteLabelSettings wl ON wl.GymId = gs.GymId AND wl.IsWhiteLabelEnabled = 1
            WHERE gs.Status = N'Active' AND gs.CurrentPeriodEnd <= DATEADD(DAY, 30, SYSUTCDATETIME())) AS ExpiringWhiteLabelPlans;

    SELECT TOP 10 wl.BrandName, wl.SubDomain, wl.CustomDomain, gs.Status, gs.CurrentPeriodEnd
    FROM dbo.WhiteLabelSettings wl
    LEFT JOIN dbo.GymSubscriptions gs ON gs.GymId = wl.GymId
    WHERE wl.IsWhiteLabelEnabled = 1
    ORDER BY gs.CurrentPeriodEnd;

    SELECT CAST(wl.CreatedAt AS DATE) AS AdoptionDate, COUNT(*) AS EnabledCount
    FROM dbo.WhiteLabelSettings wl
    WHERE wl.IsWhiteLabelEnabled = 1 AND wl.CreatedAt >= DATEADD(MONTH, -12, SYSUTCDATETIME())
    GROUP BY CAST(wl.CreatedAt AS DATE)
    ORDER BY AdoptionDate;
END
GO

/* Seed default email templates for gyms with white label enabled */
INSERT INTO dbo.WhiteLabelEmailTemplates (GymId, TemplateName, Subject, Body, IsActive)
SELECT wl.GymId, t.TemplateName, t.Subject, t.Body, 1
FROM dbo.WhiteLabelSettings wl
CROSS APPLY (VALUES
    (N'WelcomeEmail', N'Welcome to {{brandName}}', N'<p>Welcome to {{brandName}}! We are excited to have you.</p>'),
    (N'PasswordReset', N'Reset your {{brandName}} password', N'<p>Use this link to reset your password for {{brandName}}.</p>'),
    (N'MembershipRenewal', N'Your {{brandName}} membership renewal', N'<p>Your membership at {{brandName}} is due for renewal.</p>'),
    (N'TrialExpiry', N'Your trial at {{brandName}} is ending', N'<p>Your trial period at {{brandName}} is expiring soon.</p>')
) t(TemplateName, Subject, Body)
WHERE wl.IsWhiteLabelEnabled = 1
  AND NOT EXISTS (SELECT 1 FROM dbo.WhiteLabelEmailTemplates e WHERE e.GymId = wl.GymId AND e.TemplateName = t.TemplateName);
GO
