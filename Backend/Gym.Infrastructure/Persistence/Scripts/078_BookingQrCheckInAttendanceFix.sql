/* sp_BookingQrCheckIn: MemberAttendance insert must include AttendanceStatusId and CreatedAt. */
CREATE OR ALTER PROCEDURE dbo.sp_BookingQrCheckIn
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @QrToken NVARCHAR(64),
    @MarkedByUserId UNIQUEIDENTIFIER,
    @BookingId INT OUTPUT,
    @ErrorMessage NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @BookingId = 0;
    SET @ErrorMessage = NULL;

    IF NOT EXISTS (SELECT 1 FROM dbo.MemberQrTokens WHERE GymId = @GymId AND MemberId = @MemberId AND QrToken = @QrToken)
    BEGIN SET @ErrorMessage = N'Invalid QR token.'; RETURN; END

    SELECT TOP 1 @BookingId = sb.Id
    FROM dbo.SlotBookings sb
    INNER JOIN dbo.ClassSchedules cs ON cs.Id = sb.ClassScheduleId
    WHERE sb.GymId = @GymId AND sb.MemberId = @MemberId AND sb.BookingDate = CAST(SYSUTCDATETIME() AS DATE)
      AND sb.Status = N'Booked'
    ORDER BY cs.StartTime;

    IF @BookingId IS NULL BEGIN SET @ErrorMessage = N'No active booking found for today.'; RETURN; END
    IF EXISTS (SELECT 1 FROM dbo.SlotBookings WHERE Id = @BookingId AND Status = N'CheckedIn')
    BEGIN SET @ErrorMessage = N'Already checked in.'; RETURN; END

    UPDATE dbo.SlotBookings SET Status = N'CheckedIn', CheckInTime = SYSUTCDATETIME()
    WHERE Id = @BookingId AND GymId = @GymId AND Status = N'Booked';

    IF NOT EXISTS (SELECT 1 FROM dbo.MemberAttendance WHERE GymId = @GymId AND MemberId = @MemberId AND AttendanceDate = CAST(SYSUTCDATETIME() AS DATE))
    BEGIN
        INSERT INTO dbo.MemberAttendance (GymId, MemberId, AttendanceStatusId, AttendanceDate, CheckInAt, Notes, MarkedByUserId, CreatedAt)
        VALUES (@GymId, @MemberId, 1, CAST(SYSUTCDATETIME() AS DATE), SYSUTCDATETIME(), N'Booking QR check-in', @MarkedByUserId, SYSUTCDATETIME());
    END
END
GO
