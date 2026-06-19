namespace Gym.Application.DTOs.Members;

public class UpdateMemberDto
{
    public string? FullName { get; set; }
    public string? LoginIdentifier { get; set; }
    public string? Email { get; set; }
    public int? TrainerId { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public bool IsActive { get; set; } = true;
}
