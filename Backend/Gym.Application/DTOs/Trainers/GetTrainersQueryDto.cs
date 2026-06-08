using Gym.Application.DTOs.Common;

namespace Gym.Application.DTOs.Trainers;

public class GetTrainersQueryDto
{
    public Guid? GymId { get; set; }
    public string? Search { get; set; }
    public bool IncludeInactive { get; set; }
    public PagedRequestDto Paging { get; set; } = new();
}
