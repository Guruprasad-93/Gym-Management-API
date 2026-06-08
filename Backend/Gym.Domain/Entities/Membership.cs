using Gym.Domain.Enums;

namespace Gym.Domain.Entities;

public class Membership : BaseEntity
{
    public Guid GymId { get; private set; }
    public int MemberId { get; private set; }
    public int MembershipPlanId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string Status { get; private set; } = MembershipStatus.Active;

    public Gym Gym { get; private set; } = null!;
    public Member Member { get; private set; } = null!;
    public MembershipPlan MembershipPlan { get; private set; } = null!;

    private Membership() { }

    public static Membership Create(
        Guid gymId,
        int memberId,
        int membershipPlanId,
        DateOnly startDate,
        DateOnly endDate) =>
        new()
        {
            GymId = gymId,
            MemberId = memberId,
            MembershipPlanId = membershipPlanId,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow
        };
}
