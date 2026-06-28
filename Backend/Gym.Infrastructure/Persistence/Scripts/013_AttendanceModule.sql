/*
  Attendance Management Module
  - AttendanceStatuses lookup
  - MemberAttendance (enhanced)
  - TrainerAttendance
  - Audit log insert SP
*/

/* ========== ATTENDANCE STATUSES ========== */
IF OBJECT_ID(N'dbo.AttendanceStatuses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AttendanceStatuses
    (
        AttendanceStatusId INT NOT NULL PRIMARY KEY,
        Code NVARCHAR(30) NOT NULL,
        Name NVARCHAR(50) NOT NULL,
        Description NVARCHAR(200) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_AttendanceStatuses_IsActive DEFAULT (1)
    );
END
GO

MERGE dbo.AttendanceStatuses AS t
USING (VALUES
    (1, N'CHECKED_IN', N'Checked In', N'Member checked in; session open'),
    (2, N'CHECKED_OUT', N'Checked Out', N'Member checked out; session complete'),
    (3, N'PRESENT', N'Present', N'Manually marked present'),
    (4, N'ABSENT', N'Absent', N'Manually marked absent'),
    (5, N'LATE', N'Late', N'Marked late'),
    (6, N'EXCUSED', N'Excused', N'Excused absence')
) AS s (AttendanceStatusId, Code, Name, Description)
ON t.AttendanceStatusId = s.AttendanceStatusId
WHEN NOT MATCHED BY TARGET THEN
    INSERT (AttendanceStatusId, Code, Name, Description) VALUES (s.AttendanceStatusId, s.Code, s.Name, s.Description);
GO

/* ========== MEMBER ATTENDANCE SCHEMA ========== */
IF COL_LENGTH('dbo.MemberAttendance', 'AttendanceStatusId') IS NULL
    ALTER TABLE dbo.MemberAttendance ADD AttendanceStatusId INT NULL;
GO
IF COL_LENGTH('dbo.MemberAttendance', 'TrainerId') IS NULL
    ALTER TABLE dbo.MemberAttendance ADD TrainerId INT NULL;
GO
IF COL_LENGTH('dbo.MemberAttendance', 'Notes') IS NULL
    ALTER TABLE dbo.MemberAttendance ADD Notes NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.MemberAttendance', 'MarkedByUserId') IS NULL
    ALTER TABLE dbo.MemberAttendance ADD MarkedByUserId UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH('dbo.MemberAttendance', 'UpdatedAt') IS NULL
    ALTER TABLE dbo.MemberAttendance ADD UpdatedAt DATETIME2 NULL;
GO
IF COL_LENGTH('dbo.MemberAttendance', 'AttendanceDate') IS NULL
    ALTER TABLE dbo.MemberAttendance ADD AttendanceDate DATE NULL;
GO

UPDATE dbo.MemberAttendance
SET AttendanceStatusId = 2,
    AttendanceDate = CAST(CheckInAt AS DATE)
WHERE AttendanceStatusId IS NULL AND CheckOutAt IS NOT NULL;
GO
UPDATE dbo.MemberAttendance
SET AttendanceStatusId = 1,
    AttendanceDate = CAST(CheckInAt AS DATE)
WHERE AttendanceStatusId IS NULL AND CheckOutAt IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_MemberAttendance_AttendanceStatuses')
BEGIN
    ALTER TABLE dbo.MemberAttendance
    ADD CONSTRAINT FK_MemberAttendance_AttendanceStatuses
        FOREIGN KEY (AttendanceStatusId) REFERENCES dbo.AttendanceStatuses (AttendanceStatusId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_MemberAttendance_Trainers')
BEGIN
    ALTER TABLE dbo.MemberAttendance
    ADD CONSTRAINT FK_MemberAttendance_Trainers
        FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers (TrainerId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_MemberAttendance_MarkedBy')
BEGIN
    ALTER TABLE dbo.MemberAttendance
    ADD CONSTRAINT FK_MemberAttendance_MarkedBy
        FOREIGN KEY (MarkedByUserId) REFERENCES dbo.Users (Id);
END
GO

/* ========== TRAINER ATTENDANCE ========== */
IF OBJECT_ID(N'dbo.TrainerAttendance', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainerAttendance
    (
        TrainerAttendanceId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        TrainerId INT NOT NULL,
        AttendanceStatusId INT NOT NULL,
        AttendanceDate DATE NOT NULL,
        CheckInAt DATETIME2 NULL,
        CheckOutAt DATETIME2 NULL,
        Notes NVARCHAR(500) NULL,
        MarkedByUserId UNIQUEIDENTIFIER NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_TrainerAttendance_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_TrainerAttendance_Trainers FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers (TrainerId),
        CONSTRAINT FK_TrainerAttendance_Status FOREIGN KEY (AttendanceStatusId) REFERENCES dbo.AttendanceStatuses (AttendanceStatusId),
        CONSTRAINT FK_TrainerAttendance_MarkedBy FOREIGN KEY (MarkedByUserId) REFERENCES dbo.Users (Id)
    );
    CREATE INDEX IX_TrainerAttendance_Gym_Date ON dbo.TrainerAttendance (GymId, AttendanceDate);
END
GO

/* ========== AUDIT LOG INSERT ========== */
CREATE OR ALTER PROCEDURE dbo.sp_AuditLog_Insert
    @GymId UNIQUEIDENTIFIER = NULL,
    @UserId UNIQUEIDENTIFIER = NULL,
    @EntityName NVARCHAR(100),
    @EntityId NVARCHAR(50),
    @Action NVARCHAR(50),
    @OldValues NVARCHAR(MAX) = NULL,
    @NewValues NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AuditLogs (GymId, UserId, EntityName, EntityId, Action, OldValues, NewValues, CreatedAt)
    VALUES (@GymId, @UserId, @EntityName, @EntityId, @Action, @OldValues, @NewValues, SYSUTCDATETIME());
END
GO

/* ========== STATUS LIST ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetAttendanceStatuses
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AttendanceStatusId, Code, Name, Description
    FROM dbo.AttendanceStatuses
    WHERE IsActive = 1
    ORDER BY AttendanceStatusId;
END
GO

/* ========== MEMBER CHECK-IN ========== */
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
            WHERE GymId = @GymId AND MemberId = @MemberId AND CheckOutAt IS NULL
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

/* ========== MEMBER CHECK-OUT ========== */
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
        WHERE GymId = @GymId AND MemberId = @MemberId AND CheckOutAt IS NULL
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

/* ========== MARK ATTENDANCE (manual) ========== */
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

/* ========== GET BY ID ========== */
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

/* ========== TODAY ========== */
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

/* ========== DATE RANGE PAGED ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMemberAttendanceByDateRange
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL,
    @MemberId INT = NULL,
    @FromDate DATE,
    @ToDate DATE,
    @StatusId INT = NULL,
    @Search NVARCHAR(200) = NULL,
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
      AND (@Search IS NULL OR u.Name LIKE N'%' + @Search + N'%' OR u.Email LIKE N'%' + @Search + N'%');

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

/* ========== MEMBER HISTORY ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMemberAttendanceHistory
    @GymId UNIQUEIDENTIFIER = NULL,
    @MemberId INT,
    @TrainerId INT = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @F DATE = COALESCE(@FromDate, DATEADD(MONTH, -3, CAST(SYSUTCDATETIME() AS DATE)));
    DECLARE @T DATE = COALESCE(@ToDate, CAST(SYSUTCDATETIME() AS DATE));

    EXEC dbo.sp_GetMemberAttendanceByDateRange
        @GymId = @GymId,
        @TrainerId = @TrainerId,
        @MemberId = @MemberId,
        @FromDate = @F,
        @ToDate = @T,
        @StatusId = NULL,
        @Search = NULL,
        @PageNumber = @PageNumber,
        @PageSize = @PageSize,
        @SortColumn = 'AttendanceDate',
        @SortDirection = 'DESC',
        @TotalCount = @TotalCount OUTPUT;
END
GO

/* ========== DAILY REPORT ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetDailyAttendanceReport
    @GymId UNIQUEIDENTIFIER = NULL,
    @ReportDate DATE,
    @TrainerId INT = NULL
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
    GROUP BY st.AttendanceStatusId, st.Code, st.Name;

    SELECT
        ma.MemberAttendanceId,
        ma.MemberId,
        u.Name AS MemberName,
        st.Name AS StatusName,
        ma.CheckInAt,
        ma.CheckOutAt
    FROM dbo.MemberAttendance ma
    INNER JOIN dbo.Members m ON m.MemberId = ma.MemberId
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.AttendanceStatuses st ON st.AttendanceStatusId = ma.AttendanceStatusId
    WHERE ma.AttendanceDate = @ReportDate
      AND (ma.GymId = @GymId)
      AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)
    ORDER BY u.Name;
END
GO

/* ========== MONTHLY REPORT ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMonthlyAttendanceReport
    @GymId UNIQUEIDENTIFIER = NULL,
    @Year INT,
    @Month INT,
    @TrainerId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @From DATE = DATEFROMPARTS(@Year, @Month, 1);
    DECLARE @To DATE = EOMONTH(@From);

    SELECT
        m.MemberId,
        u.Name AS MemberName,
        COUNT(CASE WHEN ma.AttendanceStatusId IN (1, 2, 3) THEN 1 END) AS PresentDays,
        COUNT(CASE WHEN ma.AttendanceStatusId = 4 THEN 1 END) AS AbsentDays,
        COUNT(CASE WHEN ma.AttendanceStatusId = 5 THEN 1 END) AS LateDays,
        COUNT(CASE WHEN ma.AttendanceStatusId = 6 THEN 1 END) AS ExcusedDays,
        COUNT(*) AS TotalRecords
    FROM dbo.Members m
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    LEFT JOIN dbo.MemberAttendance ma ON ma.MemberId = m.MemberId
        AND ma.AttendanceDate >= @From AND ma.AttendanceDate <= @To
    WHERE m.IsDeleted = 0 AND m.IsActive = 1
      AND (m.GymId = @GymId)
      AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)
    GROUP BY m.MemberId, u.Name
    ORDER BY u.Name;
END
GO

/* ========== TRAINER CHECK-IN ========== */
CREATE OR ALTER PROCEDURE dbo.sp_TrainerAttendance_CheckIn
    @GymId UNIQUEIDENTIFIER,
    @TrainerId INT,
    @MarkedByUserId UNIQUEIDENTIFIER = NULL,
    @Notes NVARCHAR(500) = NULL,
    @TrainerAttendanceId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Trainers t WHERE t.TrainerId = @TrainerId AND t.GymId = @GymId AND t.IsActive = 1)
        THROW 50410, 'Trainer not found.', 1;

    IF EXISTS (SELECT 1 FROM dbo.TrainerAttendance WHERE GymId = @GymId AND TrainerId = @TrainerId AND CheckOutAt IS NULL)
        THROW 50411, 'Trainer already checked in.', 1;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    INSERT INTO dbo.TrainerAttendance (GymId, TrainerId, AttendanceStatusId, AttendanceDate, CheckInAt, Notes, MarkedByUserId, CreatedAt)
    VALUES (@GymId, @TrainerId, 1, CAST(@Now AS DATE), @Now, @Notes, @MarkedByUserId, @Now);
    SET @TrainerAttendanceId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_TrainerAttendance_CheckOut
    @GymId UNIQUEIDENTIFIER,
    @TrainerId INT,
    @MarkedByUserId UNIQUEIDENTIFIER = NULL,
    @TrainerAttendanceId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Id INT;
    SELECT TOP 1 @Id = TrainerAttendanceId FROM dbo.TrainerAttendance
    WHERE GymId = @GymId AND TrainerId = @TrainerId AND CheckOutAt IS NULL ORDER BY CheckInAt DESC;
    IF @Id IS NULL THROW 50412, 'No open trainer check-in.', 1;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    UPDATE dbo.TrainerAttendance SET CheckOutAt = @Now, AttendanceStatusId = 2, UpdatedAt = @Now,
        MarkedByUserId = COALESCE(@MarkedByUserId, MarkedByUserId) WHERE TrainerAttendanceId = @Id;
    SET @TrainerAttendanceId = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetTrainerAttendanceByDateRange
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL,
    @FromDate DATE,
    @ToDate DATE,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF OBJECT_ID('tempdb..#TFiltered') IS NOT NULL DROP TABLE #TFiltered;

    SELECT ta.TrainerAttendanceId, ta.GymId, ta.TrainerId, u.Name AS TrainerName,
           ta.AttendanceStatusId, st.Name AS StatusName, ta.AttendanceDate,
           ta.CheckInAt, ta.CheckOutAt, ta.Notes, ta.CreatedAt
    INTO #TFiltered
    FROM dbo.TrainerAttendance ta
    INNER JOIN dbo.Trainers t ON t.TrainerId = ta.TrainerId
    INNER JOIN dbo.Users u ON u.Id = t.UserId
    INNER JOIN dbo.AttendanceStatuses st ON st.AttendanceStatusId = ta.AttendanceStatusId
    WHERE ta.AttendanceDate >= @FromDate AND ta.AttendanceDate <= @ToDate
      AND (ta.GymId = @GymId)
      AND (@TrainerId IS NULL OR ta.TrainerId = @TrainerId);

    SET @TotalCount = (SELECT COUNT(*) FROM #TFiltered);
    SELECT * FROM #TFiltered ORDER BY AttendanceDate DESC, CheckInAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

/* ========== ATTENDANCE DASHBOARD STATS ========== */
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
