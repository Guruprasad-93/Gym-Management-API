using Gym.Application.DTOs.WorkoutPlans;

namespace Gym.Application.Interfaces;

public interface IWorkoutPlanRepository
{
    Task<IReadOnlyList<ExerciseCategoryDto>> GetCategoriesAsync(Guid gymId, bool includeInactive, CancellationToken cancellationToken = default);
    Task<ExerciseCategoryDto> CreateCategoryAsync(Guid gymId, CreateExerciseCategoryDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExerciseDto>> GetExercisesAsync(Guid? gymId, bool includeInactive, int? categoryId, string? muscleGroup, string? search, CancellationToken cancellationToken = default);
    Task<ExerciseDto?> GetExerciseByIdAsync(int exerciseId, Guid? gymId, CancellationToken cancellationToken = default);
    Task<int> CreateExerciseAsync(Guid gymId, CreateExerciseDto dto, CancellationToken cancellationToken = default);
    Task UpdateExerciseAsync(int exerciseId, Guid gymId, UpdateExerciseDto dto, CancellationToken cancellationToken = default);
    Task DeleteExerciseAsync(int exerciseId, Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkoutPlanListDto>> GetPlansAsync(Guid? gymId, bool includeInactive, string? search, CancellationToken cancellationToken = default);
    Task<WorkoutPlanDetailDto?> GetPlanByIdAsync(int workoutPlanId, Guid? gymId, CancellationToken cancellationToken = default);
    Task<int> CreatePlanAsync(Guid gymId, CreateWorkoutPlanDto dto, Guid? createdByUserId, CancellationToken cancellationToken = default);
    Task UpdatePlanAsync(int workoutPlanId, Guid gymId, UpdateWorkoutPlanDto dto, CancellationToken cancellationToken = default);
    Task DeletePlanAsync(int workoutPlanId, Guid gymId, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int workoutPlanId, Guid gymId, bool isActive, CancellationToken cancellationToken = default);
    Task<int> ClonePlanAsync(int sourcePlanId, Guid gymId, string? newPlanName, Guid? createdByUserId, CancellationToken cancellationToken = default);
    Task<int> AssignToMemberAsync(Guid gymId, AssignWorkoutPlanDto dto, Guid? assignedByUserId, CancellationToken cancellationToken = default);
    Task UnassignAsync(int assignedWorkoutPlanId, Guid gymId, CancellationToken cancellationToken = default);
    Task<MemberWorkoutPlanViewDto> GetMemberWorkoutAsync(int memberId, Guid? gymId, bool activeOnly, CancellationToken cancellationToken = default);
    Task<int> UpsertProgressAsync(Guid gymId, UpdateWorkoutProgressDto dto, CancellationToken cancellationToken = default);
}
