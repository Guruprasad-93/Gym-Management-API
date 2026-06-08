namespace Gym.Application.DTOs.Trainers;

public class TrainerDashboardDto
{
    public int TrainerId { get; set; }
    public int AssignedActiveMembers { get; set; }
    public int AssignedInactiveMembers { get; set; }
    public int UnassignedMembersInGym { get; set; }
    public int ActiveDietPlans { get; set; }
    public int ActiveWorkoutPlans { get; set; }
}
