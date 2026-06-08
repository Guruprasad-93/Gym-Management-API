namespace Gym.Application.DTOs.Trainers;

public class TrainerDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public Guid? UserId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Specialization { get; set; }
    public string? Bio { get; set; }
    public bool IsActive { get; set; }
    public int AssignedMemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
