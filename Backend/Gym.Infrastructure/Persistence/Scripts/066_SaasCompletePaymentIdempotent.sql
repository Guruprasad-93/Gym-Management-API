-- 066_SaasCompletePaymentIdempotent.sql
-- SEC-01: Make SaaS payment completion idempotent (Pending-only, duplicate-safe).

CREATE OR ALTER PROCEDURE dbo.sp_Saas_CompletePayment
    @SaasPaymentId INT,
    @RazorpayPaymentId NVARCHAR(100),
    @WasAlreadyCompleted BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @WasAlreadyCompleted = 0;

    DECLARE @CurrentStatus NVARCHAR(20);

    BEGIN TRANSACTION;

    SELECT @CurrentStatus = p.Status
    FROM dbo.SaasSubscriptionPayments p WITH (UPDLOCK, HOLDLOCK)
    WHERE p.SaasPaymentId = @SaasPaymentId;

    IF @CurrentStatus IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50058, 'Payment not found.', 1;
    END

    IF @CurrentStatus = N'Completed'
    BEGIN
        SET @WasAlreadyCompleted = 1;
        COMMIT TRANSACTION;
        RETURN;
    END

    IF @CurrentStatus <> N'Pending'
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50061, 'Payment cannot be confirmed in its current status.', 1;
    END

    UPDATE dbo.SaasSubscriptionPayments
    SET Status = N'Completed',
        RazorpayPaymentId = @RazorpayPaymentId,
        PaidAt = SYSUTCDATETIME()
    WHERE SaasPaymentId = @SaasPaymentId
      AND Status = N'Pending';

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT @CurrentStatus = p.Status
        FROM dbo.SaasSubscriptionPayments p
        WHERE p.SaasPaymentId = @SaasPaymentId;

        IF @CurrentStatus = N'Completed'
        BEGIN
            SET @WasAlreadyCompleted = 1;
            COMMIT TRANSACTION;
            RETURN;
        END

        ROLLBACK TRANSACTION;
        THROW 50061, 'Payment cannot be confirmed in its current status.', 1;
    END

    COMMIT TRANSACTION;
END
GO
