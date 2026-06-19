namespace Gym.Infrastructure.Persistence.Models;

internal sealed class UserRow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LoginIdentifier { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public Guid? GymId { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int TokenVersion { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }
}
