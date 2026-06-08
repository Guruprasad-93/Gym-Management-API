namespace Gym.Domain.Entities;

public class MembershipPlan : BaseEntity
{
    public Guid GymId { get; private set; }
    public string PlanName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DurationDays { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Gym Gym { get; private set; } = null!;

    private MembershipPlan() { }

    public static MembershipPlan Create(
        Guid gymId,
        string planName,
        int durationDays,
        decimal price,
        string? description = null) =>
        new()
        {
            GymId = gymId,
            PlanName = planName.Trim(),
            Description = description?.Trim(),
            DurationDays = durationDays,
            Price = price,
            CreatedAt = DateTime.UtcNow
        };
}
