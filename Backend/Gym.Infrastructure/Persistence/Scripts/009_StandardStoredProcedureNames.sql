/*
  Standard naming aliases + hardened procedures (TRY/CATCH, transactions).
  Deployed after 001-008. Repositories call dbo.sp_CreateGym, dbo.sp_LoginUser, etc.
*/

/* ========== LOGIN ========== */
CREATE OR ALTER PROCEDURE dbo.sp_LoginUser
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        SELECT
            u.Id AS UserId,
            u.Name AS FullName,
            u.Email,
            u.Password,
            u.GymId,
            ISNULL(u.IsActive, 1) AS UserIsActive,
            ISNULL(u.TokenVersion, 0) AS TokenVersion,
            ISNULL(u.MustChangePassword, 0) AS MustChangePassword,
            g.Name AS GymName,
            ISNULL(g.IsActive, 1) AS GymIsActive
        FROM dbo.Users u
        LEFT JOIN dbo.Gyms g ON g.GymId = u.GymId
        WHERE u.Email = LTRIM(RTRIM(LOWER(@Email)));
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== GYM ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateGym
    @GymId UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @Address NVARCHAR(500) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(256) = NULL,
    @LogoUrl NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM dbo.Gyms WHERE GymId = @GymId)
            THROW 50010, 'Gym already exists.', 1;

        INSERT INTO dbo.Gyms (GymId, Name, Address, Phone, Email, LogoUrl, IsActive, CreatedAt)
        VALUES (@GymId, @Name, @Address, @Phone, @Email, @LogoUrl, 1, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetGymById
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT GymId, Name, Address, Phone, Email, LogoUrl, IsActive, CreatedAt, UpdatedAt
        FROM dbo.Gyms WHERE GymId = @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllGyms
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT GymId, Name, Address, Phone, Email, LogoUrl, IsActive, CreatedAt, UpdatedAt
        FROM dbo.Gyms ORDER BY Name;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateGym
    @GymId UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @Address NVARCHAR(500) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(256) = NULL,
    @LogoUrl NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Gyms WHERE GymId = @GymId)
            THROW 50011, 'Gym not found.', 1;

        UPDATE dbo.Gyms
        SET Name = @Name, Address = @Address, Phone = @Phone, Email = @Email,
            LogoUrl = @LogoUrl, UpdatedAt = SYSUTCDATETIME()
        WHERE GymId = @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteGym
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        /* Soft delete */
        UPDATE dbo.Gyms SET IsActive = 0, UpdatedAt = SYSUTCDATETIME() WHERE GymId = @GymId;
        IF @@ROWCOUNT = 0
            THROW 50011, 'Gym not found.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SetGymActive
    @GymId UNIQUEIDENTIFIER,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Gyms SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME() WHERE GymId = @GymId;
        IF @@ROWCOUNT = 0
            THROW 50011, 'Gym not found.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== GYM ADMIN (standard names) ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateGymAdmin
    @UserId UNIQUEIDENTIFIER,
    @GymId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @Email NVARCHAR(256),
    @Password NVARCHAR(500),
    @MustChangePassword BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        EXEC dbo.sp_GymAdmin_Create @UserId, @GymId, @Name, @Email, @Password, @MustChangePassword;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllGymAdmins
    @GymId UNIQUEIDENTIFIER = NULL,
    @Search NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = N'Name',
    @SortDirection NVARCHAR(4) = N'ASC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_GymAdmin_GetAll @GymId, @Search, @PageNumber, @PageSize, @SortColumn, @SortDirection, @TotalCount OUTPUT;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetGymAdminById
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_GymAdmin_GetById @UserId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateGymAdmin
    @UserId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @Email NVARCHAR(256),
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_GymAdmin_Update @UserId, @Name, @Email, @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SetGymAdminActive
    @UserId UNIQUEIDENTIFIER,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_GymAdmin_SetActive @UserId, @IsActive;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ResetGymAdminPassword
    @UserId UNIQUEIDENTIFIER,
    @PasswordHash NVARCHAR(500),
    @MustChangePassword BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_GymAdmin_ResetPassword @UserId, @PasswordHash, @MustChangePassword;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== TRAINER / MEMBER aliases ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateTrainer
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER = NULL,
    @Specialization NVARCHAR(200) = NULL,
    @Bio NVARCHAR(1000) = NULL,
    @TrainerId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Trainer_Insert @GymId, @UserId, @Specialization, @Bio, @TrainerId OUTPUT;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateTrainer
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER,
    @Specialization NVARCHAR(200) = NULL,
    @Bio NVARCHAR(1000) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Trainer_Update @TrainerId, @GymId, @Specialization, @Bio, @IsActive;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteTrainer
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Trainer_Delete @TrainerId, @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetTrainerById
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Trainer_GetById @TrainerId, @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllTrainers
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Trainer_GetAll @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateMember
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @TrainerId INT = NULL,
    @DateOfBirth DATE = NULL,
    @Gender NVARCHAR(20) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @EmergencyContact NVARCHAR(100) = NULL,
    @JoinDate DATE,
    @MemberId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Member_Insert @GymId, @UserId, @TrainerId, @DateOfBirth, @Gender, @Phone, @EmergencyContact, @JoinDate, @MemberId OUTPUT;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateMember
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER,
    @TrainerId INT = NULL,
    @DateOfBirth DATE = NULL,
    @Gender NVARCHAR(20) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @EmergencyContact NVARCHAR(100) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Member_Update @MemberId, @GymId, @TrainerId, @DateOfBirth, @Gender, @Phone, @EmergencyContact, @IsActive;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteMember
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Member_Delete @MemberId, @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberById
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Member_GetById @MemberId, @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllMembers
    @GymId UNIQUEIDENTIFIER,
    @SearchTerm NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Member_GetAll @GymId, @SearchTerm;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateMembership
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @MembershipPlanId INT,
    @StartDate DATE,
    @MembershipId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        EXEC dbo.sp_Membership_Insert @GymId, @MemberId, @MembershipPlanId, @StartDate, @MembershipId OUTPUT;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMembershipById
    @MembershipId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Membership_GetById @MembershipId, @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== PAYMENT (transactional) ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreatePayment
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT = NULL,
    @MembershipId INT = NULL,
    @Amount DECIMAL(18, 2),
    @PaymentDate DATETIME2,
    @PaymentMethod NVARCHAR(50),
    @TransactionReference NVARCHAR(100) = NULL,
    @Status NVARCHAR(20) = N'Completed',
    @Notes NVARCHAR(500) = NULL,
    @PaymentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @Amount <= 0
            THROW 50020, 'Payment amount must be greater than zero.', 1;

        IF @MemberId IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM dbo.Members WHERE MemberId = @MemberId AND GymId = @GymId AND IsActive = 1)
            THROW 50021, 'Member not found for this gym.', 1;

        INSERT INTO dbo.Payments (GymId, MemberId, MembershipId, Amount, PaymentDate, PaymentMethod,
            TransactionReference, Status, Notes, CreatedAt)
        VALUES (@GymId, @MemberId, @MembershipId, @Amount, @PaymentDate, @PaymentMethod,
            @TransactionReference, @Status, @Notes, SYSUTCDATETIME());

        SET @PaymentId = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllPayments
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Payment_GetByGym @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== DASHBOARD ========== */
CREATE OR ALTER PROCEDURE dbo.sp_GetDashboardStatistics
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Dashboard_SuperAdmin;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetGymDashboardStatistics
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_Dashboard_GymAdmin @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== ROLE / PRIVILEGE ========== */
CREATE OR ALTER PROCEDURE dbo.sp_AssignPrivilegeToRole
    @RoleId INT,
    @PrivilegeId INT,
    @RolePrivilegeId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        EXEC dbo.sp_RolePrivilege_Insert @RoleId, @PrivilegeId, @RolePrivilegeId OUTPUT;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RemovePrivilegeFromRole
    @RoleId INT,
    @PrivilegeId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_RolePrivilege_Delete @RoleId, @PrivilegeId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
