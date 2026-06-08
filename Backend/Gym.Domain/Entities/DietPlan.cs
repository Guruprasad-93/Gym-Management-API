namespace Gym.Domain.Entities;

public class DietPlan : BaseEntity
{
    public Guid GymId { get; private set; }
    public int MemberId { get; private set; }
    public int? TrainerId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Gym Gym { get; private set; } = null!;
    public Member Member { get; private set; } = null!;
    public Trainer? Trainer { get; private set; }

    private DietPlan() { }

    public static DietPlan Create(
        Guid gymId,
        int memberId,
        string title,
        DateOnly startDate,
        int? trainerId = null,
        string? description = null) =>
        new()
        {
            GymId = gymId,
            MemberId = memberId,
            TrainerId = trainerId,
            Title = title.Trim(),
            Description = description?.Trim(),
            StartDate = startDate,
            CreatedAt = DateTime.UtcNow
        };
}
