/*
  Auto check-out improvements: 24x7 session duration, closing-time accuracy, dashboard KPIs, forgot check-out report.
*/

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ========== SETTINGS SCHEMA ========== */
IF COL_LENGTH('dbo.AttendanceSettings', 'Is24Hours') IS NULL
    ALTER TABLE dbo.AttendanceSettings ADD Is24Hours BIT NOT NULL
        CONSTRAINT DF_AttendanceSettings_Is24Hours DEFAULT (0);
GO

IF COL_LENGTH('dbo.AttendanceSettings', 'MaximumSessionHours') IS NULL
    ALTER TABLE dbo.AttendanceSettings ADD MaximumSessionHours INT NOT NULL
        CONSTRAINT DF_AttendanceSettings_MaxSessionHours DEFAULT (12);
GO

/* ========== SETTINGS PROCEDURES ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetAttendanceSettings
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.AttendanceSettings WHERE GymId = @GymId)
        INSERT INTO dbo.AttendanceSettings (GymId) VALUES (@GymId);

    SELECT GymId, OpeningTime, ClosingTime, AutoCheckoutEnabled,
           UseClosingTimeForAutoCheckout, CheckoutReminderMinutesBefore, TimeZoneId,
           Is24Hours, MaximumSessionHours
    FROM dbo.AttendanceSettings
    WHERE GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpsertAttendanceSettings
    @GymId UNIQUEIDENTIFIER,
    @OpeningTime TIME(0),
    @ClosingTime TIME(0),
    @AutoCheckoutEnabled BIT,
    @UseClosingTimeForAutoCheckout BIT,
    @CheckoutReminderMinutesBefore INT,
    @TimeZoneId NVARCHAR(100),
    @Is24Hours BIT,
    @MaximumSessionHours INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.AttendanceSettings WHERE GymId = @GymId)
        UPDATE dbo.AttendanceSettings
        SET OpeningTime = @OpeningTime,
            ClosingTime = @ClosingTime,
            AutoCheckoutEnabled = @AutoCheckoutEnabled,
            UseClosingTimeForAutoCheckout = @UseClosingTimeForAutoCheckout,
            CheckoutReminderMinutesBefore = @CheckoutReminderMinutesBefore,
            TimeZoneId = @TimeZoneId,
            Is24Hours = @Is24Hours,
            MaximumSessionHours = @MaximumSessionHours
        WHERE GymId = @GymId;
    ELSE
        INSERT INTO dbo.AttendanceSettings (
            GymId, OpeningTime, ClosingTime, AutoCheckoutEnabled,
            UseClosingTimeForAutoCheckout, CheckoutReminderMinutesBefore, TimeZoneId,
            Is24Hours, MaximumSessionHours)
        VALUES (
            @GymId, @OpeningTime, @ClosingTime, @AutoCheckoutEnabled,
            @UseClosingTimeForAutoCheckout, @CheckoutReminderMinutesBefore, @TimeZoneId,
            @Is24Hours, @MaximumSessionHours);
END
GO

/* ========== AUTO CHECK-OUT JOB ========== */
CREATE OR ALTER PROCEDURE dbo.sp_ProcessAttendanceAutoCheckout
    @ProcessedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @ProcessedCount = 0;

    DECLARE @NowUtc DATETIME2 = SYSUTCDATETIME();

    IF OBJECT_ID('tempdb..#DueSessions') IS NOT NULL DROP TABLE #DueSessions;

    SELECT
        ma.MemberAttendanceId,
        CASE
            WHEN s.Is24Hours = 1 THEN DATEADD(HOUR, s.MaximumSessionHours, ma.CheckInAt)
            ELSE CAST(
                (CAST(ma.AttendanceDate AS DATETIME) + CAST(s.ClosingTime AS DATETIME))
                AT TIME ZONE COALESCE(NULLIF(LTRIM(RTRIM(s.TimeZoneId)), N''), N'India Standard Time')
                AT TIME ZONE 'UTC' AS DATETIME2)
        END AS DueCheckOutAt
    INTO #DueSessions
    FROM dbo.MemberAttendance ma
    INNER JOIN dbo.AttendanceSettings s ON s.GymId = ma.GymId
    WHERE ma.CheckOutAt IS NULL
      AND ma.AttendanceStatusId = 1
      AND s.AutoCheckoutEnabled = 1
      AND ma.CheckInAt IS NOT NULL
      AND (
          (s.Is24Hours = 1 AND @NowUtc >= DATEADD(HOUR, s.MaximumSessionHours, ma.CheckInAt))
          OR (
              s.Is24Hours = 0
              AND @NowUtc >= CAST(
                  (CAST(ma.AttendanceDate AS DATETIME) + CAST(s.ClosingTime AS DATETIME))
                  AT TIME ZONE COALESCE(NULLIF(LTRIM(RTRIM(s.TimeZoneId)), N''), N'India Standard Time')
                  AT TIME ZONE 'UTC' AS DATETIME2)
          )
      );

    UPDATE ma
    SET CheckOutAt = d.DueCheckOutAt,
        AttendanceStatusId = 7,
        IsAutoCheckout = 1,
        CheckoutType = N'Auto',
        Notes = CASE
            WHEN ma.Notes IS NULL OR ma.Notes = N'' THEN
                CASE WHEN s.Is24Hours = 1 THEN N'Auto checked out after maximum session duration'
                     ELSE N'Auto checked out at gym closing' END
            ELSE ma.Notes + N'; ' +
                CASE WHEN s.Is24Hours = 1 THEN N'Auto checked out after maximum session duration'
                     ELSE N'Auto checked out at gym closing' END
        END,
        UpdatedAt = @NowUtc
    FROM dbo.MemberAttendance ma
    INNER JOIN #DueSessions d ON d.MemberAttendanceId = ma.MemberAttendanceId
    INNER JOIN dbo.AttendanceSettings s ON s.GymId = ma.GymId;

    SET @ProcessedCount = @@ROWCOUNT;
END
GO

/* ========== CHECKOUT REMINDER (closing-time gyms only) ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMembersForCheckoutReminderPush
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NowUtc DATETIME2 = SYSUTCDATETIME();

    SELECT DISTINCT m.GymId, m.MemberId, m.UserId, u.Name AS MemberName
    FROM dbo.MemberAttendance ma
    INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId AND m.IsDeleted = 0 AND m.IsActive = 1
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.AttendanceSettings s ON s.GymId = ma.GymId
    WHERE ma.CheckOutAt IS NULL
      AND ma.AttendanceStatusId = 1
      AND s.AutoCheckoutEnabled = 1
      AND s.Is24Hours = 0
      AND s.CheckoutReminderMinutesBefore > 0
      AND CAST((@NowUtc AT TIME ZONE 'UTC' AT TIME ZONE COALESCE(NULLIF(LTRIM(RTRIM(s.TimeZoneId)), N''), N'India Standard Time')) AS TIME)
          BETWEEN DATEADD(MINUTE, -s.CheckoutReminderMinutesBefore, s.ClosingTime) AND s.ClosingTime
      AND EXISTS (
          SELECT 1 FROM dbo.DeviceTokens dt
          WHERE dt.UserId = m.UserId AND dt.GymId = m.GymId AND dt.IsActive = 1);
END
GO

/* ========== DASHBOARD KPIs ========== */
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
            AND (ma.GymId = @GymId) AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)) AS AbsentToday,
        (SELECT COUNT(*) FROM dbo.MemberAttendance ma
            INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId
            WHERE ma.AttendanceDate = @Today AND ma.CheckOutAt IS NOT NULL
            AND (ma.GymId = @GymId) AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)) AS CheckedOutToday,
        (SELECT COUNT(*) FROM dbo.MemberAttendance ma
            INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId
            WHERE ma.AttendanceDate = @Today
            AND (ma.CheckoutType = N'Auto' OR ma.IsAutoCheckout = 1 OR ma.AttendanceStatusId = 7)
            AND (ma.GymId = @GymId) AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)) AS AutoCheckedOutToday,
        (SELECT COUNT(*) FROM dbo.MemberAttendance ma
            INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId
            WHERE ma.AttendanceDate = @Today AND ma.CheckoutType = N'Manual'
            AND (ma.GymId = @GymId) AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)) AS ManualCheckOutToday;
END
GO

/* ========== FORGOT CHECK-OUT REPORT ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetForgotCheckOutReport
    @GymId UNIQUEIDENTIFIER = NULL,
    @BranchId INT = NULL,
    @MemberId INT = NULL,
    @FromDate DATE,
    @ToDate DATE,
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF OBJECT_ID('tempdb..#ForgotCheckOut') IS NOT NULL DROP TABLE #ForgotCheckOut;

    SELECT
        m.MemberId,
        u.Name AS MemberName,
        m.BranchId,
        b.BranchName,
        COUNT(*) AS TotalAutoCheckOutCount,
        MAX(ma.CheckOutAt) AS LastAutoCheckOutAt
    INTO #ForgotCheckOut
    FROM dbo.MemberAttendance ma
    INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId AND m.IsDeleted = 0
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    LEFT JOIN dbo.Branches b ON b.BranchId = m.BranchId
    WHERE ma.AttendanceDate >= @FromDate AND ma.AttendanceDate <= @ToDate
      AND (ma.GymId = @GymId)
      AND (@MemberId IS NULL OR ma.MemberId = @MemberId)
      AND (@BranchId IS NULL OR m.BranchId = @BranchId)
      AND (ma.CheckoutType = N'Auto' OR ma.IsAutoCheckout = 1 OR ma.AttendanceStatusId = 7)
    GROUP BY m.MemberId, u.Name, m.BranchId, b.BranchName;

    SET @TotalCount = (SELECT COUNT(*) FROM #ForgotCheckOut);

    SELECT
        MemberId,
        MemberName,
        BranchId,
        BranchName,
        TotalAutoCheckOutCount,
        LastAutoCheckOutAt,
        CAST(LastAutoCheckOutAt AS DATE) AS LastAutoCheckOutDate
    FROM #ForgotCheckOut
    ORDER BY TotalAutoCheckOutCount DESC, LastAutoCheckOutAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
