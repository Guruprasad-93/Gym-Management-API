/*
  Ensure workout seed procedures exist and library seed does not rely on nested EXEC.
*/

CREATE OR ALTER PROCEDURE dbo.sp_SeedExerciseCategories
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ExerciseCategories (GymId, CategoryName, Description)
    SELECT @GymId, v.CategoryName, v.Description
    FROM (VALUES
        (N'Strength', N'Compound and isolation strength exercises'),
        (N'Cardio', N'Cardiovascular conditioning'),
        (N'Flexibility', N'Stretching and mobility'),
        (N'HIIT', N'High intensity interval training'),
        (N'Bodyweight', N'No equipment exercises')
    ) v(CategoryName, Description)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.ExerciseCategories ec
        WHERE ec.GymId = @GymId AND ec.CategoryName = v.CategoryName);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SeedExerciseLibrary
    @GymId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.ExerciseLibrary (GymId, ExerciseCategoryId, ExerciseName, MuscleGroup, Difficulty, Instructions, IsActive)
    SELECT @GymId, ec.ExerciseCategoryId, v.ExerciseName, v.MuscleGroup, v.Difficulty, v.Instructions, 1
    FROM (VALUES
        (N'Strength', N'Barbell Bench Press', N'Chest', N'Intermediate', N'Flat barbell bench press'),
        (N'Strength', N'Barbell Squat', N'Legs', N'Intermediate', N'Back squat with barbell'),
        (N'Strength', N'Romanian Deadlift', N'Hamstrings', N'Intermediate', N'Hip-hinge deadlift variation'),
        (N'Strength', N'Lat Pulldown', N'Back', N'Beginner', N'Cable lat pulldown'),
        (N'Strength', N'Dumbbell Shoulder Press', N'Shoulders', N'Beginner', N'Seated or standing shoulder press'),
        (N'Strength', N'Dumbbell Bicep Curl', N'Biceps', N'Beginner', N'Alternating or simultaneous curls'),
        (N'Strength', N'Tricep Rope Pushdown', N'Triceps', N'Beginner', N'Cable rope pushdown'),
        (N'Cardio', N'Treadmill Run', N'Cardio', N'Beginner', N'Steady-state or interval running'),
        (N'Cardio', N'Stationary Bike', N'Cardio', N'Beginner', N'Low-impact cycling'),
        (N'Flexibility', N'Full Body Stretch', N'Full Body', N'Beginner', N'General mobility routine'),
        (N'Bodyweight', N'Push-ups', N'Chest', N'Beginner', N'Standard push-up'),
        (N'Bodyweight', N'Bodyweight Squat', N'Legs', N'Beginner', N'Air squat'),
        (N'Bodyweight', N'Plank', N'Core', N'Beginner', N'Forearm plank hold'),
        (N'HIIT', N'Burpees', N'Full Body', N'Intermediate', N'Full burpee with jump'),
        (N'HIIT', N'Jump Rope', N'Cardio', N'Beginner', N'Continuous or interval skipping')
    ) v(CategoryName, ExerciseName, MuscleGroup, Difficulty, Instructions)
    INNER JOIN dbo.ExerciseCategories ec
        ON ec.GymId = @GymId AND ec.CategoryName = v.CategoryName
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.ExerciseLibrary e
        WHERE e.GymId = @GymId AND e.ExerciseName = v.ExerciseName AND e.IsDeleted = 0);
END
GO
