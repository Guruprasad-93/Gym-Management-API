/*
  Audience filters for manual push notification campaigns.
*/

CREATE OR ALTER PROCEDURE dbo.sp_GetMobilePushCampaignRecipients
    @GymId UNIQUEIDENTIFIER,
    @TargetAudience NVARCHAR(50),
    @BranchId INT = NULL,
    @ExpiringWithinDays INT = 30,
    @UserIdsCsv NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    DECLARE @Audience NVARCHAR(50) = UPPER(LTRIM(RTRIM(@TargetAudience)));

    IF @Audience = N'SELECTEDMEMBERS'
    BEGIN
        SELECT DISTINCT m.UserId
        FROM dbo.Members m
        INNER JOIN STRING_SPLIT(@UserIdsCsv, N',') s ON TRY_CAST(LTRIM(RTRIM(s.value)) AS UNIQUEIDENTIFIER) = m.UserId
        WHERE m.GymId = @GymId
          AND m.IsDeleted = 0
          AND (@BranchId IS NULL OR m.BranchId = @BranchId)
          AND EXISTS (
              SELECT 1 FROM dbo.DeviceTokens dt
              WHERE dt.UserId = m.UserId AND dt.GymId = @GymId AND dt.IsActive = 1);
        RETURN;
    END

    IF @Audience = N'EXPIRINGMEMBERS'
    BEGIN
        SELECT DISTINCT m.UserId
        FROM dbo.Members m
        INNER JOIN dbo.Memberships ms ON ms.MemberId = m.MemberId AND ms.GymId = m.GymId
        WHERE m.GymId = @GymId
          AND m.IsDeleted = 0
          AND m.IsActive = 1
          AND ms.[Status] = N'Active'
          AND CAST(ms.EndDate AS DATE) BETWEEN @Today AND DATEADD(DAY, @ExpiringWithinDays, @Today)
          AND (@BranchId IS NULL OR m.BranchId = @BranchId)
          AND EXISTS (
              SELECT 1 FROM dbo.DeviceTokens dt
              WHERE dt.UserId = m.UserId AND dt.GymId = @GymId AND dt.IsActive = 1);
        RETURN;
    END

    IF @Audience = N'ALLMEMBERS'
    BEGIN
        SELECT DISTINCT m.UserId
        FROM dbo.Members m
        WHERE m.GymId = @GymId
          AND m.IsDeleted = 0
          AND (@BranchId IS NULL OR m.BranchId = @BranchId)
          AND EXISTS (
              SELECT 1 FROM dbo.DeviceTokens dt
              WHERE dt.UserId = m.UserId AND dt.GymId = @GymId AND dt.IsActive = 1);
        RETURN;
    END

    -- ActiveMembers (default)
    SELECT DISTINCT m.UserId
    FROM dbo.Members m
    WHERE m.GymId = @GymId
      AND m.IsDeleted = 0
      AND m.IsActive = 1
      AND (@BranchId IS NULL OR m.BranchId = @BranchId)
      AND EXISTS (
          SELECT 1 FROM dbo.DeviceTokens dt
          WHERE dt.UserId = m.UserId AND dt.GymId = @GymId AND dt.IsActive = 1);
END
GO
