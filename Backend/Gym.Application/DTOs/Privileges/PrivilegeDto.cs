namespace Gym.Application.DTOs.Privileges;

public class PrivilegeDto
{
    public int Id { get; set; }
    public string PrivilegeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
