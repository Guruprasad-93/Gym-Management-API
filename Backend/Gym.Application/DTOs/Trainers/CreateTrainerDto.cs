namespace Gym.Application.DTOs.Trainers;

public class CreateTrainerDto
{
    /// <summary>Optional for SuperAdmin when creating in a specific gym.</summary>
    public Guid? GymId { get; set; }

    public Guid? UserId { get; set; }

    /// <summary>Required when <see cref="UserId"/> is not set – creates linked user with Trainer role.</summary>
    public string? Name { get; set; }

    public string? LoginIdentifier { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Specialization { get; set; }
    public string? Bio { get; set; }
}
