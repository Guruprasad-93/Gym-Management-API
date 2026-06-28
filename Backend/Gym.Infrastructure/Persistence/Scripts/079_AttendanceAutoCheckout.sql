/*
  Automatic and manual member check-out: schema, settings, status, and stored procedures.
*/

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ========== SCHEMA ========== */
IF COL_LENGTH('dbo.MemberAttendance', 'IsAutoCheckout') IS NULL
    ALTER TABLE dbo.MemberAttendance ADD IsAutoCheckout BIT NOT NULL
        CONSTRAINT DF_MemberAttendance_IsAutoCheckout DEFAULT (0);
GO

IF COL_LENGTH('dbo.MemberAttendance', 'CheckoutType') IS NULL
    ALTER TABLE dbo.MemberAttendance ADD CheckoutType NVARCHAR(20) NULL;
GO

UPDATE dbo.MemberAttendance
SET CheckoutType = N'Normal', IsAutoCheckout = 0
WHERE CheckOutAt IS NOT NULL AND CheckoutType IS NULL;
GO

MERGE dbo.AttendanceStatuses AS t
USING (VALUES
    (7, N'AUTO_CHECKED_OUT', N'Auto Checked Out', N'Session closed automatically at gym closing time')
) AS s (AttendanceStatusId, Code, Name, Description)
ON t.AttendanceStatusId = s.AttendanceStatusId
WHEN NOT MATCHED BY TARGET THEN
    INSERT (AttendanceStatusId, Code, Name, Description) VALUES (s.AttendanceStatusId, s.Code, s.Name, s.Description);
GO

IF OBJECT_ID(N'dbo.AttendanceSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AttendanceSettings
    (
        GymId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AttendanceSettings PRIMARY KEY,
        OpeningTime TIME(0) NOT NULL CONSTRAINT DF_AttendanceSettings_Opening DEFAULT ('06:00'),
        ClosingTime TIME(0) NOT NULL CONSTRAINT DF_AttendanceSettings_Closing DEFAULT ('22:00'),
        AutoCheckoutEnabled BIT NOT NULL CONSTRAINT DF_AttendanceSettings_AutoCheckout DEFAULT (1),
        UseClosingTimeForAutoCheckout BIT NOT NULL CONSTRAINT DF_AttendanceSettings_UseClosingTime DEFAULT (1),
        CheckoutReminderMinutesBefore INT NOT NULL CONSTRAINT DF_AttendanceSettings_Reminder DEFAULT (30),
        TimeZoneId NVARCHAR(100) NOT NULL CONSTRAINT DF_AttendanceSettings_TimeZone DEFAULT (N'India Standard Time'),
        CONSTRAINT FK_AttendanceSettings_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId)
    );
END
GO

/* ========== SETTINGS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetAttendanceSettings
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.AttendanceSettings WHERE GymId = @GymId)
        INSERT INTO dbo.AttendanceSettings (GymId) VALUES (@GymId);

    SELECT GymId, OpeningTime, ClosingTime, AutoCheckoutEnabled,
           UseClosingTimeForAutoCheckout, CheckoutReminderMinutesBefore, TimeZoneId
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
    @TimeZoneId NVARCHAR(100)
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
            TimeZoneId = @TimeZoneId
        WHERE GymId = @GymId;
    ELSE
        INSERT INTO dbo.AttendanceSettings (
            GymId, OpeningTime, ClosingTime, AutoCheckoutEnabled,
            UseClosingTimeForAutoCheckout, CheckoutReminderMinutesBefore, TimeZoneId)
        VALUES (
            @GymId, @OpeningTime, @ClosingTime, @AutoCheckoutEnabled,
            @UseClosingTimeForAutoCheckout, @CheckoutReminderMinutesBefore, @TimeZoneId);
END
GO

/* ========== CHECK-OUT ========== */
CREATE OR ALTER PROCEDURE dbo.sp_MemberAttendance_CheckOut
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT = NULL,
    @TargetAttendanceId INT = NULL,
    @MarkedByUserId UNIQUEIDENTIFIER = NULL,
    @Notes NVARCHAR(500) = NULL,
    @CheckoutType NVARCHAR(20) = N'Normal',
    @MemberAttendanceId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CheckoutType NOT IN (N'Normal', N'Manual')
            THROW 50410, 'Invalid checkout type for manual checkout.', 1;

        DECLARE @OpenId INT;

        IF @TargetAttendanceId IS NOT NULL
        BEGIN
            SELECT @OpenId = ma.MemberAttendanceId
            FROM dbo.MemberAttendance ma
            WHERE ma.MemberAttendanceId = @TargetAttendanceId
              AND ma.GymId = @GymId
              AND ma.CheckOutAt IS NULL
              AND ma.AttendanceStatusId = 1;

            IF @OpenId IS NULL
                THROW 50402, 'No open check-in session found for this attendance record.', 1;

            IF @MemberId IS NOT NULL AND EXISTS (
                SELECT 1 FROM dbo.MemberAttendance WHERE MemberAttendanceId = @OpenId AND MemberId <> @MemberId)
                THROW 50411, 'Attendance record does not belong to the specified member.', 1;
        END
        ELSE
        BEGIN
            IF @MemberId IS NULL
                THROW 50412, 'MemberId or TargetAttendanceId is required.', 1;

            SELECT TOP 1 @OpenId = MemberAttendanceId
            FROM dbo.MemberAttendance
            WHERE GymId = @GymId
              AND MemberId = @MemberId
              AND CheckOutAt IS NULL
              AND AttendanceStatusId = 1
            ORDER BY CheckInAt DESC;

            IF @OpenId IS NULL
                THROW 50402, 'No open check-in session found for this member.', 1;
        END

        DECLARE @Now DATETIME2 = SYSUTCDATETIME();

        UPDATE dbo.MemberAttendance
        SET CheckOutAt = @Now,
            AttendanceStatusId = 2,
            CheckoutType = @CheckoutType,
            IsAutoCheckout = 0,
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

/* ========== AUTO CHECK-OUT JOB ========== */
CREATE OR ALTER PROCEDURE dbo.sp_ProcessAttendanceAutoCheckout
    @ProcessedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @ProcessedCount = 0;

    DECLARE @NowUtc DATETIME2 = SYSUTCDATETIME();

    IF OBJECT_ID('tempdb..#GymsDue') IS NOT NULL DROP TABLE #GymsDue;

    SELECT
        s.GymId,
        s.ClosingTime,
        s.UseClosingTimeForAutoCheckout,
        COALESCE(NULLIF(LTRIM(RTRIM(s.TimeZoneId)), N''), N'India Standard Time') AS TimeZoneId,
        CAST((@NowUtc AT TIME ZONE 'UTC' AT TIME ZONE COALESCE(NULLIF(LTRIM(RTRIM(s.TimeZoneId)), N''), N'India Standard Time')) AS TIME) AS LocalTime,
        CAST((@NowUtc AT TIME ZONE 'UTC' AT TIME ZONE COALESCE(NULLIF(LTRIM(RTRIM(s.TimeZoneId)), N''), N'India Standard Time')) AS DATE) AS LocalDate
    INTO #GymsDue
    FROM dbo.AttendanceSettings s
    WHERE s.AutoCheckoutEnabled = 1;

    UPDATE ma
    SET CheckOutAt = CASE
            WHEN g.UseClosingTimeForAutoCheckout = 1 THEN
                CAST(
                    (CAST(g.LocalDate AS DATETIME) + CAST(g.ClosingTime AS DATETIME))
                    AT TIME ZONE g.TimeZoneId AT TIME ZONE 'UTC' AS DATETIME2)
            ELSE @NowUtc
        END,
        AttendanceStatusId = 7,
        IsAutoCheckout = 1,
        CheckoutType = N'Auto',
        Notes = CASE
            WHEN ma.Notes IS NULL OR ma.Notes = N'' THEN N'Auto checked out at gym closing'
            ELSE ma.Notes + N'; Auto checked out at gym closing'
        END,
        UpdatedAt = @NowUtc
    FROM dbo.MemberAttendance ma
    INNER JOIN #GymsDue g ON g.GymId = ma.GymId
    WHERE ma.CheckOutAt IS NULL
      AND ma.AttendanceStatusId = 1
      AND g.LocalTime >= g.ClosingTime;

    SET @ProcessedCount = @@ROWCOUNT;
END
GO

/* ========== CHECKOUT REMINDER PUSH ========== */
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
      AND s.CheckoutReminderMinutesBefore > 0
      AND CAST((@NowUtc AT TIME ZONE 'UTC' AT TIME ZONE COALESCE(NULLIF(LTRIM(RTRIM(s.TimeZoneId)), N''), N'India Standard Time')) AS TIME)
          BETWEEN DATEADD(MINUTE, -s.CheckoutReminderMinutesBefore, s.ClosingTime) AND s.ClosingTime
      AND EXISTS (
          SELECT 1 FROM dbo.DeviceTokens dt
          WHERE dt.UserId = m.UserId AND dt.GymId = m.GymId AND dt.IsActive = 1);
END
GO

/* ========== QUERY HELPERS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMemberAttendanceById
    @MemberAttendanceId INT,
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
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
        ma.CheckoutType,
        ma.IsAutoCheckout,
        ma.Notes,
        ma.MarkedByUserId,
        mu.Name AS MarkedByName,
        ma.CreatedAt,
        ma.UpdatedAt
    FROM dbo.MemberAttendance ma
    INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.AttendanceStatuses st ON st.AttendanceStatusId = ma.AttendanceStatusId
    LEFT JOIN dbo.Trainers t ON t.TrainerId = ma.TrainerId
    LEFT JOIN dbo.Users tu ON tu.Id = t.UserId
    LEFT JOIN dbo.Users mu ON mu.Id = ma.MarkedByUserId
    WHERE ma.MemberAttendanceId = @MemberAttendanceId
      AND (ma.GymId = @GymId)
      AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId OR ma.TrainerId = @TrainerId);
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
        ma.CheckoutType,
        ma.IsAutoCheckout,
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

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberAttendanceByDateRange
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL,
    @MemberId INT = NULL,
    @FromDate DATE,
    @ToDate DATE,
    @StatusId INT = NULL,
    @Search NVARCHAR(200) = NULL,
    @OpenOnly BIT = 0,
    @CheckoutTypeFilter NVARCHAR(20) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = 'CheckInAt',
    @SortDirection NVARCHAR(4) = 'DESC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF OBJECT_ID('tempdb..#Filtered') IS NOT NULL DROP TABLE #Filtered;

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
        ma.CheckoutType,
        ma.IsAutoCheckout,
        ma.Notes,
        ma.CreatedAt
    INTO #Filtered
    FROM dbo.MemberAttendance ma
    INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId AND m.IsDeleted = 0
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.AttendanceStatuses st ON st.AttendanceStatusId = ma.AttendanceStatusId
    LEFT JOIN dbo.Trainers t ON t.TrainerId = ma.TrainerId
    LEFT JOIN dbo.Users tu ON tu.Id = t.UserId
    WHERE ma.AttendanceDate >= @FromDate AND ma.AttendanceDate <= @ToDate
      AND (ma.GymId = @GymId)
      AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)
      AND (@MemberId IS NULL OR ma.MemberId = @MemberId)
      AND (@StatusId IS NULL OR ma.AttendanceStatusId = @StatusId)
      AND (@Search IS NULL OR u.Name LIKE N'%' + @Search + N'%' OR u.Email LIKE N'%' + @Search + N'%')
      AND (@OpenOnly = 0 OR (ma.CheckOutAt IS NULL AND ma.AttendanceStatusId = 1))
      AND (
          @CheckoutTypeFilter IS NULL
          OR (@CheckoutTypeFilter = N'Open' AND ma.CheckOutAt IS NULL AND ma.AttendanceStatusId = 1)
          OR (@CheckoutTypeFilter IN (N'Normal', N'Manual', N'Auto') AND ma.CheckoutType = @CheckoutTypeFilter)
      );

    SET @TotalCount = (SELECT COUNT(*) FROM #Filtered);

    SELECT * FROM #Filtered
    ORDER BY
        CASE WHEN @SortColumn = 'MemberName' AND @SortDirection = 'ASC' THEN MemberName END ASC,
        CASE WHEN @SortColumn = 'MemberName' AND @SortDirection = 'DESC' THEN MemberName END DESC,
        CASE WHEN @SortColumn = 'AttendanceDate' AND @SortDirection = 'ASC' THEN AttendanceDate END ASC,
        CASE WHEN @SortColumn = 'AttendanceDate' AND @SortDirection = 'DESC' THEN AttendanceDate END DESC,
        CASE WHEN @SortColumn = 'StatusName' AND @SortDirection = 'ASC' THEN StatusName END ASC,
        CASE WHEN @SortColumn = 'StatusName' AND @SortDirection = 'DESC' THEN StatusName END DESC,
        CheckInAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetDailyAttendanceReport
    @GymId UNIQUEIDENTIFIER = NULL,
    @ReportDate DATE,
    @TrainerId INT = NULL,
    @OpenOnly BIT = 0,
    @CheckoutTypeFilter NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        @ReportDate AS ReportDate,
        st.AttendanceStatusId,
        st.Code AS StatusCode,
        st.Name AS StatusName,
        COUNT(*) AS RecordCount
    FROM dbo.MemberAttendance ma
    INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId AND m.IsDeleted = 0
    INNER JOIN dbo.AttendanceStatuses st ON st.AttendanceStatusId = ma.AttendanceStatusId
    WHERE ma.AttendanceDate = @ReportDate
      AND (ma.GymId = @GymId)
      AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)
      AND (@OpenOnly = 0 OR (ma.CheckOutAt IS NULL AND ma.AttendanceStatusId = 1))
      AND (
          @CheckoutTypeFilter IS NULL
          OR (@CheckoutTypeFilter = N'Open' AND ma.CheckOutAt IS NULL AND ma.AttendanceStatusId = 1)
          OR (@CheckoutTypeFilter IN (N'Normal', N'Manual', N'Auto') AND ma.CheckoutType = @CheckoutTypeFilter)
      )
    GROUP BY st.AttendanceStatusId, st.Code, st.Name;

    SELECT
        ma.MemberAttendanceId,
        ma.MemberId,
        u.Name AS MemberName,
        st.Name AS StatusName,
        st.Code AS StatusCode,
        ma.CheckInAt,
        ma.CheckOutAt,
        ma.CheckoutType,
        ma.IsAutoCheckout
    FROM dbo.MemberAttendance ma
    INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.AttendanceStatuses st ON st.AttendanceStatusId = ma.AttendanceStatusId
    WHERE ma.AttendanceDate = @ReportDate
      AND (ma.GymId = @GymId)
      AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)
      AND (@OpenOnly = 0 OR (ma.CheckOutAt IS NULL AND ma.AttendanceStatusId = 1))
      AND (
          @CheckoutTypeFilter IS NULL
          OR (@CheckoutTypeFilter = N'Open' AND ma.CheckOutAt IS NULL AND ma.AttendanceStatusId = 1)
          OR (@CheckoutTypeFilter IN (N'Normal', N'Manual', N'Auto') AND ma.CheckoutType = @CheckoutTypeFilter)
      )
    ORDER BY u.Name;
END
GO

/* ========== MEMBER DASHBOARD TODAY VISIT ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMemberTodayVisit
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST((SYSUTCDATETIME() AT TIME ZONE 'UTC' AT TIME ZONE 'India Standard Time') AS DATE);

    SELECT TOP 1
        ma.CheckInAt,
        ma.CheckOutAt,
        st.Code AS StatusCode,
        st.Name AS StatusName,
        ma.CheckoutType,
        ma.IsAutoCheckout,
        CASE WHEN ma.CheckOutAt IS NULL AND ma.AttendanceStatusId = 1 THEN 1 ELSE 0 END AS IsCurrentlyCheckedIn,
        mu.Name AS CheckedOutByName
    FROM dbo.MemberAttendance ma
    INNER JOIN dbo.AttendanceStatuses st ON st.AttendanceStatusId = ma.AttendanceStatusId
    LEFT JOIN dbo.Users mu ON mu.Id = ma.MarkedByUserId AND ma.CheckoutType = N'Manual'
    WHERE ma.GymId = @GymId
      AND ma.MemberId = @MemberId
      AND ma.AttendanceDate = @Today
    ORDER BY ma.CheckInAt DESC;
END
GO
