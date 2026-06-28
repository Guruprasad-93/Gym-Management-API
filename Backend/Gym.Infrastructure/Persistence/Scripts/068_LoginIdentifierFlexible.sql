/*
  068 — Flexible LoginIdentifier (up to 100 chars, any format, trim-only normalization).
*/
IF COL_LENGTH('dbo.Users', 'LoginIdentifier') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Users ALTER COLUMN LoginIdentifier NVARCHAR(100) NOT NULL;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_LoginUser
    @LoginIdentifier NVARCHAR(100),
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @NormalizedLogin NVARCHAR(100) = LTRIM(RTRIM(@LoginIdentifier));

    BEGIN TRY
        SELECT
            u.Id AS UserId,
            u.Name AS FullName,
            u.Email,
            u.LoginIdentifier,
            u.Password,
            u.GymId,
            ISNULL(u.IsActive, 1) AS UserIsActive,
            ISNULL(u.TokenVersion, 0) AS TokenVersion,
            ISNULL(u.MustChangePassword, 0) AS MustChangePassword,
            g.Name AS GymName,
            ISNULL(g.IsActive, 1) AS GymIsActive
        FROM dbo.Users u
        LEFT JOIN dbo.Gyms g ON g.GymId = u.GymId
        WHERE u.LoginIdentifier = @NormalizedLogin
          AND (
                (@GymId IS NULL AND u.GymId IS NULL)
                OR u.GymId = @GymId
              );
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_ExistsByLoginIdentifier
    @LoginIdentifier NVARCHAR(100),
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NormalizedLogin NVARCHAR(100) = LTRIM(RTRIM(@LoginIdentifier));
    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1 FROM dbo.Users u
        WHERE u.LoginIdentifier = @NormalizedLogin
          AND ((@GymId IS NULL AND u.GymId IS NULL) OR u.GymId = @GymId)
    ) THEN 1 ELSE 0 END AS BIT);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetByLoginIdentifier
    @LoginIdentifier NVARCHAR(100),
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NormalizedLogin NVARCHAR(100) = LTRIM(RTRIM(@LoginIdentifier));
    SELECT Id, Name, Email, LoginIdentifier, Password, GymId, CreatedDate,
           ISNULL(IsActive, 1) AS IsActive,
           ISNULL(TokenVersion, 0) AS TokenVersion,
           PasswordResetToken, PasswordResetTokenExpiresAt
    FROM dbo.Users u
    WHERE u.LoginIdentifier = @NormalizedLogin
      AND ((@GymId IS NULL AND u.GymId IS NULL) OR u.GymId = @GymId);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_Insert
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @LoginIdentifier NVARCHAR(100),
    @Email NVARCHAR(256) = NULL,
    @Password NVARCHAR(500),
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Users (Id, Name, LoginIdentifier, Email, Password, GymId, CreatedDate)
    VALUES (@Id, @Name, LTRIM(RTRIM(@LoginIdentifier)), NULLIF(LTRIM(RTRIM(LOWER(@Email))), ''), @Password, @GymId, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_SetPasswordResetToken
    @LoginIdentifier NVARCHAR(100),
    @GymId UNIQUEIDENTIFIER = NULL,
    @ResetToken NVARCHAR(500),
    @ExpiresAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NormalizedLogin NVARCHAR(100) = LTRIM(RTRIM(@LoginIdentifier));
    UPDATE dbo.Users
    SET PasswordResetToken = @ResetToken,
        PasswordResetTokenExpiresAt = @ExpiresAt
    WHERE LoginIdentifier = @NormalizedLogin
      AND ((@GymId IS NULL AND GymId IS NULL) OR GymId = @GymId)
      AND ISNULL(IsActive, 1) = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_ResetPassword
    @LoginIdentifier NVARCHAR(100),
    @GymId UNIQUEIDENTIFIER = NULL,
    @ResetToken NVARCHAR(500),
    @PasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NormalizedLogin NVARCHAR(100) = LTRIM(RTRIM(@LoginIdentifier));
    UPDATE dbo.Users
    SET Password = @PasswordHash,
        PasswordResetToken = NULL,
        PasswordResetTokenExpiresAt = NULL,
        TokenVersion = TokenVersion + 1
    WHERE LoginIdentifier = @NormalizedLogin
      AND ((@GymId IS NULL AND GymId IS NULL) OR GymId = @GymId)
      AND PasswordResetToken = @ResetToken
      AND PasswordResetTokenExpiresAt > SYSUTCDATETIME()
      AND ISNULL(IsActive, 1) = 1;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymAdmin_Create
    @UserId UNIQUEIDENTIFIER,
    @GymId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @LoginIdentifier NVARCHAR(100),
    @Email NVARCHAR(256) = NULL,
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

    DECLARE @NormalizedLogin NVARCHAR(100) = LTRIM(RTRIM(@LoginIdentifier));
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE GymId = @GymId AND LoginIdentifier = @NormalizedLogin)
        THROW 50003, 'A user with this login identifier already exists.', 1;

    IF @Email IS NOT NULL AND LTRIM(RTRIM(@Email)) <> ''
       AND EXISTS (SELECT 1 FROM dbo.Users WHERE Email = LTRIM(RTRIM(LOWER(@Email))))
        THROW 50003, 'A user with this email already exists.', 1;

    INSERT INTO dbo.Users (Id, Name, LoginIdentifier, Email, Password, GymId, IsActive, MustChangePassword, TokenVersion, CreatedDate)
    VALUES (
        @UserId,
        @Name,
        @NormalizedLogin,
        NULLIF(LTRIM(RTRIM(LOWER(@Email))), ''),
        @Password,
        @GymId,
        1,
        @MustChangePassword,
        0,
        SYSUTCDATETIME());

    INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedAt)
    VALUES (@UserId, @RoleId, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateGymAdmin
    @UserId UNIQUEIDENTIFIER,
    @GymId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @LoginIdentifier NVARCHAR(100),
    @Email NVARCHAR(256) = NULL,
    @Password NVARCHAR(500),
    @MustChangePassword BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_GymAdmin_Create @UserId, @GymId, @Name, @LoginIdentifier, @Email, @Password, @MustChangePassword;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymAdmin_Update
    @UserId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @LoginIdentifier NVARCHAR(100),
    @Email NVARCHAR(256) = NULL,
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

    DECLARE @NormalizedLogin NVARCHAR(100) = LTRIM(RTRIM(@LoginIdentifier));
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE GymId = @GymId AND LoginIdentifier = @NormalizedLogin AND Id <> @UserId)
        THROW 50003, 'A user with this login identifier already exists.', 1;

    IF @Email IS NOT NULL AND LTRIM(RTRIM(@Email)) <> ''
       AND EXISTS (SELECT 1 FROM dbo.Users WHERE Email = LTRIM(RTRIM(LOWER(@Email))) AND Id <> @UserId)
        THROW 50003, 'A user with this email already exists.', 1;

    UPDATE dbo.Users
    SET Name = @Name,
        LoginIdentifier = @NormalizedLogin,
        Email = NULLIF(LTRIM(RTRIM(LOWER(@Email))), ''),
        GymId = @GymId
    WHERE Id = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateGymAdmin
    @UserId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @LoginIdentifier NVARCHAR(100),
    @Email NVARCHAR(256) = NULL,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_GymAdmin_Update @UserId, @Name, @LoginIdentifier, @Email, @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateMember
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER,
    @FullName NVARCHAR(100) = NULL,
    @LoginIdentifier NVARCHAR(100) = NULL,
    @Email NVARCHAR(256) = NULL,
    @TrainerId INT = NULL,
    @DateOfBirth DATE = NULL,
    @Gender NVARCHAR(20) = NULL,
    @Height DECIMAL(5, 2) = NULL,
    @Weight DECIMAL(6, 2) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Address NVARCHAR(500) = NULL,
    @EmergencyContact NVARCHAR(200) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Members WHERE MemberId = @MemberId AND GymId = @GymId AND IsDeleted = 0)
            THROW 50043, 'Member not found.', 1;

        IF @TrainerId IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM dbo.Trainers WHERE TrainerId = @TrainerId AND GymId = @GymId AND IsActive = 1)
            THROW 50042, 'Trainer not found or inactive.', 1;

        DECLARE @UserId UNIQUEIDENTIFIER;
        SELECT @UserId = UserId FROM dbo.Members WHERE MemberId = @MemberId;

        IF @FullName IS NOT NULL
            UPDATE dbo.Users SET Name = @FullName WHERE Id = @UserId;

        IF @LoginIdentifier IS NOT NULL AND LTRIM(RTRIM(@LoginIdentifier)) <> ''
        BEGIN
            DECLARE @NormalizedLogin NVARCHAR(100) = LTRIM(RTRIM(@LoginIdentifier));
            IF EXISTS (SELECT 1 FROM dbo.Users WHERE GymId = @GymId AND LoginIdentifier = @NormalizedLogin AND Id <> @UserId)
                THROW 50044, 'A user with this login identifier already exists.', 1;
            UPDATE dbo.Users SET LoginIdentifier = @NormalizedLogin WHERE Id = @UserId;
        END

        IF @Email IS NOT NULL
        BEGIN
            IF LTRIM(RTRIM(@Email)) <> ''
               AND EXISTS (SELECT 1 FROM dbo.Users WHERE Email = LTRIM(RTRIM(LOWER(@Email))) AND Id <> @UserId)
                THROW 50044, 'A user with this email already exists.', 1;
            UPDATE dbo.Users SET Email = NULLIF(LTRIM(RTRIM(LOWER(@Email))), '') WHERE Id = @UserId;
        END

        UPDATE dbo.Members
        SET TrainerId = @TrainerId,
            DateOfBirth = @DateOfBirth,
            Gender = @Gender,
            Height = @Height,
            Weight = @Weight,
            Phone = @Phone,
            Address = @Address,
            EmergencyContact = @EmergencyContact,
            IsActive = @IsActive,
            UpdatedAt = SYSUTCDATETIME()
        WHERE MemberId = @MemberId AND GymId = @GymId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
