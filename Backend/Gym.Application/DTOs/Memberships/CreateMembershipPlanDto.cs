namespace Gym.Application.DTOs.Memberships;

public class CreateMembershipPlanDto
{
    public Guid? GymId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int DurationInMonths { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
}
