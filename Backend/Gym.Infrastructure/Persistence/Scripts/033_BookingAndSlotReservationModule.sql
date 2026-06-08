/*
  Booking & Slot Reservation Module
*/

/* ========== CLASS SCHEDULES ========== */
IF OBJECT_ID(N'dbo.ClassSchedules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClassSchedules
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        BranchId INT NOT NULL,
        ClassName NVARCHAR(150) NOT NULL,
        Description NVARCHAR(1000) NULL,
        TrainerId INT NOT NULL,
        DayOfWeek INT NOT NULL,
        StartTime TIME NOT NULL,
        EndTime TIME NOT NULL,
        Capacity INT NOT NULL CONSTRAINT DF_ClassSchedules_Capacity DEFAULT (20),
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_ClassSchedules_Status DEFAULT (N'Active'),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_ClassSchedules_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_ClassSchedules_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_ClassSchedules_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches (BranchId),
        CONSTRAINT FK_ClassSchedules_Trainers FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers (TrainerId)
    );
    CREATE INDEX IX_ClassSchedules_Gym ON dbo.ClassSchedules (GymId, BranchId, Status);
END
GO

/* ========== SLOT BOOKINGS ========== */
IF OBJECT_ID(N'dbo.SlotBookings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SlotBookings
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        BranchId INT NOT NULL,
        MemberId INT NOT NULL,
        ClassScheduleId INT NOT NULL,
        BookingDate DATE NOT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_SlotBookings_Status DEFAULT (N'Booked'),
        CheckInTime DATETIME2 NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_SlotBookings_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_SlotBookings_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_SlotBookings_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches (BranchId),
        CONSTRAINT FK_SlotBookings_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT FK_SlotBookings_ClassSchedules FOREIGN KEY (ClassScheduleId) REFERENCES dbo.ClassSchedules (Id)
    );
    CREATE INDEX IX_SlotBookings_Member ON dbo.SlotBookings (GymId, MemberId, BookingDate DESC);
    CREATE INDEX IX_SlotBookings_Schedule ON dbo.SlotBookings (GymId, ClassScheduleId, BookingDate, Status);
    CREATE UNIQUE INDEX UX_SlotBookings_Active ON dbo.SlotBookings (GymId, MemberId, ClassScheduleId, BookingDate)
        WHERE Status IN (N'Booked', N'CheckedIn', N'Completed', N'NoShow');
END
GO

/* ========== TRAINER AVAILABILITY ========== */
IF OBJECT_ID(N'dbo.TrainerAvailability', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainerAvailability
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        BranchId INT NOT NULL,
        TrainerId INT NOT NULL,
        DayOfWeek INT NOT NULL,
        StartTime TIME NOT NULL,
        EndTime TIME NOT NULL,
        IsAvailable BIT NOT NULL CONSTRAINT DF_TrainerAvailability_Available DEFAULT (1),
        CONSTRAINT FK_TrainerAvailability_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_TrainerAvailability_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches (BranchId),
        CONSTRAINT FK_TrainerAvailability_Trainers FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers (TrainerId)
    );
    CREATE INDEX IX_TrainerAvailability_Trainer ON dbo.TrainerAvailability (GymId, TrainerId, DayOfWeek);
END
GO

/* ========== BOOKING WAITLIST ========== */
IF OBJECT_ID(N'dbo.BookingWaitlist', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BookingWaitlist
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        ClassScheduleId INT NOT NULL,
        BookingDate DATE NOT NULL,
        Position INT NOT NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_BookingWaitlist_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_BookingWaitlist_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_BookingWaitlist_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT FK_BookingWaitlist_ClassSchedules FOREIGN KEY (ClassScheduleId) REFERENCES dbo.ClassSchedules (Id),
        CONSTRAINT UX_BookingWaitlist_Member UNIQUE (GymId, MemberId, ClassScheduleId, BookingDate)
    );
    CREATE INDEX IX_BookingWaitlist_Schedule ON dbo.BookingWaitlist (GymId, ClassScheduleId, BookingDate, Position);
END
GO

/* ========== BOOKING SETTINGS ========== */
IF OBJECT_ID(N'dbo.BookingSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BookingSettings
    (
        GymId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        MaxBookingsPerDay INT NOT NULL CONSTRAINT DF_BookingSettings_MaxBookings DEFAULT (3),
        AllowWaitlist BIT NOT NULL CONSTRAINT DF_BookingSettings_Waitlist DEFAULT (1),
        CancellationWindowHours INT NOT NULL CONSTRAINT DF_BookingSettings_CancelWindow DEFAULT (2),
        ReminderMinutesBefore INT NOT NULL CONSTRAINT DF_BookingSettings_Reminder DEFAULT (60),
        CONSTRAINT FK_BookingSettings_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId)
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpsertBookingSettings
    @GymId UNIQUEIDENTIFIER,
    @MaxBookingsPerDay INT,
    @AllowWaitlist BIT,
    @CancellationWindowHours INT,
    @ReminderMinutesBefore INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.BookingSettings WHERE GymId = @GymId)
        UPDATE dbo.BookingSettings SET MaxBookingsPerDay = @MaxBookingsPerDay, AllowWaitlist = @AllowWaitlist,
            CancellationWindowHours = @CancellationWindowHours, ReminderMinutesBefore = @ReminderMinutesBefore
        WHERE GymId = @GymId;
    ELSE
        INSERT INTO dbo.BookingSettings (GymId, MaxBookingsPerDay, AllowWaitlist, CancellationWindowHours, ReminderMinutesBefore)
        VALUES (@GymId, @MaxBookingsPerDay, @AllowWaitlist, @CancellationWindowHours, @ReminderMinutesBefore);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBookingSettings
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.BookingSettings WHERE GymId = @GymId)
        INSERT INTO dbo.BookingSettings (GymId) VALUES (@GymId);
    SELECT GymId, MaxBookingsPerDay, AllowWaitlist, CancellationWindowHours, ReminderMinutesBefore
    FROM dbo.BookingSettings WHERE GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateClassSchedule
    @GymId UNIQUEIDENTIFIER,
    @BranchId INT,
    @ClassName NVARCHAR(150),
    @Description NVARCHAR(1000) = NULL,
    @TrainerId INT,
    @DayOfWeek INT,
    @StartTime TIME,
    @EndTime TIME,
    @Capacity INT,
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ClassSchedules (GymId, BranchId, ClassName, Description, TrainerId, DayOfWeek, StartTime, EndTime, Capacity)
    VALUES (@GymId, @BranchId, @ClassName, @Description, @TrainerId, @DayOfWeek, @StartTime, @EndTime, @Capacity);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateClassSchedule
    @GymId UNIQUEIDENTIFIER,
    @Id INT,
    @BranchId INT,
    @ClassName NVARCHAR(150),
    @Description NVARCHAR(1000) = NULL,
    @TrainerId INT,
    @DayOfWeek INT,
    @StartTime TIME,
    @EndTime TIME,
    @Capacity INT,
    @Status NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.ClassSchedules SET BranchId = @BranchId, ClassName = @ClassName, Description = @Description,
        TrainerId = @TrainerId, DayOfWeek = @DayOfWeek, StartTime = @StartTime, EndTime = @EndTime,
        Capacity = @Capacity, Status = @Status
    WHERE Id = @Id AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetClassScheduleById
    @GymId UNIQUEIDENTIFIER,
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT cs.Id, cs.GymId, cs.BranchId, b.BranchName, cs.ClassName, cs.Description, cs.TrainerId, tu.Name AS TrainerName,
           cs.DayOfWeek, cs.StartTime, cs.EndTime, cs.Capacity, cs.Status, cs.CreatedDate
    FROM dbo.ClassSchedules cs
    INNER JOIN dbo.Branches b ON b.BranchId = cs.BranchId
    INNER JOIN dbo.Trainers t ON t.TrainerId = cs.TrainerId
    INNER JOIN dbo.Users tu ON tu.Id = t.UserId
    WHERE cs.Id = @Id AND cs.GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetClassSchedulesPaged
    @GymId UNIQUEIDENTIFIER,
    @BranchId INT = NULL,
    @Status NVARCHAR(30) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @TotalCount = COUNT(*)
    FROM dbo.ClassSchedules cs
    WHERE cs.GymId = @GymId
      AND (@BranchId IS NULL OR cs.BranchId = @BranchId)
      AND (@Status IS NULL OR cs.Status = @Status);

    SELECT cs.Id, cs.GymId, cs.BranchId, b.BranchName, cs.ClassName, cs.Description, cs.TrainerId, tu.Name AS TrainerName,
           cs.DayOfWeek, cs.StartTime, cs.EndTime, cs.Capacity, cs.Status, cs.CreatedDate
    FROM dbo.ClassSchedules cs
    INNER JOIN dbo.Branches b ON b.BranchId = cs.BranchId
    INNER JOIN dbo.Trainers t ON t.TrainerId = cs.TrainerId
    INNER JOIN dbo.Users tu ON tu.Id = t.UserId
    WHERE cs.GymId = @GymId
      AND (@BranchId IS NULL OR cs.BranchId = @BranchId)
      AND (@Status IS NULL OR cs.Status = @Status)
    ORDER BY cs.DayOfWeek, cs.StartTime
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteClassSchedule
    @GymId UNIQUEIDENTIFIER,
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.ClassSchedules SET Status = N'Cancelled' WHERE Id = @Id AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAvailableSlots
    @GymId UNIQUEIDENTIFIER,
    @BranchId INT = NULL,
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH Dates AS (
        SELECT @FromDate AS SlotDate
        UNION ALL
        SELECT DATEADD(DAY, 1, SlotDate) FROM Dates WHERE SlotDate < @ToDate
    )
    SELECT cs.Id AS ClassScheduleId, cs.GymId, cs.BranchId, b.BranchName, cs.ClassName, cs.Description,
           cs.TrainerId, tu.Name AS TrainerName, d.SlotDate AS BookingDate, cs.StartTime, cs.EndTime, cs.Capacity,
           cs.Capacity - ISNULL((SELECT COUNT(*) FROM dbo.SlotBookings sb
               WHERE sb.ClassScheduleId = cs.Id AND sb.BookingDate = d.SlotDate
                 AND sb.Status IN (N'Booked', N'CheckedIn', N'Completed', N'NoShow')), 0) AS RemainingCapacity,
           ISNULL((SELECT COUNT(*) FROM dbo.BookingWaitlist w
               WHERE w.ClassScheduleId = cs.Id AND w.BookingDate = d.SlotDate), 0) AS WaitlistCount
    FROM dbo.ClassSchedules cs
    INNER JOIN dbo.Branches b ON b.BranchId = cs.BranchId
    INNER JOIN dbo.Trainers t ON t.TrainerId = cs.TrainerId
    INNER JOIN dbo.Users tu ON tu.Id = t.UserId
    CROSS JOIN Dates d
    WHERE cs.GymId = @GymId AND cs.Status = N'Active'
      AND (@BranchId IS NULL OR cs.BranchId = @BranchId)
      AND DATEPART(WEEKDAY, d.SlotDate) = CASE cs.DayOfWeek WHEN 0 THEN 1 WHEN 1 THEN 2 WHEN 2 THEN 3 WHEN 3 THEN 4 WHEN 4 THEN 5 WHEN 5 THEN 6 ELSE 7 END
      AND d.SlotDate >= CAST(SYSUTCDATETIME() AS DATE)
    OPTION (MAXRECURSION 366);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateSlotBooking
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @ClassScheduleId INT,
    @BookingDate DATE,
    @Id INT OUTPUT,
    @ErrorMessage NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @ErrorMessage = NULL;
    SET @Id = 0;

    DECLARE @BranchId INT, @Capacity INT, @BookedCount INT, @MaxPerDay INT, @MemberBookingsToday INT;

    SELECT @BranchId = BranchId, @Capacity = Capacity FROM dbo.ClassSchedules
    WHERE Id = @ClassScheduleId AND GymId = @GymId AND Status = N'Active';
    IF @BranchId IS NULL BEGIN SET @ErrorMessage = N'Class schedule not found or inactive.'; RETURN; END

    IF EXISTS (SELECT 1 FROM dbo.SlotBookings WHERE GymId = @GymId AND MemberId = @MemberId AND ClassScheduleId = @ClassScheduleId
               AND BookingDate = @BookingDate AND Status IN (N'Booked', N'CheckedIn', N'Completed', N'NoShow'))
    BEGIN SET @ErrorMessage = N'Duplicate booking.'; RETURN; END

    SELECT @MaxPerDay = MaxBookingsPerDay FROM dbo.BookingSettings WHERE GymId = @GymId;
    IF @MaxPerDay IS NULL SET @MaxPerDay = 3;
    SELECT @MemberBookingsToday = COUNT(*) FROM dbo.SlotBookings
    WHERE GymId = @GymId AND MemberId = @MemberId AND BookingDate = @BookingDate AND Status IN (N'Booked', N'CheckedIn');
    IF @MemberBookingsToday >= @MaxPerDay BEGIN SET @ErrorMessage = N'Max bookings per day exceeded.'; RETURN; END

    SELECT @BookedCount = COUNT(*) FROM dbo.SlotBookings
    WHERE GymId = @GymId AND ClassScheduleId = @ClassScheduleId AND BookingDate = @BookingDate
      AND Status IN (N'Booked', N'CheckedIn', N'Completed', N'NoShow');
    IF @BookedCount >= @Capacity BEGIN SET @ErrorMessage = N'Class is full.'; RETURN; END

    INSERT INTO dbo.SlotBookings (GymId, BranchId, MemberId, ClassScheduleId, BookingDate)
    VALUES (@GymId, @BranchId, @MemberId, @ClassScheduleId, @BookingDate);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CancelSlotBooking
    @GymId UNIQUEIDENTIFIER,
    @BookingId INT,
    @MemberId INT = NULL,
    @PromotedBookingId INT OUTPUT,
    @PromotedMemberId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @PromotedBookingId = 0;
    SET @PromotedMemberId = 0;

    DECLARE @ClassScheduleId INT, @BookingDate DATE, @CancellationWindow INT, @ClassStart DATETIME2;

    SELECT @ClassScheduleId = sb.ClassScheduleId, @BookingDate = sb.BookingDate,
           @ClassStart = CAST(sb.BookingDate AS DATETIME) + CAST(cs.StartTime AS DATETIME)
    FROM dbo.SlotBookings sb
    INNER JOIN dbo.ClassSchedules cs ON cs.Id = sb.ClassScheduleId
    WHERE sb.Id = @BookingId AND sb.GymId = @GymId AND sb.Status = N'Booked'
      AND (@MemberId IS NULL OR sb.MemberId = @MemberId);

    IF @ClassScheduleId IS NULL RETURN;

    SELECT @CancellationWindow = CancellationWindowHours FROM dbo.BookingSettings WHERE GymId = @GymId;
    IF @CancellationWindow IS NULL SET @CancellationWindow = 2;
    IF SYSUTCDATETIME() > DATEADD(HOUR, -@CancellationWindow, @ClassStart) RETURN;

    UPDATE dbo.SlotBookings SET Status = N'Cancelled' WHERE Id = @BookingId AND GymId = @GymId;

    DECLARE @NextWaitlistId INT, @NextMemberId INT;
    SELECT TOP 1 @NextWaitlistId = w.Id, @NextMemberId = w.MemberId
    FROM dbo.BookingWaitlist w
    WHERE w.GymId = @GymId AND w.ClassScheduleId = @ClassScheduleId AND w.BookingDate = @BookingDate
    ORDER BY w.Position ASC;

    IF @NextWaitlistId IS NOT NULL
    BEGIN
        DECLARE @NewBookingId INT, @Err NVARCHAR(200);
        EXEC dbo.sp_CreateSlotBooking @GymId, @NextMemberId, @ClassScheduleId, @BookingDate, @NewBookingId OUTPUT, @Err OUTPUT;
        IF @NewBookingId > 0
        BEGIN
            DELETE FROM dbo.BookingWaitlist WHERE Id = @NextWaitlistId;
            SET @PromotedBookingId = @NewBookingId;
            SET @PromotedMemberId = @NextMemberId;
        END
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_JoinBookingWaitlist
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @ClassScheduleId INT,
    @BookingDate DATE,
    @Id INT OUTPUT,
    @ErrorMessage NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @ErrorMessage = NULL;
    SET @Id = 0;

    DECLARE @AllowWaitlist BIT = 1;
    SELECT @AllowWaitlist = AllowWaitlist FROM dbo.BookingSettings WHERE GymId = @GymId;
    IF @AllowWaitlist = 0 BEGIN SET @ErrorMessage = N'Waitlist is disabled.'; RETURN; END

    IF EXISTS (SELECT 1 FROM dbo.BookingWaitlist WHERE GymId = @GymId AND MemberId = @MemberId
               AND ClassScheduleId = @ClassScheduleId AND BookingDate = @BookingDate)
    BEGIN SET @ErrorMessage = N'Already on waitlist.'; RETURN; END

    DECLARE @Position INT = ISNULL((SELECT MAX(Position) FROM dbo.BookingWaitlist
        WHERE GymId = @GymId AND ClassScheduleId = @ClassScheduleId AND BookingDate = @BookingDate), 0) + 1;

    INSERT INTO dbo.BookingWaitlist (GymId, MemberId, ClassScheduleId, BookingDate, Position)
    VALUES (@GymId, @MemberId, @ClassScheduleId, @BookingDate, @Position);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetSlotBookingsPaged
    @GymId UNIQUEIDENTIFIER,
    @BranchId INT = NULL,
    @MemberId INT = NULL,
    @ClassScheduleId INT = NULL,
    @Status NVARCHAR(30) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @TotalCount = COUNT(*)
    FROM dbo.SlotBookings sb
    WHERE sb.GymId = @GymId
      AND (@BranchId IS NULL OR sb.BranchId = @BranchId)
      AND (@MemberId IS NULL OR sb.MemberId = @MemberId)
      AND (@ClassScheduleId IS NULL OR sb.ClassScheduleId = @ClassScheduleId)
      AND (@Status IS NULL OR sb.Status = @Status)
      AND (@FromDate IS NULL OR sb.BookingDate >= @FromDate)
      AND (@ToDate IS NULL OR sb.BookingDate <= @ToDate);

    SELECT sb.Id, sb.GymId, sb.BranchId, b.BranchName, sb.MemberId, u.Name AS MemberName, sb.ClassScheduleId,
           cs.ClassName, cs.StartTime, cs.EndTime, sb.BookingDate, sb.Status, sb.CheckInTime, sb.CreatedDate,
           tu.Name AS TrainerName
    FROM dbo.SlotBookings sb
    INNER JOIN dbo.ClassSchedules cs ON cs.Id = sb.ClassScheduleId
    INNER JOIN dbo.Branches b ON b.BranchId = sb.BranchId
    INNER JOIN dbo.Members m ON m.MemberId = sb.MemberId
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.Trainers t ON t.TrainerId = cs.TrainerId
    INNER JOIN dbo.Users tu ON tu.Id = t.UserId
    WHERE sb.GymId = @GymId
      AND (@BranchId IS NULL OR sb.BranchId = @BranchId)
      AND (@MemberId IS NULL OR sb.MemberId = @MemberId)
      AND (@ClassScheduleId IS NULL OR sb.ClassScheduleId = @ClassScheduleId)
      AND (@Status IS NULL OR sb.Status = @Status)
      AND (@FromDate IS NULL OR sb.BookingDate >= @FromDate)
      AND (@ToDate IS NULL OR sb.BookingDate <= @ToDate)
    ORDER BY sb.BookingDate DESC, cs.StartTime
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

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
        INSERT INTO dbo.MemberAttendance (GymId, MemberId, AttendanceDate, CheckInAt, MarkedByUserId)
        VALUES (@GymId, @MemberId, CAST(SYSUTCDATETIME() AS DATE), SYSUTCDATETIME(), @MarkedByUserId);
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ProcessNoShowBookings
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE sb SET Status = N'NoShow'
    FROM dbo.SlotBookings sb
    INNER JOIN dbo.ClassSchedules cs ON cs.Id = sb.ClassScheduleId
    WHERE sb.Status = N'Booked'
      AND (@GymId IS NULL OR sb.GymId = @GymId)
      AND CAST(sb.BookingDate AS DATETIME) + CAST(cs.EndTime AS DATETIME) < SYSUTCDATETIME();

    UPDATE sb SET Status = N'Completed'
    FROM dbo.SlotBookings sb
    INNER JOIN dbo.ClassSchedules cs ON cs.Id = sb.ClassScheduleId
    WHERE sb.Status = N'CheckedIn'
      AND (@GymId IS NULL OR sb.GymId = @GymId)
      AND CAST(sb.BookingDate AS DATETIME) + CAST(cs.EndTime AS DATETIME) < SYSUTCDATETIME();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBookingsForReminder
    @MinutesBefore INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT sb.Id AS BookingId, sb.GymId, sb.MemberId, m.UserId, m.Phone, u.Name AS MemberName,
           cs.ClassName, sb.BookingDate, cs.StartTime, cs.TrainerId, tu.Name AS TrainerName, tu2.Id AS TrainerUserId
    FROM dbo.SlotBookings sb
    INNER JOIN dbo.ClassSchedules cs ON cs.Id = sb.ClassScheduleId
    INNER JOIN dbo.Members m ON m.MemberId = sb.MemberId
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    INNER JOIN dbo.Trainers t ON t.TrainerId = cs.TrainerId
    INNER JOIN dbo.Users tu ON tu.Id = t.UserId
    INNER JOIN dbo.Users tu2 ON tu2.Id = t.UserId
    WHERE sb.Status = N'Booked'
      AND sb.BookingDate = CAST(SYSUTCDATETIME() AS DATE)
      AND DATEDIFF(MINUTE, SYSUTCDATETIME(), CAST(sb.BookingDate AS DATETIME) + CAST(cs.StartTime AS DATETIME))
          BETWEEN @MinutesBefore - 5 AND @MinutesBefore + 5;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetTrainerSchedule
    @GymId UNIQUEIDENTIFIER,
    @TrainerId INT,
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT cs.Id AS ClassScheduleId, cs.ClassName, cs.BranchId, b.BranchName, cs.DayOfWeek,
           cs.StartTime, cs.EndTime, cs.Capacity, cs.Status,
           (SELECT COUNT(*) FROM dbo.SlotBookings sb WHERE sb.ClassScheduleId = cs.Id
            AND sb.BookingDate BETWEEN @FromDate AND @ToDate AND sb.Status IN (N'Booked', N'CheckedIn', N'Completed')) AS BookingCount
    FROM dbo.ClassSchedules cs
    INNER JOIN dbo.Branches b ON b.BranchId = cs.BranchId
    WHERE cs.GymId = @GymId AND cs.TrainerId = @TrainerId AND cs.Status = N'Active'
    ORDER BY cs.DayOfWeek, cs.StartTime;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBookingAnalytics
    @GymId UNIQUEIDENTIFIER,
    @BranchId INT = NULL,
    @Days INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @FromDate DATE = DATEADD(DAY, -@Days, CAST(SYSUTCDATETIME() AS DATE));
    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);

    SELECT
        (SELECT COUNT(*) FROM dbo.SlotBookings sb WHERE sb.GymId = @GymId AND (@BranchId IS NULL OR sb.BranchId = @BranchId)
            AND sb.BookingDate >= @FromDate AND sb.Status <> N'Cancelled') AS TotalBookings,
        (SELECT COUNT(*) FROM dbo.SlotBookings sb WHERE sb.GymId = @GymId AND (@BranchId IS NULL OR sb.BranchId = @BranchId)
            AND sb.BookingDate = @Today AND sb.Status IN (N'Booked', N'CheckedIn', N'Completed')) AS TodaysBookings,
        CASE WHEN (SELECT COUNT(*) FROM dbo.SlotBookings sb WHERE sb.GymId = @GymId AND sb.BookingDate >= @FromDate AND sb.Status IN (N'Booked', N'CheckedIn', N'Completed', N'NoShow')) = 0 THEN 0
             ELSE CAST((SELECT COUNT(*) FROM dbo.SlotBookings sb WHERE sb.GymId = @GymId AND sb.BookingDate >= @FromDate AND sb.Status IN (N'CheckedIn', N'Completed')) AS DECIMAL(18,2))
                  / (SELECT COUNT(*) FROM dbo.SlotBookings sb WHERE sb.GymId = @GymId AND sb.BookingDate >= @FromDate AND sb.Status IN (N'Booked', N'CheckedIn', N'Completed', N'NoShow')) * 100 END AS OccupancyPercent,
        CASE WHEN (SELECT COUNT(*) FROM dbo.SlotBookings sb WHERE sb.GymId = @GymId AND sb.BookingDate >= @FromDate AND sb.Status IN (N'Booked', N'CheckedIn', N'NoShow', N'Completed')) = 0 THEN 0
             ELSE CAST((SELECT COUNT(*) FROM dbo.SlotBookings sb WHERE sb.GymId = @GymId AND sb.BookingDate >= @FromDate AND sb.Status = N'NoShow') AS DECIMAL(18,2))
                  / (SELECT COUNT(*) FROM dbo.SlotBookings sb WHERE sb.GymId = @GymId AND sb.BookingDate >= @FromDate AND sb.Status IN (N'Booked', N'CheckedIn', N'NoShow', N'Completed')) * 100 END AS NoShowPercent,
        CASE WHEN (SELECT COUNT(*) FROM dbo.SlotBookings sb WHERE sb.GymId = @GymId AND sb.BookingDate >= @FromDate) = 0 THEN 0
             ELSE CAST((SELECT COUNT(*) FROM dbo.SlotBookings sb WHERE sb.GymId = @GymId AND sb.BookingDate >= @FromDate AND sb.Status = N'Cancelled') AS DECIMAL(18,2))
                  / (SELECT COUNT(*) FROM dbo.SlotBookings sb WHERE sb.GymId = @GymId AND sb.BookingDate >= @FromDate) * 100 END AS CancellationPercent;

    SELECT CAST(sb.BookingDate AS DATE) AS Label, COUNT(*) AS BookingCount
    FROM dbo.SlotBookings sb
    WHERE sb.GymId = @GymId AND (@BranchId IS NULL OR sb.BranchId = @BranchId) AND sb.BookingDate >= @FromDate
      AND sb.Status <> N'Cancelled'
    GROUP BY CAST(sb.BookingDate AS DATE)
    ORDER BY Label;

    SELECT cs.ClassName AS Label, COUNT(*) AS BookingCount
    FROM dbo.SlotBookings sb
    INNER JOIN dbo.ClassSchedules cs ON cs.Id = sb.ClassScheduleId
    WHERE sb.GymId = @GymId AND (@BranchId IS NULL OR sb.BranchId = @BranchId) AND sb.BookingDate >= @FromDate
      AND sb.Status <> N'Cancelled'
    GROUP BY cs.ClassName
    ORDER BY BookingCount DESC;

    SELECT DATEPART(HOUR, cs.StartTime) AS Label, COUNT(*) AS BookingCount
    FROM dbo.SlotBookings sb
    INNER JOIN dbo.ClassSchedules cs ON cs.Id = sb.ClassScheduleId
    WHERE sb.GymId = @GymId AND (@BranchId IS NULL OR sb.BranchId = @BranchId) AND sb.BookingDate >= @FromDate
      AND sb.Status <> N'Cancelled'
    GROUP BY DATEPART(HOUR, cs.StartTime)
    ORDER BY Label;

    SELECT b.BranchName AS Label, COUNT(*) AS BookingCount
    FROM dbo.SlotBookings sb
    INNER JOIN dbo.Branches b ON b.BranchId = sb.BranchId
    WHERE sb.GymId = @GymId AND sb.BookingDate >= @FromDate AND sb.Status <> N'Cancelled'
    GROUP BY b.BranchName
    ORDER BY BookingCount DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateTrainerAvailability
    @GymId UNIQUEIDENTIFIER,
    @BranchId INT,
    @TrainerId INT,
    @DayOfWeek INT,
    @StartTime TIME,
    @EndTime TIME,
    @IsAvailable BIT = 1,
    @Id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.TrainerAvailability (GymId, BranchId, TrainerId, DayOfWeek, StartTime, EndTime, IsAvailable)
    VALUES (@GymId, @BranchId, @TrainerId, @DayOfWeek, @StartTime, @EndTime, @IsAvailable);
    SET @Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetTrainerAvailability
    @GymId UNIQUEIDENTIFIER,
    @TrainerId INT = NULL,
    @BranchId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ta.Id, ta.GymId, ta.BranchId, b.BranchName, ta.TrainerId, tu.Name AS TrainerName,
           ta.DayOfWeek, ta.StartTime, ta.EndTime, ta.IsAvailable
    FROM dbo.TrainerAvailability ta
    INNER JOIN dbo.Branches b ON b.BranchId = ta.BranchId
    INNER JOIN dbo.Trainers t ON t.TrainerId = ta.TrainerId
    INNER JOIN dbo.Users tu ON tu.Id = t.UserId
    WHERE ta.GymId = @GymId
      AND (@TrainerId IS NULL OR ta.TrainerId = @TrainerId)
      AND (@BranchId IS NULL OR ta.BranchId = @BranchId)
      AND ta.IsAvailable = 1
    ORDER BY ta.DayOfWeek, ta.StartTime;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetBookingAiContext
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.SlotBookings sb WHERE sb.GymId = @GymId AND sb.BookingDate >= DATEADD(DAY, -30, CAST(SYSUTCDATETIME() AS DATE))) AS BookingsLast30Days,
        (SELECT TOP 1 DATEPART(HOUR, cs.StartTime) FROM dbo.SlotBookings sb
            INNER JOIN dbo.ClassSchedules cs ON cs.Id = sb.ClassScheduleId
            WHERE sb.GymId = @GymId GROUP BY DATEPART(HOUR, cs.StartTime) ORDER BY COUNT(*) DESC) AS PeakHour,
        (SELECT TOP 1 cs.ClassName FROM dbo.SlotBookings sb
            INNER JOIN dbo.ClassSchedules cs ON cs.Id = sb.ClassScheduleId
            WHERE sb.GymId = @GymId GROUP BY cs.ClassName ORDER BY COUNT(*) DESC) AS MostPopularClass,
        (SELECT AVG(CAST(cs.Capacity AS FLOAT)) FROM dbo.ClassSchedules cs WHERE cs.GymId = @GymId AND cs.Status = N'Active') AS AvgClassCapacity,
        (SELECT COUNT(*) FROM dbo.ClassSchedules cs WHERE cs.GymId = @GymId AND cs.Status = N'Active') AS ActiveClasses;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetGymsForBookingJob
AS
BEGIN
    SET NOCOUNT ON;
    SELECT g.GymId FROM dbo.Gyms g WHERE g.IsActive = 1;
END
GO
