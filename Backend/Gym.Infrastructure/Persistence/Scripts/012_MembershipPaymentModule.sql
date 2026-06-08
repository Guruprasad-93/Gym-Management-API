/*
  Membership & Payment Management Module
  TRY/CATCH, transactions, GymId isolation, auto status calculation
*/

/* ========== SCHEMA EXTENSIONS ========== */
IF COL_LENGTH('dbo.MembershipPlans', 'DurationInMonths') IS NULL
    ALTER TABLE dbo.MembershipPlans ADD DurationInMonths INT NULL;
GO
UPDATE dbo.MembershipPlans
SET DurationInMonths = CASE WHEN DurationDays >= 30 THEN DurationDays / 30 ELSE 1 END
WHERE DurationInMonths IS NULL;
GO
IF COL_LENGTH('dbo.Memberships', 'Amount') IS NULL
    ALTER TABLE dbo.Memberships ADD Amount DECIMAL(18, 2) NULL;
GO
IF COL_LENGTH('dbo.Memberships', 'Notes') IS NULL
    ALTER TABLE dbo.Memberships ADD Notes NVARCHAR(500) NULL;
GO

/* ========== MEMBERSHIP PLAN CRUD ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateMembershipPlan
    @GymId UNIQUEIDENTIFIER,
    @PlanName NVARCHAR(100),
    @DurationInMonths INT,
    @Price DECIMAL(18, 2),
    @Description NVARCHAR(500) = NULL,
    @MembershipPlanId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Gyms WHERE GymId = @GymId AND IsActive = 1)
            THROW 50050, 'Gym not found or inactive.', 1;
        IF @DurationInMonths < 1
            THROW 50051, 'Duration must be at least 1 month.', 1;
        IF @Price < 0
            THROW 50052, 'Price cannot be negative.', 1;

        INSERT INTO dbo.MembershipPlans (GymId, PlanName, Description, DurationDays, DurationInMonths, Price, IsActive, CreatedAt)
        VALUES (@GymId, @PlanName, @Description, @DurationInMonths * 30, @DurationInMonths, @Price, 1, SYSUTCDATETIME());

        SET @MembershipPlanId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateMembershipPlan
    @MembershipPlanId INT,
    @GymId UNIQUEIDENTIFIER,
    @PlanName NVARCHAR(100),
    @DurationInMonths INT,
    @Price DECIMAL(18, 2),
    @Description NVARCHAR(500) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.MembershipPlans WHERE MembershipPlanId = @MembershipPlanId AND GymId = @GymId)
            THROW 50053, 'Membership plan not found.', 1;

        UPDATE dbo.MembershipPlans
        SET PlanName = @PlanName,
            Description = @Description,
            DurationInMonths = @DurationInMonths,
            DurationDays = @DurationInMonths * 30,
            Price = @Price,
            IsActive = @IsActive,
            UpdatedAt = SYSUTCDATETIME()
        WHERE MembershipPlanId = @MembershipPlanId AND GymId = @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteMembershipPlan
    @MembershipPlanId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM dbo.Memberships ms WHERE ms.MembershipPlanId = @MembershipPlanId AND ms.Status = N'Active' AND ms.EndDate >= CAST(GETUTCDATE() AS DATE))
            THROW 50054, 'Cannot delete plan with active memberships.', 1;

        UPDATE dbo.MembershipPlans
        SET IsActive = 0, UpdatedAt = SYSUTCDATETIME()
        WHERE MembershipPlanId = @MembershipPlanId AND GymId = @GymId;

        IF @@ROWCOUNT = 0
            THROW 50053, 'Membership plan not found.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMembershipPlans
    @GymId UNIQUEIDENTIFIER = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            MembershipPlanId,
            GymId,
            PlanName,
            Description,
            ISNULL(DurationInMonths, DurationDays / 30) AS DurationInMonths,
            DurationDays,
            Price,
            IsActive,
            CreatedAt,
            UpdatedAt
        FROM dbo.MembershipPlans
        WHERE (GymId = @GymId)
          AND (@IncludeInactive = 1 OR IsActive = 1)
        ORDER BY PlanName;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== MEMBERSHIP CRUD ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreateMembership
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @MembershipPlanId INT,
    @StartDate DATE,
    @Notes NVARCHAR(500) = NULL,
    @MembershipId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Members WHERE MemberId = @MemberId AND GymId = @GymId AND IsDeleted = 0)
            THROW 50043, 'Member not found.', 1;

        IF EXISTS (
            SELECT 1 FROM dbo.Memberships ms
            WHERE ms.MemberId = @MemberId AND ms.GymId = @GymId
              AND ms.Status = N'Active' AND ms.EndDate >= CAST(GETUTCDATE() AS DATE))
            THROW 50055, 'Member already has an active membership.', 1;

        DECLARE @DurationInMonths INT;
        DECLARE @Price DECIMAL(18, 2);
        DECLARE @EndDate DATE;

        SELECT @DurationInMonths = ISNULL(DurationInMonths, DurationDays / 30),
               @Price = Price
        FROM dbo.MembershipPlans
        WHERE MembershipPlanId = @MembershipPlanId AND GymId = @GymId AND IsActive = 1;

        IF @DurationInMonths IS NULL
            THROW 50053, 'Membership plan not found.', 1;

        SET @EndDate = DATEADD(MONTH, @DurationInMonths, @StartDate);

        INSERT INTO dbo.Memberships (GymId, MemberId, MembershipPlanId, StartDate, EndDate, Amount, Status, Notes, CreatedAt)
        VALUES (@GymId, @MemberId, @MembershipPlanId, @StartDate, @EndDate, @Price, N'Active', @Notes, SYSUTCDATETIME());

        SET @MembershipId = SCOPE_IDENTITY();
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RenewMembership
    @MembershipId INT,
    @GymId UNIQUEIDENTIFIER,
    @Notes NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @MemberId INT;
        DECLARE @PlanId INT;
        DECLARE @DurationInMonths INT;
        DECLARE @Price DECIMAL(18, 2);
        DECLARE @NewStart DATE;
        DECLARE @NewEnd DATE;

        SELECT @MemberId = ms.MemberId, @PlanId = ms.MembershipPlanId
        FROM dbo.Memberships ms
        WHERE ms.MembershipId = @MembershipId AND ms.GymId = @GymId;

        IF @MemberId IS NULL
            THROW 50056, 'Membership not found.', 1;

        SELECT @DurationInMonths = ISNULL(DurationInMonths, DurationDays / 30), @Price = Price
        FROM dbo.MembershipPlans WHERE MembershipPlanId = @PlanId;

        SET @NewStart = CASE
            WHEN EXISTS (SELECT 1 FROM dbo.Memberships WHERE MembershipId = @MembershipId AND EndDate >= CAST(GETUTCDATE() AS DATE))
            THEN (SELECT EndDate FROM dbo.Memberships WHERE MembershipId = @MembershipId)
            ELSE CAST(GETUTCDATE() AS DATE)
        END;
        SET @NewEnd = DATEADD(MONTH, @DurationInMonths, @NewStart);

        UPDATE dbo.Memberships
        SET Status = N'Expired', UpdatedAt = SYSUTCDATETIME()
        WHERE MembershipId = @MembershipId;

        INSERT INTO dbo.Memberships (GymId, MemberId, MembershipPlanId, StartDate, EndDate, Amount, Status, Notes, CreatedAt)
        VALUES (@GymId, @MemberId, @PlanId, @NewStart, @NewEnd, @Price, N'Active', @Notes, SYSUTCDATETIME());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CancelMembership
    @MembershipId INT,
    @GymId UNIQUEIDENTIFIER,
    @Notes NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Memberships
        SET Status = N'Cancelled',
            Notes = ISNULL(@Notes, Notes),
            UpdatedAt = SYSUTCDATETIME()
        WHERE MembershipId = @MembershipId AND GymId = @GymId;

        IF @@ROWCOUNT = 0
            THROW 50056, 'Membership not found.', 1;
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
        SELECT
            ms.MembershipId,
            ms.GymId,
            ms.MemberId,
            ms.MembershipPlanId,
            ms.StartDate,
            ms.EndDate,
            ms.Amount,
            ms.Notes,
            ms.CreatedAt,
            ms.UpdatedAt,
            mp.PlanName,
            mp.Price AS PlanPrice,
            ISNULL(mp.DurationInMonths, mp.DurationDays / 30) AS DurationInMonths,
            u.Name AS MemberName,
            u.Email AS MemberEmail,
            CASE
                WHEN ms.Status = N'Cancelled' THEN N'Cancelled'
                WHEN ms.EndDate >= CAST(GETUTCDATE() AS DATE) THEN N'Active'
                ELSE N'Expired'
            END AS Status
        FROM dbo.Memberships ms
        INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
        INNER JOIN dbo.Members m ON m.MemberId = ms.MemberId
        INNER JOIN dbo.Users u ON u.Id = m.UserId
        WHERE ms.MembershipId = @MembershipId
          AND (ms.GymId = @GymId);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMembershipDetails
    @MembershipId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_GetMembershipById @MembershipId = @MembershipId, @GymId = @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllMemberships
    @GymId UNIQUEIDENTIFIER = NULL,
    @MemberId INT = NULL,
    @Search NVARCHAR(200) = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @SearchPattern NVARCHAR(202) = NULL;
        IF @Search IS NOT NULL AND LEN(LTRIM(RTRIM(@Search))) > 0
            SET @SearchPattern = N'%' + LTRIM(RTRIM(@Search)) + N'%';

        SELECT
            ms.MembershipId,
            ms.GymId,
            ms.MemberId,
            ms.MembershipPlanId,
            ms.StartDate,
            ms.EndDate,
            ms.Amount,
            ms.Notes,
            ms.CreatedAt,
            ms.UpdatedAt,
            mp.PlanName,
            mp.Price AS PlanPrice,
            ISNULL(mp.DurationInMonths, mp.DurationDays / 30) AS DurationInMonths,
            u.Name AS MemberName,
            u.Email AS MemberEmail,
            CASE
                WHEN ms.Status = N'Cancelled' THEN N'Cancelled'
                WHEN ms.EndDate >= CAST(GETUTCDATE() AS DATE) THEN N'Active'
                ELSE N'Expired'
            END AS Status
        FROM dbo.Memberships ms
        INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
        INNER JOIN dbo.Members m ON m.MemberId = ms.MemberId
        INNER JOIN dbo.Users u ON u.Id = m.UserId
        WHERE (ms.GymId = @GymId)
          AND (@MemberId IS NULL OR ms.MemberId = @MemberId)
          AND (
              @SearchPattern IS NULL
              OR u.Name LIKE @SearchPattern
              OR mp.PlanName LIKE @SearchPattern
          )
          AND (
              @IncludeInactive = 1
              OR (ms.Status <> N'Cancelled' AND ms.EndDate >= CAST(GETUTCDATE() AS DATE))
          )
        ORDER BY ms.StartDate DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetExpiredMemberships
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            ms.MembershipId,
            ms.GymId,
            ms.MemberId,
            ms.MembershipPlanId,
            ms.StartDate,
            ms.EndDate,
            ms.Amount,
            ms.Notes,
            ms.CreatedAt,
            ms.UpdatedAt,
            mp.PlanName,
            mp.Price AS PlanPrice,
            ISNULL(mp.DurationInMonths, mp.DurationDays / 30) AS DurationInMonths,
            u.Name AS MemberName,
            u.Email AS MemberEmail,
            N'Expired' AS Status
        FROM dbo.Memberships ms
        INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
        INNER JOIN dbo.Members m ON m.MemberId = ms.MemberId
        INNER JOIN dbo.Users u ON u.Id = m.UserId
        WHERE ms.Status <> N'Cancelled'
          AND ms.EndDate < CAST(GETUTCDATE() AS DATE)
          AND (ms.GymId = @GymId)
        ORDER BY ms.EndDate DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== PAYMENT ========== */
CREATE OR ALTER PROCEDURE dbo.sp_CreatePayment
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @MembershipId INT,
    @Amount DECIMAL(18, 2),
    @PaymentDate DATETIME2,
    @PaymentMethod NVARCHAR(50),
    @TransactionReference NVARCHAR(100) = NULL,
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

        IF @PaymentMethod NOT IN (N'Cash', N'UPI', N'Card', N'Bank Transfer')
            THROW 50057, 'Invalid payment method.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Members WHERE MemberId = @MemberId AND GymId = @GymId AND IsDeleted = 0)
            THROW 50043, 'Member not found.', 1;

        DECLARE @ExpectedAmount DECIMAL(18, 2);
        SELECT @ExpectedAmount = ISNULL(ms.Amount, mp.Price)
        FROM dbo.Memberships ms
        INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
        WHERE ms.MembershipId = @MembershipId AND ms.GymId = @GymId;

        IF @ExpectedAmount IS NULL
            THROW 50056, 'Membership not found.', 1;

        IF @Amount <> @ExpectedAmount
            THROW 50058, 'Payment amount must match membership amount.', 1;

        INSERT INTO dbo.Payments (GymId, MemberId, MembershipId, Amount, PaymentDate, PaymentMethod,
            TransactionReference, Status, Notes, CreatedAt)
        VALUES (@GymId, @MemberId, @MembershipId, @Amount, @PaymentDate, @PaymentMethod,
            @TransactionReference, N'Completed', @Notes, SYSUTCDATETIME());

        SET @PaymentId = SCOPE_IDENTITY();
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPaymentHistory
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
            p.UpdatedAt,
            u.Name AS MemberName,
            u.Email AS MemberEmail,
            mp.PlanName AS MembershipPlanName
        FROM dbo.Payments p
        LEFT JOIN dbo.Members m ON m.MemberId = p.MemberId
        LEFT JOIN dbo.Users u ON u.Id = m.UserId
        LEFT JOIN dbo.Memberships ms ON ms.MembershipId = p.MembershipId
        LEFT JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
        WHERE (p.GymId = @GymId)
          AND (
              @SearchPattern IS NULL
              OR u.Name LIKE @SearchPattern
              OR p.TransactionReference LIKE @SearchPattern
              OR p.PaymentMethod LIKE @SearchPattern
          )
        ORDER BY p.PaymentDate DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPaymentsByMember
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        EXEC dbo.sp_GetMemberPaymentHistory @MemberId = @MemberId, @GymId = @GymId;
    END TRY
    BEGIN CATCH
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
        EXEC dbo.sp_GetPaymentHistory @GymId = @GymId, @Search = NULL;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GenerateInvoice
    @PaymentId INT,
    @GymId UNIQUEIDENTIFIER,
    @InvoiceId INT OUTPUT,
    @InvoiceNumber NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Payments WHERE PaymentId = @PaymentId AND GymId = @GymId)
            THROW 50059, 'Payment not found.', 1;

        IF EXISTS (SELECT 1 FROM dbo.Invoices WHERE PaymentId = @PaymentId)
        BEGIN
            SELECT @InvoiceId = InvoiceId, @InvoiceNumber = InvoiceNumber
            FROM dbo.Invoices WHERE PaymentId = @PaymentId;
            COMMIT TRANSACTION;
            RETURN;
        END

        DECLARE @MemberId INT;
        DECLARE @Amount DECIMAL(18, 2);
        SELECT @MemberId = MemberId, @Amount = Amount FROM dbo.Payments WHERE PaymentId = @PaymentId;

        SET @InvoiceNumber = N'INV-' + FORMAT(SYSUTCDATETIME(), 'yyyyMMdd') + N'-' + RIGHT(N'00000' + CAST(@PaymentId AS NVARCHAR(10)), 5);

        INSERT INTO dbo.Invoices (GymId, PaymentId, MemberId, InvoiceNumber, Amount, IssuedAt, CreatedAt)
        VALUES (@GymId, @PaymentId, @MemberId, @InvoiceNumber, @Amount, SYSUTCDATETIME(), SYSUTCDATETIME());

        SET @InvoiceId = SCOPE_IDENTITY();
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetInvoiceById
    @InvoiceId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            i.InvoiceId,
            i.GymId,
            i.PaymentId,
            i.MemberId,
            i.InvoiceNumber,
            i.Amount,
            i.IssuedAt,
            i.CreatedAt,
            g.Name AS GymName,
            g.Address AS GymAddress,
            g.Phone AS GymPhone,
            g.Email AS GymEmail,
            u.Name AS MemberName,
            u.Email AS MemberEmail,
            m.Phone AS MemberPhone,
            p.PaymentDate,
            p.PaymentMethod,
            p.TransactionReference,
            p.Notes AS PaymentNotes,
            mp.PlanName AS MembershipPlanName
        FROM dbo.Invoices i
        INNER JOIN dbo.Gyms g ON g.GymId = i.GymId
        INNER JOIN dbo.Members m ON m.MemberId = i.MemberId
        INNER JOIN dbo.Users u ON u.Id = m.UserId
        INNER JOIN dbo.Payments p ON p.PaymentId = i.PaymentId
        LEFT JOIN dbo.Memberships ms ON ms.MembershipId = p.MembershipId
        LEFT JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
        WHERE i.InvoiceId = @InvoiceId
          AND (i.GymId = @GymId);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetRevenueDashboard
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE Status = N'Completed' AND (GymId = @GymId)), 0) AS TotalRevenue,
            ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE Status = N'Completed' AND (GymId = @GymId)
                AND YEAR(PaymentDate) = YEAR(GETUTCDATE()) AND MONTH(PaymentDate) = MONTH(GETUTCDATE())), 0) AS MonthlyRevenue,
            (SELECT COUNT(*) FROM dbo.Memberships ms WHERE ms.Status <> N'Cancelled' AND ms.EndDate < CAST(GETUTCDATE() AS DATE) AND (ms.GymId = @GymId)) AS ExpiredMemberships,
            (SELECT COUNT(*) FROM dbo.Memberships ms WHERE ms.Status <> N'Cancelled' AND ms.EndDate >= CAST(GETUTCDATE() AS DATE) AND (ms.GymId = @GymId)) AS ActiveMemberships,
            (SELECT COUNT(*) FROM dbo.Memberships ms WHERE ms.Status <> N'Cancelled'
                AND ms.EndDate BETWEEN CAST(GETUTCDATE() AS DATE) AND DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE))
                AND (ms.GymId = @GymId)) AS PendingRenewals;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMonthlyRevenueSummary
    @GymId UNIQUEIDENTIFIER = NULL,
    @Months INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @Months < 1 SET @Months = 12;
        IF @Months > 24 SET @Months = 24;

        ;WITH Months AS (
            SELECT TOP (@Months)
                DATEADD(MONTH, -ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 1, DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1)) AS MonthStart
            FROM sys.all_objects
        )
        SELECT
            YEAR(m.MonthStart) AS [Year],
            MONTH(m.MonthStart) AS [Month],
            DATENAME(MONTH, m.MonthStart) + N' ' + CAST(YEAR(m.MonthStart) AS NVARCHAR(4)) AS MonthLabel,
            ISNULL(SUM(p.Amount), 0) AS Revenue
        FROM Months m
        LEFT JOIN dbo.Payments p ON p.Status = N'Completed'
            AND (p.GymId = @GymId)
            AND p.PaymentDate >= m.MonthStart
            AND p.PaymentDate < DATEADD(MONTH, 1, m.MonthStart)
        GROUP BY m.MonthStart
        ORDER BY m.MonthStart;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ========== ENHANCED GYM DASHBOARD ========== */
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
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.Payments WHERE GymId = @GymId AND Status = N'Completed'
            AND YEAR(PaymentDate) = YEAR(GETUTCDATE()) AND MONTH(PaymentDate) = MONTH(GETUTCDATE())) AS MonthlyRevenue,
        (SELECT COUNT(*) FROM dbo.Memberships WHERE GymId = @GymId AND Status <> N'Cancelled' AND EndDate < CAST(GETUTCDATE() AS DATE)) AS ExpiredMemberships,
        (SELECT COUNT(*) FROM dbo.Memberships WHERE GymId = @GymId AND Status <> N'Cancelled' AND EndDate >= CAST(GETUTCDATE() AS DATE)) AS ActiveMemberships,
        (SELECT COUNT(*) FROM dbo.Memberships WHERE GymId = @GymId AND Status <> N'Cancelled'
            AND EndDate BETWEEN CAST(GETUTCDATE() AS DATE) AND DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE))) AS PendingRenewals,
        (SELECT COUNT(*) FROM dbo.Trainers WHERE GymId = @GymId AND IsActive = 1) AS TotalTrainers;
END
GO

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
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.Payments WHERE Status = N'Completed'
            AND YEAR(PaymentDate) = YEAR(GETUTCDATE()) AND MONTH(PaymentDate) = MONTH(GETUTCDATE())) AS MonthlyRevenue,
        (SELECT COUNT(*) FROM dbo.Memberships WHERE Status <> N'Cancelled' AND EndDate < CAST(GETUTCDATE() AS DATE)) AS ExpiredMemberships,
        (SELECT COUNT(*) FROM dbo.Memberships WHERE Status <> N'Cancelled' AND EndDate >= CAST(GETUTCDATE() AS DATE)) AS ActiveMemberships,
        (SELECT COUNT(*) FROM dbo.Memberships WHERE Status <> N'Cancelled'
            AND EndDate BETWEEN CAST(GETUTCDATE() AS DATE) AND DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE))) AS PendingRenewals,
        (SELECT COUNT(*) FROM dbo.Trainers WHERE IsActive = 1) AS TotalTrainers;
END
GO
