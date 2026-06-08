namespace Gym.Infrastructure.Persistence.Models;

internal sealed class GymAdminRow
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid GymId { get; set; }
    public string? GymName { get; set; }
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime CreatedDate { get; set; }
}
