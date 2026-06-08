namespace Gym.Domain.Entities;

public class Member : BaseEntity
{
    public Guid GymId { get; private set; }
    public Guid UserId { get; private set; }
    public int? TrainerId { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? Gender { get; private set; }
    public string? Phone { get; private set; }
    public string? EmergencyContact { get; private set; }
    public DateOnly JoinDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Gym Gym { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public Trainer? Trainer { get; private set; }

    private Member() { }

    public static Member Create(
        Guid gymId,
        Guid userId,
        DateOnly joinDate,
        int? trainerId = null) =>
        new()
        {
            GymId = gymId,
            UserId = userId,
            TrainerId = trainerId,
            JoinDate = joinDate,
            CreatedAt = DateTime.UtcNow
        };
}
