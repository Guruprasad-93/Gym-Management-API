/*
  Public Gym Website Builder Module
  Settings, pages, sections, gallery, testimonials, website leads, analytics
*/

IF OBJECT_ID(N'dbo.GymWebsiteSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GymWebsiteSettings
    (
        GymId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        WebsiteSlug NVARCHAR(100) NOT NULL,
        WebsiteTitle NVARCHAR(200) NULL,
        WebsiteDescription NVARCHAR(1000) NULL,
        LogoFileId BIGINT NULL,
        BannerFileId BIGINT NULL,
        PrimaryColor NVARCHAR(20) NULL,
        SecondaryColor NVARCHAR(20) NULL,
        ContactPhone NVARCHAR(20) NULL,
        ContactEmail NVARCHAR(256) NULL,
        WhatsAppNumber NVARCHAR(20) NULL,
        [Address] NVARCHAR(500) NULL,
        GoogleMapEmbedUrl NVARCHAR(1000) NULL,
        FacebookUrl NVARCHAR(500) NULL,
        InstagramUrl NVARCHAR(500) NULL,
        YoutubeUrl NVARCHAR(500) NULL,
        MetaTitle NVARCHAR(200) NULL,
        MetaDescription NVARCHAR(500) NULL,
        MetaKeywords NVARCHAR(500) NULL,
        IsPublished BIT NOT NULL CONSTRAINT DF_GymWebsiteSettings_IsPublished DEFAULT (0),
        PublishedDate DATETIME2 NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_GymWebsiteSettings_CreatedDate DEFAULT (SYSUTCDATETIME()),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT FK_GymWebsiteSettings_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_GymWebsiteSettings_LogoFile FOREIGN KEY (LogoFileId) REFERENCES dbo.Files (FileId),
        CONSTRAINT FK_GymWebsiteSettings_BannerFile FOREIGN KEY (BannerFileId) REFERENCES dbo.Files (FileId)
    );
    CREATE UNIQUE INDEX UX_GymWebsiteSettings_Slug ON dbo.GymWebsiteSettings (WebsiteSlug);
END
GO

IF OBJECT_ID(N'dbo.GymWebsitePages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GymWebsitePages
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        PageName NVARCHAR(200) NOT NULL,
        Slug NVARCHAR(100) NOT NULL,
        PageContent NVARCHAR(MAX) NULL,
        DisplayOrder INT NOT NULL CONSTRAINT DF_GymWebsitePages_DisplayOrder DEFAULT (0),
        IsActive BIT NOT NULL CONSTRAINT DF_GymWebsitePages_IsActive DEFAULT (1),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_GymWebsitePages_CreatedDate DEFAULT (SYSUTCDATETIME()),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT FK_GymWebsitePages_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId)
    );
    CREATE UNIQUE INDEX UX_GymWebsitePages_Gym_Slug ON dbo.GymWebsitePages (GymId, Slug);
    CREATE INDEX IX_GymWebsitePages_GymId ON dbo.GymWebsitePages (GymId, DisplayOrder);
END
GO

IF OBJECT_ID(N'dbo.GymWebsiteSections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GymWebsiteSections
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        SectionType NVARCHAR(50) NOT NULL,
        Title NVARCHAR(200) NULL,
        Subtitle NVARCHAR(500) NULL,
        [Description] NVARCHAR(MAX) NULL,
        ImageFileId BIGINT NULL,
        DisplayOrder INT NOT NULL CONSTRAINT DF_GymWebsiteSections_DisplayOrder DEFAULT (0),
        IsVisible BIT NOT NULL CONSTRAINT DF_GymWebsiteSections_IsVisible DEFAULT (1),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_GymWebsiteSections_CreatedDate DEFAULT (SYSUTCDATETIME()),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT FK_GymWebsiteSections_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_GymWebsiteSections_ImageFile FOREIGN KEY (ImageFileId) REFERENCES dbo.Files (FileId)
    );
    CREATE INDEX IX_GymWebsiteSections_GymId ON dbo.GymWebsiteSections (GymId, DisplayOrder);
END
GO

IF OBJECT_ID(N'dbo.GymWebsiteTestimonials', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GymWebsiteTestimonials
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberName NVARCHAR(200) NOT NULL,
        Rating INT NOT NULL CONSTRAINT DF_GymWebsiteTestimonials_Rating DEFAULT (5),
        ReviewText NVARCHAR(2000) NULL,
        ImageFileId BIGINT NULL,
        IsApproved BIT NOT NULL CONSTRAINT DF_GymWebsiteTestimonials_IsApproved DEFAULT (0),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_GymWebsiteTestimonials_CreatedDate DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_GymWebsiteTestimonials_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_GymWebsiteTestimonials_ImageFile FOREIGN KEY (ImageFileId) REFERENCES dbo.Files (FileId)
    );
    CREATE INDEX IX_GymWebsiteTestimonials_GymId ON dbo.GymWebsiteTestimonials (GymId, IsApproved);
END
GO

IF OBJECT_ID(N'dbo.GymWebsiteGallery', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GymWebsiteGallery
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        FileId BIGINT NOT NULL,
        Caption NVARCHAR(500) NULL,
        DisplayOrder INT NOT NULL CONSTRAINT DF_GymWebsiteGallery_DisplayOrder DEFAULT (0),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_GymWebsiteGallery_CreatedDate DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_GymWebsiteGallery_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_GymWebsiteGallery_Files FOREIGN KEY (FileId) REFERENCES dbo.Files (FileId)
    );
    CREATE INDEX IX_GymWebsiteGallery_GymId ON dbo.GymWebsiteGallery (GymId, DisplayOrder);
END
GO

IF OBJECT_ID(N'dbo.WebsiteLeadCaptures', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WebsiteLeadCaptures
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        LeadId INT NULL,
        Name NVARCHAR(200) NOT NULL,
        MobileNumber NVARCHAR(20) NOT NULL,
        Email NVARCHAR(256) NULL,
        Source NVARCHAR(50) NOT NULL CONSTRAINT DF_WebsiteLeadCaptures_Source DEFAULT (N'Website'),
        InterestedPlan NVARCHAR(200) NULL,
        Notes NVARCHAR(1000) NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_WebsiteLeadCaptures_Status DEFAULT (N'New'),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_WebsiteLeadCaptures_CreatedDate DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_WebsiteLeadCaptures_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_WebsiteLeadCaptures_Leads FOREIGN KEY (LeadId) REFERENCES dbo.Leads (LeadId)
    );
    CREATE INDEX IX_WebsiteLeadCaptures_GymId ON dbo.WebsiteLeadCaptures (GymId, CreatedDate DESC);
    CREATE INDEX IX_WebsiteLeadCaptures_Status ON dbo.WebsiteLeadCaptures (GymId, [Status]);
END
GO

/* ========== SETTINGS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_UpsertGymWebsiteSettings
    @GymId UNIQUEIDENTIFIER,
    @WebsiteSlug NVARCHAR(100),
    @WebsiteTitle NVARCHAR(200) = NULL,
    @WebsiteDescription NVARCHAR(1000) = NULL,
    @LogoFileId BIGINT = NULL,
    @BannerFileId BIGINT = NULL,
    @PrimaryColor NVARCHAR(20) = NULL,
    @SecondaryColor NVARCHAR(20) = NULL,
    @ContactPhone NVARCHAR(20) = NULL,
    @ContactEmail NVARCHAR(256) = NULL,
    @WhatsAppNumber NVARCHAR(20) = NULL,
    @Address NVARCHAR(500) = NULL,
    @GoogleMapEmbedUrl NVARCHAR(1000) = NULL,
    @FacebookUrl NVARCHAR(500) = NULL,
    @InstagramUrl NVARCHAR(500) = NULL,
    @YoutubeUrl NVARCHAR(500) = NULL,
    @MetaTitle NVARCHAR(200) = NULL,
    @MetaDescription NVARCHAR(500) = NULL,
    @MetaKeywords NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @WebsiteSlug = LOWER(LTRIM(RTRIM(@WebsiteSlug)));
    IF EXISTS (SELECT 1 FROM dbo.GymWebsiteSettings WHERE WebsiteSlug = @WebsiteSlug AND GymId <> @GymId)
        THROW 50001, N'Website slug is already in use.', 1;

    IF EXISTS (SELECT 1 FROM dbo.GymWebsiteSettings WHERE GymId = @GymId)
        UPDATE dbo.GymWebsiteSettings SET
            WebsiteSlug = @WebsiteSlug, WebsiteTitle = @WebsiteTitle, WebsiteDescription = @WebsiteDescription,
            LogoFileId = @LogoFileId, BannerFileId = @BannerFileId, PrimaryColor = @PrimaryColor, SecondaryColor = @SecondaryColor,
            ContactPhone = @ContactPhone, ContactEmail = @ContactEmail, WhatsAppNumber = @WhatsAppNumber,
            [Address] = @Address, GoogleMapEmbedUrl = @GoogleMapEmbedUrl,
            FacebookUrl = @FacebookUrl, InstagramUrl = @InstagramUrl, YoutubeUrl = @YoutubeUrl,
            MetaTitle = @MetaTitle, MetaDescription = @MetaDescription, MetaKeywords = @MetaKeywords,
            UpdatedDate = SYSUTCDATETIME()
        WHERE GymId = @GymId;
    ELSE
        INSERT INTO dbo.GymWebsiteSettings (GymId, WebsiteSlug, WebsiteTitle, WebsiteDescription, LogoFileId, BannerFileId,
            PrimaryColor, SecondaryColor, ContactPhone, ContactEmail, WhatsAppNumber, [Address], GoogleMapEmbedUrl,
            FacebookUrl, InstagramUrl, YoutubeUrl, MetaTitle, MetaDescription, MetaKeywords)
        VALUES (@GymId, @WebsiteSlug, @WebsiteTitle, @WebsiteDescription, @LogoFileId, @BannerFileId,
            @PrimaryColor, @SecondaryColor, @ContactPhone, @ContactEmail, @WhatsAppNumber, @Address, @GoogleMapEmbedUrl,
            @FacebookUrl, @InstagramUrl, @YoutubeUrl, @MetaTitle, @MetaDescription, @MetaKeywords);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetGymWebsiteSettings
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT s.*, lf.PublicUrl AS LogoUrl, bf.PublicUrl AS BannerUrl
    FROM dbo.GymWebsiteSettings s
    LEFT JOIN dbo.Files lf ON lf.FileId = s.LogoFileId
    LEFT JOIN dbo.Files bf ON bf.FileId = s.BannerFileId
    WHERE s.GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SetGymWebsitePublished
    @GymId UNIQUEIDENTIFIER,
    @IsPublished BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.GymWebsiteSettings
    SET IsPublished = @IsPublished,
        PublishedDate = CASE WHEN @IsPublished = 1 THEN SYSUTCDATETIME() ELSE PublishedDate END,
        UpdatedDate = SYSUTCDATETIME()
    WHERE GymId = @GymId;
END
GO

/* ========== PAGES ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateGymWebsitePage
    @GymId UNIQUEIDENTIFIER,
    @PageName NVARCHAR(200),
    @Slug NVARCHAR(100),
    @PageContent NVARCHAR(MAX) = NULL,
    @DisplayOrder INT = 0,
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Slug = LOWER(LTRIM(RTRIM(@Slug)));
    INSERT INTO dbo.GymWebsitePages (GymId, PageName, Slug, PageContent, DisplayOrder)
    VALUES (@GymId, @PageName, @Slug, @PageContent, @DisplayOrder);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateGymWebsitePage
    @GymId UNIQUEIDENTIFIER, @Id INT,
    @PageName NVARCHAR(200), @Slug NVARCHAR(100), @PageContent NVARCHAR(MAX) = NULL,
    @DisplayOrder INT = 0, @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET @Slug = LOWER(LTRIM(RTRIM(@Slug)));
    UPDATE dbo.GymWebsitePages SET PageName = @PageName, Slug = @Slug, PageContent = @PageContent,
        DisplayOrder = @DisplayOrder, IsActive = @IsActive, UpdatedDate = SYSUTCDATETIME()
    WHERE Id = @Id AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteGymWebsitePage
    @GymId UNIQUEIDENTIFIER, @Id INT
AS
BEGIN SET NOCOUNT ON; DELETE FROM dbo.GymWebsitePages WHERE Id = @Id AND GymId = @GymId; END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetGymWebsitePages
    @GymId UNIQUEIDENTIFIER, @ActiveOnly BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.GymWebsitePages
    WHERE GymId = @GymId AND (@ActiveOnly = 0 OR IsActive = 1)
    ORDER BY DisplayOrder, PageName;
END
GO

/* ========== SECTIONS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateGymWebsiteSection
    @GymId UNIQUEIDENTIFIER, @SectionType NVARCHAR(50), @Title NVARCHAR(200) = NULL,
    @Subtitle NVARCHAR(500) = NULL, @Description NVARCHAR(MAX) = NULL, @ImageFileId BIGINT = NULL,
    @DisplayOrder INT = 0, @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.GymWebsiteSections (GymId, SectionType, Title, Subtitle, [Description], ImageFileId, DisplayOrder)
    VALUES (@GymId, @SectionType, @Title, @Subtitle, @Description, @ImageFileId, @DisplayOrder);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateGymWebsiteSection
    @GymId UNIQUEIDENTIFIER, @Id INT, @SectionType NVARCHAR(50), @Title NVARCHAR(200) = NULL,
    @Subtitle NVARCHAR(500) = NULL, @Description NVARCHAR(MAX) = NULL, @ImageFileId BIGINT = NULL,
    @DisplayOrder INT = 0, @IsVisible BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.GymWebsiteSections SET SectionType = @SectionType, Title = @Title, Subtitle = @Subtitle,
        [Description] = @Description, ImageFileId = @ImageFileId, DisplayOrder = @DisplayOrder,
        IsVisible = @IsVisible, UpdatedDate = SYSUTCDATETIME()
    WHERE Id = @Id AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteGymWebsiteSection
    @GymId UNIQUEIDENTIFIER, @Id INT
AS
BEGIN SET NOCOUNT ON; DELETE FROM dbo.GymWebsiteSections WHERE Id = @Id AND GymId = @GymId; END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetGymWebsiteSections
    @GymId UNIQUEIDENTIFIER, @VisibleOnly BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT s.*, f.PublicUrl AS ImageUrl
    FROM dbo.GymWebsiteSections s
    LEFT JOIN dbo.Files f ON f.FileId = s.ImageFileId
    WHERE s.GymId = @GymId AND (@VisibleOnly = 0 OR s.IsVisible = 1)
    ORDER BY s.DisplayOrder, s.Id;
END
GO

/* ========== TESTIMONIALS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateGymWebsiteTestimonial
    @GymId UNIQUEIDENTIFIER, @MemberName NVARCHAR(200), @Rating INT, @ReviewText NVARCHAR(2000) = NULL,
    @ImageFileId BIGINT = NULL, @IsApproved BIT = 0, @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.GymWebsiteTestimonials (GymId, MemberName, Rating, ReviewText, ImageFileId, IsApproved)
    VALUES (@GymId, @MemberName, @Rating, @ReviewText, @ImageFileId, @IsApproved);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateGymWebsiteTestimonial
    @GymId UNIQUEIDENTIFIER, @Id INT, @MemberName NVARCHAR(200), @Rating INT,
    @ReviewText NVARCHAR(2000) = NULL, @ImageFileId BIGINT = NULL, @IsApproved BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.GymWebsiteTestimonials SET MemberName = @MemberName, Rating = @Rating, ReviewText = @ReviewText,
        ImageFileId = @ImageFileId, IsApproved = @IsApproved WHERE Id = @Id AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteGymWebsiteTestimonial
    @GymId UNIQUEIDENTIFIER, @Id INT
AS
BEGIN SET NOCOUNT ON; DELETE FROM dbo.GymWebsiteTestimonials WHERE Id = @Id AND GymId = @GymId; END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetGymWebsiteTestimonials
    @GymId UNIQUEIDENTIFIER, @ApprovedOnly BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.*, f.PublicUrl AS ImageUrl
    FROM dbo.GymWebsiteTestimonials t
    LEFT JOIN dbo.Files f ON f.FileId = t.ImageFileId
    WHERE t.GymId = @GymId AND (@ApprovedOnly = 0 OR t.IsApproved = 1)
    ORDER BY t.CreatedDate DESC;
END
GO

/* ========== GALLERY ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateGymWebsiteGalleryItem
    @GymId UNIQUEIDENTIFIER, @FileId BIGINT, @Caption NVARCHAR(500) = NULL, @DisplayOrder INT = 0, @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.GymWebsiteGallery (GymId, FileId, Caption, DisplayOrder) VALUES (@GymId, @FileId, @Caption, @DisplayOrder);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateGymWebsiteGalleryItem
    @GymId UNIQUEIDENTIFIER, @Id INT, @Caption NVARCHAR(500) = NULL, @DisplayOrder INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.GymWebsiteGallery SET Caption = @Caption, DisplayOrder = @DisplayOrder WHERE Id = @Id AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteGymWebsiteGalleryItem
    @GymId UNIQUEIDENTIFIER, @Id INT
AS
BEGIN SET NOCOUNT ON; DELETE FROM dbo.GymWebsiteGallery WHERE Id = @Id AND GymId = @GymId; END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetGymWebsiteGallery
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT g.*, f.PublicUrl, f.OriginalFileName
    FROM dbo.GymWebsiteGallery g
    INNER JOIN dbo.Files f ON f.FileId = g.FileId
    WHERE g.GymId = @GymId
    ORDER BY g.DisplayOrder, g.Id;
END
GO

/* ========== WEBSITE LEADS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateWebsiteLeadCapture
    @GymId UNIQUEIDENTIFIER, @LeadId INT = NULL, @Name NVARCHAR(200), @MobileNumber NVARCHAR(20),
    @Email NVARCHAR(256) = NULL, @Source NVARCHAR(50), @InterestedPlan NVARCHAR(200) = NULL,
    @Notes NVARCHAR(1000) = NULL, @Status NVARCHAR(30) = N'New', @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.WebsiteLeadCaptures (GymId, LeadId, Name, MobileNumber, Email, Source, InterestedPlan, Notes, [Status])
    VALUES (@GymId, @LeadId, @Name, @MobileNumber, @Email, @Source, @InterestedPlan, @Notes, @Status);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SearchWebsiteLeadCaptures
    @GymId UNIQUEIDENTIFIER, @Search NVARCHAR(200) = NULL, @Status NVARCHAR(30) = NULL,
    @PageNumber INT = 1, @PageSize INT = 20, @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    SELECT @TotalCount = COUNT(*)
    FROM dbo.WebsiteLeadCaptures w
    WHERE w.GymId = @GymId
      AND (@Status IS NULL OR w.[Status] = @Status)
      AND (@Search IS NULL OR w.Name LIKE N'%' + @Search + N'%' OR w.MobileNumber LIKE N'%' + @Search + N'%' OR w.Email LIKE N'%' + @Search + N'%');

    SELECT w.*, l.[Status] AS LeadStatus
    FROM dbo.WebsiteLeadCaptures w
    LEFT JOIN dbo.Leads l ON l.LeadId = w.LeadId
    WHERE w.GymId = @GymId
      AND (@Status IS NULL OR w.[Status] = @Status)
      AND (@Search IS NULL OR w.Name LIKE N'%' + @Search + N'%' OR w.MobileNumber LIKE N'%' + @Search + N'%' OR w.Email LIKE N'%' + @Search + N'%')
    ORDER BY w.CreatedDate DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ConvertWebsiteLeadCapture
    @GymId UNIQUEIDENTIFIER, @WebsiteLeadId INT, @LeadId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.WebsiteLeadCaptures SET LeadId = @LeadId, [Status] = N'Converted'
    WHERE Id = @WebsiteLeadId AND GymId = @GymId;
END
GO

/* ========== PUBLIC WEBSITE ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetPublicWebsiteBySlug
    @WebsiteSlug NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET @WebsiteSlug = LOWER(LTRIM(RTRIM(@WebsiteSlug)));

    SELECT s.*, g.Name AS GymName, lf.PublicUrl AS LogoUrl, bf.PublicUrl AS BannerUrl
    FROM dbo.GymWebsiteSettings s
    INNER JOIN dbo.Gyms g ON g.GymId = s.GymId
    LEFT JOIN dbo.Files lf ON lf.FileId = s.LogoFileId
    LEFT JOIN dbo.Files bf ON bf.FileId = s.BannerFileId
    WHERE s.WebsiteSlug = @WebsiteSlug AND s.IsPublished = 1 AND g.IsActive = 1;

    SELECT s.*, f.PublicUrl AS ImageUrl
    FROM dbo.GymWebsiteSections s
    INNER JOIN dbo.GymWebsiteSettings ws ON ws.GymId = s.GymId
    LEFT JOIN dbo.Files f ON f.FileId = s.ImageFileId
    WHERE ws.WebsiteSlug = @WebsiteSlug AND ws.IsPublished = 1 AND s.IsVisible = 1
    ORDER BY s.DisplayOrder;

    SELECT * FROM dbo.GymWebsitePages p
    INNER JOIN dbo.GymWebsiteSettings ws ON ws.GymId = p.GymId
    WHERE ws.WebsiteSlug = @WebsiteSlug AND ws.IsPublished = 1 AND p.IsActive = 1
    ORDER BY p.DisplayOrder;

    SELECT t.*, f.PublicUrl AS ImageUrl
    FROM dbo.GymWebsiteTestimonials t
    INNER JOIN dbo.GymWebsiteSettings ws ON ws.GymId = t.GymId
    LEFT JOIN dbo.Files f ON f.FileId = t.ImageFileId
    WHERE ws.WebsiteSlug = @WebsiteSlug AND ws.IsPublished = 1 AND t.IsApproved = 1
    ORDER BY t.CreatedDate DESC;

    SELECT g.*, f.PublicUrl, f.OriginalFileName
    FROM dbo.GymWebsiteGallery g
    INNER JOIN dbo.GymWebsiteSettings ws ON ws.GymId = g.GymId
    INNER JOIN dbo.Files f ON f.FileId = g.FileId
    WHERE ws.WebsiteSlug = @WebsiteSlug AND ws.IsPublished = 1
    ORDER BY g.DisplayOrder;

    SELECT mp.* FROM dbo.MembershipPlans mp
    INNER JOIN dbo.GymWebsiteSettings ws ON ws.GymId = mp.GymId
    WHERE ws.WebsiteSlug = @WebsiteSlug AND ws.IsPublished = 1 AND mp.IsActive = 1
    ORDER BY mp.Price;

    SELECT t.TrainerId AS Id, u.Name AS FullName, t.Specialization, t.Bio,
           pf.PublicUrl AS ProfileImageUrl
    FROM dbo.Trainers t
    INNER JOIN dbo.GymWebsiteSettings ws ON ws.GymId = t.GymId
    INNER JOIN dbo.Users u ON u.Id = t.UserId
    LEFT JOIN dbo.Files pf ON pf.FileId = t.ProfilePhotoFileId AND pf.IsDeleted = 0
    WHERE ws.WebsiteSlug = @WebsiteSlug AND ws.IsPublished = 1 AND t.IsActive = 1
    ORDER BY u.Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetGymIdByWebsiteSlug
    @WebsiteSlug NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT GymId FROM dbo.GymWebsiteSettings
    WHERE WebsiteSlug = LOWER(LTRIM(RTRIM(@WebsiteSlug))) AND IsPublished = 1;
END
GO

/* ========== ANALYTICS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetWebsiteAnalyticsOverview
    @GymId UNIQUEIDENTIFIER, @Days INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @FromDate DATETIME2 = DATEADD(DAY, -@Days, SYSUTCDATETIME());

    SELECT
        (SELECT COUNT(*) FROM dbo.WebsiteLeadCaptures WHERE GymId = @GymId) AS TotalWebsiteLeads,
        (SELECT COUNT(*) FROM dbo.WebsiteLeadCaptures WHERE GymId = @GymId AND [Status] = N'TrialScheduled') AS TrialRequests,
        (SELECT COUNT(*) FROM dbo.WebsiteLeadCaptures WHERE GymId = @GymId AND [Status] = N'Converted') AS ConvertedLeads,
        CASE WHEN (SELECT COUNT(*) FROM dbo.WebsiteLeadCaptures WHERE GymId = @GymId) = 0 THEN 0
             ELSE CAST((SELECT COUNT(*) FROM dbo.WebsiteLeadCaptures WHERE GymId = @GymId AND [Status] = N'Converted') AS DECIMAL(10,2))
                  / (SELECT COUNT(*) FROM dbo.WebsiteLeadCaptures WHERE GymId = @GymId) * 100 END AS LeadConversionRate,
        (SELECT COUNT(*) FROM dbo.WebsiteLeadCaptures WHERE GymId = @GymId AND CreatedDate >= @FromDate) AS LeadsInPeriod;

    SELECT CAST(CreatedDate AS DATE) AS LeadDate, COUNT(*) AS LeadCount
    FROM dbo.WebsiteLeadCaptures
    WHERE GymId = @GymId AND CreatedDate >= @FromDate
    GROUP BY CAST(CreatedDate AS DATE)
    ORDER BY LeadDate;

    SELECT TOP 10 Source, COUNT(*) AS LeadCount
    FROM dbo.WebsiteLeadCaptures WHERE GymId = @GymId AND CreatedDate >= @FromDate
    GROUP BY Source ORDER BY LeadCount DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWebsiteNotificationRecipients
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT u.Id AS UserId, u.Name, u.Email, g.Phone AS PhoneNumber, N'GymAdmin' AS RecipientType
    FROM dbo.Users u
    INNER JOIN dbo.UserRoles ur ON ur.UserId = u.Id
    INNER JOIN dbo.Roles r ON r.RoleId = ur.RoleId
    INNER JOIN dbo.Gyms g ON g.GymId = u.GymId
    WHERE u.GymId = @GymId AND r.RoleName = N'GymAdmin' AND u.IsActive = 1
    UNION
    SELECT DISTINCT u.Id, u.Name, u.Email, b.Phone, N'BranchManager'
    FROM dbo.BranchManagers bm
    INNER JOIN dbo.Users u ON u.Id = bm.UserId
    INNER JOIN dbo.Branches b ON b.BranchId = bm.BranchId AND b.GymId = bm.GymId
    WHERE bm.GymId = @GymId AND bm.IsActive = 1 AND u.IsActive = 1;
END
GO

/* Seed default WhatsApp templates for website events */
INSERT INTO dbo.NotificationTemplates (GymId, NotificationType, TemplateName, BodyTemplate, VariablesJson, IsActive, CreatedAt)
SELECT g.GymId, t.NotificationType, t.TemplateName, t.BodyTemplate, t.VariablesJson, 1, SYSUTCDATETIME()
FROM dbo.Gyms g
CROSS APPLY (VALUES
    (N'WebsiteLeadCreated', N'Website Lead Created', N'New website lead: {{leadName}} ({{mobileNumber}}). Plan: {{interestedPlan}}.', N'["leadName","mobileNumber","interestedPlan"]'),
    (N'TrialBooked', N'Trial Booked', N'Free trial booked by {{leadName}} on {{trialDate}} at {{trialTime}}.', N'["leadName","trialDate","trialTime"]')
) t(NotificationType, TemplateName, BodyTemplate, VariablesJson)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.NotificationTemplates nt
    WHERE nt.GymId = g.GymId AND nt.NotificationType = t.NotificationType
);
GO
