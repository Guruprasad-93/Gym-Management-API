namespace Gym.Application.DTOs.Trainers;

public class UpdateTrainerDto
{
    public string? Specialization { get; set; }
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;
}
