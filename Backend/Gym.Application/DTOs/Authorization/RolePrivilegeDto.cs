namespace Gym.Application.DTOs.Authorization;

public class RolePrivilegeDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int PrivilegeId { get; set; }
    public string PrivilegeName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
