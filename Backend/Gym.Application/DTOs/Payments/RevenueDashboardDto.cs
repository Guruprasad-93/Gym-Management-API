namespace Gym.Application.DTOs.Payments;

public class RevenueDashboardDto
{
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int ExpiredMemberships { get; set; }
    public int ActiveMemberships { get; set; }
    public int PendingRenewals { get; set; }
}

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}
