/*
  Razorpay Payment Gateway Module
  Extends Payments for online checkout, verification, refunds, membership activation
*/

IF COL_LENGTH('dbo.Payments', 'RazorpayOrderId') IS NULL
    ALTER TABLE dbo.Payments ADD RazorpayOrderId NVARCHAR(100) NULL;
GO
IF COL_LENGTH('dbo.Payments', 'RazorpayPaymentId') IS NULL
    ALTER TABLE dbo.Payments ADD RazorpayPaymentId NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_RazorpayOrderId' AND object_id = OBJECT_ID(N'dbo.Payments'))
    CREATE INDEX IX_Payments_RazorpayOrderId ON dbo.Payments (RazorpayOrderId) WHERE RazorpayOrderId IS NOT NULL;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateRazorpayPaymentOrder
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @MembershipId INT,
    @RazorpayOrderId NVARCHAR(100),
    @Amount DECIMAL(18, 2),
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

        IF NOT EXISTS (SELECT 1 FROM dbo.Members WHERE MemberId = @MemberId AND GymId = @GymId AND IsDeleted = 0)
            THROW 50043, 'Member not found.', 1;

        DECLARE @ExpectedAmount DECIMAL(18, 2);
        SELECT @ExpectedAmount = ISNULL(ms.Amount, mp.Price)
        FROM dbo.Memberships ms
        INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
        WHERE ms.MembershipId = @MembershipId AND ms.GymId = @GymId AND ms.MemberId = @MemberId;

        IF @ExpectedAmount IS NULL
            THROW 50056, 'Membership not found.', 1;

        IF @Amount <> @ExpectedAmount
            THROW 50058, 'Payment amount must match membership amount.', 1;

        IF EXISTS (
            SELECT 1 FROM dbo.Payments
            WHERE MembershipId = @MembershipId AND Status = N'Pending' AND PaymentMethod = N'Razorpay')
            THROW 50060, 'A pending Razorpay payment already exists for this membership.', 1;

        INSERT INTO dbo.Payments (
            GymId, MemberId, MembershipId, Amount, PaymentDate, PaymentMethod,
            TransactionReference, RazorpayOrderId, Status, Notes, CreatedAt)
        VALUES (
            @GymId, @MemberId, @MembershipId, @Amount, SYSUTCDATETIME(), N'Razorpay',
            @RazorpayOrderId, @RazorpayOrderId, N'Pending', @Notes, SYSUTCDATETIME());

        SET @PaymentId = SCOPE_IDENTITY();
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetPaymentByRazorpayOrderId
    @RazorpayOrderId NVARCHAR(100),
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
            p.RazorpayOrderId,
            p.RazorpayPaymentId,
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
        WHERE p.RazorpayOrderId = @RazorpayOrderId
          AND (p.GymId = @GymId);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ConfirmRazorpayPayment
    @GymId UNIQUEIDENTIFIER,
    @RazorpayOrderId NVARCHAR(100),
    @RazorpayPaymentId NVARCHAR(100),
    @RenewMembership BIT = 1,
    @PaymentId INT OUTPUT,
    @NewMembershipId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        SET @NewMembershipId = NULL;

        DECLARE @CurrentStatus NVARCHAR(20);
        DECLARE @MembershipId INT;
        DECLARE @MemberId INT;

        SELECT @PaymentId = PaymentId,
               @CurrentStatus = Status,
               @MembershipId = MembershipId,
               @MemberId = MemberId
        FROM dbo.Payments
        WHERE RazorpayOrderId = @RazorpayOrderId AND GymId = @GymId;

        IF @PaymentId IS NULL
            THROW 50059, 'Payment not found.', 1;

        IF @CurrentStatus = N'Completed'
        BEGIN
            SELECT @NewMembershipId = ms.MembershipId
            FROM dbo.Memberships ms
            WHERE ms.MemberId = @MemberId AND ms.GymId = @GymId AND ms.Status = N'Active'
            ORDER BY ms.StartDate DESC;
            COMMIT TRANSACTION;
            RETURN;
        END

        IF @CurrentStatus <> N'Pending'
            THROW 50061, 'Payment cannot be confirmed in its current status.', 1;

        UPDATE dbo.Payments
        SET Status = N'Completed',
            RazorpayPaymentId = @RazorpayPaymentId,
            TransactionReference = @RazorpayPaymentId,
            PaymentDate = SYSUTCDATETIME(),
            UpdatedAt = SYSUTCDATETIME()
        WHERE PaymentId = @PaymentId;

        IF @RenewMembership = 1
        BEGIN
            DECLARE @ShouldRenew BIT = 0;
            IF EXISTS (
                SELECT 1 FROM dbo.Memberships
                WHERE MembershipId = @MembershipId
                  AND (EndDate < CAST(GETUTCDATE() AS DATE) OR Status = N'Expired'))
                SET @ShouldRenew = 1;

            IF @ShouldRenew = 1
            BEGIN
                EXEC dbo.sp_RenewMembership
                    @MembershipId = @MembershipId,
                    @GymId = @GymId,
                    @Notes = N'Renewed after Razorpay payment';

                SELECT TOP 1 @NewMembershipId = ms.MembershipId
                FROM dbo.Memberships ms
                WHERE ms.MemberId = @MemberId AND ms.GymId = @GymId
                ORDER BY ms.CreatedAt DESC;
            END
            ELSE
                SET @NewMembershipId = @MembershipId;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_FailRazorpayPayment
    @GymId UNIQUEIDENTIFIER,
    @RazorpayOrderId NVARCHAR(100),
    @FailureReason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Payments
        SET Status = N'Failed',
            Notes = COALESCE(@FailureReason, Notes),
            UpdatedAt = SYSUTCDATETIME()
        WHERE RazorpayOrderId = @RazorpayOrderId
          AND GymId = @GymId
          AND Status = N'Pending';

        IF @@ROWCOUNT = 0
            THROW 50059, 'Payment not found.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RefundPayment
    @PaymentId INT,
    @GymId UNIQUEIDENTIFIER,
    @RefundReference NVARCHAR(100) = NULL,
    @Notes NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (
            SELECT 1 FROM dbo.Payments
            WHERE PaymentId = @PaymentId AND GymId = @GymId AND Status = N'Completed' AND PaymentMethod = N'Razorpay')
            THROW 50062, 'Only completed Razorpay payments can be refunded.', 1;

        UPDATE dbo.Payments
        SET Status = N'Refunded',
            Notes = COALESCE(@Notes, Notes),
            TransactionReference = COALESCE(@RefundReference, TransactionReference),
            UpdatedAt = SYSUTCDATETIME()
        WHERE PaymentId = @PaymentId AND GymId = @GymId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberPayableMembership
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT TOP 1
            ms.MembershipId,
            ms.GymId,
            ms.MemberId,
            ms.MembershipPlanId,
            ms.StartDate,
            ms.EndDate,
            ISNULL(ms.Amount, mp.Price) AS Amount,
            ms.Status,
            mp.PlanName,
            mp.Price AS PlanPrice,
            ISNULL(mp.DurationInMonths, mp.DurationDays / 30) AS DurationInMonths
        FROM dbo.Memberships ms
        INNER JOIN dbo.MembershipPlans mp ON mp.MembershipPlanId = ms.MembershipPlanId
        WHERE ms.MemberId = @MemberId AND ms.GymId = @GymId AND ms.Status <> N'Cancelled'
        ORDER BY ms.StartDate DESC;
    END TRY
    BEGIN CATCH
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
            p.RazorpayOrderId,
            p.RazorpayPaymentId,
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
              OR p.RazorpayOrderId LIKE @SearchPattern
              OR p.RazorpayPaymentId LIKE @SearchPattern
              OR p.PaymentMethod LIKE @SearchPattern
              OR p.Status LIKE @SearchPattern
          )
        ORDER BY p.PaymentDate DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

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
            p.RazorpayOrderId,
            p.RazorpayPaymentId,
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
