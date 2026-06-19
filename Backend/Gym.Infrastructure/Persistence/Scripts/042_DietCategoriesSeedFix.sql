/*
  New gyms created after 015_DietPlanModule.sql never received default diet categories.
  Adds sp_SeedDietCategories and backfills gyms that have none.
*/

CREATE OR ALTER PROCEDURE dbo.sp_SeedDietCategories
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.DietCategories (GymId, CategoryName, Description)
    SELECT @GymId, v.CategoryName, v.Description
    FROM (VALUES
        (N'Weight Loss', N'Calorie deficit focused plans'),
        (N'Muscle Gain', N'High protein bulking plans'),
        (N'Maintenance', N'Balanced maintenance nutrition'),
        (N'Vegetarian', N'Plant-based meal plans'),
        (N'Keto', N'Low carbohydrate plans')
    ) v(CategoryName, Description)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.DietCategories dc
        WHERE dc.GymId = @GymId AND dc.CategoryName = v.CategoryName);
END
GO

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
    SELECT 1 FROM dbo.DietCategories dc WHERE dc.GymId = g.GymId)
  AND NOT EXISTS (
    SELECT 1 FROM dbo.DietCategories dc
    WHERE dc.GymId = g.GymId AND dc.CategoryName = v.CategoryName);
GO
