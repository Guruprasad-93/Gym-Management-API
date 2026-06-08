IF COL_LENGTH('dbo.Users', 'IsActive') IS NULL
    ALTER TABLE dbo.Users ADD IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1);
GO

IF COL_LENGTH('dbo.Users', 'TokenVersion') IS NULL
    ALTER TABLE dbo.Users ADD TokenVersion INT NOT NULL CONSTRAINT DF_Users_TokenVersion DEFAULT (0);
GO

IF COL_LENGTH('dbo.Users', 'PasswordResetToken') IS NULL
    ALTER TABLE dbo.Users ADD PasswordResetToken NVARCHAR(500) NULL;
GO

IF COL_LENGTH('dbo.Users', 'PasswordResetTokenExpiresAt') IS NULL
    ALTER TABLE dbo.Users ADD PasswordResetTokenExpiresAt DATETIME2 NULL;
GO

IF OBJECT_ID(N'dbo.UserLoginSessions', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.UserLoginSessions', 'LoginSessionGuid') IS NULL
BEGIN
    ALTER TABLE dbo.UserLoginSessions ADD LoginSessionGuid UNIQUEIDENTIFIER NULL;
    UPDATE dbo.UserLoginSessions SET LoginSessionGuid = NEWID() WHERE LoginSessionGuid IS NULL;
    ALTER TABLE dbo.UserLoginSessions ALTER COLUMN LoginSessionGuid UNIQUEIDENTIFIER NOT NULL;
    CREATE UNIQUE INDEX IX_UserLoginSessions_LoginSessionGuid ON dbo.UserLoginSessions (LoginSessionGuid);
END
GO
