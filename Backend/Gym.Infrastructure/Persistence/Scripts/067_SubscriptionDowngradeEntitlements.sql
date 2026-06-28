/* Subscription downgrade entitlements: plan feature matrix + sync on plan change. */

/* Rename Enterprise plan to Premium Pro (display name aligned with product). */
UPDATE dbo.SaasSubscriptionPlans
SET PlanCode = N'PremiumPro',
    PlanName = N'Premium Pro',
    Description = N'Full platform access including website builder and pro features'
WHERE PlanCode = N'Enterprise';

/* Premium plan: white label without website builder. */
DECLARE @PremiumPlanId INT = (SELECT SaasPlanId FROM dbo.SaasSubscriptionPlans WHERE PlanCode = N'Premium');
DECLARE @WhiteLabelFeatureId INT = (SELECT FeatureId FROM dbo.SystemFeatures WHERE FeatureCode = N'WHITE_LABEL');
DECLARE @CustomBrandingFeatureId INT = (SELECT FeatureId FROM dbo.SystemFeatures WHERE FeatureCode = N'CUSTOM_BRANDING');

IF @PremiumPlanId IS NOT NULL AND @WhiteLabelFeatureId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.PlanFeatures WHERE SaasPlanId = @PremiumPlanId AND FeatureId = @WhiteLabelFeatureId)
        INSERT INTO dbo.PlanFeatures (SaasPlanId, FeatureId, IsIncluded, CreatedAt)
        VALUES (@PremiumPlanId, @WhiteLabelFeatureId, 1, SYSUTCDATETIME());
    ELSE
        UPDATE dbo.PlanFeatures SET IsIncluded = 1
        WHERE SaasPlanId = @PremiumPlanId AND FeatureId = @WhiteLabelFeatureId;
END

IF @PremiumPlanId IS NOT NULL AND @CustomBrandingFeatureId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.PlanFeatures WHERE SaasPlanId = @PremiumPlanId AND FeatureId = @CustomBrandingFeatureId)
        INSERT INTO dbo.PlanFeatures (SaasPlanId, FeatureId, IsIncluded, CreatedAt)
        VALUES (@PremiumPlanId, @CustomBrandingFeatureId, 1, SYSUTCDATETIME());
    ELSE
        UPDATE dbo.PlanFeatures SET IsIncluded = 1
        WHERE SaasPlanId = @PremiumPlanId AND FeatureId = @CustomBrandingFeatureId;
END

/* Ensure Premium does not include website builder (downgrade target from Premium Pro). */
DECLARE @WebsiteBuilderFeatureId INT = (SELECT FeatureId FROM dbo.SystemFeatures WHERE FeatureCode = N'WEBSITE_BUILDER');
IF @PremiumPlanId IS NOT NULL AND @WebsiteBuilderFeatureId IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.PlanFeatures WHERE SaasPlanId = @PremiumPlanId AND FeatureId = @WebsiteBuilderFeatureId)
        UPDATE dbo.PlanFeatures SET IsIncluded = 0
        WHERE SaasPlanId = @PremiumPlanId AND FeatureId = @WebsiteBuilderFeatureId;
END
GO

/* Disable runtime white-label branding when the new plan no longer includes WHITE_LABEL.
   Website builder data is intentionally preserved — access is enforced in API/UI. */
CREATE OR ALTER PROCEDURE dbo.sp_SyncGymEntitlementsAfterPlanChange
    @GymId UNIQUEIDENTIFIER,
    @SaasPlanId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @HasWhiteLabel BIT = 0;

    IF EXISTS (
        SELECT 1
        FROM dbo.PlanFeatures pf
        INNER JOIN dbo.SystemFeatures f ON f.FeatureId = pf.FeatureId
        WHERE pf.SaasPlanId = @SaasPlanId
          AND pf.IsIncluded = 1
          AND f.FeatureCode = N'WHITE_LABEL'
          AND f.IsActive = 1)
        SET @HasWhiteLabel = 1;

    IF @HasWhiteLabel = 0
       AND EXISTS (SELECT 1 FROM dbo.WhiteLabelSettings WHERE GymId = @GymId AND IsWhiteLabelEnabled = 1)
        EXEC dbo.sp_SetWhiteLabelEnabled @GymId, @IsWhiteLabelEnabled = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_UpdateSubscriptionPlan
    @GymId UNIQUEIDENTIFIER,
    @SaasPlanId INT,
    @BillingCycle NVARCHAR(20) = NULL,
    @PricingOptionId INT = NULL,
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
    DECLARE @DurationValue INT = NULL;
    DECLARE @DurationUnit NVARCHAR(20) = NULL;
    DECLARE @ResolvedBillingCycle NVARCHAR(20) = @BillingCycle;

    SELECT @PlanName = PlanName FROM dbo.SaasSubscriptionPlans WHERE SaasPlanId = @SaasPlanId;
    IF @PlanName IS NULL THROW 50051, 'Plan not found.', 1;

    IF @PricingOptionId IS NOT NULL
    BEGIN
        SELECT
            @DurationValue = po.DurationValue,
            @DurationUnit = po.DurationUnit,
            @ResolvedBillingCycle = COALESCE(@BillingCycle, CONCAT(po.DurationValue, N' ', po.DurationUnit))
        FROM dbo.PlanPricingOptions po
        WHERE po.PricingOptionId = @PricingOptionId AND po.SaasPlanId = @SaasPlanId;

        IF @DurationValue IS NULL THROW 50052, 'Pricing option not found for plan.', 1;
    END

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

    DECLARE @PeriodEnd DATETIME2;

    IF @DurationValue IS NOT NULL AND @DurationUnit IS NOT NULL
        SET @PeriodEnd = dbo.fn_CalculateSubscriptionPeriodEnd(@PeriodStart, @DurationValue, @DurationUnit);
    ELSE
        SET @PeriodEnd = dbo.fn_Saas_CalculatePeriodEnd(@PeriodStart, COALESCE(@ResolvedBillingCycle, N'Monthly'));

    UPDATE dbo.GymSubscriptions
    SET Status = N'Cancelled', CancelledAt = @Now, UpdatedAt = @Now
    WHERE GymId = @GymId
      AND Status IN (N'Trial', N'Active', N'PastDue', N'Expired')
      AND CancelledAt IS NULL;

    INSERT INTO dbo.GymSubscriptions
        (GymId, SaasPlanId, PlanName, StartDate, EndDate, Amount, Status, BillingCycle,
         PricingOptionId, DurationValue, DurationUnit,
         TrialEndsAt, CurrentPeriodStart, CurrentPeriodEnd, GraceEndsAt,
         RazorpayOrderId, RazorpayPaymentId, RazorpaySubscriptionId, CreatedAt)
    VALUES
        (@GymId, @SaasPlanId, @PlanName, CAST(@PeriodStart AS DATE), CAST(@PeriodEnd AS DATE), @Amount, N'Active',
         COALESCE(@ResolvedBillingCycle, N'Monthly'),
         @PricingOptionId, @DurationValue, @DurationUnit,
         NULL, @PeriodStart, @PeriodEnd, DATEADD(DAY, @GracePeriodDays, @PeriodEnd),
         @RazorpayOrderId, @RazorpayPaymentId, @RazorpaySubscriptionId, @Now);

    UPDATE dbo.Gyms
    SET IsActive = 1, UpdatedAt = @Now
    WHERE GymId = @GymId;

    EXEC dbo.sp_SyncGymEntitlementsAfterPlanChange @GymId, @SaasPlanId;
END
GO
