/*
  Multi-Branch / Franchise Management Module
*/

/* ========== BRANCHES ========== */
IF OBJECT_ID(N'dbo.Branches', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Branches
    (
        BranchId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        BranchName NVARCHAR(200) NOT NULL,
        BranchCode NVARCHAR(20) NULL,
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        Phone NVARCHAR(20) NULL,
        Email NVARCHAR(256) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Branches_IsActive DEFAULT (1),
        IsDeleted BIT NOT NULL CONSTRAINT DF_Branches_IsDeleted DEFAULT (0),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_Branches_Created DEFAULT (SYSUTCDATETIME()),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT FK_Branches_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT UX_Branches_Gym_Code UNIQUE (GymId, BranchCode)
    );
    CREATE INDEX IX_Branches_GymId ON dbo.Branches (GymId) WHERE IsDeleted = 0;
END
GO

/* ========== BRANCH MANAGERS ========== */
IF OBJECT_ID(N'dbo.BranchManagers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BranchManagers
    (
        BranchManagerId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        BranchId INT NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        AssignedDate DATETIME2 NOT NULL CONSTRAINT DF_BranchManagers_Assigned DEFAULT (SYSUTCDATETIME()),
        IsActive BIT NOT NULL CONSTRAINT DF_BranchManagers_IsActive DEFAULT (1),
        CONSTRAINT FK_BranchManagers_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_BranchManagers_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches (BranchId),
        CONSTRAINT FK_BranchManagers_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
    );
    CREATE INDEX IX_BranchManagers_Branch ON dbo.BranchManagers (GymId, BranchId) WHERE IsActive = 1;
END
GO

/* ========== BRANCH TRANSFER HISTORY ========== */
IF OBJECT_ID(N'dbo.BranchTransferHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BranchTransferHistory
    (
        TransferId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        EntityType NVARCHAR(20) NOT NULL,
        EntityId INT NOT NULL,
        FromBranchId INT NULL,
        ToBranchId INT NOT NULL,
        TransferredByUserId UNIQUEIDENTIFIER NULL,
        TransferDate DATETIME2 NOT NULL CONSTRAINT DF_BranchTransferHistory_Date DEFAULT (SYSUTCDATETIME()),
        Notes NVARCHAR(500) NULL,
        CONSTRAINT FK_BranchTransferHistory_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_BranchTransferHistory_From FOREIGN KEY (FromBranchId) REFERENCES dbo.Branches (BranchId),
        CONSTRAINT FK_BranchTransferHistory_To FOREIGN KEY (ToBranchId) REFERENCES dbo.Branches (BranchId)
    );
    CREATE INDEX IX_BranchTransferHistory_Gym ON dbo.BranchTransferHistory (GymId, EntityType, TransferDate DESC);
END
GO

/* ========== BRANCH TARGETS ========== */
IF OBJECT_ID(N'dbo.BranchTargets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BranchTargets
    (
        TargetId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        BranchId INT NOT NULL,
        TargetMonth DATE NOT NULL,
        RevenueTarget DECIMAL(18, 2) NOT NULL CONSTRAINT DF_BranchTargets_Revenue DEFAULT (0),
        NewMembersTarget INT NOT NULL CONSTRAINT DF_BranchTargets_Members DEFAULT (0),
        LeadConversionsTarget INT NOT NULL CONSTRAINT DF_BranchTargets_Leads DEFAULT (0),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_BranchTargets_Created DEFAULT (SYSUTCDATETIME()),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT FK_BranchTargets_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_BranchTargets_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches (BranchId),
        CONSTRAINT UX_BranchTargets_Branch_Month UNIQUE (GymId, BranchId, TargetMonth)
    );
END
GO

/* ========== BRANCH ANNOUNCEMENTS ========== */
IF OBJECT_ID(N'dbo.BranchAnnouncements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BranchAnnouncements
    (
        AnnouncementId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        BranchId INT NULL,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        TargetAudience NVARCHAR(30) NOT NULL CONSTRAINT DF_BranchAnnouncements_Audience DEFAULT (N'All'),
        IsActive BIT NOT NULL CONSTRAINT DF_BranchAnnouncements_IsActive DEFAULT (1),
        PublishDate DATETIME2 NOT NULL CONSTRAINT DF_BranchAnnouncements_Publish DEFAULT (SYSUTCDATETIME()),
        ExpiryDate DATETIME2 NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_BranchAnnouncements_Created DEFAULT (SYSUTCDATETIME()),
        IsDeleted BIT NOT NULL CONSTRAINT DF_BranchAnnouncements_IsDeleted DEFAULT (0),
        CONSTRAINT FK_BranchAnnouncements_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_BranchAnnouncements_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches (BranchId)
    );
    CREATE INDEX IX_BranchAnnouncements_Gym ON dbo.BranchAnnouncements (GymId, PublishDate DESC) WHERE IsDeleted = 0;
END
GO

/* ========== ADD BranchId TO OPERATIONAL TABLES ========== */
IF COL_LENGTH('dbo.Members', 'BranchId') IS NULL
    ALTER TABLE dbo.Members ADD BranchId INT NULL;
GO
IF COL_LENGTH('dbo.Trainers', 'BranchId') IS NULL
    ALTER TABLE dbo.Trainers ADD BranchId INT NULL;
GO
IF COL_LENGTH('dbo.MemberAttendance', 'BranchId') IS NULL
    ALTER TABLE dbo.MemberAttendance ADD BranchId INT NULL;
GO
IF COL_LENGTH('dbo.Payments', 'BranchId') IS NULL
    ALTER TABLE dbo.Payments ADD BranchId INT NULL;
GO
IF COL_LENGTH('dbo.Leads', 'BranchId') IS NULL
    ALTER TABLE dbo.Leads ADD BranchId INT NULL;
GO
IF COL_LENGTH('dbo.Expenses', 'BranchId') IS NULL
    ALTER TABLE dbo.Expenses ADD BranchId INT NULL;
GO
IF COL_LENGTH('dbo.Payrolls', 'BranchId') IS NULL
    ALTER TABLE dbo.Payrolls ADD BranchId INT NULL;
GO

-- Legacy Expenses table (004) lacks IsDeleted; payroll module (028) skips create when table exists
IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL AND COL_LENGTH('dbo.Expenses', 'IsDeleted') IS NULL
    ALTER TABLE dbo.Expenses ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Expenses_IsDeleted_Branch030 DEFAULT (0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Members_Branches')
    ALTER TABLE dbo.Members ADD CONSTRAINT FK_Members_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches (BranchId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Trainers_Branches')
    ALTER TABLE dbo.Trainers ADD CONSTRAINT FK_Trainers_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches (BranchId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_MemberAttendance_Branches')
    ALTER TABLE dbo.MemberAttendance ADD CONSTRAINT FK_MemberAttendance_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches (BranchId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Payments_Branches')
    ALTER TABLE dbo.Payments ADD CONSTRAINT FK_Payments_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches (BranchId);
GO
IF OBJECT_ID(N'dbo.Leads', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Leads_Branches')
    ALTER TABLE dbo.Leads ADD CONSTRAINT FK_Leads_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches (BranchId);
GO
IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Expenses_Branches')
    ALTER TABLE dbo.Expenses ADD CONSTRAINT FK_Expenses_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches (BranchId);
GO
IF OBJECT_ID(N'dbo.Payrolls', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Payrolls_Branches')
    ALTER TABLE dbo.Payrolls ADD CONSTRAINT FK_Payrolls_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches (BranchId);
GO

CREATE OR ALTER PROCEDURE dbo.sp_EnsureDefaultBranch
    @GymId UNIQUEIDENTIFIER,
    @BranchId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 @BranchId = BranchId FROM dbo.Branches
    WHERE GymId = @GymId AND IsDeleted = 0 ORDER BY BranchId;
    IF @BranchId IS NOT NULL RETURN;
    INSERT INTO dbo.Branches (GymId, BranchName, BranchCode, IsActive)
    VALUES (@GymId, N'Main Branch', N'MAIN', 1);
    SET @BranchId = SCOPE_IDENTITY();
END
GO

-- Backfill default branch for existing gyms
DECLARE @GymId UNIQUEIDENTIFIER;
DECLARE @BranchId INT;
DECLARE gym_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT GymId FROM dbo.Gyms;
OPEN gym_cursor;
FETCH NEXT FROM gym_cursor INTO @GymId;
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC dbo.sp_EnsureDefaultBranch @GymId, @BranchId OUTPUT;
    UPDATE dbo.Members SET BranchId = @BranchId WHERE GymId = @GymId AND BranchId IS NULL;
    UPDATE dbo.Trainers SET BranchId = @BranchId WHERE GymId = @GymId AND BranchId IS NULL;
    UPDATE dbo.MemberAttendance SET BranchId = @BranchId WHERE GymId = @GymId AND BranchId IS NULL;
    UPDATE dbo.Payments SET BranchId = @BranchId WHERE GymId = @GymId AND BranchId IS NULL;
    IF OBJECT_ID(N'dbo.Leads', N'U') IS NOT NULL
        UPDATE dbo.Leads SET BranchId = @BranchId WHERE GymId = @GymId AND BranchId IS NULL;
    IF OBJECT_ID(N'dbo.Expenses', N'U') IS NOT NULL
        UPDATE dbo.Expenses SET BranchId = @BranchId WHERE GymId = @GymId AND BranchId IS NULL;
    IF OBJECT_ID(N'dbo.Payrolls', N'U') IS NOT NULL
        UPDATE dbo.Payrolls SET BranchId = @BranchId WHERE GymId = @GymId AND BranchId IS NULL;
    FETCH NEXT FROM gym_cursor INTO @GymId;
END
CLOSE gym_cursor;
DEALLOCATE gym_cursor;
GO

/* ========== BRANCH CRUD ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateBranch
    @GymId UNIQUEIDENTIFIER,
    @BranchName NVARCHAR(200),
    @BranchCode NVARCHAR(20) = NULL,
    @Address NVARCHAR(500) = NULL,
    @City NVARCHAR(100) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(256) = NULL,
    @BranchId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Branches (GymId, BranchName, BranchCode, Address, City, Phone, Email)
    VALUES (@GymId, @BranchName, @BranchCode, @Address, @City, @Phone, @Email);
    SET @BranchId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateBranch
    @BranchId INT, @GymId UNIQUEIDENTIFIER,
    @BranchName NVARCHAR(200), @BranchCode NVARCHAR(20) = NULL,
    @Address NVARCHAR(500) = NULL, @City NVARCHAR(100) = NULL,
    @Phone NVARCHAR(20) = NULL, @Email NVARCHAR(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Branches SET BranchName = @BranchName, BranchCode = @BranchCode, Address = @Address,
        City = @City, Phone = @Phone, Email = @Email, UpdatedDate = SYSUTCDATETIME()
    WHERE BranchId = @BranchId AND GymId = @GymId AND IsDeleted = 0;
    IF @@ROWCOUNT = 0 THROW 50410, 'Branch not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SetBranchActive
    @BranchId INT, @GymId UNIQUEIDENTIFIER, @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Branches SET IsActive = @IsActive, UpdatedDate = SYSUTCDATETIME()
    WHERE BranchId = @BranchId AND GymId = @GymId AND IsDeleted = 0;
    IF @@ROWCOUNT = 0 THROW 50410, 'Branch not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteBranch
    @BranchId INT, @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF (SELECT COUNT(*) FROM dbo.Branches WHERE GymId = @GymId AND IsDeleted = 0) <= 1
        AND EXISTS (SELECT 1 FROM dbo.Branches WHERE BranchId = @BranchId AND GymId = @GymId AND IsDeleted = 0)
        THROW 50411, 'Cannot delete the only branch.', 1;
    UPDATE dbo.Branches SET IsDeleted = 1, IsActive = 0, UpdatedDate = SYSUTCDATETIME()
    WHERE BranchId = @BranchId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBranchById
    @BranchId INT, @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT b.*, bm.UserId AS ManagerUserId, u.Name AS ManagerName
    FROM dbo.Branches b
    LEFT JOIN dbo.BranchManagers bm ON bm.BranchId = b.BranchId AND bm.GymId = b.GymId AND bm.IsActive = 1
    LEFT JOIN dbo.Users u ON u.Id = bm.UserId
    WHERE b.BranchId = @BranchId AND b.GymId = @GymId AND b.IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBranchesPaged
    @GymId UNIQUEIDENTIFIER,
    @Search NVARCHAR(100) = NULL,
    @IncludeInactive BIT = 0,
    @PageNumber INT = 1, @PageSize INT = 20,
    @SortColumn NVARCHAR(50) = N'BranchName', @SortDirection NVARCHAR(4) = N'ASC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH Filtered AS (
        SELECT b.BranchId, b.GymId, b.BranchName, b.BranchCode, b.Address, b.City, b.Phone, b.Email,
               b.IsActive, b.CreatedDate, b.UpdatedDate,
               bm.UserId AS ManagerUserId, u.Name AS ManagerName,
               (SELECT COUNT(*) FROM dbo.Members m WHERE m.BranchId = b.BranchId AND m.IsDeleted = 0) AS MemberCount,
               (SELECT COUNT(*) FROM dbo.Trainers t WHERE t.BranchId = b.BranchId AND t.IsActive = 1) AS TrainerCount
        FROM dbo.Branches b
        LEFT JOIN dbo.BranchManagers bm ON bm.BranchId = b.BranchId AND bm.GymId = b.GymId AND bm.IsActive = 1
        LEFT JOIN dbo.Users u ON u.Id = bm.UserId
        WHERE b.GymId = @GymId AND b.IsDeleted = 0
          AND (@IncludeInactive = 1 OR b.IsActive = 1)
          AND (@Search IS NULL OR b.BranchName LIKE N'%' + @Search + N'%' OR b.BranchCode LIKE N'%' + @Search + N'%')
    )
    SELECT @TotalCount = COUNT(*) FROM Filtered;
    SELECT * FROM Filtered
    ORDER BY
        CASE WHEN @SortColumn = N'BranchName' AND @SortDirection = N'ASC' THEN BranchName END ASC,
        CASE WHEN @SortColumn = N'BranchName' AND @SortDirection = N'DESC' THEN BranchName END DESC,
        BranchName ASC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllBranches
    @GymId UNIQUEIDENTIFIER, @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT BranchId, GymId, BranchName, BranchCode, Address, City, Phone, Email, IsActive
    FROM dbo.Branches WHERE GymId = @GymId AND IsDeleted = 0
      AND (@IncludeInactive = 1 OR IsActive = 1)
    ORDER BY BranchName;
END
GO

/* ========== BRANCH MANAGERS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_AssignBranchManager
    @GymId UNIQUEIDENTIFIER, @BranchId INT, @UserId UNIQUEIDENTIFIER,
    @BranchManagerId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.BranchManagers SET IsActive = 0 WHERE GymId = @GymId AND BranchId = @BranchId AND IsActive = 1;
    INSERT INTO dbo.BranchManagers (GymId, BranchId, UserId) VALUES (@GymId, @BranchId, @UserId);
    SET @BranchManagerId = SCOPE_IDENTITY();
END
GO

/* ========== TRANSFERS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_TransferMemberBranch
    @GymId UNIQUEIDENTIFIER, @MemberId INT, @ToBranchId INT,
    @TransferredByUserId UNIQUEIDENTIFIER = NULL, @Notes NVARCHAR(500) = NULL,
    @TransferId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @FromBranchId INT;
    SELECT @FromBranchId = BranchId FROM dbo.Members WHERE MemberId = @MemberId AND GymId = @GymId AND IsDeleted = 0;
    IF @FromBranchId IS NULL THROW 50412, 'Member not found.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Branches WHERE BranchId = @ToBranchId AND GymId = @GymId AND IsDeleted = 0 AND IsActive = 1)
        THROW 50410, 'Target branch not found.', 1;
    IF @FromBranchId = @ToBranchId THROW 50413, 'Member is already at this branch.', 1;
    UPDATE dbo.Members SET BranchId = @ToBranchId, UpdatedAt = SYSUTCDATETIME() WHERE MemberId = @MemberId;
    INSERT INTO dbo.BranchTransferHistory (GymId, EntityType, EntityId, FromBranchId, ToBranchId, TransferredByUserId, Notes)
    VALUES (@GymId, N'Member', @MemberId, @FromBranchId, @ToBranchId, @TransferredByUserId, @Notes);
    SET @TransferId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_TransferTrainerBranch
    @GymId UNIQUEIDENTIFIER, @TrainerId INT, @ToBranchId INT,
    @TransferredByUserId UNIQUEIDENTIFIER = NULL, @Notes NVARCHAR(500) = NULL,
    @TransferId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @FromBranchId INT;
    SELECT @FromBranchId = BranchId FROM dbo.Trainers WHERE TrainerId = @TrainerId AND GymId = @GymId AND IsActive = 1;
    IF @FromBranchId IS NULL THROW 50414, 'Trainer not found.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Branches WHERE BranchId = @ToBranchId AND GymId = @GymId AND IsDeleted = 0 AND IsActive = 1)
        THROW 50410, 'Target branch not found.', 1;
    IF @FromBranchId = @ToBranchId THROW 50413, 'Trainer is already at this branch.', 1;
    UPDATE dbo.Trainers SET BranchId = @ToBranchId, UpdatedAt = SYSUTCDATETIME() WHERE TrainerId = @TrainerId;
    INSERT INTO dbo.BranchTransferHistory (GymId, EntityType, EntityId, FromBranchId, ToBranchId, TransferredByUserId, Notes)
    VALUES (@GymId, N'Trainer', @TrainerId, @FromBranchId, @ToBranchId, @TransferredByUserId, @Notes);
    SET @TransferId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBranchTransferHistory
    @GymId UNIQUEIDENTIFIER, @EntityType NVARCHAR(20) = NULL, @BranchId INT = NULL,
    @PageNumber INT = 1, @PageSize INT = 20, @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH Filtered AS (
        SELECT t.*, fb.BranchName AS FromBranchName, tb.BranchName AS ToBranchName, u.Name AS TransferredByName,
               CASE t.EntityType
                   WHEN N'Member' THEN (SELECT mu.Name FROM dbo.Members m INNER JOIN dbo.Users mu ON mu.Id = m.UserId WHERE m.MemberId = t.EntityId)
                   WHEN N'Trainer' THEN (SELECT tu.Name FROM dbo.Trainers tr INNER JOIN dbo.Users tu ON tu.Id = tr.UserId WHERE tr.TrainerId = t.EntityId)
               END AS EntityName
        FROM dbo.BranchTransferHistory t
        LEFT JOIN dbo.Branches fb ON fb.BranchId = t.FromBranchId
        LEFT JOIN dbo.Branches tb ON tb.BranchId = t.ToBranchId
        LEFT JOIN dbo.Users u ON u.Id = t.TransferredByUserId
        WHERE t.GymId = @GymId
          AND (@EntityType IS NULL OR t.EntityType = @EntityType)
          AND (@BranchId IS NULL OR t.FromBranchId = @BranchId OR t.ToBranchId = @BranchId)
    )
    SELECT @TotalCount = COUNT(*) FROM Filtered;
    SELECT * FROM Filtered ORDER BY TransferDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

/* ========== TARGETS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_UpsertBranchTarget
    @GymId UNIQUEIDENTIFIER, @BranchId INT, @TargetMonth DATE,
    @RevenueTarget DECIMAL(18, 2), @NewMembersTarget INT, @LeadConversionsTarget INT,
    @TargetId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    MERGE dbo.BranchTargets AS t
    USING (SELECT @GymId AS GymId, @BranchId AS BranchId, @TargetMonth AS TargetMonth) AS s
    ON t.GymId = s.GymId AND t.BranchId = s.BranchId AND t.TargetMonth = s.TargetMonth
    WHEN MATCHED THEN UPDATE SET RevenueTarget = @RevenueTarget, NewMembersTarget = @NewMembersTarget,
        LeadConversionsTarget = @LeadConversionsTarget, UpdatedDate = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN INSERT (GymId, BranchId, TargetMonth, RevenueTarget, NewMembersTarget, LeadConversionsTarget)
        VALUES (@GymId, @BranchId, @TargetMonth, @RevenueTarget, @NewMembersTarget, @LeadConversionsTarget);
    SELECT @TargetId = TargetId FROM dbo.BranchTargets WHERE GymId = @GymId AND BranchId = @BranchId AND TargetMonth = @TargetMonth;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBranchTargets
    @GymId UNIQUEIDENTIFIER, @BranchId INT = NULL, @TargetMonth DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Month DATE = ISNULL(@TargetMonth, DATEFROMPARTS(YEAR(SYSUTCDATETIME()), MONTH(SYSUTCDATETIME()), 1));
    DECLARE @MonthEnd DATE = EOMONTH(@Month);
    SELECT bt.*, b.BranchName,
        ActualRevenue = ISNULL((SELECT SUM(p.Amount) FROM dbo.Payments p WHERE p.GymId = bt.GymId AND p.BranchId = bt.BranchId
            AND CAST(p.PaymentDate AS DATE) BETWEEN @Month AND @MonthEnd AND p.Status = N'Completed'), 0),
        ActualNewMembers = ISNULL((SELECT COUNT(*) FROM dbo.Members m WHERE m.GymId = bt.GymId AND m.BranchId = bt.BranchId
            AND CAST(m.JoinDate AS DATE) BETWEEN @Month AND @MonthEnd AND m.IsDeleted = 0), 0),
        ActualLeadConversions = ISNULL((SELECT COUNT(*) FROM dbo.Leads l WHERE l.GymId = bt.GymId AND l.BranchId = bt.BranchId
            AND l.Status = N'Converted' AND CAST(l.UpdatedDate AS DATE) BETWEEN @Month AND @MonthEnd), 0)
    FROM dbo.BranchTargets bt
    INNER JOIN dbo.Branches b ON b.BranchId = bt.BranchId
    WHERE bt.GymId = @GymId AND (@BranchId IS NULL OR bt.BranchId = @BranchId) AND bt.TargetMonth = @Month;
END
GO

/* ========== ANNOUNCEMENTS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateBranchAnnouncement
    @GymId UNIQUEIDENTIFIER, @BranchId INT = NULL, @Title NVARCHAR(200), @Message NVARCHAR(MAX),
    @TargetAudience NVARCHAR(30), @ExpiryDate DATETIME2 = NULL, @CreatedByUserId UNIQUEIDENTIFIER = NULL,
    @AnnouncementId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.BranchAnnouncements (GymId, BranchId, Title, Message, TargetAudience, ExpiryDate, CreatedByUserId)
    VALUES (@GymId, @BranchId, @Title, @Message, @TargetAudience, @ExpiryDate, @CreatedByUserId);
    SET @AnnouncementId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBranchAnnouncements
    @GymId UNIQUEIDENTIFIER, @BranchId INT = NULL, @TargetAudience NVARCHAR(30) = NULL,
    @ActiveOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.*, b.BranchName
    FROM dbo.BranchAnnouncements a
    LEFT JOIN dbo.Branches b ON b.BranchId = a.BranchId
    WHERE a.GymId = @GymId AND a.IsDeleted = 0
      AND (@BranchId IS NULL OR a.BranchId IS NULL OR a.BranchId = @BranchId)
      AND (@TargetAudience IS NULL OR a.TargetAudience IN (@TargetAudience, N'All'))
      AND (@ActiveOnly = 0 OR (a.IsActive = 1 AND a.PublishDate <= SYSUTCDATETIME() AND (a.ExpiryDate IS NULL OR a.ExpiryDate >= SYSUTCDATETIME())))
    ORDER BY a.PublishDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteBranchAnnouncement
    @AnnouncementId INT, @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.BranchAnnouncements SET IsDeleted = 1 WHERE AnnouncementId = @AnnouncementId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBranchAnnouncementRecipients
    @GymId UNIQUEIDENTIFIER, @AnnouncementId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @BranchId INT, @Audience NVARCHAR(30);
    SELECT @BranchId = BranchId, @Audience = TargetAudience FROM dbo.BranchAnnouncements
    WHERE AnnouncementId = @AnnouncementId AND GymId = @GymId AND IsDeleted = 0;
    IF @Audience IN (N'Members', N'All')
        SELECT DISTINCT m.MemberId, m.Phone, m.UserId AS RecipientUserId, u.Name AS RecipientName
        FROM dbo.Members m INNER JOIN dbo.Users u ON u.Id = m.UserId
        WHERE m.GymId = @GymId AND m.IsDeleted = 0 AND m.IsActive = 1 AND m.Phone IS NOT NULL
          AND (@BranchId IS NULL OR m.BranchId = @BranchId);
    IF @Audience IN (N'Staff', N'All')
        SELECT DISTINCT t.TrainerId AS MemberId, m.Phone, t.UserId AS RecipientUserId, u.Name AS RecipientName
        FROM dbo.Trainers t
        INNER JOIN dbo.Users u ON u.Id = t.UserId
        LEFT JOIN dbo.Members m ON m.UserId = t.UserId AND m.GymId = @GymId AND m.IsDeleted = 0
        WHERE t.GymId = @GymId AND t.IsActive = 1 AND m.Phone IS NOT NULL
          AND (@BranchId IS NULL OR t.BranchId = @BranchId);
END
GO

/* ========== DASHBOARD ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetBranchDashboard
    @GymId UNIQUEIDENTIFIER, @BranchId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
    DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);

    SELECT b.BranchId, b.BranchName,
        MemberCount = (SELECT COUNT(*) FROM dbo.Members m WHERE m.BranchId = b.BranchId AND m.IsDeleted = 0 AND m.IsActive = 1),
        TrainerCount = (SELECT COUNT(*) FROM dbo.Trainers t WHERE t.BranchId = b.BranchId AND t.IsActive = 1),
        RevenueMonth = ISNULL((SELECT SUM(p.Amount) FROM dbo.Payments p WHERE p.BranchId = b.BranchId AND p.Status = N'Completed'
            AND CAST(p.PaymentDate AS DATE) >= @MonthStart), 0),
        AttendanceMonth = ISNULL((SELECT COUNT(DISTINCT ma.MemberId) FROM dbo.MemberAttendance ma WHERE ma.BranchId = b.BranchId
            AND ma.AttendanceDate >= @MonthStart), 0),
        LeadsOpen = ISNULL((SELECT COUNT(*) FROM dbo.Leads l WHERE l.BranchId = b.BranchId AND l.Status NOT IN (N'Converted', N'Lost')), 0),
        ExpensesMonth = ISNULL((SELECT SUM(e.Amount) FROM dbo.Expenses e WHERE e.BranchId = b.BranchId AND e.IsDeleted = 0
            AND e.ExpenseDate >= @MonthStart), 0)
    FROM dbo.Branches b
    WHERE b.GymId = @GymId AND b.IsDeleted = 0 AND b.IsActive = 1
      AND (@BranchId IS NULL OR b.BranchId = @BranchId)
    ORDER BY b.BranchName;
END
GO

/* ========== ANALYTICS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetBranchAnalyticsComparison
    @GymId UNIQUEIDENTIFIER, @Months INT = 6
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @FromDate DATE = DATEADD(MONTH, -@Months, CAST(SYSUTCDATETIME() AS DATE));

    -- Branch ranking by revenue
    SELECT b.BranchId, b.BranchName,
        TotalRevenue = ISNULL(SUM(CASE WHEN p.Status = N'Completed' THEN p.Amount ELSE 0 END), 0),
        TotalExpenses = ISNULL((SELECT SUM(e.Amount) FROM dbo.Expenses e WHERE e.BranchId = b.BranchId AND e.IsDeleted = 0 AND e.ExpenseDate >= @FromDate), 0),
        MemberCount = (SELECT COUNT(*) FROM dbo.Members m WHERE m.BranchId = b.BranchId AND m.IsDeleted = 0),
        AttendanceCount = ISNULL((SELECT COUNT(*) FROM dbo.MemberAttendance ma WHERE ma.BranchId = b.BranchId AND ma.AttendanceDate >= @FromDate), 0),
        LeadConversions = ISNULL((SELECT COUNT(*) FROM dbo.Leads l WHERE l.BranchId = b.BranchId AND l.Status = N'Converted' AND l.UpdatedDate >= @FromDate), 0)
    FROM dbo.Branches b
    LEFT JOIN dbo.Payments p ON p.BranchId = b.BranchId AND CAST(p.PaymentDate AS DATE) >= @FromDate
    WHERE b.GymId = @GymId AND b.IsDeleted = 0
    GROUP BY b.BranchId, b.BranchName
    ORDER BY TotalRevenue DESC;

    -- Monthly revenue by branch
    SELECT b.BranchName, YEAR(p.PaymentDate) AS [Year], MONTH(p.PaymentDate) AS [Month], SUM(p.Amount) AS Revenue
    FROM dbo.Payments p INNER JOIN dbo.Branches b ON b.BranchId = p.BranchId
    WHERE p.GymId = @GymId AND p.Status = N'Completed' AND CAST(p.PaymentDate AS DATE) >= @FromDate
    GROUP BY b.BranchName, YEAR(p.PaymentDate), MONTH(p.PaymentDate)
    ORDER BY [Year], [Month], b.BranchName;
END
GO
