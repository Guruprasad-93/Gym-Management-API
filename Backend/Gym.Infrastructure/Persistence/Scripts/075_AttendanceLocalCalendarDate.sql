/*
  Attendance calendar date should follow the gym's local day (India Standard Time for MVP gyms).
  Check-in/out timestamps remain UTC (SYSUTCDATETIME); only AttendanceDate uses local midnight boundaries.
*/

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
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

CREATE OR ALTER PROCEDURE dbo.sp_GetTodayMemberAttendance
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL,
    @Search NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST((SYSUTCDATETIME() AT TIME ZONE 'UTC' AT TIME ZONE 'India Standard Time') AS DATE);

    SELECT
        ma.MemberAttendanceId,
        ma.GymId,
        ma.MemberId,
        u.Name AS MemberName,
        u.Email AS MemberEmail,
        ma.TrainerId,
        tu.Name AS TrainerName,
        ma.AttendanceStatusId,
        st.Code AS StatusCode,
        st.Name AS StatusName,
        ma.AttendanceDate,
        ma.CheckInAt,
        ma.CheckOutAt,
        ma.Notes,
        ma.CreatedAt
    FROM dbo.MemberAttendance ma
    INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId AND m.IsDeleted = 0
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.AttendanceStatuses st ON st.AttendanceStatusId = ma.AttendanceStatusId
    LEFT JOIN dbo.Trainers t ON t.TrainerId = ma.TrainerId
    LEFT JOIN dbo.Users tu ON tu.Id = t.UserId
    WHERE ma.AttendanceDate = @Today
      AND (ma.GymId = @GymId)
      AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)
      AND (@Search IS NULL OR u.Name LIKE N'%' + @Search + N'%' OR u.Email LIKE N'%' + @Search + N'%')
    ORDER BY ma.CheckInAt DESC, u.Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAttendanceDashboard
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST((SYSUTCDATETIME() AT TIME ZONE 'UTC' AT TIME ZONE 'India Standard Time') AS DATE);

    SELECT
        (SELECT COUNT(*) FROM dbo.Members m WHERE m.IsDeleted = 0 AND m.IsActive = 1
            AND (m.GymId = @GymId) AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)) AS TotalActiveMembers,
        (SELECT COUNT(DISTINCT ma.MemberId) FROM dbo.MemberAttendance ma
            INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId
            WHERE ma.AttendanceDate = @Today AND (ma.GymId = @GymId)
            AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)) AS MembersPresentToday,
        (SELECT COUNT(*) FROM dbo.MemberAttendance ma
            INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId
            WHERE ma.AttendanceDate = @Today AND ma.CheckOutAt IS NULL AND ma.AttendanceStatusId = 1
            AND (ma.GymId = @GymId) AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)) AS CurrentlyCheckedIn,
        (SELECT COUNT(*) FROM dbo.MemberAttendance ma
            INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId
            WHERE ma.AttendanceDate = @Today AND ma.AttendanceStatusId = 4
            AND (ma.GymId = @GymId) AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)) AS AbsentToday;
END
GO
