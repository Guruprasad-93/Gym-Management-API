/*
  Fix sp_GetTrainerDashboard: DietPlans/WorkoutPlans have no TrainerId column.
  Count active plan assignments for members assigned to the trainer instead.
*/

CREATE OR ALTER PROCEDURE dbo.sp_GetTrainerDashboard
    @TrainerId INT,
    @GymId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (
            SELECT 1 FROM dbo.Trainers t
            WHERE t.TrainerId = @TrainerId
              AND (@GymId IS NULL OR t.GymId = @GymId))
            THROW 50032, 'Trainer not found.', 1;

        DECLARE @TrainerGymId UNIQUEIDENTIFIER =
            (SELECT GymId FROM dbo.Trainers WHERE TrainerId = @TrainerId);

        SELECT
            @TrainerId AS TrainerId,
            (SELECT COUNT(*)
             FROM dbo.Members m
             WHERE m.TrainerId = @TrainerId
               AND m.IsActive = 1
               AND m.IsDeleted = 0) AS AssignedActiveMembers,
            (SELECT COUNT(*)
             FROM dbo.Members m
             WHERE m.TrainerId = @TrainerId
               AND m.IsActive = 0
               AND m.IsDeleted = 0) AS AssignedInactiveMembers,
            (SELECT COUNT(*)
             FROM dbo.Members m
             WHERE m.GymId = @TrainerGymId
               AND m.TrainerId IS NULL
               AND m.IsActive = 1
               AND m.IsDeleted = 0) AS UnassignedMembersInGym,
            (SELECT COUNT(*)
             FROM dbo.AssignedDietPlans adp
             INNER JOIN dbo.Members m ON m.MemberId = adp.MemberId
             WHERE m.TrainerId = @TrainerId
               AND adp.IsActive = 1
               AND m.IsDeleted = 0) AS ActiveDietPlans,
            (SELECT COUNT(*)
             FROM dbo.AssignedWorkoutPlans awp
             INNER JOIN dbo.Members m ON m.MemberId = awp.MemberId
             WHERE m.TrainerId = @TrainerId
               AND awp.IsActive = 1
               AND m.IsDeleted = 0) AS ActiveWorkoutPlans;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
