-- 054_SubscriptionRenewalDateLogic.sql
-- Renewal period stacking: preserve unused days on early/grace renewal; start fresh after grace.

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

    DECLARE @PlanName NVARCHAR(100);
    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    DECLARE @Today DATE = CAST(@Now AS DATE);

    SELECT @PlanName = PlanName FROM dbo.SaasSubscriptionPlans WHERE SaasPlanId = @SaasPlanId;
    IF @PlanName IS NULL THROW 50051, 'Plan not found.', 1;

    DECLARE @ExistingPeriodEnd DATETIME2 = NULL;
    DECLARE @ExistingGraceEndsAt DATETIME2 = NULL;

    SELECT TOP 1
        @ExistingPeriodEnd = gs.CurrentPeriodEnd,
        @ExistingGraceEndsAt = gs.GraceEndsAt
    FROM dbo.GymSubscriptions gs
    WHERE gs.GymId = @GymId
    ORDER BY gs.GymSubscriptionId DESC;

    DECLARE @PeriodStart DATETIME2;

    IF @ExistingPeriodEnd IS NULL
        SET @PeriodStart = CAST(@Today AS DATETIME2);
    ELSE IF @ExistingGraceEndsAt IS NOT NULL AND @Today > CAST(@ExistingGraceEndsAt AS DATE)
        SET @PeriodStart = CAST(@Today AS DATETIME2);
    ELSE IF @ExistingGraceEndsAt IS NULL AND CAST(@ExistingPeriodEnd AS DATE) < @Today
        SET @PeriodStart = CAST(@Today AS DATETIME2);
    ELSE
        SET @PeriodStart = DATEADD(DAY, 1, CAST(@ExistingPeriodEnd AS DATE));

    DECLARE @PeriodEnd DATETIME2 = CASE
        WHEN @BillingCycle = N'Yearly' THEN DATEADD(YEAR, 1, @PeriodStart)
        ELSE DATEADD(MONTH, 1, @PeriodStart)
    END;

    UPDATE dbo.GymSubscriptions
    SET Status = N'Cancelled', CancelledAt = @Now, UpdatedAt = @Now
    WHERE GymId = @GymId
      AND Status IN (N'Trial', N'Active', N'PastDue', N'Expired')
      AND CancelledAt IS NULL;

    INSERT INTO dbo.GymSubscriptions
        (GymId, SaasPlanId, PlanName, StartDate, EndDate, Amount, Status, BillingCycle,
         TrialEndsAt, CurrentPeriodStart, CurrentPeriodEnd, GraceEndsAt,
         RazorpayOrderId, RazorpayPaymentId, RazorpaySubscriptionId, CreatedAt)
    VALUES
        (@GymId, @SaasPlanId, @PlanName, CAST(@PeriodStart AS DATE), CAST(@PeriodEnd AS DATE), @Amount, N'Active', @BillingCycle,
         NULL, @PeriodStart, @PeriodEnd, DATEADD(DAY, @GracePeriodDays, @PeriodEnd),
         @RazorpayOrderId, @RazorpayPaymentId, @RazorpaySubscriptionId, @Now);

    UPDATE dbo.Gyms
    SET IsActive = 1, UpdatedAt = @Now
    WHERE GymId = @GymId;
END
GO
