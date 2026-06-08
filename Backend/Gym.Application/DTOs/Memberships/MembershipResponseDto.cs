namespace Gym.Application.DTOs.Memberships;

public class MembershipResponseDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public int MembershipPlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal PlanPrice { get; set; }
    public int DurationInMonths { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal? Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

/// <summary>Backward-compatible alias.</summary>
public class MembershipDto : MembershipResponseDto;
