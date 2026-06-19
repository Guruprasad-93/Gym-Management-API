namespace Gym.Application.DTOs.GymAdmins;

public class GymAdminDto
{
    public Guid UserId { get; set; }
    public Guid GymId { get; set; }
    public string GymName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LoginIdentifier { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime CreatedDate { get; set; }
}
