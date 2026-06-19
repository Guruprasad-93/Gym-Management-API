using Gym.Application.DTOs.DietPlans;

namespace Gym.Application.Interfaces;

public interface IDietPlanRepository
{
    Task SeedCategoriesAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DietCategoryDto>> GetCategoriesAsync(Guid gymId, bool includeInactive, CancellationToken cancellationToken = default);
    Task<DietCategoryDto> CreateCategoryAsync(Guid gymId, CreateDietCategoryDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DietPlanListDto>> GetPlansAsync(Guid? gymId, bool includeInactive, int? categoryId, string? search, CancellationToken cancellationToken = default);
    Task<DietPlanDetailDto?> GetPlanByIdAsync(int dietPlanId, Guid? gymId, CancellationToken cancellationToken = default);
    Task<int> CreatePlanAsync(Guid gymId, CreateDietPlanDto dto, Guid? createdByUserId, CancellationToken cancellationToken = default);
    Task UpdatePlanAsync(int dietPlanId, Guid gymId, UpdateDietPlanDto dto, CancellationToken cancellationToken = default);
    Task DeletePlanAsync(int dietPlanId, Guid gymId, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int dietPlanId, Guid gymId, bool isActive, CancellationToken cancellationToken = default);
    Task<int> ClonePlanAsync(int sourcePlanId, Guid gymId, string? newPlanName, Guid? createdByUserId, CancellationToken cancellationToken = default);
    Task<int> AssignToMemberAsync(Guid gymId, AssignDietPlanDto dto, Guid? assignedByUserId, CancellationToken cancellationToken = default);
    Task UnassignAsync(int assignedDietPlanId, Guid gymId, CancellationToken cancellationToken = default);
    Task<MemberDietPlanViewDto> GetMemberDietAsync(int memberId, Guid? gymId, bool activeOnly, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberDietAssignmentDto>> GetMemberAssignmentsAsync(int memberId, Guid gymId, CancellationToken cancellationToken = default);
}
