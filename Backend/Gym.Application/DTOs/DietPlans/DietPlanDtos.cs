namespace Gym.Application.DTOs.DietPlans;

public class DietCategoryDto
{
    public int DietCategoryId { get; set; }
    public Guid GymId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDietCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class DietPlanListDto
{
    public int DietPlanId { get; set; }
    public Guid GymId { get; set; }
    public int? DietCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? TargetCalories { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ItemCount { get; set; }
    public int ActiveAssignmentCount { get; set; }
}

public class DietPlanItemDto
{
    public int DietPlanItemId { get; set; }
    public int DietPlanId { get; set; }
    public string MealTime { get; set; } = string.Empty;
    public string FoodName { get; set; } = string.Empty;
    public string? Quantity { get; set; }
    public decimal? Calories { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}

public class DietPlanDetailDto
{
    public int DietPlanId { get; set; }
    public Guid GymId { get; set; }
    public int? DietCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? TargetCalories { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyList<DietPlanItemDto> Items { get; set; } = [];
}

public class DietPlanItemInputDto
{
    public string MealTime { get; set; } = string.Empty;
    public string FoodName { get; set; } = string.Empty;
    public string? Quantity { get; set; }
    public decimal? Calories { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}

public class CreateDietPlanDto
{
    public Guid? GymId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DietCategoryId { get; set; }
    public int? TargetCalories { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<DietPlanItemInputDto> Items { get; set; } = [];
}

public class UpdateDietPlanDto
{
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DietCategoryId { get; set; }
    public int? TargetCalories { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<DietPlanItemInputDto> Items { get; set; } = [];
}

public class CloneDietPlanDto
{
    public string? NewPlanName { get; set; }
}

public class AssignDietPlanDto
{
    public int MemberId { get; set; }
    public int DietPlanId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Notes { get; set; }
    public bool DeactivatePrevious { get; set; } = true;
}

public class MemberDietAssignmentDto
{
    public int AssignedDietPlanId { get; set; }
    public int DietPlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class MemberDietPlanViewDto
{
    public int? AssignedDietPlanId { get; set; }
    public int MemberId { get; set; }
    public string? MemberName { get; set; }
    public int? DietPlanId { get; set; }
    public string? PlanName { get; set; }
    public string? PlanDescription { get; set; }
    public int? TargetCalories { get; set; }
    public string? CategoryName { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? AssignmentNotes { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<DietPlanItemDto> Items { get; set; } = [];
}
