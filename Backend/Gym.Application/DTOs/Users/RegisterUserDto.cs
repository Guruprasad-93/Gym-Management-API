using System.ComponentModel.DataAnnotations;

namespace Gym.Application.DTOs.Users;

public class RegisterUserDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LoginIdentifier { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; set; }

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    public Guid? GymId { get; set; }
}
