/* Repair sp_MemberAttendance_QrCheckIn when 074 referenced non-existent MemberQrCodes.
   Canonical token table: dbo.MemberQrTokens (029_MemberSelfServiceModule.sql). */
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
