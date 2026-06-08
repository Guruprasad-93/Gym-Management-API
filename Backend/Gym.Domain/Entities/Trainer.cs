namespace Gym.Domain.Entities;

public class Trainer : BaseEntity
{
    public Guid GymId { get; private set; }
    public Guid? UserId { get; private set; }
    public string? Specialization { get; private set; }
    public string? Bio { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Gym Gym { get; private set; } = null!;
    public User? User { get; private set; }

    private Trainer() { }

    public static Trainer Create(Guid gymId, Guid? userId = null, string? specialization = null, string? bio = null) =>
        new()
        {
            GymId = gymId,
            UserId = userId,
            Specialization = specialization?.Trim(),
            Bio = bio?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
}
