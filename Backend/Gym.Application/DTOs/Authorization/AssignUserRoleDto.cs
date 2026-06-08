using System.ComponentModel.DataAnnotations;

namespace Gym.Application.DTOs.Authorization;

public class AssignUserRoleDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public int RoleId { get; set; }
}
