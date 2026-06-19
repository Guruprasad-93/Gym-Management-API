/*
  Fix branch transfers when members/trainers have no BranchId assigned yet.
*/

-- Backfill missing branch assignments
DECLARE @GymId UNIQUEIDENTIFIER;
DECLARE @BranchId INT;

DECLARE gym_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT DISTINCT GymId FROM dbo.Members WHERE BranchId IS NULL AND IsDeleted = 0
    UNION
    SELECT DISTINCT GymId FROM dbo.Trainers WHERE BranchId IS NULL AND IsActive = 1;

OPEN gym_cursor;
FETCH NEXT FROM gym_cursor INTO @GymId;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC dbo.sp_EnsureDefaultBranch @GymId, @BranchId OUTPUT;

    UPDATE dbo.Members
    SET BranchId = @BranchId, UpdatedAt = SYSUTCDATETIME()
    WHERE GymId = @GymId AND BranchId IS NULL AND IsDeleted = 0;

    UPDATE dbo.Trainers
    SET BranchId = @BranchId, UpdatedAt = SYSUTCDATETIME()
    WHERE GymId = @GymId AND BranchId IS NULL AND IsActive = 1;

    FETCH NEXT FROM gym_cursor INTO @GymId;
END

CLOSE gym_cursor;
DEALLOCATE gym_cursor;
GO

CREATE OR ALTER PROCEDURE dbo.sp_TransferMemberBranch
    @GymId UNIQUEIDENTIFIER, @MemberId INT, @ToBranchId INT,
    @TransferredByUserId UNIQUEIDENTIFIER = NULL, @Notes NVARCHAR(500) = NULL,
    @TransferId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @FromBranchId INT;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Members
        WHERE MemberId = @MemberId AND GymId = @GymId AND IsDeleted = 0)
        THROW 50412, 'Member not found.', 1;

    SELECT @FromBranchId = BranchId
    FROM dbo.Members
    WHERE MemberId = @MemberId AND GymId = @GymId AND IsDeleted = 0;

    IF @FromBranchId IS NULL
    BEGIN
        EXEC dbo.sp_EnsureDefaultBranch @GymId, @FromBranchId OUTPUT;
        UPDATE dbo.Members
        SET BranchId = @FromBranchId, UpdatedAt = SYSUTCDATETIME()
        WHERE MemberId = @MemberId;
    END

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Branches
        WHERE BranchId = @ToBranchId AND GymId = @GymId AND IsDeleted = 0 AND IsActive = 1)
        THROW 50410, 'Target branch not found.', 1;

    IF @FromBranchId = @ToBranchId
        THROW 50413, 'Member is already at this branch.', 1;

    UPDATE dbo.Members
    SET BranchId = @ToBranchId, UpdatedAt = SYSUTCDATETIME()
    WHERE MemberId = @MemberId;

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

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Trainers
        WHERE TrainerId = @TrainerId AND GymId = @GymId AND IsActive = 1)
        THROW 50414, 'Trainer not found.', 1;

    SELECT @FromBranchId = BranchId
    FROM dbo.Trainers
    WHERE TrainerId = @TrainerId AND GymId = @GymId AND IsActive = 1;

    IF @FromBranchId IS NULL
    BEGIN
        EXEC dbo.sp_EnsureDefaultBranch @GymId, @FromBranchId OUTPUT;
        UPDATE dbo.Trainers
        SET BranchId = @FromBranchId, UpdatedAt = SYSUTCDATETIME()
        WHERE TrainerId = @TrainerId;
    END

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Branches
        WHERE BranchId = @ToBranchId AND GymId = @GymId AND IsDeleted = 0 AND IsActive = 1)
        THROW 50410, 'Target branch not found.', 1;

    IF @FromBranchId = @ToBranchId
        THROW 50413, 'Trainer is already at this branch.', 1;

    UPDATE dbo.Trainers
    SET BranchId = @ToBranchId, UpdatedAt = SYSUTCDATETIME()
    WHERE TrainerId = @TrainerId;

    INSERT INTO dbo.BranchTransferHistory (GymId, EntityType, EntityId, FromBranchId, ToBranchId, TransferredByUserId, Notes)
    VALUES (@GymId, N'Trainer', @TrainerId, @FromBranchId, @ToBranchId, @TransferredByUserId, @Notes);

    SET @TransferId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateMember
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @TrainerId INT = NULL,
    @DateOfBirth DATE = NULL,
    @Gender NVARCHAR(20) = NULL,
    @Height DECIMAL(5, 2) = NULL,
    @Weight DECIMAL(6, 2) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Address NVARCHAR(500) = NULL,
    @EmergencyContact NVARCHAR(200) = NULL,
    @JoinDate DATE,
    @MemberId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @BranchId INT;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Gyms WHERE GymId = @GymId AND IsActive = 1)
            THROW 50040, 'Gym not found or inactive.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @UserId)
            THROW 50041, 'User not found.', 1;

        IF @TrainerId IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM dbo.Trainers WHERE TrainerId = @TrainerId AND GymId = @GymId AND IsActive = 1)
            THROW 50042, 'Trainer not found or inactive.', 1;

        EXEC dbo.sp_EnsureDefaultBranch @GymId, @BranchId OUTPUT;

        INSERT INTO dbo.Members (
            GymId, UserId, TrainerId, BranchId, DateOfBirth, Gender, Height, Weight, Phone, Address,
            EmergencyContact, JoinDate, IsActive, IsDeleted, CreatedAt)
        VALUES (
            @GymId, @UserId, @TrainerId, @BranchId, @DateOfBirth, @Gender, @Height, @Weight, @Phone, @Address,
            @EmergencyContact, @JoinDate, 1, 0, SYSUTCDATETIME());

        SET @MemberId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
