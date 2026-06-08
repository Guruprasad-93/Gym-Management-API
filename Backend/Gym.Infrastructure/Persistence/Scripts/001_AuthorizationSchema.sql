IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        RoleId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        RoleName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsSystemRole BIT NOT NULL CONSTRAINT DF_Roles_IsSystemRole DEFAULT (0),
        CreatedDate DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName)
    );
END
GO

IF OBJECT_ID(N'dbo.Privileges', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Privileges
    (
        PrivilegeId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        PrivilegeName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        Category NVARCHAR(100) NOT NULL,
        CreatedDate DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT UQ_Privileges_PrivilegeName UNIQUE (PrivilegeName)
    );
END
GO

IF OBJECT_ID(N'dbo.RolePrivileges', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RolePrivileges
    (
        RolePrivilegeId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        RoleId INT NOT NULL,
        PrivilegeId INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT UQ_RolePrivileges_Role_Privilege UNIQUE (RoleId, PrivilegeId),
        CONSTRAINT FK_RolePrivileges_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId) ON DELETE CASCADE,
        CONSTRAINT FK_RolePrivileges_Privileges FOREIGN KEY (PrivilegeId) REFERENCES dbo.Privileges (PrivilegeId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoles
    (
        UserRoleId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        RoleId INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT UQ_UserRoles_User_Role UNIQUE (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId) ON DELETE CASCADE
    );
END
GO

IF COL_LENGTH('dbo.Users', 'GymId') IS NULL
    ALTER TABLE dbo.Users ADD GymId UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH('dbo.Users', 'Role') IS NOT NULL
    ALTER TABLE dbo.Users DROP COLUMN Role;
GO
