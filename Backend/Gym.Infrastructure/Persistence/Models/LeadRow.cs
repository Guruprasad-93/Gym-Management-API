namespace Gym.Infrastructure.Persistence.Models;

internal sealed class LeadRow
{
    public int LeadId { get; set; }
    public Guid GymId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public string? Address { get; set; }
    public string LeadSource { get; set; } = string.Empty;
    public int? InterestedPlanId { get; set; }
    public string? InterestedPlanName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? AssignedTrainerId { get; set; }
    public string? AssignedTrainerName { get; set; }
    public string? Notes { get; set; }
    public int? ConvertedMemberId { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

internal sealed class LeadActivityRow
{
    public int ActivityId { get; set; }
    public int LeadId { get; set; }
    public Guid GymId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public Guid? CreatedBy { get; set; }
}

internal sealed class LeadFollowUpRow
{
    public int FollowUpId { get; set; }
    public int LeadId { get; set; }
    public Guid GymId { get; set; }
    public DateTime FollowUpDate { get; set; }
    public string FollowUpType { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? NextFollowUpDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public string? LeadName { get; set; }
    public string? MobileNumber { get; set; }
}

internal sealed class LeadTrialRow
{
    public int TrialId { get; set; }
    public int LeadId { get; set; }
    public Guid GymId { get; set; }
    public int? TrainerId { get; set; }
    public string? TrainerName { get; set; }
    public DateTime TrialDate { get; set; }
    public string AttendanceStatus { get; set; } = string.Empty;
    public string? Feedback { get; set; }
    public int? Rating { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? LeadName { get; set; }
    public string? MobileNumber { get; set; }
}

internal sealed class LeadDashboardRow
{
    public int TotalLeads { get; set; }
    public int NewLeadsToday { get; set; }
    public int ConvertedLeads { get; set; }
    public int LostLeads { get; set; }
    public int PendingFollowUps { get; set; }
    public int TodaysTrials { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal TrialConversionRate { get; set; }
}

internal sealed class NamedCountRow2
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

internal sealed class LeadConversionRow
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public int Conversions { get; set; }
    public int NewLeads { get; set; }
}

internal sealed class TrainerLeadPerfRow
{
    public int? TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int TotalLeads { get; set; }
    public int ConvertedLeads { get; set; }
}

internal sealed class LeadReminderRow
{
    public string ReminderType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public Guid GymId { get; set; }
    public int LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
}
