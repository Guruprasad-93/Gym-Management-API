/* MVP API stored procedures */

CREATE OR ALTER PROCEDURE dbo.sp_Gym_Insert
    @GymId UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @Address NVARCHAR(500) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(256) = NULL,
    @LogoUrl NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    INSERT INTO dbo.Gyms (GymId, Name, Address, Phone, Email, LogoUrl, IsActive, CreatedAt)
    VALUES (@GymId, @Name, @Address, @Phone, @Email, @LogoUrl, 1, @Now);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Gym_GetById
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT GymId, Name, Address, Phone, Email, LogoUrl, IsActive, CreatedAt, UpdatedAt
    FROM dbo.Gyms WHERE GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Gym_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT GymId, Name, Address, Phone, Email, LogoUrl, IsActive, CreatedAt, UpdatedAt
    FROM dbo.Gyms ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Gym_Update
    @GymId UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @Address NVARCHAR(500) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(256) = NULL,
    @LogoUrl NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Gyms
    SET Name = @Name, Address = @Address, Phone = @Phone, Email = @Email, LogoUrl = @LogoUrl,
        UpdatedAt = SYSUTCDATETIME()
    WHERE GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Gym_Delete
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Gyms WHERE GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Gym_SetActive
    @GymId UNIQUEIDENTIFIER,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Gyms SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME() WHERE GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GymAdmin_Create
    @UserId UNIQUEIDENTIFIER,
    @GymId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @Email NVARCHAR(256),
    @Password NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RoleId INT;
    SELECT @RoleId = RoleId FROM dbo.Roles WHERE RoleName = N'GymAdmin';

    IF @RoleId IS NULL
        THROW 50001, 'GymAdmin role does not exist. Seed roles first.', 1;

    INSERT INTO dbo.Users (Id, Name, Email, Password, GymId, CreatedDate)
    VALUES (@UserId, @Name, @Email, @Password, @GymId, SYSUTCDATETIME());

    INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedAt)
    VALUES (@UserId, @RoleId, SYSUTCDATETIME());
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Trainer_Insert
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER = NULL,
    @Specialization NVARCHAR(200) = NULL,
    @Bio NVARCHAR(1000) = NULL,
    @TrainerId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Trainers (GymId, UserId, Specialization, Bio, IsActive, CreatedAt)
    VALUES (@GymId, @UserId, @Specialization, @Bio, 1, SYSUTCDATETIME());
    SET @TrainerId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Trainer_GetById
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TrainerId, GymId, UserId, Specialization, Bio, IsActive, CreatedAt, UpdatedAt
    FROM dbo.Trainers
    WHERE TrainerId = @TrainerId AND (GymId = @GymId);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Trainer_GetAll
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TrainerId, GymId, UserId, Specialization, Bio, IsActive, CreatedAt, UpdatedAt
    FROM dbo.Trainers WHERE GymId = @GymId ORDER BY TrainerId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Trainer_Update
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER = NULL,
    @Specialization NVARCHAR(200) = NULL,
    @Bio NVARCHAR(1000) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Trainers
    SET UserId = @UserId, Specialization = @Specialization, Bio = @Bio, IsActive = @IsActive,
        UpdatedAt = SYSUTCDATETIME()
    WHERE TrainerId = @TrainerId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Trainer_Delete
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Trainers SET IsActive = 0, UpdatedAt = SYSUTCDATETIME()
    WHERE TrainerId = @TrainerId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Member_Insert
    @GymId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @TrainerId INT = NULL,
    @DateOfBirth DATE = NULL,
    @Gender NVARCHAR(20) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @EmergencyContact NVARCHAR(200) = NULL,
    @JoinDate DATE,
    @MemberId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Members (GymId, UserId, TrainerId, DateOfBirth, Gender, Phone, EmergencyContact, JoinDate, IsActive, CreatedAt)
    VALUES (@GymId, @UserId, @TrainerId, @DateOfBirth, @Gender, @Phone, @EmergencyContact, @JoinDate, 1, SYSUTCDATETIME());
    SET @MemberId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Member_GetById
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.MemberId, m.GymId, m.UserId, m.TrainerId, m.DateOfBirth, m.Gender, m.Phone,
           m.EmergencyContact, m.JoinDate, m.IsActive, m.CreatedAt, m.UpdatedAt,
           u.Name AS UserName, u.Email AS UserEmail
    FROM dbo.Members m
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE m.MemberId = @MemberId AND (m.GymId = @GymId);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Member_GetAll
    @GymId UNIQUEIDENTIFIER,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.MemberId, m.GymId, m.UserId, m.TrainerId, m.DateOfBirth, m.Gender, m.Phone,
           m.EmergencyContact, m.JoinDate, m.IsActive, m.CreatedAt, m.UpdatedAt,
           u.Name AS UserName, u.Email AS UserEmail
    FROM dbo.Members m
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    WHERE m.GymId = @GymId
      AND (@SearchTerm IS NULL OR u.Name LIKE N'%' + @SearchTerm + N'%' OR u.Email LIKE N'%' + @SearchTerm + N'%')
    ORDER BY u.Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Member_Update
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER,
    @TrainerId INT = NULL,
    @DateOfBirth DATE = NULL,
    @Gender NVARCHAR(20) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @EmergencyContact NVARCHAR(200) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Members
    SET TrainerId = @TrainerId, DateOfBirth = @DateOfBirth, Gender = @Gender, Phone = @Phone,
        EmergencyContact = @EmergencyContact, IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
    WHERE MemberId = @MemberId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Member_Delete
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Members SET IsActive = 0, UpdatedAt = SYSUTCDATETIME()
    WHERE MemberId = @MemberId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MembershipPlan_Insert
    @GymId UNIQUEIDENTIFIER,
    @PlanName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @DurationDays INT,
    @Price DECIMAL(18, 2),
    @MembershipPlanId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.MembershipPlans (GymId, PlanName, Description, DurationDays, Price, IsActive, CreatedAt)
    VALUES (@GymId, @PlanName, @Description, @DurationDays, @Price, 1, SYSUTCDATETIME());
    SET @MembershipPlanId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Membership_Insert
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @MembershipPlanId INT,
    @StartDate DATE,
    @MembershipId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @DurationDays INT;
    DECLARE @EndDate DATE;

    SELECT @DurationDays = DurationDays FROM dbo.MembershipPlans
    WHERE MembershipPlanId = @MembershipPlanId AND GymId = @GymId;

    IF @DurationDays IS NULL
        THROW 50002, 'Membership plan not found for this gym.', 1;

    SET @EndDate = DATEADD(DAY, @DurationDays, @StartDate);

    INSERT INTO dbo.Memberships (GymId, MemberId, MembershipPlanId, StartDate, EndDate, Status, CreatedAt)
    VALUES (@GymId, @MemberId, @MembershipPlanId, @StartDate, @EndDate, N'Active', SYSUTCDATETIME());

    SET @MembershipId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Membership_GetById
    @MembershipId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ms.MembershipId, ms.GymId, ms.MemberId, ms.MembershipPlanId, ms.StartDate, ms.EndDate, ms.Status,
           ms.CreatedAt, ms.UpdatedAt, mp.PlanName, mp.Price
    FROM dbo.Memberships ms
    INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
    WHERE ms.MembershipId = @MembershipId AND (ms.GymId = @GymId);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Payment_Insert
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
    INSERT INTO dbo.Payments (GymId, MemberId, MembershipId, Amount, PaymentDate, PaymentMethod,
        TransactionReference, Status, Notes, CreatedAt)
    VALUES (@GymId, @MemberId, @MembershipId, @Amount, @PaymentDate, @PaymentMethod,
        @TransactionReference, @Status, @Notes, SYSUTCDATETIME());
    SET @PaymentId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Payment_GetByGym
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PaymentId, GymId, MemberId, MembershipId, Amount, PaymentDate, PaymentMethod,
           TransactionReference, Status, Notes, CreatedAt, UpdatedAt
    FROM dbo.Payments WHERE GymId = @GymId ORDER BY PaymentDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Dashboard_SuperAdmin
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.Gyms) AS TotalGyms,
        (SELECT COUNT(*) FROM dbo.Gyms WHERE IsActive = 1) AS ActiveGyms,
        (SELECT COUNT(*) FROM dbo.Members WHERE IsActive = 1) AS TotalMembers,
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.Payments WHERE Status = N'Completed') AS TotalRevenue,
        (SELECT COUNT(*) FROM dbo.Memberships WHERE Status = N'Active' AND EndDate < CAST(GETUTCDATE() AS DATE)) AS ExpiredMemberships;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Dashboard_GymAdmin
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.Members WHERE GymId = @GymId AND IsActive = 1) AS TotalMembers,
        (SELECT COUNT(*) FROM dbo.Memberships WHERE GymId = @GymId AND Status = N'Active') AS ActiveMemberships,
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.Payments WHERE GymId = @GymId AND Status = N'Completed') AS TotalRevenue,
        (SELECT COUNT(*) FROM dbo.Trainers WHERE GymId = @GymId AND IsActive = 1) AS TotalTrainers,
        (SELECT COUNT(*) FROM dbo.Memberships WHERE GymId = @GymId AND Status = N'Active' AND EndDate < CAST(GETUTCDATE() AS DATE)) AS ExpiredMemberships;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Role_GetById
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId, RoleName, Description, IsSystemRole, CreatedDate, CreatedAt, UpdatedAt
    FROM dbo.Roles WHERE RoleId = @RoleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Privilege_GetById
    @PrivilegeId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PrivilegeId, PrivilegeName, Description, Category, CreatedDate, CreatedAt, UpdatedAt
    FROM dbo.Privileges WHERE PrivilegeId = @PrivilegeId;
END
GO
