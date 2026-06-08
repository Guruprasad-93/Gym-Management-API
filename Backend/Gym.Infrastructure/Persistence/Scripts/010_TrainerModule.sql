/*
  Trainer Management Module – stored procedures (TRY/CATCH, GymId isolation, soft delete)
*/

/* ========== CREATE ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateTrainer
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER = NULL,
    @Specialization NVARCHAR(200) = NULL,
    @Bio NVARCHAR(1000) = NULL,
    @TrainerId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Gyms WHERE GymId = @GymId AND IsActive = 1)
            THROW 50030, 'Gym not found or inactive.', 1;

        IF @UserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @UserId)
            THROW 50031, 'User not found.', 1;

        INSERT INTO dbo.Trainers (GymId, UserId, Specialization, Bio, IsActive, CreatedAt)
        VALUES (@GymId, @UserId, @Specialization, @Bio, 1, SYSUTCDATETIME());

        SET @TrainerId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== UPDATE ========== */
CREATE OR ALTER PROCEDURE dbo.sp_UpdateTrainer
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER = NULL,
    @Specialization NVARCHAR(200) = NULL,
    @Bio NVARCHAR(1000) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Trainers WHERE TrainerId = @TrainerId AND GymId = @GymId)
            THROW 50032, 'Trainer not found.', 1;

        UPDATE dbo.Trainers
        SET UserId = ISNULL(@UserId, UserId),
            Specialization = @Specialization,
            Bio = @Bio,
            IsActive = @IsActive,
            UpdatedAt = SYSUTCDATETIME()
        WHERE TrainerId = @TrainerId AND GymId = @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== SOFT DELETE ========== */
CREATE OR ALTER PROCEDURE dbo.sp_DeleteTrainer
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER,
    @UnassignMembers BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Trainers WHERE TrainerId = @TrainerId AND GymId = @GymId)
            THROW 50032, 'Trainer not found.', 1;

        UPDATE dbo.Trainers
        SET IsActive = 0, UpdatedAt = SYSUTCDATETIME()
        WHERE TrainerId = @TrainerId AND GymId = @GymId;

        IF @UnassignMembers = 1
        BEGIN
            UPDATE dbo.Members
            SET TrainerId = NULL, UpdatedAt = SYSUTCDATETIME()
            WHERE TrainerId = @TrainerId AND GymId = @GymId;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/* ========== GET BY USER ID (trainer self-service) ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetTrainerByUserId
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            t.TrainerId,
            t.GymId,
            t.UserId,
            u.Name AS UserName,
            u.Email AS UserEmail,
            t.Specialization,
            t.Bio,
            t.IsActive,
            t.CreatedAt,
            t.UpdatedAt,
            (SELECT COUNT(*) FROM dbo.Members m WHERE m.TrainerId = t.TrainerId AND m.IsActive = 1) AS AssignedMemberCount
        FROM dbo.Trainers t
        LEFT JOIN dbo.Users u ON u.Id = t.UserId
        WHERE t.UserId = @UserId AND t.IsActive = 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== GET BY ID ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetTrainerById
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            t.TrainerId,
            t.GymId,
            t.UserId,
            u.Name AS UserName,
            u.Email AS UserEmail,
            t.Specialization,
            t.Bio,
            t.IsActive,
            t.CreatedAt,
            t.UpdatedAt,
            (SELECT COUNT(*) FROM dbo.Members m WHERE m.TrainerId = t.TrainerId AND m.IsActive = 1) AS AssignedMemberCount
        FROM dbo.Trainers t
        LEFT JOIN dbo.Users u ON u.Id = t.UserId
        WHERE t.TrainerId = @TrainerId
          AND (t.GymId = @GymId);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== GET ALL (paged) ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetAllTrainers
    @GymId UNIQUEIDENTIFIER = NULL,
    @Search NVARCHAR(200) = NULL,
    @IncludeInactive BIT = 0,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = N'Specialization',
    @SortDirection NVARCHAR(4) = N'ASC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @PageNumber < 1 SET @PageNumber = 1;
        IF @PageSize < 1 SET @PageSize = 10;
        IF @PageSize > 100 SET @PageSize = 100;

        DECLARE @SearchPattern NVARCHAR(202) = NULL;
        IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
            SET @SearchPattern = N'%' + LTRIM(RTRIM(@Search)) + N'%';

        SELECT
            t.TrainerId,
            t.GymId,
            t.UserId,
            u.Name AS UserName,
            u.Email AS UserEmail,
            t.Specialization,
            t.Bio,
            t.IsActive,
            t.CreatedAt,
            t.UpdatedAt,
            (SELECT COUNT(*) FROM dbo.Members m WHERE m.TrainerId = t.TrainerId AND m.IsActive = 1) AS AssignedMemberCount
        INTO #Filtered
        FROM dbo.Trainers t
        LEFT JOIN dbo.Users u ON u.Id = t.UserId
        WHERE (t.GymId = @GymId)
          AND (@IncludeInactive = 1 OR t.IsActive = 1)
          AND (
              @SearchPattern IS NULL
              OR t.Specialization LIKE @SearchPattern
              OR t.Bio LIKE @SearchPattern
              OR u.Name LIKE @SearchPattern
              OR u.Email LIKE @SearchPattern
              OR CAST(t.TrainerId AS NVARCHAR(20)) LIKE @SearchPattern
          );

        SET @TotalCount = (SELECT COUNT(*) FROM #Filtered);

        SELECT TrainerId, GymId, UserId, UserName, UserEmail, Specialization, Bio, IsActive, CreatedAt, UpdatedAt, AssignedMemberCount
        FROM #Filtered
        ORDER BY
            CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'UserName' THEN UserName END ASC,
            CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'UserName' THEN UserName END DESC,
            CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'Specialization' THEN Specialization END ASC,
            CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'Specialization' THEN Specialization END DESC,
            CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'CreatedAt' THEN CreatedAt END ASC,
            CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'CreatedAt' THEN CreatedAt END DESC,
            TrainerId ASC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        DROP TABLE #Filtered;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== SEARCH (alias – same as get all with search) ========== */
CREATE OR ALTER PROCEDURE dbo.sp_SearchTrainers
    @GymId UNIQUEIDENTIFIER = NULL,
    @Search NVARCHAR(200),
    @IncludeInactive BIT = 0,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_GetAllTrainers
            @GymId = @GymId,
            @Search = @Search,
            @IncludeInactive = @IncludeInactive,
            @PageNumber = @PageNumber,
            @PageSize = @PageSize,
            @SortColumn = N'UserName',
            @SortDirection = N'ASC',
            @TotalCount = @TotalCount OUTPUT;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== MEMBER ASSIGNMENT ========== */
CREATE OR ALTER PROCEDURE dbo.sp_AssignMemberToTrainer
    @TrainerId INT,
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Trainers WHERE TrainerId = @TrainerId AND GymId = @GymId AND IsActive = 1)
            THROW 50032, 'Trainer not found or inactive.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Members WHERE MemberId = @MemberId AND GymId = @GymId AND IsActive = 1)
            THROW 50033, 'Member not found or inactive.', 1;

        UPDATE dbo.Members
        SET TrainerId = @TrainerId, UpdatedAt = SYSUTCDATETIME()
        WHERE MemberId = @MemberId AND GymId = @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RemoveTrainerAssignment
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Members
        SET TrainerId = NULL, UpdatedAt = SYSUTCDATETIME()
        WHERE MemberId = @MemberId AND GymId = @GymId;

        IF @@ROWCOUNT = 0
            THROW 50033, 'Member not found.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetTrainerMembers
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER = NULL,
    @Search NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @SearchPattern NVARCHAR(202) = NULL;
        IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
            SET @SearchPattern = N'%' + LTRIM(RTRIM(@Search)) + N'%';

        SELECT
            m.MemberId,
            m.GymId,
            m.UserId,
            m.TrainerId,
            m.DateOfBirth,
            m.Gender,
            m.Phone,
            m.EmergencyContact,
            m.JoinDate,
            m.IsActive,
            m.CreatedAt,
            m.UpdatedAt,
            u.Name AS UserName,
            u.Email AS UserEmail
        FROM dbo.Members m
        INNER JOIN dbo.Users u ON u.Id = m.UserId
        INNER JOIN dbo.Trainers t ON t.TrainerId = m.TrainerId
        WHERE m.TrainerId = @TrainerId
          AND (m.GymId = @GymId)
          AND (
              @SearchPattern IS NULL
              OR u.Name LIKE @SearchPattern
              OR u.Email LIKE @SearchPattern
              OR m.Phone LIKE @SearchPattern
          )
        ORDER BY u.Name;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== TRAINER DASHBOARD ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetTrainerDashboard
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (
            SELECT 1 FROM dbo.Trainers t
            WHERE t.TrainerId = @TrainerId
              AND (@GymId IS NULL OR t.GymId = @GymId))
            THROW 50032, 'Trainer not found.', 1;

        DECLARE @TrainerGymId UNIQUEIDENTIFIER =
            (SELECT GymId FROM dbo.Trainers WHERE TrainerId = @TrainerId);

        SELECT
            @TrainerId AS TrainerId,
            (SELECT COUNT(*)
             FROM dbo.Members m
             WHERE m.TrainerId = @TrainerId
               AND m.IsActive = 1
               AND m.IsDeleted = 0) AS AssignedActiveMembers,
            (SELECT COUNT(*)
             FROM dbo.Members m
             WHERE m.TrainerId = @TrainerId
               AND m.IsActive = 0
               AND m.IsDeleted = 0) AS AssignedInactiveMembers,
            (SELECT COUNT(*)
             FROM dbo.Members m
             WHERE m.GymId = @TrainerGymId
               AND m.TrainerId IS NULL
               AND m.IsActive = 1
               AND m.IsDeleted = 0) AS UnassignedMembersInGym,
            (SELECT COUNT(*)
             FROM dbo.AssignedDietPlans adp
             INNER JOIN dbo.Members m ON m.MemberId = adp.MemberId
             WHERE m.TrainerId = @TrainerId
               AND adp.IsActive = 1
               AND m.IsDeleted = 0) AS ActiveDietPlans,
            (SELECT COUNT(*)
             FROM dbo.AssignedWorkoutPlans awp
             INNER JOIN dbo.Members m ON m.MemberId = awp.MemberId
             WHERE m.TrainerId = @TrainerId
               AND awp.IsActive = 1
               AND m.IsDeleted = 0) AS ActiveWorkoutPlans;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* Unassigned members for assign dialog */
CREATE OR ALTER PROCEDURE dbo.sp_GetUnassignedMembers
    @GymId UNIQUEIDENTIFIER,
    @Search NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @SearchPattern NVARCHAR(202) = NULL;
        IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
            SET @SearchPattern = N'%' + LTRIM(RTRIM(@Search)) + N'%';

        SELECT
            m.MemberId,
            m.GymId,
            m.UserId,
            m.TrainerId,
            m.DateOfBirth,
            m.Gender,
            m.Phone,
            m.EmergencyContact,
            m.JoinDate,
            m.IsActive,
            m.CreatedAt,
            m.UpdatedAt,
            u.Name AS UserName,
            u.Email AS UserEmail
        FROM dbo.Members m
        INNER JOIN dbo.Users u ON u.Id = m.UserId
        WHERE m.GymId = @GymId
          AND m.TrainerId IS NULL
          AND m.IsActive = 1
          AND (
              @SearchPattern IS NULL
              OR u.Name LIKE @SearchPattern
              OR u.Email LIKE @SearchPattern
          )
        ORDER BY u.Name;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
