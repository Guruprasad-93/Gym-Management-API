CREATE OR ALTER PROCEDURE dbo.sp_User_ExistsByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email) THEN 1 ELSE 0 END AS BIT);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Email, Password, GymId, CreatedDate
    FROM dbo.Users
    WHERE Email = @Email;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Email, Password, GymId, CreatedDate
    FROM dbo.Users
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_Insert
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @Email NVARCHAR(256),
    @Password NVARCHAR(500),
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Users (Id, Name, Email, Password, GymId, CreatedDate)
    VALUES (@Id, @Name, @Email, @Password, @GymId, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_AnyExists
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Users) THEN 1 ELSE 0 END AS BIT);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetPermissions
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT p.PrivilegeName
    FROM dbo.UserRoles ur
    INNER JOIN dbo.RolePrivileges rp ON rp.RoleId = ur.RoleId
    INNER JOIN dbo.Privileges p ON p.PrivilegeId = rp.PrivilegeId
    WHERE ur.UserId = @UserId
    ORDER BY p.PrivilegeName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetRoles
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT r.RoleName
    FROM dbo.UserRoles ur
    INNER JOIN dbo.Roles r ON r.RoleId = ur.RoleId
    WHERE ur.UserId = @UserId
    ORDER BY r.RoleName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Role_GetById
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId, RoleName, Description, IsSystemRole, CreatedDate, CreatedAt, UpdatedAt
    FROM dbo.Roles
    WHERE RoleId = @RoleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Role_GetByName
    @RoleName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId, RoleName, Description, IsSystemRole, CreatedDate, CreatedAt, UpdatedAt
    FROM dbo.Roles
    WHERE RoleName = @RoleName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Role_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId, RoleName, Description, IsSystemRole, CreatedDate, CreatedAt, UpdatedAt
    FROM dbo.Roles
    ORDER BY RoleName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Role_AnyExists
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Roles) THEN 1 ELSE 0 END AS BIT);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Role_Insert
    @RoleName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @IsSystemRole BIT = 0,
    @RoleId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    INSERT INTO dbo.Roles (RoleName, Description, IsSystemRole, CreatedDate, CreatedAt)
    VALUES (@RoleName, @Description, @IsSystemRole, @Now, @Now);
    SET @RoleId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Role_Update
    @RoleId INT,
    @RoleName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Roles
    SET RoleName = @RoleName,
        Description = @Description,
        UpdatedAt = SYSUTCDATETIME()
    WHERE RoleId = @RoleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Role_Delete
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Roles WHERE RoleId = @RoleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Role_IsAssignedToUsers
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.UserRoles WHERE RoleId = @RoleId) THEN 1 ELSE 0 END AS BIT);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Privilege_GetById
    @PrivilegeId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PrivilegeId, PrivilegeName, Description, Category, CreatedDate, CreatedAt, UpdatedAt
    FROM dbo.Privileges
    WHERE PrivilegeId = @PrivilegeId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Privilege_GetByName
    @PrivilegeName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PrivilegeId, PrivilegeName, Description, Category, CreatedDate, CreatedAt, UpdatedAt
    FROM dbo.Privileges
    WHERE PrivilegeName = UPPER(LTRIM(RTRIM(@PrivilegeName)));
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Privilege_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PrivilegeId, PrivilegeName, Description, Category, CreatedDate, CreatedAt, UpdatedAt
    FROM dbo.Privileges
    ORDER BY Category, PrivilegeName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Privilege_Insert
    @PrivilegeName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @Category NVARCHAR(100),
    @PrivilegeId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    INSERT INTO dbo.Privileges (PrivilegeName, Description, Category, CreatedDate, CreatedAt)
    VALUES (UPPER(LTRIM(RTRIM(@PrivilegeName))), @Description, @Category, @Now, @Now);
    SET @PrivilegeId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Privilege_Update
    @PrivilegeId INT,
    @PrivilegeName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @Category NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Privileges
    SET PrivilegeName = UPPER(LTRIM(RTRIM(@PrivilegeName))),
        Description = @Description,
        Category = @Category,
        UpdatedAt = SYSUTCDATETIME()
    WHERE PrivilegeId = @PrivilegeId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Privilege_Delete
    @PrivilegeId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Privileges WHERE PrivilegeId = @PrivilegeId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Privilege_IsAssignedToRoles
    @PrivilegeId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.RolePrivileges WHERE PrivilegeId = @PrivilegeId) THEN 1 ELSE 0 END AS BIT);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RolePrivilege_GetByRoleId
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        rp.RolePrivilegeId,
        rp.RoleId,
        r.RoleName,
        rp.PrivilegeId,
        p.PrivilegeName,
        p.Category
    FROM dbo.RolePrivileges rp
    INNER JOIN dbo.Roles r ON r.RoleId = rp.RoleId
    INNER JOIN dbo.Privileges p ON p.PrivilegeId = rp.PrivilegeId
    WHERE rp.RoleId = @RoleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RolePrivilege_Get
    @RoleId INT,
    @PrivilegeId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RolePrivilegeId, RoleId, PrivilegeId, CreatedAt, UpdatedAt
    FROM dbo.RolePrivileges
    WHERE RoleId = @RoleId AND PrivilegeId = @PrivilegeId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RolePrivilege_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RolePrivilegeId, RoleId, PrivilegeId, CreatedAt, UpdatedAt
    FROM dbo.RolePrivileges;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RolePrivilege_Insert
    @RoleId INT,
    @PrivilegeId INT,
    @RolePrivilegeId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    INSERT INTO dbo.RolePrivileges (RoleId, PrivilegeId, CreatedAt)
    VALUES (@RoleId, @PrivilegeId, @Now);
    SET @RolePrivilegeId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RolePrivilege_Delete
    @RoleId INT,
    @PrivilegeId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.RolePrivileges
    WHERE RoleId = @RoleId AND PrivilegeId = @PrivilegeId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UserRole_GetByUserId
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        ur.UserRoleId,
        ur.UserId,
        u.Name AS UserName,
        u.Email AS UserEmail,
        ur.RoleId,
        r.RoleName
    FROM dbo.UserRoles ur
    INNER JOIN dbo.Users u ON u.Id = ur.UserId
    INNER JOIN dbo.Roles r ON r.RoleId = ur.RoleId
    WHERE ur.UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UserRole_Get
    @UserId UNIQUEIDENTIFIER,
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserRoleId, UserId, RoleId, CreatedAt, UpdatedAt
    FROM dbo.UserRoles
    WHERE UserId = @UserId AND RoleId = @RoleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UserRole_Insert
    @UserId UNIQUEIDENTIFIER,
    @RoleId INT,
    @UserRoleId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedAt)
    VALUES (@UserId, @RoleId, @Now);
    SET @UserRoleId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UserRole_Delete
    @UserId UNIQUEIDENTIFIER,
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.UserRoles
    WHERE UserId = @UserId AND RoleId = @RoleId;
END
GO
