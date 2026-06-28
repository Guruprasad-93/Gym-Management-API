/* MVP business tables – Gym Management SaaS */

IF OBJECT_ID(N'dbo.Gyms', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Gyms
    (
        GymId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Address NVARCHAR(500) NULL,
        Phone NVARCHAR(20) NULL,
        Email NVARCHAR(256) NULL,
        LogoUrl NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Gyms_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL
    );
END
GO

IF OBJECT_ID(N'dbo.FK_Users_Gyms', N'F') IS NULL
BEGIN
    ALTER TABLE dbo.Users
    ADD CONSTRAINT FK_Users_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId);
END
GO

IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshTokens
    (
        RefreshTokenId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        Token NVARCHAR(500) NOT NULL,
        ExpiresAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        RevokedAt DATETIME2 NULL,
        ReplacedByToken NVARCHAR(500) NULL,
        DeviceInfo NVARCHAR(256) NULL,
        IpAddress NVARCHAR(50) NULL,
        CONSTRAINT UQ_RefreshTokens_Token UNIQUE (Token),
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens (UserId);
END
GO

IF OBJECT_ID(N'dbo.MembershipPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MembershipPlans
    (
        MembershipPlanId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        PlanName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        DurationDays INT NOT NULL,
        Price DECIMAL(18, 2) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_MembershipPlans_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_MembershipPlans_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE
    );
    CREATE INDEX IX_MembershipPlans_GymId ON dbo.MembershipPlans (GymId);
END
GO

IF OBJECT_ID(N'dbo.Trainers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Trainers
    (
        TrainerId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NULL,
        Specialization NVARCHAR(200) NULL,
        Bio NVARCHAR(1000) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Trainers_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_Trainers_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE,
        CONSTRAINT FK_Trainers_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
    );
    CREATE INDEX IX_Trainers_GymId ON dbo.Trainers (GymId);
END
GO

IF OBJECT_ID(N'dbo.Members', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Members
    (
        MemberId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        TrainerId INT NULL,
        DateOfBirth DATE NULL,
        Gender NVARCHAR(20) NULL,
        Phone NVARCHAR(20) NULL,
        EmergencyContact NVARCHAR(200) NULL,
        JoinDate DATE NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Members_IsActive DEFAULT (1),
        IsDeleted BIT NOT NULL CONSTRAINT DF_Members_IsDeleted DEFAULT (0),
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_Members_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE,
        CONSTRAINT FK_Members_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id),
        CONSTRAINT FK_Members_Trainers FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers (TrainerId)
    );
    CREATE INDEX IX_Members_GymId ON dbo.Members (GymId);
    CREATE UNIQUE INDEX IX_Members_Gym_User ON dbo.Members (GymId, UserId);
END
GO

/* Soft-delete column required by scripts 010+ (011 also adds this for legacy databases). */
IF OBJECT_ID(N'dbo.Members', N'U') IS NOT NULL AND COL_LENGTH('dbo.Members', 'IsDeleted') IS NULL
    ALTER TABLE dbo.Members ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Members_IsDeleted DEFAULT (0) WITH VALUES;
GO

IF OBJECT_ID(N'dbo.Memberships', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Memberships
    (
        MembershipId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        MembershipPlanId INT NOT NULL,
        StartDate DATE NOT NULL,
        EndDate DATE NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Memberships_Status DEFAULT ('Active'),
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_Memberships_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_Memberships_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId) ON DELETE CASCADE,
        CONSTRAINT FK_Memberships_Plans FOREIGN KEY (MembershipPlanId) REFERENCES dbo.MembershipPlans (MembershipPlanId)
    );
    CREATE INDEX IX_Memberships_GymId ON dbo.Memberships (GymId);
    CREATE INDEX IX_Memberships_MemberId ON dbo.Memberships (MemberId);
END
GO

IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments
    (
        PaymentId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NULL,
        MembershipId INT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        PaymentDate DATETIME2 NOT NULL,
        PaymentMethod NVARCHAR(50) NOT NULL,
        TransactionReference NVARCHAR(100) NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Payments_Status DEFAULT ('Completed'),
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_Payments_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_Payments_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT FK_Payments_Memberships FOREIGN KEY (MembershipId) REFERENCES dbo.Memberships (MembershipId)
    );
    CREATE INDEX IX_Payments_GymId ON dbo.Payments (GymId);
END
GO

IF OBJECT_ID(N'dbo.DietPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DietPlans
    (
        DietPlanId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        TrainerId INT NULL,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(2000) NULL,
        StartDate DATE NOT NULL,
        EndDate DATE NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_DietPlans_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_DietPlans_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_DietPlans_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId) ON DELETE CASCADE,
        CONSTRAINT FK_DietPlans_Trainers FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers (TrainerId)
    );
    CREATE INDEX IX_DietPlans_GymId ON dbo.DietPlans (GymId);
    CREATE INDEX IX_DietPlans_MemberId ON dbo.DietPlans (MemberId);
END
GO

IF OBJECT_ID(N'dbo.WorkoutPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkoutPlans
    (
        WorkoutPlanId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        TrainerId INT NULL,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(2000) NULL,
        StartDate DATE NOT NULL,
        EndDate DATE NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_WorkoutPlans_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_WorkoutPlans_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_WorkoutPlans_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId) ON DELETE CASCADE,
        CONSTRAINT FK_WorkoutPlans_Trainers FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers (TrainerId)
    );
    CREATE INDEX IX_WorkoutPlans_GymId ON dbo.WorkoutPlans (GymId);
    CREATE INDEX IX_WorkoutPlans_MemberId ON dbo.WorkoutPlans (MemberId);
END
GO
