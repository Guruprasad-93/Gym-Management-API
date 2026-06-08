namespace Gym.Infrastructure.Persistence.Models;

internal sealed class TrainerRow
{
    public int TrainerId { get; set; }
    public Guid GymId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? Specialization { get; set; }
    public string? Bio { get; set; }
    public bool IsActive { get; set; }
    public int AssignedMemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

internal sealed class TrainerDashboardRow
{
    public int TrainerId { get; set; }
    public int AssignedActiveMembers { get; set; }
    public int AssignedInactiveMembers { get; set; }
    public int UnassignedMembersInGym { get; set; }
    public int ActiveDietPlans { get; set; }
    public int ActiveWorkoutPlans { get; set; }
}
