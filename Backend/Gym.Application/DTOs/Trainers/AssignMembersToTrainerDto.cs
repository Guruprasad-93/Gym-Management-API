namespace Gym.Application.DTOs.Trainers;

public class AssignMembersToTrainerDto
{
    public IReadOnlyList<int> MemberIds { get; set; } = Array.Empty<int>();
}
