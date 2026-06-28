-- 057_SaasBillingCycles.sql
-- Add Quarterly and Half-Yearly billing cycles with period-end calculation.

IF COL_LENGTH('dbo.SaasSubscriptionPlans', 'QuarterlyPrice') IS NULL
    ALTER TABLE dbo.SaasSubscriptionPlans ADD QuarterlyPrice DECIMAL(18, 2) NOT NULL
        CONSTRAINT DF_SaasSubscriptionPlans_QuarterlyPrice DEFAULT (0) WITH VALUES;
GO

IF COL_LENGTH('dbo.SaasSubscriptionPlans', 'HalfYearlyPrice') IS NULL
    ALTER TABLE dbo.SaasSubscriptionPlans ADD HalfYearlyPrice DECIMAL(18, 2) NOT NULL
        CONSTRAINT DF_SaasSubscriptionPlans_HalfYearlyPrice DEFAULT (0) WITH VALUES;
GO

UPDATE dbo.SaasSubscriptionPlans
SET QuarterlyPrice = MonthlyPrice * 3,
    HalfYearlyPrice = MonthlyPrice * 6,
    UpdatedAt = SYSUTCDATETIME()
WHERE QuarterlyPrice = 0 OR HalfYearlyPrice = 0;
GO

CREATE OR ALTER FUNCTION dbo.fn_Saas_CalculatePeriodEnd
(
    @PeriodStart DATETIME2,
    @BillingCycle NVARCHAR(20)
)
RETURNS DATETIME2
AS
BEGIN
    DECLARE @Cycle NVARCHAR(20) = UPPER(LTRIM(RTRIM(@BillingCycle)));

    RETURN CASE @Cycle
        WHEN N'MONTHLY' THEN DATEADD(MONTH, 1, @PeriodStart)
        WHEN N'QUARTERLY' THEN DATEADD(MONTH, 3, @PeriodStart)
        WHEN N'HALFYEARLY' THEN DATEADD(MONTH, 6, @PeriodStart)
        WHEN N'HALF-YEARLY' THEN DATEADD(MONTH, 6, @PeriodStart)
        WHEN N'HALF YEARLY' THEN DATEADD(MONTH, 6, @PeriodStart)
        WHEN N'YEARLY' THEN DATEADD(YEAR, 1, @PeriodStart)
        ELSE DATEADD(MONTH, 1, @PeriodStart)
    END;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetAllPlans
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SaasPlanId, PlanCode, PlanName, MaxMembers, MaxTrainers, StorageLimitMb,
           WhatsAppNotificationLimit, MonthlyPrice, QuarterlyPrice, HalfYearlyPrice, YearlyPrice,
           TrialDays, IsActive, SortOrder
    FROM dbo.SaasSubscriptionPlans
    WHERE IsActive = 1
    ORDER BY SortOrder;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetPlanById
    @SaasPlanId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SaasPlanId, PlanCode, PlanName, MaxMembers, MaxTrainers, StorageLimitMb,
           WhatsAppNotificationLimit, MonthlyPrice, QuarterlyPrice, HalfYearlyPrice, YearlyPrice,
           TrialDays, IsActive, SortOrder
    FROM dbo.SaasSubscriptionPlans WHERE SaasPlanId = @SaasPlanId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetPlanByCode
    @PlanCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SaasPlanId, PlanCode, PlanName, MaxMembers, MaxTrainers, StorageLimitMb,
           WhatsAppNotificationLimit, MonthlyPrice, QuarterlyPrice, HalfYearlyPrice, YearlyPrice,
           TrialDays, IsActive, SortOrder
    FROM dbo.SaasSubscriptionPlans WHERE PlanCode = @PlanCode AND IsActive = 1;
END
GO

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

    DECLARE @PeriodEnd DATETIME2 = dbo.fn_Saas_CalculatePeriodEnd(@PeriodStart, @BillingCycle);

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
