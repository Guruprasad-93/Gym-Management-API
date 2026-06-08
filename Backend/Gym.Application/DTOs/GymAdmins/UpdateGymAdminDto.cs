namespace Gym.Application.DTOs.GymAdmins;

public class UpdateGymAdminDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid GymId { get; set; }
}
