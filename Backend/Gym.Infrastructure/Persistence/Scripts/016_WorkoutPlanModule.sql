/*
  Workout Plan Management Module
*/

/* Replace legacy MVP WorkoutPlans (003) so module schema and procedures can be created */
IF OBJECT_ID(N'dbo.WorkoutPlans', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.WorkoutPlans', 'PlanName') IS NULL
BEGIN
    DECLARE @dropWorkoutPlanFks NVARCHAR(MAX) = N'';
    SELECT @dropWorkoutPlanFks += N'ALTER TABLE '
        + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(parent_object_id))
        + N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
    FROM sys.foreign_keys
    WHERE referenced_object_id = OBJECT_ID(N'dbo.WorkoutPlans');
    IF @dropWorkoutPlanFks <> N''
        EXEC sys.sp_executesql @dropWorkoutPlanFks;

    DROP TABLE dbo.WorkoutPlans;
END
GO

-- Exercise Categories
IF OBJECT_ID(N'dbo.ExerciseCategories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExerciseCategories
    (
        ExerciseCategoryId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        CategoryName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_ExerciseCategories_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ExerciseCategories_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_ExerciseCategories_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE
    );
    CREATE INDEX IX_ExerciseCategories_GymId ON dbo.ExerciseCategories (GymId);
END
GO

-- Exercise Library
IF OBJECT_ID(N'dbo.ExerciseLibrary', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExerciseLibrary
    (
        ExerciseId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        ExerciseCategoryId INT NULL,
        ExerciseName NVARCHAR(200) NOT NULL,
        MuscleGroup NVARCHAR(100) NULL,
        Difficulty NVARCHAR(50) NULL,
        Instructions NVARCHAR(MAX) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_ExerciseLibrary_IsActive DEFAULT (1),
        IsDeleted BIT NOT NULL CONSTRAINT DF_ExerciseLibrary_IsDeleted DEFAULT (0),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ExerciseLibrary_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_ExerciseLibrary_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE,
        CONSTRAINT FK_ExerciseLibrary_Categories FOREIGN KEY (ExerciseCategoryId) REFERENCES dbo.ExerciseCategories (ExerciseCategoryId)
    );
    CREATE INDEX IX_ExerciseLibrary_GymId ON dbo.ExerciseLibrary (GymId);
END
GO

-- Workout Plans
IF OBJECT_ID(N'dbo.WorkoutPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkoutPlans
    (
        WorkoutPlanId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        PlanName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Goal NVARCHAR(200) NULL,
        DurationWeeks INT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_WorkoutPlans_IsActive DEFAULT (1),
        IsDeleted BIT NOT NULL CONSTRAINT DF_WorkoutPlans_IsDeleted DEFAULT (0),
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_WorkoutPlans_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_WorkoutPlans_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE
    );
    CREATE INDEX IX_WorkoutPlans_GymId ON dbo.WorkoutPlans (GymId);
END
GO

-- Workout Plan Exercises
IF OBJECT_ID(N'dbo.WorkoutPlanExercises', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkoutPlanExercises
    (
        WorkoutPlanExerciseId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        WorkoutPlanId INT NOT NULL,
        DayNumber INT NOT NULL,
        ExerciseId INT NOT NULL,
        Sets INT NULL,
        Reps NVARCHAR(50) NULL,
        Weight NVARCHAR(50) NULL,
        RestSeconds INT NULL,
        Notes NVARCHAR(500) NULL,
        SortOrder INT NOT NULL CONSTRAINT DF_WorkoutPlanExercises_SortOrder DEFAULT (0),
        CONSTRAINT FK_WorkoutPlanExercises_Plans FOREIGN KEY (WorkoutPlanId) REFERENCES dbo.WorkoutPlans (WorkoutPlanId) ON DELETE CASCADE,
        CONSTRAINT FK_WorkoutPlanExercises_Exercises FOREIGN KEY (ExerciseId) REFERENCES dbo.ExerciseLibrary (ExerciseId)
    );
    CREATE INDEX IX_WorkoutPlanExercises_PlanId ON dbo.WorkoutPlanExercises (WorkoutPlanId);
END
GO

-- Assigned Workout Plans
IF OBJECT_ID(N'dbo.AssignedWorkoutPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AssignedWorkoutPlans
    (
        AssignedWorkoutPlanId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        WorkoutPlanId INT NOT NULL,
        AssignedByUserId UNIQUEIDENTIFIER NULL,
        StartDate DATE NOT NULL,
        EndDate DATE NULL,
        Notes NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_AssignedWorkoutPlans_IsActive DEFAULT (1),
        AssignedAt DATETIME2 NOT NULL CONSTRAINT DF_AssignedWorkoutPlans_AssignedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_AssignedWorkoutPlans_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_AssignedWorkoutPlans_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT FK_AssignedWorkoutPlans_Plans FOREIGN KEY (WorkoutPlanId) REFERENCES dbo.WorkoutPlans (WorkoutPlanId)
    );
    CREATE INDEX IX_AssignedWorkoutPlans_Member ON dbo.AssignedWorkoutPlans (GymId, MemberId, IsActive);
END
GO

-- Member Workout Progress
IF OBJECT_ID(N'dbo.MemberWorkoutProgress', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberWorkoutProgress
    (
        MemberWorkoutProgressId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        AssignedWorkoutPlanId INT NOT NULL,
        WorkoutPlanExerciseId INT NOT NULL,
        IsCompleted BIT NOT NULL CONSTRAINT DF_MemberWorkoutProgress_IsCompleted DEFAULT (0),
        CompletionPercentage DECIMAL(5, 2) NULL,
        TrainerNotes NVARCHAR(500) NULL,
        MemberNotes NVARCHAR(500) NULL,
        CompletedAt DATETIME2 NULL,
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_MemberWorkoutProgress_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_MemberWorkoutProgress_Assigned FOREIGN KEY (AssignedWorkoutPlanId) REFERENCES dbo.AssignedWorkoutPlans (AssignedWorkoutPlanId) ON DELETE CASCADE,
        CONSTRAINT FK_MemberWorkoutProgress_Exercise FOREIGN KEY (WorkoutPlanExerciseId) REFERENCES dbo.WorkoutPlanExercises (WorkoutPlanExerciseId),
        CONSTRAINT UQ_MemberWorkoutProgress_AssignmentExercise UNIQUE (AssignedWorkoutPlanId, WorkoutPlanExerciseId)
    );
END
GO

INSERT INTO dbo.ExerciseCategories (GymId, CategoryName, Description)
SELECT g.GymId, v.CategoryName, v.Description
FROM dbo.Gyms g
CROSS APPLY (VALUES
    (N'Strength', N'Compound and isolation strength exercises'),
    (N'Cardio', N'Cardiovascular conditioning'),
    (N'Flexibility', N'Stretching and mobility'),
    (N'HIIT', N'High intensity interval training'),
    (N'Bodyweight', N'No equipment exercises')
) v(CategoryName, Description)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ExerciseCategories ec
    WHERE ec.GymId = g.GymId AND ec.CategoryName = v.CategoryName
);
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetExerciseCategories
    @GymId UNIQUEIDENTIFIER,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ExerciseCategoryId, GymId, CategoryName, Description, IsActive, CreatedAt
    FROM dbo.ExerciseCategories
    WHERE GymId = @GymId AND (@IncludeInactive = 1 OR IsActive = 1)
    ORDER BY CategoryName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateExerciseCategory
    @GymId UNIQUEIDENTIFIER,
    @CategoryName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @ExerciseCategoryId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ExerciseCategories (GymId, CategoryName, Description)
    VALUES (@GymId, @CategoryName, @Description);
    SET @ExerciseCategoryId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetExerciseLibrary
    @GymId UNIQUEIDENTIFIER = NULL,
    @IncludeInactive BIT = 0,
    @CategoryId INT = NULL,
    @MuscleGroup NVARCHAR(100) = NULL,
    @Search NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        e.ExerciseId,
        e.GymId,
        e.ExerciseCategoryId,
        c.CategoryName,
        e.ExerciseName,
        e.MuscleGroup,
        e.Difficulty,
        e.Instructions,
        e.IsActive,
        e.CreatedAt,
        e.UpdatedAt
    FROM dbo.ExerciseLibrary e
    LEFT JOIN dbo.ExerciseCategories c ON c.ExerciseCategoryId = e.ExerciseCategoryId
    WHERE e.IsDeleted = 0
      AND (e.GymId = @GymId)
      AND (@IncludeInactive = 1 OR e.IsActive = 1)
      AND (@CategoryId IS NULL OR e.ExerciseCategoryId = @CategoryId)
      AND (@MuscleGroup IS NULL OR e.MuscleGroup = @MuscleGroup)
      AND (@Search IS NULL OR e.ExerciseName LIKE N'%' + @Search + N'%' OR e.MuscleGroup LIKE N'%' + @Search + N'%')
    ORDER BY e.ExerciseName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetExerciseById
    @ExerciseId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        e.ExerciseId, e.GymId, e.ExerciseCategoryId, c.CategoryName,
        e.ExerciseName, e.MuscleGroup, e.Difficulty, e.Instructions, e.IsActive, e.CreatedAt, e.UpdatedAt
    FROM dbo.ExerciseLibrary e
    LEFT JOIN dbo.ExerciseCategories c ON c.ExerciseCategoryId = e.ExerciseCategoryId
    WHERE e.ExerciseId = @ExerciseId AND e.IsDeleted = 0
      AND (e.GymId = @GymId);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateExercise
    @GymId UNIQUEIDENTIFIER,
    @ExerciseName NVARCHAR(200),
    @ExerciseCategoryId INT = NULL,
    @MuscleGroup NVARCHAR(100) = NULL,
    @Difficulty NVARCHAR(50) = NULL,
    @Instructions NVARCHAR(MAX) = NULL,
    @IsActive BIT = 1,
    @ExerciseId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ExerciseLibrary (GymId, ExerciseCategoryId, ExerciseName, MuscleGroup, Difficulty, Instructions, IsActive)
    VALUES (@GymId, @ExerciseCategoryId, @ExerciseName, @MuscleGroup, @Difficulty, @Instructions, @IsActive);
    SET @ExerciseId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateExercise
    @ExerciseId INT,
    @GymId UNIQUEIDENTIFIER,
    @ExerciseName NVARCHAR(200),
    @ExerciseCategoryId INT = NULL,
    @MuscleGroup NVARCHAR(100) = NULL,
    @Difficulty NVARCHAR(50) = NULL,
    @Instructions NVARCHAR(MAX) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.ExerciseLibrary
    SET ExerciseName = @ExerciseName, ExerciseCategoryId = @ExerciseCategoryId,
        MuscleGroup = @MuscleGroup, Difficulty = @Difficulty, Instructions = @Instructions,
        IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
    WHERE ExerciseId = @ExerciseId AND GymId = @GymId AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteExercise
    @ExerciseId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.ExerciseLibrary SET IsDeleted = 1, IsActive = 0, UpdatedAt = SYSUTCDATETIME()
    WHERE ExerciseId = @ExerciseId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWorkoutPlans
    @GymId UNIQUEIDENTIFIER = NULL,
    @IncludeInactive BIT = 0,
    @Search NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        wp.WorkoutPlanId, wp.GymId, wp.PlanName, wp.Description, wp.Goal, wp.DurationWeeks,
        wp.IsActive, wp.CreatedAt, wp.UpdatedAt,
        (SELECT COUNT(*) FROM dbo.WorkoutPlanExercises wpe WHERE wpe.WorkoutPlanId = wp.WorkoutPlanId) AS ExerciseCount,
        (SELECT COUNT(*) FROM dbo.AssignedWorkoutPlans a WHERE a.WorkoutPlanId = wp.WorkoutPlanId AND a.IsActive = 1) AS ActiveAssignmentCount
    FROM dbo.WorkoutPlans wp
    WHERE wp.IsDeleted = 0
      AND (wp.GymId = @GymId)
      AND (@IncludeInactive = 1 OR wp.IsActive = 1)
      AND (@Search IS NULL OR wp.PlanName LIKE N'%' + @Search + N'%' OR wp.Goal LIKE N'%' + @Search + N'%')
    ORDER BY wp.PlanName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetWorkoutPlanById
    @WorkoutPlanId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT WorkoutPlanId, GymId, PlanName, Description, Goal, DurationWeeks, IsActive, CreatedAt, UpdatedAt
    FROM dbo.WorkoutPlans
    WHERE WorkoutPlanId = @WorkoutPlanId AND IsDeleted = 0
      AND (GymId = @GymId);

    SELECT
        wpe.WorkoutPlanExerciseId, wpe.WorkoutPlanId, wpe.DayNumber, wpe.ExerciseId,
        e.ExerciseName, e.MuscleGroup, e.Difficulty, c.CategoryName,
        wpe.Sets, wpe.Reps, wpe.Weight, wpe.RestSeconds, wpe.Notes, wpe.SortOrder
    FROM dbo.WorkoutPlanExercises wpe
    INNER JOIN dbo.ExerciseLibrary e ON e.ExerciseId = wpe.ExerciseId
    LEFT JOIN dbo.ExerciseCategories c ON c.ExerciseCategoryId = e.ExerciseCategoryId
    WHERE wpe.WorkoutPlanId = @WorkoutPlanId
    ORDER BY wpe.DayNumber, wpe.SortOrder, wpe.WorkoutPlanExerciseId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateWorkoutPlan
    @GymId UNIQUEIDENTIFIER,
    @PlanName NVARCHAR(200),
    @Description NVARCHAR(MAX) = NULL,
    @Goal NVARCHAR(200) = NULL,
    @DurationWeeks INT = NULL,
    @IsActive BIT = 1,
    @CreatedByUserId UNIQUEIDENTIFIER = NULL,
    @WorkoutPlanId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.WorkoutPlans (GymId, PlanName, Description, Goal, DurationWeeks, IsActive, CreatedByUserId)
    VALUES (@GymId, @PlanName, @Description, @Goal, @DurationWeeks, @IsActive, @CreatedByUserId);
    SET @WorkoutPlanId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateWorkoutPlan
    @WorkoutPlanId INT,
    @GymId UNIQUEIDENTIFIER,
    @PlanName NVARCHAR(200),
    @Description NVARCHAR(MAX) = NULL,
    @Goal NVARCHAR(200) = NULL,
    @DurationWeeks INT = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.WorkoutPlans
    SET PlanName = @PlanName, Description = @Description, Goal = @Goal,
        DurationWeeks = @DurationWeeks, IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
    WHERE WorkoutPlanId = @WorkoutPlanId AND GymId = @GymId AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteWorkoutPlan
    @WorkoutPlanId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.AssignedWorkoutPlans WHERE WorkoutPlanId = @WorkoutPlanId AND IsActive = 1)
        THROW 50010, 'Cannot delete a workout plan with active member assignments.', 1;
    UPDATE dbo.WorkoutPlans SET IsDeleted = 1, IsActive = 0, UpdatedAt = SYSUTCDATETIME()
    WHERE WorkoutPlanId = @WorkoutPlanId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SetWorkoutPlanActive
    @WorkoutPlanId INT,
    @GymId UNIQUEIDENTIFIER,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.WorkoutPlans SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
    WHERE WorkoutPlanId = @WorkoutPlanId AND GymId = @GymId AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ReplaceWorkoutPlanExercises
    @WorkoutPlanId INT,
    @GymId UNIQUEIDENTIFIER,
    @ExercisesJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.WorkoutPlans WHERE WorkoutPlanId = @WorkoutPlanId AND GymId = @GymId AND IsDeleted = 0)
        THROW 50011, 'Workout plan not found.', 1;

    DELETE FROM dbo.WorkoutPlanExercises WHERE WorkoutPlanId = @WorkoutPlanId;

    IF @ExercisesJson IS NOT NULL AND LEN(LTRIM(@ExercisesJson)) > 2
    BEGIN
        INSERT INTO dbo.WorkoutPlanExercises (WorkoutPlanId, DayNumber, ExerciseId, Sets, Reps, Weight, RestSeconds, Notes, SortOrder)
        SELECT @WorkoutPlanId, j.DayNumber, j.ExerciseId, j.Sets, j.Reps, j.Weight, j.RestSeconds, j.Notes, ISNULL(j.SortOrder, 0)
        FROM OPENJSON(@ExercisesJson)
        WITH (
            DayNumber INT '$.dayNumber',
            ExerciseId INT '$.exerciseId',
            Sets INT '$.sets',
            Reps NVARCHAR(50) '$.reps',
            Weight NVARCHAR(50) '$.weight',
            RestSeconds INT '$.restSeconds',
            Notes NVARCHAR(500) '$.notes',
            SortOrder INT '$.sortOrder'
        ) j;
    END
    UPDATE dbo.WorkoutPlans SET UpdatedAt = SYSUTCDATETIME() WHERE WorkoutPlanId = @WorkoutPlanId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CloneWorkoutPlan
    @SourceWorkoutPlanId INT,
    @GymId UNIQUEIDENTIFIER,
    @NewPlanName NVARCHAR(200) = NULL,
    @CreatedByUserId UNIQUEIDENTIFIER = NULL,
    @NewWorkoutPlanId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Name NVARCHAR(200);
    SELECT @Name = ISNULL(@NewPlanName, PlanName + N' (Copy)')
    FROM dbo.WorkoutPlans WHERE WorkoutPlanId = @SourceWorkoutPlanId AND GymId = @GymId AND IsDeleted = 0;
    IF @Name IS NULL THROW 50011, 'Source workout plan not found.', 1;

    INSERT INTO dbo.WorkoutPlans (GymId, PlanName, Description, Goal, DurationWeeks, IsActive, CreatedByUserId)
    SELECT GymId, @Name, Description, Goal, DurationWeeks, 1, @CreatedByUserId
    FROM dbo.WorkoutPlans WHERE WorkoutPlanId = @SourceWorkoutPlanId;
    SET @NewWorkoutPlanId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkoutPlanExercises (WorkoutPlanId, DayNumber, ExerciseId, Sets, Reps, Weight, RestSeconds, Notes, SortOrder)
    SELECT @NewWorkoutPlanId, DayNumber, ExerciseId, Sets, Reps, Weight, RestSeconds, Notes, SortOrder
    FROM dbo.WorkoutPlanExercises WHERE WorkoutPlanId = @SourceWorkoutPlanId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_AssignWorkoutPlanToMember
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @WorkoutPlanId INT,
    @AssignedByUserId UNIQUEIDENTIFIER = NULL,
    @StartDate DATE,
    @EndDate DATE = NULL,
    @Notes NVARCHAR(500) = NULL,
    @DeactivatePrevious BIT = 1,
    @AssignedWorkoutPlanId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Members m WHERE m.MemberId = @MemberId AND m.GymId = @GymId AND m.IsDeleted = 0)
        THROW 50004, 'Member not found.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.WorkoutPlans wp WHERE wp.WorkoutPlanId = @WorkoutPlanId AND wp.GymId = @GymId AND wp.IsDeleted = 0 AND wp.IsActive = 1)
        THROW 50011, 'Workout plan not found or inactive.', 1;

    IF @DeactivatePrevious = 1
        UPDATE dbo.AssignedWorkoutPlans SET IsActive = 0 WHERE GymId = @GymId AND MemberId = @MemberId AND IsActive = 1;

    INSERT INTO dbo.AssignedWorkoutPlans (GymId, MemberId, WorkoutPlanId, AssignedByUserId, StartDate, EndDate, Notes, IsActive)
    VALUES (@GymId, @MemberId, @WorkoutPlanId, @AssignedByUserId, @StartDate, @EndDate, @Notes, 1);
    SET @AssignedWorkoutPlanId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UnassignWorkoutPlan
    @AssignedWorkoutPlanId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.AssignedWorkoutPlans SET IsActive = 0
    WHERE AssignedWorkoutPlanId = @AssignedWorkoutPlanId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberWorkoutPlan
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER = NULL,
    @ActiveOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1)
        aw.AssignedWorkoutPlanId, aw.GymId, aw.MemberId, u.Name AS MemberName,
        aw.WorkoutPlanId, wp.PlanName, wp.Description AS PlanDescription, wp.Goal, wp.DurationWeeks,
        aw.StartDate, aw.EndDate, aw.Notes AS AssignmentNotes, aw.IsActive, aw.AssignedAt
    FROM dbo.AssignedWorkoutPlans aw
    INNER JOIN dbo.WorkoutPlans wp ON wp.WorkoutPlanId = aw.WorkoutPlanId
    INNER JOIN dbo.Members m ON m.MemberId = aw.MemberId
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE aw.MemberId = @MemberId
      AND (aw.GymId = @GymId)
      AND (@ActiveOnly = 0 OR aw.IsActive = 1)
    ORDER BY aw.AssignedAt DESC;

    SELECT
        wpe.WorkoutPlanExerciseId, wpe.DayNumber, wpe.ExerciseId, e.ExerciseName, e.MuscleGroup,
        wpe.Sets, wpe.Reps, wpe.Weight, wpe.RestSeconds, wpe.Notes, wpe.SortOrder,
        p.MemberWorkoutProgressId, p.IsCompleted, p.CompletionPercentage,
        p.TrainerNotes, p.MemberNotes, p.CompletedAt
    FROM dbo.WorkoutPlanExercises wpe
    INNER JOIN dbo.ExerciseLibrary e ON e.ExerciseId = wpe.ExerciseId
    INNER JOIN (
        SELECT TOP (1) aw.AssignedWorkoutPlanId, aw.WorkoutPlanId
        FROM dbo.AssignedWorkoutPlans aw
        WHERE aw.MemberId = @MemberId AND (aw.GymId = @GymId) AND (@ActiveOnly = 0 OR aw.IsActive = 1)
        ORDER BY aw.AssignedAt DESC
    ) x ON x.WorkoutPlanId = wpe.WorkoutPlanId
    LEFT JOIN dbo.MemberWorkoutProgress p ON p.AssignedWorkoutPlanId = x.AssignedWorkoutPlanId AND p.WorkoutPlanExerciseId = wpe.WorkoutPlanExerciseId
    ORDER BY wpe.DayNumber, wpe.SortOrder, wpe.WorkoutPlanExerciseId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpsertMemberWorkoutProgress
    @AssignedWorkoutPlanId INT,
    @GymId UNIQUEIDENTIFIER,
    @WorkoutPlanExerciseId INT,
    @IsCompleted BIT = NULL,
    @CompletionPercentage DECIMAL(5, 2) = NULL,
    @TrainerNotes NVARCHAR(500) = NULL,
    @MemberNotes NVARCHAR(500) = NULL,
    @MemberWorkoutProgressId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (
        SELECT 1 FROM dbo.AssignedWorkoutPlans aw
        WHERE aw.AssignedWorkoutPlanId = @AssignedWorkoutPlanId AND aw.GymId = @GymId AND aw.IsActive = 1)
        THROW 50012, 'Active workout assignment not found.', 1;

    DECLARE @ExistingId INT = (
        SELECT MemberWorkoutProgressId FROM dbo.MemberWorkoutProgress
        WHERE AssignedWorkoutPlanId = @AssignedWorkoutPlanId AND WorkoutPlanExerciseId = @WorkoutPlanExerciseId);

    IF @ExistingId IS NOT NULL
    BEGIN
        UPDATE dbo.MemberWorkoutProgress
        SET IsCompleted = COALESCE(@IsCompleted, IsCompleted),
            CompletionPercentage = COALESCE(@CompletionPercentage, CompletionPercentage),
            TrainerNotes = COALESCE(@TrainerNotes, TrainerNotes),
            MemberNotes = COALESCE(@MemberNotes, MemberNotes),
            CompletedAt = CASE WHEN COALESCE(@IsCompleted, IsCompleted) = 1 THEN SYSUTCDATETIME() ELSE CompletedAt END,
            UpdatedAt = SYSUTCDATETIME()
        WHERE MemberWorkoutProgressId = @ExistingId;
        SET @MemberWorkoutProgressId = @ExistingId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MemberWorkoutProgress (AssignedWorkoutPlanId, WorkoutPlanExerciseId, IsCompleted, CompletionPercentage, TrainerNotes, MemberNotes, CompletedAt)
        VALUES (
            @AssignedWorkoutPlanId, @WorkoutPlanExerciseId,
            COALESCE(@IsCompleted, 0), @CompletionPercentage, @TrainerNotes, @MemberNotes,
            CASE WHEN @IsCompleted = 1 THEN SYSUTCDATETIME() ELSE NULL END);
        SET @MemberWorkoutProgressId = SCOPE_IDENTITY();
    END
END
GO
