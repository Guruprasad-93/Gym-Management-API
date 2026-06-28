/* Draft website preview for authenticated gym admins (no publish requirement). */
CREATE OR ALTER PROCEDURE dbo.sp_GetWebsiteBySlugForPreview
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
    WHERE s.WebsiteSlug = @WebsiteSlug AND g.IsActive = 1;

    SELECT s.*, f.PublicUrl AS ImageUrl
    FROM dbo.GymWebsiteSections s
    INNER JOIN dbo.GymWebsiteSettings ws ON ws.GymId = s.GymId
    LEFT JOIN dbo.Files f ON f.FileId = s.ImageFileId
    WHERE ws.WebsiteSlug = @WebsiteSlug AND s.IsVisible = 1
    ORDER BY s.DisplayOrder;

    SELECT p.* FROM dbo.GymWebsitePages p
    INNER JOIN dbo.GymWebsiteSettings ws ON ws.GymId = p.GymId
    WHERE ws.WebsiteSlug = @WebsiteSlug AND p.IsActive = 1
    ORDER BY p.DisplayOrder;

    SELECT t.*, f.PublicUrl AS ImageUrl
    FROM dbo.GymWebsiteTestimonials t
    INNER JOIN dbo.GymWebsiteSettings ws ON ws.GymId = t.GymId
    LEFT JOIN dbo.Files f ON f.FileId = t.ImageFileId
    WHERE ws.WebsiteSlug = @WebsiteSlug AND t.IsApproved = 1
    ORDER BY t.CreatedDate DESC;

    SELECT g.*, f.PublicUrl, f.OriginalFileName
    FROM dbo.GymWebsiteGallery g
    INNER JOIN dbo.GymWebsiteSettings ws ON ws.GymId = g.GymId
    INNER JOIN dbo.Files f ON f.FileId = g.FileId
    WHERE ws.WebsiteSlug = @WebsiteSlug
    ORDER BY g.DisplayOrder;

    SELECT mp.* FROM dbo.MembershipPlans mp
    INNER JOIN dbo.GymWebsiteSettings ws ON ws.GymId = mp.GymId
    WHERE ws.WebsiteSlug = @WebsiteSlug AND mp.IsActive = 1
    ORDER BY mp.Price;

    SELECT t.TrainerId AS Id, u.Name AS FullName, t.Specialization, t.Bio,
           pf.PublicUrl AS ProfileImageUrl
    FROM dbo.Trainers t
    INNER JOIN dbo.GymWebsiteSettings ws ON ws.GymId = t.GymId
    INNER JOIN dbo.Users u ON u.Id = t.UserId
    LEFT JOIN dbo.Files pf ON pf.FileId = t.ProfilePhotoFileId AND pf.IsDeleted = 0
    WHERE ws.WebsiteSlug = @WebsiteSlug AND t.IsActive = 1
    ORDER BY u.Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetGymIdByWebsiteSlugAny
    @WebsiteSlug NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT GymId
    FROM dbo.GymWebsiteSettings
    WHERE WebsiteSlug = LOWER(LTRIM(RTRIM(@WebsiteSlug)));
END
GO
