namespace Gym.Application.DTOs.Memberships;

public class CreateMembershipDto
{
    public int MemberId { get; set; }
    public int MembershipPlanId { get; set; }
    public DateOnly StartDate { get; set; }
    public string? Notes { get; set; }
}
