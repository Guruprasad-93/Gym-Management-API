namespace Gym.Application.DTOs.WorkoutPlans;

public class ExerciseCategoryDto
{
    public int ExerciseCategoryId { get; set; }
    public Guid GymId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateExerciseCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ExerciseDto
{
    public int ExerciseId { get; set; }
    public Guid GymId { get; set; }
    public int? ExerciseCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string? MuscleGroup { get; set; }
    public string? Difficulty { get; set; }
    public string? Instructions { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateExerciseDto
{
    public Guid? GymId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int? ExerciseCategoryId { get; set; }
    public string? MuscleGroup { get; set; }
    public string? Difficulty { get; set; }
    public string? Instructions { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateExerciseDto
{
    public string ExerciseName { get; set; } = string.Empty;
    public int? ExerciseCategoryId { get; set; }
    public string? MuscleGroup { get; set; }
    public string? Difficulty { get; set; }
    public string? Instructions { get; set; }
    public bool IsActive { get; set; } = true;
}

public class WorkoutPlanListDto
{
    public int WorkoutPlanId { get; set; }
    public Guid GymId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Goal { get; set; }
    public int? DurationWeeks { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ExerciseCount { get; set; }
    public int ActiveAssignmentCount { get; set; }
}

public class WorkoutPlanExerciseDto
{
    public int WorkoutPlanExerciseId { get; set; }
    public int WorkoutPlanId { get; set; }
    public int DayNumber { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string? MuscleGroup { get; set; }
    public string? Difficulty { get; set; }
    public string? CategoryName { get; set; }
    public int? Sets { get; set; }
    public string? Reps { get; set; }
    public string? Weight { get; set; }
    public int? RestSeconds { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
    public int? MemberWorkoutProgressId { get; set; }
    public bool? IsCompleted { get; set; }
    public decimal? CompletionPercentage { get; set; }
    public string? TrainerNotes { get; set; }
    public string? MemberNotes { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class WorkoutPlanExerciseInputDto
{
    public int DayNumber { get; set; }
    public int ExerciseId { get; set; }
    public int? Sets { get; set; }
    public string? Reps { get; set; }
    public string? Weight { get; set; }
    public int? RestSeconds { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}

public class WorkoutPlanDetailDto
{
    public int WorkoutPlanId { get; set; }
    public Guid GymId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Goal { get; set; }
    public int? DurationWeeks { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyList<WorkoutPlanExerciseDto> Exercises { get; set; } = [];
}

public class CreateWorkoutPlanDto
{
    public Guid? GymId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Goal { get; set; }
    public int? DurationWeeks { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<WorkoutPlanExerciseInputDto> Exercises { get; set; } = [];
}

public class UpdateWorkoutPlanDto
{
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Goal { get; set; }
    public int? DurationWeeks { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<WorkoutPlanExerciseInputDto> Exercises { get; set; } = [];
}

public class CloneWorkoutPlanDto
{
    public string? NewPlanName { get; set; }
}

public class AssignWorkoutPlanDto
{
    public int MemberId { get; set; }
    public int WorkoutPlanId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Notes { get; set; }
    public bool DeactivatePrevious { get; set; } = true;
}

public class MemberWorkoutPlanViewDto
{
    public int? AssignedWorkoutPlanId { get; set; }
    public int MemberId { get; set; }
    public string? MemberName { get; set; }
    public int? WorkoutPlanId { get; set; }
    public string? PlanName { get; set; }
    public string? PlanDescription { get; set; }
    public string? Goal { get; set; }
    public int? DurationWeeks { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? AssignmentNotes { get; set; }
    public bool IsActive { get; set; }
    public decimal OverallCompletionPercentage { get; set; }
    public IReadOnlyList<WorkoutPlanExerciseDto> Exercises { get; set; } = [];
}

public class UpdateWorkoutProgressDto
{
    public int MemberId { get; set; }
    public int AssignedWorkoutPlanId { get; set; }
    public int WorkoutPlanExerciseId { get; set; }
    public bool? IsCompleted { get; set; }
    public decimal? CompletionPercentage { get; set; }
    public string? TrainerNotes { get; set; }
    public string? MemberNotes { get; set; }
}
