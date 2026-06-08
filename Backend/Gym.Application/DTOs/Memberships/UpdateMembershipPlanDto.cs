namespace Gym.Application.DTOs.Memberships;

public class UpdateMembershipPlanDto
{
    public string PlanName { get; set; } = string.Empty;
    public int DurationInMonths { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
