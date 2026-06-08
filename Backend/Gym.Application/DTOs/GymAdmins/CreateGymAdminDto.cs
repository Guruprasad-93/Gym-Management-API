namespace Gym.Application.DTOs.GymAdmins;

public class CreateGymAdminDto
{
    public Guid GymId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Optional. When empty and <see cref="GenerateTemporaryPassword"/> is true, a temporary password is generated.</summary>
    public string? Password { get; set; }

    /// <summary>When true, generates a temporary password if <see cref="Password"/> is empty.</summary>
    public bool GenerateTemporaryPassword { get; set; } = true;
}
