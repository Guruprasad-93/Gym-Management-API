namespace Gym.Application.DTOs.Users;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? GymId { get; set; }
    public DateTime CreatedDate { get; set; }
}
