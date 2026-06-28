/*
  Fix attendance open-session detection.
  Manual Present/Absent marks (status 3-6) must not block member check-in.
*/

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Close legacy manual-mark rows that were incorrectly treated as open sessions.
UPDATE dbo.MemberAttendance
SET CheckOutAt = COALESCE(CheckInAt, SYSUTCDATETIME()),
    UpdatedAt = SYSUTCDATETIME()
WHERE AttendanceStatusId NOT IN (1)
  AND CheckOutAt IS NULL;
GO

CREATE OR ALTER PROCEDURE dbo.sp_MemberAttendance_CheckIn
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @TrainerId INT = NULL,
    @MarkedByUserId UNIQUEIDENTIFIER = NULL,
    @Notes NVARCHAR(500) = NULL,
    @MemberAttendanceId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Members m WHERE m.MemberId = @MemberId AND m.GymId = @GymId AND m.IsDeleted = 0 AND m.IsActive = 1)
            THROW 50400, 'Member not found or inactive.', 1;

        IF EXISTS (
            SELECT 1 FROM dbo.MemberAttendance
            WHERE GymId = @GymId
              AND MemberId = @MemberId
              AND CheckOutAt IS NULL
              AND AttendanceStatusId = 1
        )
            THROW 50401, 'Member already has an open check-in session.', 1;

        DECLARE @Now DATETIME2 = SYSUTCDATETIME();
        DECLARE @Date DATE = CAST((@Now AT TIME ZONE 'UTC' AT TIME ZONE 'India Standard Time') AS DATE);

        INSERT INTO dbo.MemberAttendance (
            GymId, MemberId, TrainerId, AttendanceStatusId, AttendanceDate,
            CheckInAt, CheckOutAt, Notes, MarkedByUserId, CreatedAt, UpdatedAt)
        VALUES (
            @GymId, @MemberId, @TrainerId, 1, @Date,
            @Now, NULL, @Notes, @MarkedByUserId, @Now, NULL);

        SET @MemberAttendanceId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MemberAttendance_CheckOut
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @MarkedByUserId UNIQUEIDENTIFIER = NULL,
    @Notes NVARCHAR(500) = NULL,
    @MemberAttendanceId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        DECLARE @OpenId INT;
        SELECT TOP 1 @OpenId = MemberAttendanceId
        FROM dbo.MemberAttendance
        WHERE GymId = @GymId
          AND MemberId = @MemberId
          AND CheckOutAt IS NULL
          AND AttendanceStatusId = 1
        ORDER BY CheckInAt DESC;

        IF @OpenId IS NULL
            THROW 50402, 'No open check-in session found for this member.', 1;

        DECLARE @Now DATETIME2 = SYSUTCDATETIME();

        UPDATE dbo.MemberAttendance
        SET CheckOutAt = @Now,
            AttendanceStatusId = 2,
            Notes = COALESCE(@Notes, Notes),
            MarkedByUserId = COALESCE(@MarkedByUserId, MarkedByUserId),
            UpdatedAt = @Now
        WHERE MemberAttendanceId = @OpenId;

        SET @MemberAttendanceId = @OpenId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MemberAttendance_Mark
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @AttendanceDate DATE,
    @AttendanceStatusId INT,
    @TrainerId INT = NULL,
    @MarkedByUserId UNIQUEIDENTIFIER = NULL,
    @Notes NVARCHAR(500) = NULL,
    @MemberAttendanceId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Members m WHERE m.MemberId = @MemberId AND m.GymId = @GymId AND m.IsDeleted = 0)
            THROW 50400, 'Member not found.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.AttendanceStatuses WHERE AttendanceStatusId = @AttendanceStatusId AND IsActive = 1)
            THROW 50403, 'Invalid attendance status.', 1;

        IF @AttendanceStatusId IN (1, 2)
            THROW 50404, 'Use check-in/check-out for session statuses.', 1;

        DECLARE @ExistingId INT;
        SELECT @ExistingId = MemberAttendanceId
        FROM dbo.MemberAttendance
        WHERE GymId = @GymId AND MemberId = @MemberId AND AttendanceDate = @AttendanceDate
          AND AttendanceStatusId NOT IN (1);

        DECLARE @Now DATETIME2 = SYSUTCDATETIME();

        IF @ExistingId IS NOT NULL
        BEGIN
            UPDATE dbo.MemberAttendance
            SET AttendanceStatusId = @AttendanceStatusId,
                TrainerId = COALESCE(@TrainerId, TrainerId),
                Notes = @Notes,
                MarkedByUserId = @MarkedByUserId,
                CheckOutAt = COALESCE(CheckOutAt, @Now),
                UpdatedAt = @Now
            WHERE MemberAttendanceId = @ExistingId;
            SET @MemberAttendanceId = @ExistingId;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.MemberAttendance (
                GymId, MemberId, TrainerId, AttendanceStatusId, AttendanceDate,
                CheckInAt, CheckOutAt, Notes, MarkedByUserId, CreatedAt, UpdatedAt)
            VALUES (
                @GymId, @MemberId, @TrainerId, @AttendanceStatusId, @AttendanceDate,
                @Now, @Now, @Notes, @MarkedByUserId, @Now, NULL);
            SET @MemberAttendanceId = SCOPE_IDENTITY();
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MemberAttendance_QrCheckIn
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @QrToken NVARCHAR(200),
    @MarkedByUserId UNIQUEIDENTIFIER = NULL,
    @MemberAttendanceId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF NOT EXISTS (
            SELECT 1
            FROM dbo.MemberQrTokens q
            INNER JOIN dbo.Members m ON m.MemberId = q.MemberId
            WHERE q.MemberId = @MemberId AND q.GymId = @GymId AND q.QrToken = @QrToken
              AND m.IsDeleted = 0 AND m.IsActive = 1
        )
            THROW 50408, 'Invalid or expired QR code.', 1;

        DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);

        IF EXISTS (
            SELECT 1 FROM dbo.MemberAttendance
            WHERE GymId = @GymId AND MemberId = @MemberId AND AttendanceDate = @Today
        )
            THROW 50409, 'Member already checked in today.', 1;

        IF EXISTS (
            SELECT 1 FROM dbo.MemberAttendance
            WHERE GymId = @GymId
              AND MemberId = @MemberId
              AND CheckOutAt IS NULL
              AND AttendanceStatusId = 1
        )
            THROW 50401, 'Member already has an open check-in session.', 1;

        DECLARE @Now DATETIME2 = SYSUTCDATETIME();
        INSERT INTO dbo.MemberAttendance (
            GymId, MemberId, AttendanceStatusId, AttendanceDate, CheckInAt, Notes, MarkedByUserId, CreatedAt)
        VALUES (@GymId, @MemberId, 1, @Today, @Now, N'QR check-in', @MarkedByUserId, @Now);
        SET @MemberAttendanceId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
