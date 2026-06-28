-- 063_Phase2FeatureSubscription.sql
-- Payment SP updates (PricingOptionId), platform plan CRUD, catalog APIs, feature route resolution.

CREATE OR ALTER PROCEDURE dbo.sp_Feature_GetApiRoutes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        f.FeatureCode,
        far.RoutePrefix,
        far.HttpMethods
    FROM dbo.FeatureApiRoutes far
    INNER JOIN dbo.SystemFeatures f ON f.FeatureId = far.FeatureId
    WHERE f.IsActive = 1
    ORDER BY LEN(far.RoutePrefix) DESC, far.RoutePrefix;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_CreatePendingPayment
    @GymId UNIQUEIDENTIFIER,
    @GymSubscriptionId INT,
    @SaasPlanId INT,
    @Amount DECIMAL(18, 2),
    @BillingCycle NVARCHAR(20) = NULL,
    @PricingOptionId INT = NULL,
    @RazorpayOrderId NVARCHAR(100),
    @SaasPaymentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @PricingOptionId IS NOT NULL
    BEGIN
        SELECT @BillingCycle = COALESCE(
            @BillingCycle,
            CONCAT(po.DurationValue, N' ', po.DurationUnit),
            N'Monthly')
        FROM dbo.PlanPricingOptions po
        WHERE po.PricingOptionId = @PricingOptionId;
    END

    INSERT INTO dbo.SaasSubscriptionPayments
        (GymId, GymSubscriptionId, SaasPlanId, Amount, BillingCycle, PricingOptionId, RazorpayOrderId, Status)
    VALUES
        (@GymId, @GymSubscriptionId, @SaasPlanId, @Amount, @BillingCycle, @PricingOptionId, @RazorpayOrderId, N'Pending');

    SET @SaasPaymentId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetPendingPayment
    @SaasPaymentId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        p.SaasPaymentId,
        p.GymId,
        p.GymSubscriptionId,
        p.SaasPlanId,
        p.Amount,
        p.BillingCycle,
        p.PricingOptionId,
        p.RazorpayOrderId,
        p.Status,
        sp.PlanName,
        sp.IsTrialPlan,
        po.DurationValue,
        po.DurationUnit,
        po.Price AS PricingOptionPrice
    FROM dbo.SaasSubscriptionPayments p
    INNER JOIN dbo.SaasSubscriptionPlans sp ON sp.SaasPlanId = p.SaasPlanId
    LEFT JOIN dbo.PlanPricingOptions po ON po.PricingOptionId = p.PricingOptionId
    WHERE p.SaasPaymentId = @SaasPaymentId;
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
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetAllPlans
    @IncludeInactive BIT = 0,
    @PublicOnly BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        sp.SaasPlanId,
        sp.PlanCode,
        sp.PlanName,
        sp.Description,
        sp.IsTrialPlan,
        sp.IsPublic,
        COALESCE(pq.MaxMembers, sp.MaxMembers) AS MaxMembers,
        COALESCE(pq.MaxTrainers, sp.MaxTrainers) AS MaxTrainers,
        COALESCE(pq.MaxBranches, 1) AS MaxBranches,
        COALESCE(pq.MaxStorageGB, CASE WHEN COALESCE(pq.StorageLimitMb, sp.StorageLimitMb) <= 0 THEN 0 ELSE (COALESCE(pq.StorageLimitMb, sp.StorageLimitMb) + 1023) / 1024 END) AS MaxStorageGB,
        COALESCE(pq.MaxSmsPerMonth, 0) AS MaxSmsPerMonth,
        COALESCE(pq.MaxWhatsappMessages, COALESCE(pq.WhatsAppNotificationLimit, sp.WhatsAppNotificationLimit)) AS MaxWhatsappMessages,
        COALESCE(pq.StorageLimitMb, sp.StorageLimitMb) AS StorageLimitMb,
        COALESCE(pq.WhatsAppNotificationLimit, sp.WhatsAppNotificationLimit) AS WhatsAppNotificationLimit,
        sp.MonthlyPrice,
        sp.QuarterlyPrice,
        sp.HalfYearlyPrice,
        sp.YearlyPrice,
        sp.TrialDays,
        sp.IsActive,
        sp.SortOrder
    FROM dbo.SaasSubscriptionPlans sp
    LEFT JOIN dbo.PlanQuotas pq ON pq.SaasPlanId = sp.SaasPlanId
    WHERE (@IncludeInactive = 1 OR sp.IsActive = 1)
      AND (@PublicOnly = 0 OR sp.IsPublic = 1)
    ORDER BY sp.SortOrder, sp.PlanName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_Platform_GetPlanDetail
    @SaasPlanId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        sp.SaasPlanId,
        sp.PlanCode,
        sp.PlanName,
        sp.Description,
        sp.IsTrialPlan,
        sp.IsPublic,
        sp.TrialDays,
        sp.IsActive,
        sp.SortOrder,
        sp.MonthlyPrice,
        sp.QuarterlyPrice,
        sp.HalfYearlyPrice,
        sp.YearlyPrice
    FROM dbo.SaasSubscriptionPlans sp
    WHERE sp.SaasPlanId = @SaasPlanId;

    EXEC dbo.sp_PlanQuota_GetByPlanId @SaasPlanId = @SaasPlanId;
    EXEC dbo.sp_PlanFeature_GetByPlanId @SaasPlanId = @SaasPlanId;
    EXEC dbo.sp_PlanPricing_GetByPlanId @SaasPlanId = @SaasPlanId, @IncludeInactive = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_Platform_CreatePlan
    @PlanCode NVARCHAR(50),
    @PlanName NVARCHAR(100),
    @Description NVARCHAR(1000) = NULL,
    @IsTrialPlan BIT = 0,
    @IsPublic BIT = 1,
    @TrialDays INT = 0,
    @SortOrder INT = 0,
    @SaasPlanId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS (SELECT 1 FROM dbo.SaasSubscriptionPlans WHERE PlanCode = @PlanCode)
        THROW 50053, 'Plan code already exists.', 1;

    INSERT INTO dbo.SaasSubscriptionPlans
        (PlanCode, PlanName, Description, IsTrialPlan, IsPublic, TrialDays, SortOrder, IsActive, CreatedAt)
    VALUES
        (@PlanCode, @PlanName, @Description, @IsTrialPlan, @IsPublic, @TrialDays, @SortOrder, 1, SYSUTCDATETIME());

    SET @SaasPlanId = SCOPE_IDENTITY();

    INSERT INTO dbo.PlanQuotas (SaasPlanId, MaxMembers, MaxTrainers, MaxBranches, MaxStorageGB, MaxSmsPerMonth, MaxWhatsappMessages, StorageLimitMb, WhatsAppNotificationLimit)
    VALUES (@SaasPlanId, 100, 10, 1, 1, 0, 0, 1024, 0);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_Platform_UpdatePlan
    @SaasPlanId INT,
    @PlanCode NVARCHAR(50),
    @PlanName NVARCHAR(100),
    @Description NVARCHAR(1000) = NULL,
    @IsTrialPlan BIT = 0,
    @IsPublic BIT = 1,
    @TrialDays INT = 0,
    @SortOrder INT = 0,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.SaasSubscriptionPlans WHERE PlanCode = @PlanCode AND SaasPlanId <> @SaasPlanId)
        THROW 50053, 'Plan code already exists.', 1;

    UPDATE dbo.SaasSubscriptionPlans
    SET PlanCode = @PlanCode,
        PlanName = @PlanName,
        Description = @Description,
        IsTrialPlan = @IsTrialPlan,
        IsPublic = @IsPublic,
        TrialDays = @TrialDays,
        SortOrder = @SortOrder,
        IsActive = @IsActive,
        UpdatedAt = SYSUTCDATETIME()
    WHERE SaasPlanId = @SaasPlanId;

    IF @@ROWCOUNT = 0 THROW 50051, 'Plan not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_Platform_DeletePlan
    @SaasPlanId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.SaasSubscriptionPlans
    SET IsActive = 0, UpdatedAt = SYSUTCDATETIME()
    WHERE SaasPlanId = @SaasPlanId;

    IF @@ROWCOUNT = 0 THROW 50051, 'Plan not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PlanQuota_Upsert
    @SaasPlanId INT,
    @MaxMembers INT,
    @MaxTrainers INT,
    @MaxBranches INT,
    @MaxStorageGB INT,
    @MaxSmsPerMonth INT,
    @MaxWhatsappMessages INT
AS
BEGIN
    SET NOCOUNT ON;

    MERGE dbo.PlanQuotas AS t
    USING (SELECT @SaasPlanId AS SaasPlanId) AS s
    ON t.SaasPlanId = s.SaasPlanId
    WHEN MATCHED THEN
        UPDATE SET
            MaxMembers = @MaxMembers,
            MaxTrainers = @MaxTrainers,
            MaxBranches = @MaxBranches,
            MaxStorageGB = @MaxStorageGB,
            MaxSmsPerMonth = @MaxSmsPerMonth,
            MaxWhatsappMessages = @MaxWhatsappMessages,
            StorageLimitMb = CASE WHEN @MaxStorageGB <= 0 THEN 0 ELSE @MaxStorageGB * 1024 END,
            WhatsAppNotificationLimit = @MaxWhatsappMessages,
            UpdatedAt = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (SaasPlanId, MaxMembers, MaxTrainers, MaxBranches, MaxStorageGB, MaxSmsPerMonth, MaxWhatsappMessages, StorageLimitMb, WhatsAppNotificationLimit)
        VALUES (@SaasPlanId, @MaxMembers, @MaxTrainers, @MaxBranches, @MaxStorageGB, @MaxSmsPerMonth, @MaxWhatsappMessages,
                CASE WHEN @MaxStorageGB <= 0 THEN 0 ELSE @MaxStorageGB * 1024 END, @MaxWhatsappMessages);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PlanFeature_SetForPlan
    @SaasPlanId INT,
    @FeatureIds NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DELETE FROM dbo.PlanFeatures WHERE SaasPlanId = @SaasPlanId;

    IF @FeatureIds IS NULL OR LTRIM(RTRIM(@FeatureIds)) = N''
        RETURN;

    INSERT INTO dbo.PlanFeatures (SaasPlanId, FeatureId, IsIncluded)
    SELECT @SaasPlanId, CAST(value AS INT), 1
    FROM STRING_SPLIT(@FeatureIds, N',')
    WHERE LTRIM(RTRIM(value)) <> N'';
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PlanPricing_Create
    @SaasPlanId INT,
    @DurationValue INT,
    @DurationUnit NVARCHAR(20),
    @Price DECIMAL(18, 2),
    @DisplayLabel NVARCHAR(100) = NULL,
    @SortOrder INT = 0,
    @PricingOptionId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.PlanPricingOptions
        (SaasPlanId, DurationValue, DurationUnit, Price, Currency, DisplayLabel, IsActive, SortOrder)
    VALUES
        (@SaasPlanId, @DurationValue, @DurationUnit, @Price, N'INR', @DisplayLabel, 1, @SortOrder);
    SET @PricingOptionId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PlanPricing_Update
    @PricingOptionId INT,
    @DurationValue INT,
    @DurationUnit NVARCHAR(20),
    @Price DECIMAL(18, 2),
    @DisplayLabel NVARCHAR(100) = NULL,
    @SortOrder INT = 0,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.PlanPricingOptions
    SET DurationValue = @DurationValue,
        DurationUnit = @DurationUnit,
        Price = @Price,
        DisplayLabel = @DisplayLabel,
        SortOrder = @SortOrder,
        IsActive = @IsActive,
        UpdatedAt = SYSUTCDATETIME()
    WHERE PricingOptionId = @PricingOptionId;

    IF @@ROWCOUNT = 0 THROW 50054, 'Pricing option not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PlanPricing_Delete
    @PricingOptionId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.PlanPricingOptions
    SET IsActive = 0, UpdatedAt = SYSUTCDATETIME()
    WHERE PricingOptionId = @PricingOptionId;

    IF @@ROWCOUNT = 0 THROW 50054, 'Pricing option not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetPlanCatalog
    @PublicOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        sp.SaasPlanId,
        sp.PlanCode,
        sp.PlanName,
        sp.Description,
        sp.IsTrialPlan,
        sp.IsPublic,
        sp.TrialDays,
        sp.SortOrder,
        COALESCE(pq.MaxMembers, sp.MaxMembers) AS MaxMembers,
        COALESCE(pq.MaxTrainers, sp.MaxTrainers) AS MaxTrainers,
        COALESCE(pq.MaxBranches, 1) AS MaxBranches,
        COALESCE(pq.MaxStorageGB, 0) AS MaxStorageGB,
        COALESCE(pq.MaxSmsPerMonth, 0) AS MaxSmsPerMonth,
        COALESCE(pq.MaxWhatsappMessages, 0) AS MaxWhatsappMessages
    FROM dbo.SaasSubscriptionPlans sp
    LEFT JOIN dbo.PlanQuotas pq ON pq.SaasPlanId = sp.SaasPlanId
    WHERE sp.IsActive = 1
      AND (@PublicOnly = 0 OR sp.IsPublic = 1)
      AND sp.IsTrialPlan = 0
    ORDER BY sp.SortOrder, sp.PlanName;

    SELECT
        po.PricingOptionId,
        po.SaasPlanId,
        po.DurationValue,
        po.DurationUnit,
        po.Price,
        po.Currency,
        po.DisplayLabel,
        po.IsActive,
        po.SortOrder
    FROM dbo.PlanPricingOptions po
    INNER JOIN dbo.SaasSubscriptionPlans sp ON sp.SaasPlanId = po.SaasPlanId
    WHERE po.IsActive = 1
      AND sp.IsActive = 1
      AND (@PublicOnly = 0 OR sp.IsPublic = 1)
      AND sp.IsTrialPlan = 0
    ORDER BY po.SaasPlanId, po.SortOrder, po.DurationUnit, po.DurationValue;

    SELECT
        pf.SaasPlanId,
        f.FeatureId,
        f.FeatureCode,
        f.FeatureName,
        f.Category
    FROM dbo.PlanFeatures pf
    INNER JOIN dbo.SystemFeatures f ON f.FeatureId = pf.FeatureId
    INNER JOIN dbo.SaasSubscriptionPlans sp ON sp.SaasPlanId = pf.SaasPlanId
    WHERE pf.IsIncluded = 1
      AND f.IsActive = 1
      AND sp.IsActive = 1
      AND (@PublicOnly = 0 OR sp.IsPublic = 1)
    ORDER BY sp.SaasPlanId, f.Category, f.SortOrder;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetPlanById
    @SaasPlanId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        sp.SaasPlanId,
        sp.PlanCode,
        sp.PlanName,
        sp.Description,
        sp.IsTrialPlan,
        sp.IsPublic,
        COALESCE(pq.MaxMembers, sp.MaxMembers) AS MaxMembers,
        COALESCE(pq.MaxTrainers, sp.MaxTrainers) AS MaxTrainers,
        COALESCE(pq.MaxBranches, 1) AS MaxBranches,
        COALESCE(pq.MaxStorageGB, 0) AS MaxStorageGB,
        COALESCE(pq.MaxSmsPerMonth, 0) AS MaxSmsPerMonth,
        COALESCE(pq.MaxWhatsappMessages, COALESCE(pq.WhatsAppNotificationLimit, sp.WhatsAppNotificationLimit)) AS MaxWhatsappMessages,
        COALESCE(pq.StorageLimitMb, sp.StorageLimitMb) AS StorageLimitMb,
        COALESCE(pq.WhatsAppNotificationLimit, sp.WhatsAppNotificationLimit) AS WhatsAppNotificationLimit,
        sp.MonthlyPrice,
        sp.QuarterlyPrice,
        sp.HalfYearlyPrice,
        sp.YearlyPrice,
        sp.TrialDays,
        sp.IsActive,
        sp.SortOrder
    FROM dbo.SaasSubscriptionPlans sp
    LEFT JOIN dbo.PlanQuotas pq ON pq.SaasPlanId = sp.SaasPlanId
    WHERE sp.SaasPlanId = @SaasPlanId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Saas_GetPlanByCode
    @PlanCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        sp.SaasPlanId,
        sp.PlanCode,
        sp.PlanName,
        sp.Description,
        sp.IsTrialPlan,
        sp.IsPublic,
        COALESCE(pq.MaxMembers, sp.MaxMembers) AS MaxMembers,
        COALESCE(pq.MaxTrainers, sp.MaxTrainers) AS MaxTrainers,
        COALESCE(pq.MaxBranches, 1) AS MaxBranches,
        COALESCE(pq.MaxStorageGB, 0) AS MaxStorageGB,
        COALESCE(pq.MaxSmsPerMonth, 0) AS MaxSmsPerMonth,
        COALESCE(pq.MaxWhatsappMessages, COALESCE(pq.WhatsAppNotificationLimit, sp.WhatsAppNotificationLimit)) AS MaxWhatsappMessages,
        COALESCE(pq.StorageLimitMb, sp.StorageLimitMb) AS StorageLimitMb,
        COALESCE(pq.WhatsAppNotificationLimit, sp.WhatsAppNotificationLimit) AS WhatsAppNotificationLimit,
        sp.MonthlyPrice,
        sp.QuarterlyPrice,
        sp.HalfYearlyPrice,
        sp.YearlyPrice,
        sp.TrialDays,
        sp.IsActive,
        sp.SortOrder
    FROM dbo.SaasSubscriptionPlans sp
    LEFT JOIN dbo.PlanQuotas pq ON pq.SaasPlanId = sp.SaasPlanId
    WHERE sp.PlanCode = @PlanCode AND sp.IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_PlanPricing_GetById
    @PricingOptionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        PricingOptionId, SaasPlanId, DurationValue, DurationUnit, Price, Currency,
        DisplayLabel, IsActive, SortOrder
    FROM dbo.PlanPricingOptions
    WHERE PricingOptionId = @PricingOptionId;
END
GO
