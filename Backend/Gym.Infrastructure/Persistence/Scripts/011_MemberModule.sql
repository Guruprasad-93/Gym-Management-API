/*
  Member Management Module – schema extensions + stored procedures
  TRY/CATCH, GymId isolation, soft delete (IsDeleted), trainer scoping
*/

/* ========== SCHEMA ========== */
IF COL_LENGTH('dbo.Members', 'Height') IS NULL
    ALTER TABLE dbo.Members ADD Height DECIMAL(5, 2) NULL;
GO
IF COL_LENGTH('dbo.Members', 'Weight') IS NULL
    ALTER TABLE dbo.Members ADD Weight DECIMAL(6, 2) NULL;
GO
IF COL_LENGTH('dbo.Members', 'Address') IS NULL
    ALTER TABLE dbo.Members ADD Address NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.Members', 'IsDeleted') IS NULL
    ALTER TABLE dbo.Members ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Members_IsDeleted DEFAULT (0);
GO

/* ========== CREATE ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateMember
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @TrainerId INT = NULL,
    @DateOfBirth DATE = NULL,
    @Gender NVARCHAR(20) = NULL,
    @Height DECIMAL(5, 2) = NULL,
    @Weight DECIMAL(6, 2) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Address NVARCHAR(500) = NULL,
    @EmergencyContact NVARCHAR(200) = NULL,
    @JoinDate DATE,
    @MemberId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Gyms WHERE GymId = @GymId AND IsActive = 1)
            THROW 50040, 'Gym not found or inactive.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @UserId)
            THROW 50041, 'User not found.', 1;

        IF @TrainerId IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM dbo.Trainers WHERE TrainerId = @TrainerId AND GymId = @GymId AND IsActive = 1)
            THROW 50042, 'Trainer not found or inactive.', 1;

        INSERT INTO dbo.Members (
            GymId, UserId, TrainerId, DateOfBirth, Gender, Height, Weight, Phone, Address,
            EmergencyContact, JoinDate, IsActive, IsDeleted, CreatedAt)
        VALUES (
            @GymId, @UserId, @TrainerId, @DateOfBirth, @Gender, @Height, @Weight, @Phone, @Address,
            @EmergencyContact, @JoinDate, 1, 0, SYSUTCDATETIME());

        SET @MemberId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== UPDATE ========== */
CREATE OR ALTER PROCEDURE dbo.sp_UpdateMember
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER,
    @FullName NVARCHAR(100) = NULL,
    @Email NVARCHAR(256) = NULL,
    @TrainerId INT = NULL,
    @DateOfBirth DATE = NULL,
    @Gender NVARCHAR(20) = NULL,
    @Height DECIMAL(5, 2) = NULL,
    @Weight DECIMAL(6, 2) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Address NVARCHAR(500) = NULL,
    @EmergencyContact NVARCHAR(200) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Members WHERE MemberId = @MemberId AND GymId = @GymId AND IsDeleted = 0)
            THROW 50043, 'Member not found.', 1;

        IF @TrainerId IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM dbo.Trainers WHERE TrainerId = @TrainerId AND GymId = @GymId AND IsActive = 1)
            THROW 50042, 'Trainer not found or inactive.', 1;

        DECLARE @UserId UNIQUEIDENTIFIER;
        SELECT @UserId = UserId FROM dbo.Members WHERE MemberId = @MemberId;

        IF @FullName IS NOT NULL
            UPDATE dbo.Users SET Name = @FullName WHERE Id = @UserId;

        IF @Email IS NOT NULL
        BEGIN
            IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email AND Id <> @UserId)
                THROW 50044, 'A user with this email already exists.', 1;
            UPDATE dbo.Users SET Email = @Email WHERE Id = @UserId;
        END

        UPDATE dbo.Members
        SET TrainerId = @TrainerId,
            DateOfBirth = @DateOfBirth,
            Gender = @Gender,
            Height = @Height,
            Weight = @Weight,
            Phone = @Phone,
            Address = @Address,
            EmergencyContact = @EmergencyContact,
            IsActive = @IsActive,
            UpdatedAt = SYSUTCDATETIME()
        WHERE MemberId = @MemberId AND GymId = @GymId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/* ========== SOFT DELETE ========== */
CREATE OR ALTER PROCEDURE dbo.sp_DeleteMember
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Members WHERE MemberId = @MemberId AND GymId = @GymId AND IsDeleted = 0)
            THROW 50043, 'Member not found.', 1;

        UPDATE dbo.Members
        SET IsDeleted = 1, IsActive = 0, TrainerId = NULL, UpdatedAt = SYSUTCDATETIME()
        WHERE MemberId = @MemberId AND GymId = @GymId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/* ========== ACTIVATE / DEACTIVATE ========== */
CREATE OR ALTER PROCEDURE dbo.sp_ActivateMember
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Members
        SET IsActive = 1, UpdatedAt = SYSUTCDATETIME()
        WHERE MemberId = @MemberId AND GymId = @GymId AND IsDeleted = 0;

        IF @@ROWCOUNT = 0
            THROW 50043, 'Member not found.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeactivateMember
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Members
        SET IsActive = 0, UpdatedAt = SYSUTCDATETIME()
        WHERE MemberId = @MemberId AND GymId = @GymId AND IsDeleted = 0;

        IF @@ROWCOUNT = 0
            THROW 50043, 'Member not found.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== TRAINER ASSIGNMENT ========== */
CREATE OR ALTER PROCEDURE dbo.sp_AssignTrainerToMember
    @MemberId INT,
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_AssignMemberToTrainer @TrainerId = @TrainerId, @MemberId = @MemberId, @GymId = @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* sp_RemoveTrainerAssignment defined in 010 */

/* ========== GET BY ID ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMemberById
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            m.MemberId,
            m.GymId,
            m.UserId,
            m.TrainerId,
            u.Name AS FullName,
            u.Email AS Email,
            m.DateOfBirth,
            CASE WHEN m.DateOfBirth IS NULL THEN NULL
                 ELSE DATEDIFF(YEAR, m.DateOfBirth, CAST(GETUTCDATE() AS DATE))
                      - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, m.DateOfBirth, CAST(GETUTCDATE() AS DATE)), m.DateOfBirth) > CAST(GETUTCDATE() AS DATE) THEN 1 ELSE 0 END
            END AS Age,
            m.Gender,
            m.Height,
            m.Weight,
            m.Phone,
            m.Address,
            m.EmergencyContact,
            m.JoinDate,
            m.IsActive,
            m.IsDeleted,
            m.CreatedAt AS CreatedDate,
            m.UpdatedAt AS UpdatedDate,
            tu.Name AS TrainerName,
            ms.MembershipStatus,
            ms.PlanName AS MembershipPlanName,
            ms.EndDate AS MembershipEndDate
        FROM dbo.Members m
        INNER JOIN dbo.Users u ON u.Id = m.UserId
        LEFT JOIN dbo.Trainers t ON t.TrainerId = m.TrainerId
        LEFT JOIN dbo.Users tu ON tu.Id = t.UserId
        OUTER APPLY (
            SELECT TOP 1
                mem.Status AS MembershipStatus,
                mp.PlanName,
                mem.EndDate
            FROM dbo.Memberships mem
            INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = mem.MembershipPlanId
            WHERE mem.MemberId = m.MemberId
            ORDER BY mem.StartDate DESC
        ) ms
        WHERE m.MemberId = @MemberId
          AND m.IsDeleted = 0
          AND (m.GymId = @GymId)
          AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberByUserId
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            m.MemberId,
            m.GymId,
            m.UserId,
            m.TrainerId,
            u.Name AS FullName,
            u.Email AS Email,
            m.DateOfBirth,
            CASE WHEN m.DateOfBirth IS NULL THEN NULL
                 ELSE DATEDIFF(YEAR, m.DateOfBirth, CAST(GETUTCDATE() AS DATE))
                      - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, m.DateOfBirth, CAST(GETUTCDATE() AS DATE)), m.DateOfBirth) > CAST(GETUTCDATE() AS DATE) THEN 1 ELSE 0 END
            END AS Age,
            m.Gender,
            m.Height,
            m.Weight,
            m.Phone,
            m.Address,
            m.EmergencyContact,
            m.JoinDate,
            m.IsActive,
            m.IsDeleted,
            m.CreatedAt AS CreatedDate,
            m.UpdatedAt AS UpdatedDate,
            tu.Name AS TrainerName,
            ms.MembershipStatus,
            ms.PlanName AS MembershipPlanName,
            ms.EndDate AS MembershipEndDate
        FROM dbo.Members m
        INNER JOIN dbo.Users u ON u.Id = m.UserId
        LEFT JOIN dbo.Trainers t ON t.TrainerId = m.TrainerId
        LEFT JOIN dbo.Users tu ON tu.Id = t.UserId
        OUTER APPLY (
            SELECT TOP 1
                mem.Status AS MembershipStatus,
                mp.PlanName,
                mem.EndDate
            FROM dbo.Memberships mem
            INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = mem.MembershipPlanId
            WHERE mem.MemberId = m.MemberId
            ORDER BY mem.StartDate DESC
        ) ms
        WHERE m.UserId = @UserId
          AND m.IsDeleted = 0;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== GET ALL (paged) ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetAllMembers
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL,
    @Search NVARCHAR(200) = NULL,
    @IncludeInactive BIT = 0,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = N'FullName',
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
            m.MemberId,
            m.GymId,
            m.UserId,
            m.TrainerId,
            u.Name AS FullName,
            u.Email AS Email,
            m.DateOfBirth,
            CASE WHEN m.DateOfBirth IS NULL THEN NULL
                 ELSE DATEDIFF(YEAR, m.DateOfBirth, CAST(GETUTCDATE() AS DATE))
                      - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, m.DateOfBirth, CAST(GETUTCDATE() AS DATE)), m.DateOfBirth) > CAST(GETUTCDATE() AS DATE) THEN 1 ELSE 0 END
            END AS Age,
            m.Gender,
            m.Height,
            m.Weight,
            m.Phone,
            m.Address,
            m.EmergencyContact,
            m.JoinDate,
            m.IsActive,
            m.IsDeleted,
            m.CreatedAt AS CreatedDate,
            m.UpdatedAt AS UpdatedDate,
            tu.Name AS TrainerName,
            ms.MembershipStatus,
            ms.PlanName AS MembershipPlanName,
            ms.EndDate AS MembershipEndDate
        INTO #Filtered
        FROM dbo.Members m
        INNER JOIN dbo.Users u ON u.Id = m.UserId
        LEFT JOIN dbo.Trainers t ON t.TrainerId = m.TrainerId
        LEFT JOIN dbo.Users tu ON tu.Id = t.UserId
        OUTER APPLY (
            SELECT TOP 1 mem.Status AS MembershipStatus, mp.PlanName, mem.EndDate
            FROM dbo.Memberships mem
            INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = mem.MembershipPlanId
            WHERE mem.MemberId = m.MemberId
            ORDER BY mem.StartDate DESC
        ) ms
        WHERE m.IsDeleted = 0
          AND (m.GymId = @GymId)
          AND (@TrainerId IS NULL OR m.TrainerId = @TrainerId)
          AND (@IncludeInactive = 1 OR m.IsActive = 1)
          AND (
              @SearchPattern IS NULL
              OR u.Name LIKE @SearchPattern
              OR u.Email LIKE @SearchPattern
              OR m.Phone LIKE @SearchPattern
              OR tu.Name LIKE @SearchPattern
              OR ms.MembershipStatus LIKE @SearchPattern
          );

        SET @TotalCount = (SELECT COUNT(*) FROM #Filtered);

        SELECT *
        FROM #Filtered
        ORDER BY
            CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'FullName' THEN FullName END ASC,
            CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'FullName' THEN FullName END DESC,
            CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'JoinDate' THEN JoinDate END ASC,
            CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'JoinDate' THEN JoinDate END DESC,
            CASE WHEN @SortDirection = N'ASC' AND @SortColumn = N'Phone' THEN Phone END ASC,
            CASE WHEN @SortDirection = N'DESC' AND @SortColumn = N'Phone' THEN Phone END DESC,
            MemberId ASC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        DROP TABLE #Filtered;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SearchMembers
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL,
    @Search NVARCHAR(200),
    @IncludeInactive BIT = 0,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_GetAllMembers
            @GymId = @GymId,
            @TrainerId = @TrainerId,
            @Search = @Search,
            @IncludeInactive = @IncludeInactive,
            @PageNumber = @PageNumber,
            @PageSize = @PageSize,
            @SortColumn = N'FullName',
            @SortDirection = N'ASC',
            @TotalCount = @TotalCount OUTPUT;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== MEMBER DETAILS ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMemberDetails
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER = NULL,
    @TrainerId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_GetMemberById @MemberId = @MemberId, @GymId = @GymId, @TrainerId = @TrainerId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== PAYMENT HISTORY ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMemberPaymentHistory
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            p.PaymentId,
            p.GymId,
            p.MemberId,
            p.MembershipId,
            p.Amount,
            p.PaymentDate,
            p.PaymentMethod,
            p.TransactionReference,
            p.Status,
            p.Notes,
            p.CreatedAt,
            p.UpdatedAt
        FROM dbo.Payments p
        INNER JOIN dbo.Members m ON m.MemberId = p.MemberId
        WHERE p.MemberId = @MemberId
          AND (m.GymId = @GymId)
        ORDER BY p.PaymentDate DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== PROGRESS TRACKING ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetMemberProgress
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            N'Note' AS ProgressType,
            mp.RecordedDate,
            mp.Notes AS Detail,
            NULL AS WeightKg,
            mp.CreatedAt
        FROM dbo.MemberProgress mp
        INNER JOIN dbo.Members m ON m.MemberId = mp.MemberId
        WHERE mp.MemberId = @MemberId AND (m.GymId = @GymId)

        UNION ALL

        SELECT
            N'Weight' AS ProgressType,
            CAST(wh.RecordedAt AS DATE) AS RecordedDate,
            NULL AS Detail,
            wh.WeightKg,
            wh.CreatedAt
        FROM dbo.WeightHistory wh
        INNER JOIN dbo.Members m ON m.MemberId = wh.MemberId
        WHERE wh.MemberId = @MemberId AND (m.GymId = @GymId)

        ORDER BY RecordedDate DESC, CreatedAt DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== DASHBOARD (enhanced) ========== */
CREATE OR ALTER PROCEDURE dbo.sp_Dashboard_SuperAdmin
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.Gyms) AS TotalGyms,
        (SELECT COUNT(*) FROM dbo.Gyms WHERE IsActive = 1) AS ActiveGyms,
        (SELECT COUNT(*) FROM dbo.Members WHERE IsDeleted = 0) AS TotalMembers,
        (SELECT COUNT(*) FROM dbo.Members WHERE IsActive = 1 AND IsDeleted = 0) AS ActiveMembers,
        (SELECT COUNT(*) FROM dbo.Members WHERE TrainerId IS NOT NULL AND IsActive = 1 AND IsDeleted = 0) AS MembersWithTrainer,
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.Payments WHERE Status = N'Completed') AS TotalRevenue,
        (SELECT COUNT(*) FROM dbo.Memberships WHERE Status = N'Active' AND EndDate < CAST(GETUTCDATE() AS DATE)) AS ExpiredMemberships,
        (SELECT COUNT(*) FROM dbo.Memberships WHERE Status = N'Active') AS ActiveMemberships,
        (SELECT COUNT(*) FROM dbo.Trainers WHERE IsActive = 1) AS TotalTrainers;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Dashboard_GymAdmin
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        0 AS TotalGyms,
        0 AS ActiveGyms,
        (SELECT COUNT(*) FROM dbo.Members WHERE GymId = @GymId AND IsDeleted = 0) AS TotalMembers,
        (SELECT COUNT(*) FROM dbo.Members WHERE GymId = @GymId AND IsActive = 1 AND IsDeleted = 0) AS ActiveMembers,
        (SELECT COUNT(*) FROM dbo.Members WHERE GymId = @GymId AND TrainerId IS NOT NULL AND IsActive = 1 AND IsDeleted = 0) AS MembersWithTrainer,
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.Payments WHERE GymId = @GymId AND Status = N'Completed') AS TotalRevenue,
        (SELECT COUNT(*) FROM dbo.Memberships WHERE GymId = @GymId AND Status = N'Active' AND EndDate < CAST(GETUTCDATE() AS DATE)) AS ExpiredMemberships,
        (SELECT COUNT(*) FROM dbo.Memberships WHERE GymId = @GymId AND Status = N'Active') AS ActiveMemberships,
        (SELECT COUNT(*) FROM dbo.Trainers WHERE GymId = @GymId AND IsActive = 1) AS TotalTrainers;
END
GO
