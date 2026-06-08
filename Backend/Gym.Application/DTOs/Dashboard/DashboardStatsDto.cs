namespace Gym.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalGyms { get; set; }
    public int ActiveGyms { get; set; }
    public int TotalMembers { get; set; }
    public int ActiveMembers { get; set; }
    public int MembersWithTrainer { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ExpiredMemberships { get; set; }
    public int ActiveMemberships { get; set; }
    public int PendingRenewals { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int TotalTrainers { get; set; }
}
