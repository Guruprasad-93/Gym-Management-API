/* Gym Admin user management module */

IF COL_LENGTH('dbo.Users', 'MustChangePassword') IS NULL
    ALTER TABLE dbo.Users ADD MustChangePassword BIT NOT NULL CONSTRAINT DF_Users_MustChangePassword DEFAULT (0);
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymAdmin_Create
    @UserId UNIQUEIDENTIFIER,
    @GymId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @Email NVARCHAR(256),
    @Password NVARCHAR(500),
    @MustChangePassword BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RoleId INT;
    SELECT @RoleId = RoleId FROM dbo.Roles WHERE RoleName = N'GymAdmin';

    IF @RoleId IS NULL
        THROW 50001, 'GymAdmin role does not exist. Seed roles first.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Gyms WHERE GymId = @GymId)
        THROW 50002, 'Gym not found.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email)
        THROW 50003, 'A user with this email already exists.', 1;

    INSERT INTO dbo.Users (Id, Name, Email, Password, GymId, IsActive, MustChangePassword, TokenVersion, CreatedDate)
    VALUES (@UserId, @Name, @Email, @Password, @GymId, 1, @MustChangePassword, 0, SYSUTCDATETIME());

    INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedAt)
    VALUES (@UserId, @RoleId, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymAdmin_GetAll
    @GymId UNIQUEIDENTIFIER = NULL,
    @Search NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = N'Name',
    @SortDirection NVARCHAR(4) = N'ASC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 10;
    IF @PageSize > 100 SET @PageSize = 100;

    DECLARE @SearchPattern NVARCHAR(202) = NULL;
    IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
        SET @SearchPattern = N'%' + LTRIM(RTRIM(@Search)) + N'%';

    SELECT
        u.Id AS UserId,
        u.Name,
        u.Email,
        u.GymId,
        g.Name AS GymName,
        ISNULL(u.IsActive, 1) AS IsActive,
        ISNULL(u.MustChangePassword, 0) AS MustChangePassword,
        u.CreatedDate
    INTO #Filtered
    FROM dbo.Users u
    INNER JOIN dbo.UserRoles ur ON u.Id = ur.UserId
    INNER JOIN dbo.Roles r ON ur.RoleId = r.RoleId AND r.RoleName = N'GymAdmin'
    LEFT JOIN dbo.Gyms g ON g.GymId = u.GymId
    WHERE (@GymId IS NULL OR u.GymId = @GymId)
      AND (@SearchPattern IS NULL OR u.Name LIKE @SearchPattern OR u.Email LIKE @SearchPattern OR g.Name LIKE @SearchPattern);

    SET @TotalCount = (SELECT COUNT(*) FROM #Filtered);

    SELECT UserId, Name, Email, GymId, GymName, IsActive, MustChangePassword, CreatedDate
    FROM #Filtered
    ORDER BY
        CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'Name' THEN Name END ASC,
        CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'Name' THEN Name END DESC,
        CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'Email' THEN Email END ASC,
        CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'Email' THEN Email END DESC,
        CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'CreatedDate' THEN CreatedDate END ASC,
        CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'CreatedDate' THEN CreatedDate END DESC,
        Name ASC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    DROP TABLE #Filtered;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymAdmin_GetById
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.Id AS UserId,
        u.Name,
        u.Email,
        u.GymId,
        g.Name AS GymName,
        ISNULL(u.IsActive, 1) AS IsActive,
        ISNULL(u.MustChangePassword, 0) AS MustChangePassword,
        u.CreatedDate
    FROM dbo.Users u
    INNER JOIN dbo.UserRoles ur ON u.Id = ur.UserId
    INNER JOIN dbo.Roles r ON ur.RoleId = r.RoleId AND r.RoleName = N'GymAdmin'
    LEFT JOIN dbo.Gyms g ON g.GymId = u.GymId
    WHERE u.Id = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymAdmin_Update
    @UserId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @Email NVARCHAR(256),
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Users u
        INNER JOIN dbo.UserRoles ur ON u.Id = ur.UserId
        INNER JOIN dbo.Roles r ON ur.RoleId = r.RoleId AND r.RoleName = N'GymAdmin'
        WHERE u.Id = @UserId)
        THROW 50004, 'Gym admin not found.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Gyms WHERE GymId = @GymId)
        THROW 50002, 'Gym not found.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email AND Id <> @UserId)
        THROW 50003, 'A user with this email already exists.', 1;

    UPDATE dbo.Users
    SET Name = @Name, Email = @Email, GymId = @GymId
    WHERE Id = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymAdmin_SetActive
    @UserId UNIQUEIDENTIFIER,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE u
    SET u.IsActive = @IsActive,
        u.TokenVersion = u.TokenVersion + CASE WHEN @IsActive = 0 THEN 1 ELSE 0 END
    FROM dbo.Users u
    INNER JOIN dbo.UserRoles ur ON u.Id = ur.UserId
    INNER JOIN dbo.Roles r ON ur.RoleId = r.RoleId AND r.RoleName = N'GymAdmin'
    WHERE u.Id = @UserId;

    IF @@ROWCOUNT = 0
        THROW 50004, 'Gym admin not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymAdmin_ResetPassword
    @UserId UNIQUEIDENTIFIER,
    @PasswordHash NVARCHAR(500),
    @MustChangePassword BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE u
    SET u.Password = @PasswordHash,
        u.MustChangePassword = @MustChangePassword,
        u.TokenVersion = u.TokenVersion + 1,
        u.PasswordResetToken = NULL,
        u.PasswordResetTokenExpiresAt = NULL
    FROM dbo.Users u
    INNER JOIN dbo.UserRoles ur ON u.Id = ur.UserId
    INNER JOIN dbo.Roles r ON ur.RoleId = r.RoleId AND r.RoleName = N'GymAdmin'
    WHERE u.Id = @UserId;

    IF @@ROWCOUNT = 0
        THROW 50004, 'Gym admin not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetLoginContext
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.Id AS UserId,
        u.Name AS FullName,
        u.Email,
        u.GymId,
        ISNULL(u.IsActive, 1) AS UserIsActive,
        ISNULL(u.TokenVersion, 0) AS TokenVersion,
        ISNULL(u.MustChangePassword, 0) AS MustChangePassword,
        g.Name AS GymName,
        ISNULL(g.IsActive, 1) AS GymIsActive
    FROM dbo.Users u
    LEFT JOIN dbo.Gyms g ON g.GymId = u.GymId
    WHERE u.Id = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_ChangePassword
    @UserId UNIQUEIDENTIFIER,
    @PasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users
    SET Password = @PasswordHash,
        TokenVersion = TokenVersion + 1,
        MustChangePassword = 0,
        PasswordResetToken = NULL,
        PasswordResetTokenExpiresAt = NULL
    WHERE Id = @UserId;
    SELECT TokenVersion FROM dbo.Users WHERE Id = @UserId;
END
GO
