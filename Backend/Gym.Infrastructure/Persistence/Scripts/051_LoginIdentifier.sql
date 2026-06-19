/*
  LoginIdentifier as primary auth identifier; Email optional.
  - Backfills LoginIdentifier from existing email local-part (max 20 chars).
  - Unique per GymId (tenant) with separate index for platform (GymId IS NULL) users.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH('dbo.Users', 'LoginIdentifier') IS NULL
    BEGIN
        ALTER TABLE dbo.Users ADD LoginIdentifier NVARCHAR(20) NULL;
    END

    /* Backfill from email local-part before making Email nullable */
    ;WITH Source AS (
        SELECT
            u.Id,
            u.GymId,
            u.Email,
            Base = LEFT(
                REPLACE(
                    SUBSTRING(
                        LOWER(LTRIM(RTRIM(u.Email))),
                        1,
                        CASE WHEN CHARINDEX('@', LOWER(LTRIM(RTRIM(u.Email)))) > 0
                             THEN CHARINDEX('@', LOWER(LTRIM(RTRIM(u.Email)))) - 1
                             ELSE LEN(LOWER(LTRIM(RTRIM(u.Email))))
                        END
                    ),
                    ' ', ''
                ),
                20
            )
        FROM dbo.Users u
        WHERE u.LoginIdentifier IS NULL OR LTRIM(RTRIM(u.LoginIdentifier)) = ''
    ),
    Numbered AS (
        SELECT
            s.Id,
            s.GymId,
            s.Base,
            rn = ROW_NUMBER() OVER (PARTITION BY s.GymId, s.Base ORDER BY s.Id)
        FROM Source s
        WHERE s.Base IS NOT NULL AND s.Base <> ''
    )
    UPDATE u
    SET LoginIdentifier = CASE
            WHEN n.rn = 1 THEN n.Base
            ELSE LEFT(n.Base, 20 - LEN(CAST(n.rn AS NVARCHAR(10)))) + CAST(n.rn AS NVARCHAR(10))
        END
    FROM dbo.Users u
    INNER JOIN Numbered n ON n.Id = u.Id;

    /* Fallback for rows still missing (empty email) */
    UPDATE dbo.Users
    SET LoginIdentifier = LEFT(REPLACE(LOWER(LTRIM(RTRIM(Name))), ' ', ''), 20)
    WHERE LoginIdentifier IS NULL OR LTRIM(RTRIM(LoginIdentifier)) = '';

    UPDATE dbo.Users
    SET LoginIdentifier = LEFT(REPLACE(CAST(Id AS NVARCHAR(36)), '-', ''), 20)
    WHERE LoginIdentifier IS NULL OR LTRIM(RTRIM(LoginIdentifier)) = '';

    ALTER TABLE dbo.Users ALTER COLUMN LoginIdentifier NVARCHAR(20) NOT NULL;

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('dbo.Users'))
        DROP INDEX IX_Users_Email ON dbo.Users;

    ALTER TABLE dbo.Users ALTER COLUMN Email NVARCHAR(256) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Users_Email_NotNull' AND object_id = OBJECT_ID('dbo.Users'))
        CREATE UNIQUE INDEX UX_Users_Email_NotNull ON dbo.Users(Email) WHERE Email IS NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Users_GymId_LoginIdentifier' AND object_id = OBJECT_ID('dbo.Users'))
        CREATE UNIQUE INDEX UX_Users_GymId_LoginIdentifier ON dbo.Users(GymId, LoginIdentifier) WHERE GymId IS NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Users_LoginIdentifier_Platform' AND object_id = OBJECT_ID('dbo.Users'))
        CREATE UNIQUE INDEX UX_Users_LoginIdentifier_Platform ON dbo.Users(LoginIdentifier) WHERE GymId IS NULL;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO

/* ========== LOGIN ========== */
CREATE OR ALTER PROCEDURE dbo.sp_LoginUser
    @LoginIdentifier NVARCHAR(20),
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

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
        WHERE u.LoginIdentifier = LTRIM(RTRIM(LOWER(@LoginIdentifier)))
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
    @LoginIdentifier NVARCHAR(20),
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1 FROM dbo.Users u
        WHERE u.LoginIdentifier = LTRIM(RTRIM(LOWER(@LoginIdentifier)))
          AND ((@GymId IS NULL AND u.GymId IS NULL) OR u.GymId = @GymId)
    ) THEN 1 ELSE 0 END AS BIT);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetByLoginIdentifier
    @LoginIdentifier NVARCHAR(20),
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Email, LoginIdentifier, Password, GymId, CreatedDate,
           ISNULL(IsActive, 1) AS IsActive,
           ISNULL(TokenVersion, 0) AS TokenVersion,
           PasswordResetToken, PasswordResetTokenExpiresAt
    FROM dbo.Users u
    WHERE u.LoginIdentifier = LTRIM(RTRIM(LOWER(@LoginIdentifier)))
      AND ((@GymId IS NULL AND u.GymId IS NULL) OR u.GymId = @GymId);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_Insert
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @LoginIdentifier NVARCHAR(20),
    @Email NVARCHAR(256) = NULL,
    @Password NVARCHAR(500),
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Users (Id, Name, LoginIdentifier, Email, Password, GymId, CreatedDate)
    VALUES (@Id, @Name, LTRIM(RTRIM(LOWER(@LoginIdentifier))), NULLIF(LTRIM(RTRIM(LOWER(@Email))), ''), @Password, @GymId, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Email, LoginIdentifier, Password, GymId, CreatedDate,
           ISNULL(IsActive, 1) AS IsActive,
           ISNULL(TokenVersion, 0) AS TokenVersion,
           PasswordResetToken, PasswordResetTokenExpiresAt
    FROM dbo.Users
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Email, LoginIdentifier, Password, GymId, CreatedDate,
           ISNULL(IsActive, 1) AS IsActive,
           ISNULL(TokenVersion, 0) AS TokenVersion,
           PasswordResetToken, PasswordResetTokenExpiresAt
    FROM dbo.Users
    WHERE Email = LTRIM(RTRIM(LOWER(@Email)));
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_SetPasswordResetToken
    @LoginIdentifier NVARCHAR(20),
    @GymId UNIQUEIDENTIFIER = NULL,
    @ResetToken NVARCHAR(500),
    @ExpiresAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users
    SET PasswordResetToken = @ResetToken,
        PasswordResetTokenExpiresAt = @ExpiresAt
    WHERE LoginIdentifier = LTRIM(RTRIM(LOWER(@LoginIdentifier)))
      AND ((@GymId IS NULL AND GymId IS NULL) OR GymId = @GymId)
      AND ISNULL(IsActive, 1) = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_ResetPassword
    @LoginIdentifier NVARCHAR(20),
    @GymId UNIQUEIDENTIFIER = NULL,
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
    WHERE LoginIdentifier = LTRIM(RTRIM(LOWER(@LoginIdentifier)))
      AND ((@GymId IS NULL AND GymId IS NULL) OR GymId = @GymId)
      AND PasswordResetToken = @ResetToken
      AND PasswordResetTokenExpiresAt > SYSUTCDATETIME()
      AND ISNULL(IsActive, 1) = 1;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

/* ========== GYM ADMIN (LoginIdentifier) ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GymAdmin_Create
    @UserId UNIQUEIDENTIFIER,
    @GymId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @LoginIdentifier NVARCHAR(20),
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

    DECLARE @NormalizedLogin NVARCHAR(20) = LTRIM(RTRIM(LOWER(@LoginIdentifier)));
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
    @LoginIdentifier NVARCHAR(20),
    @Email NVARCHAR(256) = NULL,
    @Password NVARCHAR(500),
    @MustChangePassword BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        EXEC dbo.sp_GymAdmin_Create @UserId, @GymId, @Name, @LoginIdentifier, @Email, @Password, @MustChangePassword;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
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
        u.LoginIdentifier,
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
      AND (@SearchPattern IS NULL OR u.Name LIKE @SearchPattern OR u.LoginIdentifier LIKE @SearchPattern OR u.Email LIKE @SearchPattern OR g.Name LIKE @SearchPattern);

    SET @TotalCount = (SELECT COUNT(*) FROM #Filtered);

    SELECT UserId, Name, LoginIdentifier, Email, GymId, GymName, IsActive, MustChangePassword, CreatedDate
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
        u.LoginIdentifier,
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
    @LoginIdentifier NVARCHAR(20),
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

    DECLARE @NormalizedLogin NVARCHAR(20) = LTRIM(RTRIM(LOWER(@LoginIdentifier)));
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
    @LoginIdentifier NVARCHAR(20),
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

/* ========== MEMBER (LoginIdentifier in reads/updates) ========== */
CREATE OR ALTER PROCEDURE dbo.sp_UpdateMember
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER,
    @FullName NVARCHAR(100) = NULL,
    @LoginIdentifier NVARCHAR(20) = NULL,
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
            DECLARE @NormalizedLogin NVARCHAR(20) = LTRIM(RTRIM(LOWER(@LoginIdentifier)));
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

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberById
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            m.MemberId,
            m.GymId,
            m.UserId,
            m.TrainerId,
            u.Name AS FullName,
            u.LoginIdentifier,
            u.Email AS Email,
            m.DateOfBirth,
            CASE WHEN m.DateOfBirth IS NULL THEN NULL
                 ELSE DATEDIFF(YEAR, m.DateOfBirth, CAST(GETUTCDATE() AS DATE))
                      - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, m.DateOfBirth, CAST(GETUTCDATE() AS DATE)), m.DateOfBirth) > CAST(GETUTCDATE() AS DATE) THEN 1 ELSE 0 END
            END AS Age,
            m.Gender,
            m.Height,
            m.Weight,
            m.Phone,
            m.Address,
            m.EmergencyContact,
            m.JoinDate,
            m.IsActive,
            m.IsDeleted,
            m.CreatedAt AS CreatedDate,
            m.UpdatedAt AS UpdatedDate,
            tu.Name AS TrainerName,
            ms.MembershipStatus,
            ms.PlanName AS MembershipPlanName,
            ms.EndDate AS MembershipEndDate
        FROM dbo.Members m
        INNER JOIN dbo.Users u ON u.Id = m.UserId
        LEFT JOIN dbo.Trainers t ON t.TrainerId = m.TrainerId
        LEFT JOIN dbo.Users tu ON tu.Id = t.UserId
        OUTER APPLY (
            SELECT TOP 1
                mem.Status AS MembershipStatus,
                mp.PlanName,
                mem.EndDate
            FROM dbo.Memberships mem
            INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = mem.MembershipPlanId
            WHERE mem.MemberId = m.MemberId
            ORDER BY mem.StartDate DESC
        ) ms
        WHERE m.MemberId = @MemberId
          AND m.IsDeleted = 0
          AND (m.GymId = @GymId)
          AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberByUserId
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            m.MemberId,
            m.GymId,
            m.UserId,
            m.TrainerId,
            u.Name AS FullName,
            u.LoginIdentifier,
            u.Email AS Email,
            m.DateOfBirth,
            CASE WHEN m.DateOfBirth IS NULL THEN NULL
                 ELSE DATEDIFF(YEAR, m.DateOfBirth, CAST(GETUTCDATE() AS DATE))
                      - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, m.DateOfBirth, CAST(GETUTCDATE() AS DATE)), m.DateOfBirth) > CAST(GETUTCDATE() AS DATE) THEN 1 ELSE 0 END
            END AS Age,
            m.Gender,
            m.Height,
            m.Weight,
            m.Phone,
            m.Address,
            m.EmergencyContact,
            m.JoinDate,
            m.IsActive,
            m.IsDeleted,
            m.CreatedAt AS CreatedDate,
            m.UpdatedAt AS UpdatedDate,
            tu.Name AS TrainerName,
            ms.MembershipStatus,
            ms.PlanName AS MembershipPlanName,
            ms.EndDate AS MembershipEndDate
        FROM dbo.Members m
        INNER JOIN dbo.Users u ON u.Id = m.UserId
        LEFT JOIN dbo.Trainers t ON t.TrainerId = m.TrainerId
        LEFT JOIN dbo.Users tu ON tu.Id = t.UserId
        OUTER APPLY (
            SELECT TOP 1
                mem.Status AS MembershipStatus,
                mp.PlanName,
                mem.EndDate
            FROM dbo.Memberships mem
            INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = mem.MembershipPlanId
            WHERE mem.MemberId = m.MemberId
            ORDER BY mem.StartDate DESC
        ) ms
        WHERE m.UserId = @UserId
          AND m.IsDeleted = 0;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllMembers
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL,
    @Search NVARCHAR(200) = NULL,
    @IncludeInactive BIT = 0,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = N'FullName',
    @SortDirection NVARCHAR(4) = N'ASC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @PageNumber < 1 SET @PageNumber = 1;
        IF @PageSize < 1 SET @PageSize = 10;
        IF @PageSize > 100 SET @PageSize = 100;

        DECLARE @SearchPattern NVARCHAR(202) = NULL;
        IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
            SET @SearchPattern = N'%' + LTRIM(RTRIM(@Search)) + N'%';

        SELECT
            m.MemberId,
            m.GymId,
            m.UserId,
            m.TrainerId,
            u.Name AS FullName,
            u.LoginIdentifier,
            u.Email AS Email,
            m.DateOfBirth,
            CASE WHEN m.DateOfBirth IS NULL THEN NULL
                 ELSE DATEDIFF(YEAR, m.DateOfBirth, CAST(GETUTCDATE() AS DATE))
                      - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, m.DateOfBirth, CAST(GETUTCDATE() AS DATE)), m.DateOfBirth) > CAST(GETUTCDATE() AS DATE) THEN 1 ELSE 0 END
            END AS Age,
            m.Gender,
            m.Height,
            m.Weight,
            m.Phone,
            m.Address,
            m.EmergencyContact,
            m.JoinDate,
            m.IsActive,
            m.IsDeleted,
            m.CreatedAt AS CreatedDate,
            m.UpdatedAt AS UpdatedDate,
            tu.Name AS TrainerName,
            ms.MembershipStatus,
            ms.PlanName AS MembershipPlanName,
            ms.EndDate AS MembershipEndDate
        INTO #Filtered
        FROM dbo.Members m
        INNER JOIN dbo.Users u ON u.Id = m.UserId
        LEFT JOIN dbo.Trainers t ON t.TrainerId = m.TrainerId
        LEFT JOIN dbo.Users tu ON tu.Id = t.UserId
        OUTER APPLY (
            SELECT TOP 1
                mem.Status AS MembershipStatus,
                mp.PlanName,
                mem.EndDate
            FROM dbo.Memberships mem
            INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = mem.MembershipPlanId
            WHERE mem.MemberId = m.MemberId
            ORDER BY mem.StartDate DESC
        ) ms
        WHERE m.IsDeleted = 0
          AND (m.GymId = @GymId)
          AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)
          AND (@IncludeInactive = 1 OR m.IsActive = 1)
          AND (
              @SearchPattern IS NULL
              OR u.Name LIKE @SearchPattern
              OR u.LoginIdentifier LIKE @SearchPattern
              OR u.Email LIKE @SearchPattern
              OR m.Phone LIKE @SearchPattern
              OR tu.Name LIKE @SearchPattern
              OR ms.MembershipStatus LIKE @SearchPattern
          );

        SET @TotalCount = (SELECT COUNT(*) FROM #Filtered);

        SELECT *
        FROM #Filtered
        ORDER BY
            CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'FullName' THEN FullName END ASC,
            CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'FullName' THEN FullName END DESC,
            CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'JoinDate' THEN JoinDate END ASC,
            CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'JoinDate' THEN JoinDate END DESC,
            CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'Phone' THEN Phone END ASC,
            CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'Phone' THEN Phone END DESC,
            MemberId ASC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        DROP TABLE #Filtered;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
