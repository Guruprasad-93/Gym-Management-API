using Gym.Application.DTOs.WorkoutPlans;

namespace Gym.Application.Interfaces;

public interface IWorkoutPlanService
{
    Task<IReadOnlyList<ExerciseCategoryDto>> GetCategoriesAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<ExerciseCategoryDto> CreateCategoryAsync(CreateExerciseCategoryDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExerciseDto>> GetExercisesAsync(bool includeInactive, int? categoryId, string? muscleGroup, string? search, CancellationToken cancellationToken = default);
    Task<ExerciseDto> GetExerciseByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto dto, CancellationToken cancellationToken = default);
    Task<ExerciseDto> UpdateExerciseAsync(int id, UpdateExerciseDto dto, CancellationToken cancellationToken = default);
    Task DeleteExerciseAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkoutPlanListDto>> GetPlansAsync(bool includeInactive, string? search, CancellationToken cancellationToken = default);
    Task<WorkoutPlanDetailDto> GetPlanByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<WorkoutPlanDetailDto> CreatePlanAsync(CreateWorkoutPlanDto dto, CancellationToken cancellationToken = default);
    Task<WorkoutPlanDetailDto> UpdatePlanAsync(int id, UpdateWorkoutPlanDto dto, CancellationToken cancellationToken = default);
    Task DeletePlanAsync(int id, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default);
    Task<WorkoutPlanDetailDto> ClonePlanAsync(int id, CloneWorkoutPlanDto dto, CancellationToken cancellationToken = default);
    Task<MemberWorkoutPlanViewDto> AssignToMemberAsync(AssignWorkoutPlanDto dto, CancellationToken cancellationToken = default);
    Task UnassignAsync(int assignedWorkoutPlanId, CancellationToken cancellationToken = default);
    Task<MemberWorkoutPlanViewDto> GetMemberWorkoutAsync(int memberId, CancellationToken cancellationToken = default);
    Task<MemberWorkoutPlanViewDto> GetCurrentMemberWorkoutAsync(CancellationToken cancellationToken = default);
    Task<WorkoutPlanExerciseDto> UpdateProgressAsync(UpdateWorkoutProgressDto dto, CancellationToken cancellationToken = default);
    Task<WorkoutPlanDetailDto> GetPlanForExportAsync(int id, CancellationToken cancellationToken = default);
}
