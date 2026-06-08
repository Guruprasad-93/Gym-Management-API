namespace Gym.Infrastructure.Persistence.Models;

internal sealed class MembershipPlanRow
{
    public int MembershipPlanId { get; set; }
    public Guid GymId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationInMonths { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

internal sealed class MembershipRow
{
    public int MembershipId { get; set; }
    public Guid GymId { get; set; }
    public int MemberId { get; set; }
    public int MembershipPlanId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal? Amount { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal PlanPrice { get; set; }
    public int DurationInMonths { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
}

internal sealed class InvoiceRow
{
    public int InvoiceId { get; set; }
    public Guid GymId { get; set; }
    public int PaymentId { get; set; }
    public int MemberId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string GymName { get; set; } = string.Empty;
    public string? GymAddress { get; set; }
    public string? GymPhone { get; set; }
    public string? GymEmail { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public string? MemberPhone { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public string? PaymentNotes { get; set; }
    public string? MembershipPlanName { get; set; }
}

internal sealed class RevenueDashboardRow
{
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int ExpiredMemberships { get; set; }
    public int ActiveMemberships { get; set; }
    public int PendingRenewals { get; set; }
}

internal sealed class MonthlyRevenueRow
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}
