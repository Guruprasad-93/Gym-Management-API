namespace Gym.Application.DTOs.GymAdmins;

public class CreateGymAdminResultDto
{
    public GymAdminDto Admin { get; set; } = null!;

    /// <summary>Plain-text temporary password (only returned when one was generated).</summary>
    public string? TemporaryPassword { get; set; }

    public string Message { get; set; } = string.Empty;
}
