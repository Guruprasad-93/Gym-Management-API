using Gym.Application.DTOs.DietPlans;

namespace Gym.Application.Interfaces;

public interface IDietPlanService
{
    Task<IReadOnlyList<DietCategoryDto>> GetCategoriesAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<DietCategoryDto> CreateCategoryAsync(CreateDietCategoryDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DietPlanListDto>> GetPlansAsync(bool includeInactive, int? categoryId, string? search, CancellationToken cancellationToken = default);
    Task<DietPlanDetailDto> GetPlanByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DietPlanDetailDto> CreatePlanAsync(CreateDietPlanDto dto, CancellationToken cancellationToken = default);
    Task<DietPlanDetailDto> UpdatePlanAsync(int id, UpdateDietPlanDto dto, CancellationToken cancellationToken = default);
    Task DeletePlanAsync(int id, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default);
    Task<DietPlanDetailDto> ClonePlanAsync(int id, CloneDietPlanDto dto, CancellationToken cancellationToken = default);
    Task<MemberDietPlanViewDto> AssignToMemberAsync(AssignDietPlanDto dto, CancellationToken cancellationToken = default);
    Task UnassignAsync(int assignedDietPlanId, CancellationToken cancellationToken = default);
    Task<MemberDietPlanViewDto> GetMemberDietAsync(int memberId, CancellationToken cancellationToken = default);
    Task<MemberDietPlanViewDto> GetCurrentMemberDietAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberDietAssignmentDto>> GetMemberAssignmentsAsync(int memberId, CancellationToken cancellationToken = default);
    Task<DietPlanDetailDto> GetPlanForExportAsync(int id, CancellationToken cancellationToken = default);
}
