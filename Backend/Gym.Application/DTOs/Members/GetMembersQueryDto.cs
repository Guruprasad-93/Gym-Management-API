using Gym.Application.DTOs.Common;

namespace Gym.Application.DTOs.Members;

public class GetMembersQueryDto
{
    public Guid? GymId { get; set; }
    public string? Search { get; set; }
    public bool IncludeInactive { get; set; }
    public PagedRequestDto Paging { get; set; } = new();
}
