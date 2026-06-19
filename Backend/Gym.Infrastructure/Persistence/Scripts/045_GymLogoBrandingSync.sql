/*
  Keep gym logo uploads in sync with white-label branding and resolve logo from Gyms when WL has none.
*/

CREATE OR ALTER PROCEDURE dbo.sp_Gym_SetLogoFile
    @GymId UNIQUEIDENTIFIER,
    @FileId BIGINT,
    @PublicUrl NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Gyms
    SET LogoFileId = @FileId, LogoUrl = @PublicUrl
    WHERE GymId = @GymId;

    IF EXISTS (SELECT 1 FROM dbo.WhiteLabelSettings WHERE GymId = @GymId)
    BEGIN
        UPDATE dbo.WhiteLabelSettings
        SET LogoFileId = @FileId, UpdatedAt = SYSUTCDATETIME()
        WHERE GymId = @GymId;
    END
END
GO

-- Backfill white-label logo from gym logo where missing
UPDATE w
SET w.LogoFileId = g.LogoFileId,
    w.UpdatedAt = SYSUTCDATETIME()
FROM dbo.WhiteLabelSettings w
INNER JOIN dbo.Gyms g ON g.GymId = w.GymId
WHERE g.LogoFileId IS NOT NULL
  AND (w.LogoFileId IS NULL OR w.LogoFileId <> g.LogoFileId);
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWhiteLabelSettings
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT w.*,
           COALESCE(lf.PublicUrl, gf.PublicUrl, g.LogoUrl) AS LogoUrl,
           ff.PublicUrl AS FaviconUrl,
           bg.PublicUrl AS LoginBackgroundUrl
    FROM dbo.WhiteLabelSettings w
    INNER JOIN dbo.Gyms g ON g.GymId = w.GymId
    LEFT JOIN dbo.Files lf ON lf.FileId = w.LogoFileId AND lf.IsDeleted = 0
    LEFT JOIN dbo.Files gf ON gf.FileId = g.LogoFileId AND gf.IsDeleted = 0
    LEFT JOIN dbo.Files ff ON ff.FileId = w.FaviconFileId AND ff.IsDeleted = 0
    LEFT JOIN dbo.Files bg ON bg.FileId = w.LoginBackgroundFileId AND bg.IsDeleted = 0
    WHERE w.GymId = @GymId;
END
GO
