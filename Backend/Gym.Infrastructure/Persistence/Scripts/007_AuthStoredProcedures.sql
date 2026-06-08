/* Authentication & session stored procedures */

CREATE OR ALTER PROCEDURE dbo.sp_User_GetByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Email, Password, GymId, CreatedDate,
           ISNULL(IsActive, 1) AS IsActive,
           ISNULL(TokenVersion, 0) AS TokenVersion,
           PasswordResetToken, PasswordResetTokenExpiresAt
    FROM dbo.Users
    WHERE Email = @Email;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Email, Password, GymId, CreatedDate,
           ISNULL(IsActive, 1) AS IsActive,
           ISNULL(TokenVersion, 0) AS TokenVersion,
           PasswordResetToken, PasswordResetTokenExpiresAt
    FROM dbo.Users
    WHERE Id = @Id;
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
        PasswordResetToken = NULL,
        PasswordResetTokenExpiresAt = NULL
    WHERE Id = @UserId;
    SELECT TokenVersion FROM dbo.Users WHERE Id = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_SetPasswordResetToken
    @Email NVARCHAR(256),
    @ResetToken NVARCHAR(500),
    @ExpiresAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users
    SET PasswordResetToken = @ResetToken,
        PasswordResetTokenExpiresAt = @ExpiresAt
    WHERE Email = @Email AND ISNULL(IsActive, 1) = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_ResetPassword
    @Email NVARCHAR(256),
    @ResetToken NVARCHAR(500),
    @PasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users
    SET Password = @PasswordHash,
        PasswordResetToken = NULL,
        PasswordResetTokenExpiresAt = NULL,
        TokenVersion = TokenVersion + 1
    WHERE Email = @Email
      AND PasswordResetToken = @ResetToken
      AND PasswordResetTokenExpiresAt > SYSUTCDATETIME()
      AND ISNULL(IsActive, 1) = 1;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_IncrementTokenVersion
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users SET TokenVersion = TokenVersion + 1 WHERE Id = @UserId;
    SELECT TokenVersion FROM dbo.Users WHERE Id = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UserLoginSession_EndAllForUser
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.UserLoginSessions
    SET IsActive = 0, LogoutAt = SYSUTCDATETIME()
    WHERE UserId = @UserId AND IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UserLoginSession_Create
    @UserId UNIQUEIDENTIFIER,
    @LoginSessionGuid UNIQUEIDENTIFIER,
    @DeviceInfo NVARCHAR(256) = NULL,
    @IpAddress NVARCHAR(50) = NULL,
    @UserLoginSessionId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.UserLoginSessions (UserId, LoginSessionGuid, DeviceInfo, IpAddress, LoginAt, IsActive)
    VALUES (@UserId, @LoginSessionGuid, @DeviceInfo, @IpAddress, SYSUTCDATETIME(), 1);
    SET @UserLoginSessionId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UserLoginSession_End
    @LoginSessionGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.UserLoginSessions
    SET IsActive = 0, LogoutAt = SYSUTCDATETIME()
    WHERE LoginSessionGuid = @LoginSessionGuid AND IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UserLoginSession_IsActive
    @UserId UNIQUEIDENTIFIER,
    @LoginSessionGuid UNIQUEIDENTIFIER,
    @TokenVersion INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE
        WHEN EXISTS (
            SELECT 1 FROM dbo.UserLoginSessions s
            INNER JOIN dbo.Users u ON u.Id = s.UserId
            WHERE s.UserId = @UserId
              AND s.LoginSessionGuid = @LoginSessionGuid
              AND s.IsActive = 1
              AND u.TokenVersion = @TokenVersion
              AND ISNULL(u.IsActive, 1) = 1
        ) THEN 1 ELSE 0 END AS BIT) AS IsValid;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RefreshToken_Insert
    @UserId UNIQUEIDENTIFIER,
    @Token NVARCHAR(500),
    @ExpiresAt DATETIME2,
    @DeviceInfo NVARCHAR(256) = NULL,
    @IpAddress NVARCHAR(50) = NULL,
    @RefreshTokenId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.RefreshTokens (UserId, Token, ExpiresAt, CreatedAt, DeviceInfo, IpAddress)
    VALUES (@UserId, @Token, @ExpiresAt, SYSUTCDATETIME(), @DeviceInfo, @IpAddress);
    SET @RefreshTokenId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RefreshToken_GetByToken
    @Token NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RefreshTokenId, UserId, Token, ExpiresAt, CreatedAt, RevokedAt, ReplacedByToken, DeviceInfo, IpAddress
    FROM dbo.RefreshTokens
    WHERE Token = @Token AND RevokedAt IS NULL AND ExpiresAt > SYSUTCDATETIME();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RefreshToken_Revoke
    @Token NVARCHAR(500),
    @ReplacedByToken NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.RefreshTokens
    SET RevokedAt = SYSUTCDATETIME(), ReplacedByToken = @ReplacedByToken
    WHERE Token = @Token;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RefreshToken_RevokeAllForUser
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.RefreshTokens
    SET RevokedAt = SYSUTCDATETIME()
    WHERE UserId = @UserId AND RevokedAt IS NULL;
END
GO
