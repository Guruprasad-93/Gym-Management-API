namespace Gym.Application.DTOs.Members;

public class MemberProgressDto
{
    public string ProgressType { get; set; } = string.Empty;
    public DateOnly RecordedDate { get; set; }
    public string? Detail { get; set; }
    public decimal? WeightKg { get; set; }
    public DateTime CreatedAt { get; set; }
}
