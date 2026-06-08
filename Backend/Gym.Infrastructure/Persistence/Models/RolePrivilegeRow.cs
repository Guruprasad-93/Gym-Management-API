namespace Gym.Infrastructure.Persistence.Models;

internal sealed class RolePrivilegeRow
{
    public int RolePrivilegeId { get; set; }
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public int PrivilegeId { get; set; }
    public string? PrivilegeName { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
