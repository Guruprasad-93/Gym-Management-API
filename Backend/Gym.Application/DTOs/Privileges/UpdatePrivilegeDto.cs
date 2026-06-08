using System.ComponentModel.DataAnnotations;

namespace Gym.Application.DTOs.Privileges;

public class UpdatePrivilegeDto
{
    [Required]
    [MaxLength(100)]
    public string PrivilegeName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = "General";
}
