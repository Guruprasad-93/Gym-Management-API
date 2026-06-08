namespace Gym.Infrastructure.Persistence.Models;

internal sealed class PayableMembershipRow
{
    public int MembershipId { get; set; }
    public Guid GymId { get; set; }
    public int MemberId { get; set; }
    public int MembershipPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal PlanPrice { get; set; }
    public int DurationInMonths { get; set; }
}
