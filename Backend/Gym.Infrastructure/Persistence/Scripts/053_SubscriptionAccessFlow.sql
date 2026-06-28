-- 053_SubscriptionAccessFlow.sql
-- Subscription expiry: keep gyms login-capable after grace; reactivate gym on payment.

CREATE OR ALTER PROCEDURE dbo.sp_Saas_UpdateSubscriptionPlan
    @GymId UNIQUEIDENTIFIER,
    @SaasPlanId INT,
    @BillingCycle NVARCHAR(20),
    @Amount DECIMAL(18, 2),
    @RazorpayOrderId NVARCHAR(100) = NULL,
    @RazorpayPaymentId NVARCHAR(100) = NULL,
    @RazorpaySubscriptionId NVARCHAR(100) = NULL,
    @GracePeriodDays INT = 3
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @PlanName NVARCHAR(100), @Now DATETIME2 = SYSUTCDATETIME();
    SELECT @PlanName = PlanName FROM dbo.SaasSubscriptionPlans WHERE SaasPlanId = @SaasPlanId;
    IF @PlanName IS NULL THROW 50051, 'Plan not found.', 1;

    DECLARE @PeriodEnd DATETIME2 = CASE WHEN @BillingCycle = N'Yearly' THEN DATEADD(YEAR, 1, @Now) ELSE DATEADD(MONTH, 1, @Now) END;

    UPDATE dbo.GymSubscriptions
    SET Status = N'Cancelled', CancelledAt = @Now, UpdatedAt = @Now
    WHERE GymId = @GymId AND Status IN (N'Trial', N'Active', N'PastDue') AND CancelledAt IS NULL;

    INSERT INTO dbo.GymSubscriptions
        (GymId, SaasPlanId, PlanName, StartDate, EndDate, Amount, Status, BillingCycle,
         TrialEndsAt, CurrentPeriodStart, CurrentPeriodEnd, GraceEndsAt,
         RazorpayOrderId, RazorpayPaymentId, RazorpaySubscriptionId, CreatedAt)
    VALUES
        (@GymId, @SaasPlanId, @PlanName, CAST(@Now AS DATE), CAST(@PeriodEnd AS DATE), @Amount, N'Active', @BillingCycle,
         NULL, @Now, @PeriodEnd, DATEADD(DAY, @GracePeriodDays, @PeriodEnd),
         @RazorpayOrderId, @RazorpayPaymentId, @RazorpaySubscriptionId, @Now);

    UPDATE dbo.Gyms
    SET IsActive = 1, UpdatedAt = @Now
    WHERE GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_ExpireSubscriptions
    @GracePeriodDays INT = 3
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE gs
    SET Status = N'Expired', UpdatedAt = SYSUTCDATETIME()
    FROM dbo.GymSubscriptions gs
    WHERE gs.Status IN (N'Trial', N'Active', N'PastDue')
      AND gs.GraceEndsAt IS NOT NULL
      AND gs.GraceEndsAt < SYSUTCDATETIME();
END
GO

-- Restore login for gyms previously deactivated by subscription expiry.
UPDATE g
SET IsActive = 1, UpdatedAt = SYSUTCDATETIME()
FROM dbo.Gyms g
WHERE g.IsActive = 0
  AND EXISTS (
      SELECT 1
      FROM dbo.GymSubscriptions gs
      WHERE gs.GymId = g.GymId
        AND gs.GymSubscriptionId = (
            SELECT TOP 1 GymSubscriptionId
            FROM dbo.GymSubscriptions
            WHERE GymId = g.GymId
            ORDER BY GymSubscriptionId DESC)
        AND gs.Status = N'Expired');
GO
