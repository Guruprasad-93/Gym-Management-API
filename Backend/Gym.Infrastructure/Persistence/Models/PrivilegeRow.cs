namespace Gym.Infrastructure.Persistence.Models;

internal sealed class PrivilegeRow
{
    public int PrivilegeId { get; set; }
    public string PrivilegeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
