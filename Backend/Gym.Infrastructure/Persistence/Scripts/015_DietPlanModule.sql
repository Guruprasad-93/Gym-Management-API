/*
  Diet Plan Management Module
  Tables: DietCategories, DietPlans, DietPlanItems, AssignedDietPlans
*/

/* Replace legacy MVP DietPlans (003) so module schema and procedures can be created */
IF OBJECT_ID(N'dbo.DietPlans', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.DietPlans', 'PlanName') IS NULL
BEGIN
    DECLARE @dropDietPlanFks NVARCHAR(MAX) = N'';
    SELECT @dropDietPlanFks += N'ALTER TABLE '
        + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(parent_object_id))
        + N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
    FROM sys.foreign_keys
    WHERE referenced_object_id = OBJECT_ID(N'dbo.DietPlans');
    IF @dropDietPlanFks <> N''
        EXEC sys.sp_executesql @dropDietPlanFks;

    DROP TABLE dbo.DietPlans;
END
GO

-- Diet Categories (per gym)
IF OBJECT_ID(N'dbo.DietCategories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DietCategories
    (
        DietCategoryId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        CategoryName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_DietCategories_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_DietCategories_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_DietCategories_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE
    );
    CREATE INDEX IX_DietCategories_GymId ON dbo.DietCategories (GymId);
END
GO

IF OBJECT_ID(N'dbo.DietPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DietPlans
    (
        DietPlanId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        DietCategoryId INT NULL,
        PlanName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        TargetCalories INT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_DietPlans_IsActive DEFAULT (1),
        IsDeleted BIT NOT NULL CONSTRAINT DF_DietPlans_IsDeleted DEFAULT (0),
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_DietPlans_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_DietPlans_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId) ON DELETE CASCADE,
        CONSTRAINT FK_DietPlans_Categories FOREIGN KEY (DietCategoryId) REFERENCES dbo.DietCategories (DietCategoryId)
    );
    CREATE INDEX IX_DietPlans_GymId ON dbo.DietPlans (GymId);
    CREATE INDEX IX_DietPlans_Category ON dbo.DietPlans (DietCategoryId);
END
GO

IF OBJECT_ID(N'dbo.DietPlanItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DietPlanItems
    (
        DietPlanItemId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        DietPlanId INT NOT NULL,
        MealTime NVARCHAR(50) NOT NULL,
        FoodName NVARCHAR(200) NOT NULL,
        Quantity NVARCHAR(100) NULL,
        Calories DECIMAL(10, 2) NULL,
        Notes NVARCHAR(500) NULL,
        SortOrder INT NOT NULL CONSTRAINT DF_DietPlanItems_SortOrder DEFAULT (0),
        CONSTRAINT FK_DietPlanItems_Plans FOREIGN KEY (DietPlanId) REFERENCES dbo.DietPlans (DietPlanId) ON DELETE CASCADE
    );
    CREATE INDEX IX_DietPlanItems_PlanId ON dbo.DietPlanItems (DietPlanId);
END
GO

IF OBJECT_ID(N'dbo.AssignedDietPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AssignedDietPlans
    (
        AssignedDietPlanId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        GymId UNIQUEIDENTIFIER NOT NULL,
        MemberId INT NOT NULL,
        DietPlanId INT NOT NULL,
        AssignedByUserId UNIQUEIDENTIFIER NULL,
        StartDate DATE NOT NULL,
        EndDate DATE NULL,
        Notes NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_AssignedDietPlans_IsActive DEFAULT (1),
        AssignedAt DATETIME2 NOT NULL CONSTRAINT DF_AssignedDietPlans_AssignedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_AssignedDietPlans_Gyms FOREIGN KEY (GymId) REFERENCES dbo.Gyms (GymId),
        CONSTRAINT FK_AssignedDietPlans_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members (MemberId),
        CONSTRAINT FK_AssignedDietPlans_Plans FOREIGN KEY (DietPlanId) REFERENCES dbo.DietPlans (DietPlanId)
    );
    CREATE INDEX IX_AssignedDietPlans_Member ON dbo.AssignedDietPlans (GymId, MemberId, IsActive);
END
GO

-- Seed default categories for existing gyms
INSERT INTO dbo.DietCategories (GymId, CategoryName, Description)
SELECT g.GymId, v.CategoryName, v.Description
FROM dbo.Gyms g
CROSS APPLY (VALUES
    (N'Weight Loss', N'Calorie deficit focused plans'),
    (N'Muscle Gain', N'High protein bulking plans'),
    (N'Maintenance', N'Balanced maintenance nutrition'),
    (N'Vegetarian', N'Plant-based meal plans'),
    (N'Keto', N'Low carbohydrate plans')
) v(CategoryName, Description)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.DietCategories dc
    WHERE dc.GymId = g.GymId AND dc.CategoryName = v.CategoryName
);
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetDietCategories
    @GymId UNIQUEIDENTIFIER,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DietCategoryId, GymId, CategoryName, Description, IsActive, CreatedAt
    FROM dbo.DietCategories
    WHERE GymId = @GymId
      AND (@IncludeInactive = 1 OR IsActive = 1)
    ORDER BY CategoryName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateDietCategory
    @GymId UNIQUEIDENTIFIER,
    @CategoryName NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @DietCategoryId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.DietCategories (GymId, CategoryName, Description)
    VALUES (@GymId, @CategoryName, @Description);
    SET @DietCategoryId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetDietPlans
    @GymId UNIQUEIDENTIFIER = NULL,
    @IncludeInactive BIT = 0,
    @CategoryId INT = NULL,
    @Search NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        dp.DietPlanId,
        dp.GymId,
        dp.DietCategoryId,
        dc.CategoryName,
        dp.PlanName,
        dp.Description,
        dp.TargetCalories,
        dp.IsActive,
        dp.CreatedAt,
        dp.UpdatedAt,
        (SELECT COUNT(*) FROM dbo.DietPlanItems i WHERE i.DietPlanId = dp.DietPlanId) AS ItemCount,
        (SELECT COUNT(*) FROM dbo.AssignedDietPlans a WHERE a.DietPlanId = dp.DietPlanId AND a.IsActive = 1) AS ActiveAssignmentCount
    FROM dbo.DietPlans dp
    LEFT JOIN dbo.DietCategories dc ON dc.DietCategoryId = dp.DietCategoryId
    WHERE dp.IsDeleted = 0
      AND (dp.GymId = @GymId)
      AND (@IncludeInactive = 1 OR dp.IsActive = 1)
      AND (@CategoryId IS NULL OR dp.DietCategoryId = @CategoryId)
      AND (@Search IS NULL OR dp.PlanName LIKE N'%' + @Search + N'%' OR dp.Description LIKE N'%' + @Search + N'%')
    ORDER BY dp.PlanName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetDietPlanById
    @DietPlanId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        dp.DietPlanId,
        dp.GymId,
        dp.DietCategoryId,
        dc.CategoryName,
        dp.PlanName,
        dp.Description,
        dp.TargetCalories,
        dp.IsActive,
        dp.CreatedAt,
        dp.UpdatedAt
    FROM dbo.DietPlans dp
    LEFT JOIN dbo.DietCategories dc ON dc.DietCategoryId = dp.DietCategoryId
    WHERE dp.DietPlanId = @DietPlanId
      AND dp.IsDeleted = 0
      AND (dp.GymId = @GymId);

    SELECT
        DietPlanItemId,
        DietPlanId,
        MealTime,
        FoodName,
        Quantity,
        Calories,
        Notes,
        SortOrder
    FROM dbo.DietPlanItems
    WHERE DietPlanId = @DietPlanId
    ORDER BY SortOrder, MealTime, DietPlanItemId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateDietPlan
    @GymId UNIQUEIDENTIFIER,
    @PlanName NVARCHAR(200),
    @Description NVARCHAR(MAX) = NULL,
    @DietCategoryId INT = NULL,
    @TargetCalories INT = NULL,
    @IsActive BIT = 1,
    @CreatedByUserId UNIQUEIDENTIFIER = NULL,
    @DietPlanId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @DietCategoryId IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM dbo.DietCategories WHERE DietCategoryId = @DietCategoryId AND GymId = @GymId)
        THROW 50001, 'Invalid diet category for this gym.', 1;

    INSERT INTO dbo.DietPlans (GymId, DietCategoryId, PlanName, Description, TargetCalories, IsActive, CreatedByUserId)
    VALUES (@GymId, @DietCategoryId, @PlanName, @Description, @TargetCalories, @IsActive, @CreatedByUserId);
    SET @DietPlanId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateDietPlan
    @DietPlanId INT,
    @GymId UNIQUEIDENTIFIER,
    @PlanName NVARCHAR(200),
    @Description NVARCHAR(MAX) = NULL,
    @DietCategoryId INT = NULL,
    @TargetCalories INT = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.DietPlans WHERE DietPlanId = @DietPlanId AND GymId = @GymId AND IsDeleted = 0)
        THROW 50002, 'Diet plan not found.', 1;

    IF @DietCategoryId IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM dbo.DietCategories WHERE DietCategoryId = @DietCategoryId AND GymId = @GymId)
        THROW 50001, 'Invalid diet category for this gym.', 1;

    UPDATE dbo.DietPlans
    SET PlanName = @PlanName,
        Description = @Description,
        DietCategoryId = @DietCategoryId,
        TargetCalories = @TargetCalories,
        IsActive = @IsActive,
        UpdatedAt = SYSUTCDATETIME()
    WHERE DietPlanId = @DietPlanId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteDietPlan
    @DietPlanId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.AssignedDietPlans WHERE DietPlanId = @DietPlanId AND IsActive = 1)
        THROW 50003, 'Cannot delete a diet plan with active member assignments.', 1;

    UPDATE dbo.DietPlans
    SET IsDeleted = 1, IsActive = 0, UpdatedAt = SYSUTCDATETIME()
    WHERE DietPlanId = @DietPlanId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SetDietPlanActive
    @DietPlanId INT,
    @GymId UNIQUEIDENTIFIER,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.DietPlans
    SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
    WHERE DietPlanId = @DietPlanId AND GymId = @GymId AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ReplaceDietPlanItems
    @DietPlanId INT,
    @GymId UNIQUEIDENTIFIER,
    @ItemsJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.DietPlans WHERE DietPlanId = @DietPlanId AND GymId = @GymId AND IsDeleted = 0)
        THROW 50002, 'Diet plan not found.', 1;

    DELETE FROM dbo.DietPlanItems WHERE DietPlanId = @DietPlanId;

    IF @ItemsJson IS NOT NULL AND LEN(LTRIM(@ItemsJson)) > 2
    BEGIN
        INSERT INTO dbo.DietPlanItems (DietPlanId, MealTime, FoodName, Quantity, Calories, Notes, SortOrder)
        SELECT
            @DietPlanId,
            j.MealTime,
            j.FoodName,
            j.Quantity,
            j.Calories,
            j.Notes,
            ISNULL(j.SortOrder, 0)
        FROM OPENJSON(@ItemsJson)
        WITH (
            MealTime NVARCHAR(50) '$.mealTime',
            FoodName NVARCHAR(200) '$.foodName',
            Quantity NVARCHAR(100) '$.quantity',
            Calories DECIMAL(10, 2) '$.calories',
            Notes NVARCHAR(500) '$.notes',
            SortOrder INT '$.sortOrder'
        ) j;
    END

    UPDATE dbo.DietPlans SET UpdatedAt = SYSUTCDATETIME() WHERE DietPlanId = @DietPlanId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CloneDietPlan
    @SourceDietPlanId INT,
    @GymId UNIQUEIDENTIFIER,
    @NewPlanName NVARCHAR(200) = NULL,
    @CreatedByUserId UNIQUEIDENTIFIER = NULL,
    @NewDietPlanId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Name NVARCHAR(200);
    SELECT @Name = ISNULL(@NewPlanName, PlanName + N' (Copy)')
    FROM dbo.DietPlans
    WHERE DietPlanId = @SourceDietPlanId AND GymId = @GymId AND IsDeleted = 0;

    IF @Name IS NULL
        THROW 50002, 'Source diet plan not found.', 1;

    INSERT INTO dbo.DietPlans (GymId, DietCategoryId, PlanName, Description, TargetCalories, IsActive, CreatedByUserId)
    SELECT GymId, DietCategoryId, @Name, Description, TargetCalories, 1, @CreatedByUserId
    FROM dbo.DietPlans WHERE DietPlanId = @SourceDietPlanId;

    SET @NewDietPlanId = SCOPE_IDENTITY();

    INSERT INTO dbo.DietPlanItems (DietPlanId, MealTime, FoodName, Quantity, Calories, Notes, SortOrder)
    SELECT @NewDietPlanId, MealTime, FoodName, Quantity, Calories, Notes, SortOrder
    FROM dbo.DietPlanItems WHERE DietPlanId = @SourceDietPlanId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_AssignDietPlanToMember
    @GymId UNIQUEIDENTIFIER,
    @MemberId INT,
    @DietPlanId INT,
    @AssignedByUserId UNIQUEIDENTIFIER = NULL,
    @StartDate DATE,
    @EndDate DATE = NULL,
    @Notes NVARCHAR(500) = NULL,
    @DeactivatePrevious BIT = 1,
    @AssignedDietPlanId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Members m WHERE m.MemberId = @MemberId AND m.GymId = @GymId AND m.IsDeleted = 0)
        THROW 50004, 'Member not found.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.DietPlans dp WHERE dp.DietPlanId = @DietPlanId AND dp.GymId = @GymId AND dp.IsDeleted = 0 AND dp.IsActive = 1)
        THROW 50002, 'Diet plan not found or inactive.', 1;

    IF @DeactivatePrevious = 1
        UPDATE dbo.AssignedDietPlans
        SET IsActive = 0
        WHERE GymId = @GymId AND MemberId = @MemberId AND IsActive = 1;

    INSERT INTO dbo.AssignedDietPlans (GymId, MemberId, DietPlanId, AssignedByUserId, StartDate, EndDate, Notes, IsActive)
    VALUES (@GymId, @MemberId, @DietPlanId, @AssignedByUserId, @StartDate, @EndDate, @Notes, 1);

    SET @AssignedDietPlanId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_UnassignDietPlan
    @AssignedDietPlanId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.AssignedDietPlans
    SET IsActive = 0
    WHERE AssignedDietPlanId = @AssignedDietPlanId AND GymId = @GymId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberAssignedDietPlan
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER = NULL,
    @ActiveOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1)
        ad.AssignedDietPlanId,
        ad.GymId,
        ad.MemberId,
        u.Name AS MemberName,
        ad.DietPlanId,
        dp.PlanName,
        dp.Description AS PlanDescription,
        dp.TargetCalories,
        dc.CategoryName,
        ad.StartDate,
        ad.EndDate,
        ad.Notes AS AssignmentNotes,
        ad.IsActive,
        ad.AssignedAt
    FROM dbo.AssignedDietPlans ad
    INNER JOIN dbo.DietPlans dp ON dp.DietPlanId = ad.DietPlanId
    INNER JOIN dbo.Members m ON m.MemberId = ad.MemberId
    INNER JOIN dbo.Users u ON u.Id = m.UserId
    LEFT JOIN dbo.DietCategories dc ON dc.DietCategoryId = dp.DietCategoryId
    WHERE ad.MemberId = @MemberId
      AND (ad.GymId = @GymId)
      AND (@ActiveOnly = 0 OR ad.IsActive = 1)
    ORDER BY ad.AssignedAt DESC;

    SELECT
        i.DietPlanItemId,
        i.DietPlanId,
        i.MealTime,
        i.FoodName,
        i.Quantity,
        i.Calories,
        i.Notes,
        i.SortOrder
    FROM dbo.DietPlanItems i
    INNER JOIN (
        SELECT TOP (1) ad.DietPlanId
        FROM dbo.AssignedDietPlans ad
        WHERE ad.MemberId = @MemberId
          AND (ad.GymId = @GymId)
          AND (@ActiveOnly = 0 OR ad.IsActive = 1)
        ORDER BY ad.AssignedAt DESC
    ) x ON x.DietPlanId = i.DietPlanId
    ORDER BY i.SortOrder, i.MealTime, i.DietPlanItemId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMemberDietAssignments
    @MemberId INT,
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        ad.AssignedDietPlanId,
        ad.DietPlanId,
        dp.PlanName,
        ad.StartDate,
        ad.EndDate,
        ad.IsActive,
        ad.AssignedAt
    FROM dbo.AssignedDietPlans ad
    INNER JOIN dbo.DietPlans dp ON dp.DietPlanId = ad.DietPlanId
    WHERE ad.MemberId = @MemberId AND ad.GymId = @GymId
    ORDER BY ad.AssignedAt DESC;
END
GO
