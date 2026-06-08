namespace Gym.Application.DTOs.Leads;

public class LeadDto
{
    public int Id { get; set; }
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

public class CreateLeadDto
{
    public Guid? GymId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public string? Address { get; set; }
    public string LeadSource { get; set; } = string.Empty;
    public int? InterestedPlanId { get; set; }
    public string? Status { get; set; }
    public int? AssignedTrainerId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateLeadDto
{
    public Guid? GymId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public string? Address { get; set; }
    public string LeadSource { get; set; } = string.Empty;
    public int? InterestedPlanId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? AssignedTrainerId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateLeadStatusDto
{
    public Guid? GymId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AssignTrainerToLeadDto
{
    public int LeadId { get; set; }
    public Guid? GymId { get; set; }
    public int TrainerId { get; set; }
}

public class ScheduleTrialDto
{
    public int LeadId { get; set; }
    public Guid? GymId { get; set; }
    public int? TrainerId { get; set; }
    public DateTime TrialDate { get; set; }
}

public class RecordTrialFeedbackDto
{
    public int TrialId { get; set; }
    public int LeadId { get; set; }
    public Guid? GymId { get; set; }
    public string AttendanceStatus { get; set; } = string.Empty;
    public string? Feedback { get; set; }
    public int? Rating { get; set; }
}

public class CreateLeadFollowUpDto
{
    public int LeadId { get; set; }
    public Guid? GymId { get; set; }
    public DateTime FollowUpDate { get; set; }
    public string FollowUpType { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
}

public class UpdateLeadFollowUpDto
{
    public DateTime FollowUpDate { get; set; }
    public string FollowUpType { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? NextFollowUpDate { get; set; }
}

public class ConvertLeadToMemberDto
{
    public int LeadId { get; set; }
    public Guid? GymId { get; set; }
    public int MembershipPlanId { get; set; }
    public DateOnly StartDate { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class ConvertLeadResultDto
{
    public LeadDto Lead { get; set; } = new();
    public int MemberId { get; set; }
    public int MembershipId { get; set; }
    public string? TemporaryPassword { get; set; }
}

public class LeadDetailDto
{
    public LeadDto Lead { get; set; } = new();
    public IReadOnlyList<LeadActivityDto> Activities { get; set; } = Array.Empty<LeadActivityDto>();
    public IReadOnlyList<LeadFollowUpDto> FollowUps { get; set; } = Array.Empty<LeadFollowUpDto>();
    public IReadOnlyList<LeadTrialDto> Trials { get; set; } = Array.Empty<LeadTrialDto>();
}

public class LeadActivityDto
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public Guid? CreatedBy { get; set; }
}

public class LeadFollowUpDto
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public DateTime FollowUpDate { get; set; }
    public string FollowUpType { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? NextFollowUpDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? LeadName { get; set; }
    public string? MobileNumber { get; set; }
}

public class LeadTrialDto
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public int? TrainerId { get; set; }
    public string? TrainerName { get; set; }
    public DateTime TrialDate { get; set; }
    public string AttendanceStatus { get; set; } = string.Empty;
    public string? Feedback { get; set; }
    public int? Rating { get; set; }
    public string? LeadName { get; set; }
    public string? MobileNumber { get; set; }
}

public class LeadDashboardDto
{
    public int TotalLeads { get; set; }
    public int NewLeadsToday { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal TrialConversionRate { get; set; }
    public int LostLeads { get; set; }
    public int PendingFollowUps { get; set; }
    public int TodaysTrials { get; set; }
    public int ConvertedLeads { get; set; }
}

public class LeadAnalyticsDto
{
    public LeadDashboardDto Dashboard { get; set; } = new();
    public IReadOnlyList<NamedCountDto> LeadsBySource { get; set; } = Array.Empty<NamedCountDto>();
    public IReadOnlyList<NamedCountDto> LeadsByStatus { get; set; } = Array.Empty<NamedCountDto>();
    public IReadOnlyList<LeadConversionPointDto> MonthlyConversions { get; set; } = Array.Empty<LeadConversionPointDto>();
    public IReadOnlyList<TrainerLeadPerformanceDto> TrainerPerformance { get; set; } = Array.Empty<TrainerLeadPerformanceDto>();
    public IReadOnlyList<LeadFollowUpDto> PendingFollowUps { get; set; } = Array.Empty<LeadFollowUpDto>();
    public IReadOnlyList<LeadTrialDto> TodaysTrials { get; set; } = Array.Empty<LeadTrialDto>();
}

public class NamedCountDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class LeadConversionPointDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public int Conversions { get; set; }
    public int NewLeads { get; set; }
}

public class TrainerLeadPerformanceDto
{
    public int? TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int TotalLeads { get; set; }
    public int ConvertedLeads { get; set; }
}

public class LeadSearchQueryDto
{
    public Guid? GymId { get; set; }
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? LeadSource { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortColumn { get; set; } = "CreatedDate";
    public string SortDirection { get; set; } = "DESC";
}

public class LeadReminderCandidateDto
{
    public string ReminderType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public Guid GymId { get; set; }
    public int LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
}
