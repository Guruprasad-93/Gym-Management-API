using System.ComponentModel.DataAnnotations;

namespace Gym.Application.DTOs.Roles;

public class UpdateRoleDto
{
    [Required]
    [MaxLength(100)]
    public string RoleName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
