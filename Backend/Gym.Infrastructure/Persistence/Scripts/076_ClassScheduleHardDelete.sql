/*
  Class schedule hard delete
  - Permanently removes schedules and related waitlist/booking rows (transactional)
  - Purges legacy soft-cancelled schedules
*/

SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteClassSchedule
    @GymId UNIQUEIDENTIFIER,
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.ClassSchedules WHERE Id = @Id AND GymId = @GymId)
            THROW 50404, 'Class schedule not found.', 1;

        DELETE FROM dbo.BookingWaitlist WHERE GymId = @GymId AND ClassScheduleId = @Id;
        DELETE FROM dbo.SlotBookings WHERE GymId = @GymId AND ClassScheduleId = @Id;
        DELETE FROM dbo.ClassSchedules WHERE Id = @Id AND GymId = @GymId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/* Remove legacy soft-cancelled schedules and their related rows */
BEGIN TRY
    BEGIN TRANSACTION;

    DELETE w
    FROM dbo.BookingWaitlist w
    INNER JOIN dbo.ClassSchedules cs ON cs.Id = w.ClassScheduleId AND cs.GymId = w.GymId
    WHERE cs.Status = N'Cancelled';

    DELETE sb
    FROM dbo.SlotBookings sb
    INNER JOIN dbo.ClassSchedules cs ON cs.Id = sb.ClassScheduleId AND cs.GymId = sb.GymId
    WHERE cs.Status = N'Cancelled';

    DELETE FROM dbo.ClassSchedules WHERE Status = N'Cancelled';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO
