namespace Gym.Application.DTOs.Memberships;

public class MembershipPlanResponseDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationInMonths { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}
