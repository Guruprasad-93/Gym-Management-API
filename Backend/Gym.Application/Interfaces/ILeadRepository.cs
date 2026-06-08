using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Leads;

namespace Gym.Application.Interfaces;

public interface ILeadRepository
{
    Task<LeadDto> CreateAsync(Guid gymId, Guid? createdBy, CreateLeadDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int leadId, Guid gymId, UpdateLeadDto dto, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(int leadId, Guid gymId, string status, CancellationToken cancellationToken = default);
    Task DeleteAsync(int leadId, Guid gymId, CancellationToken cancellationToken = default);
    Task<LeadDto?> GetByIdAsync(int leadId, Guid? gymId, int? trainerId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<LeadDto>> GetPagedAsync(Guid? gymId, int? trainerId, LeadSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeadDto>> SearchAllAsync(Guid? gymId, int? trainerId, LeadSearchQueryDto query, CancellationToken cancellationToken = default);
    Task AssignTrainerAsync(int leadId, Guid gymId, int trainerId, CancellationToken cancellationToken = default);
    Task MarkConvertedAsync(int leadId, Guid gymId, int memberId, CancellationToken cancellationToken = default);
    Task<int> ScheduleTrialAsync(int leadId, Guid gymId, int? trainerId, DateTime trialDate, Guid? createdBy, CancellationToken cancellationToken = default);
    Task RecordTrialFeedbackAsync(int trialId, Guid gymId, RecordTrialFeedbackDto dto, CancellationToken cancellationToken = default);
    Task<int> CreateFollowUpAsync(int leadId, Guid gymId, Guid? createdBy, CreateLeadFollowUpDto dto, CancellationToken cancellationToken = default);
    Task UpdateFollowUpAsync(int followUpId, Guid gymId, UpdateLeadFollowUpDto dto, CancellationToken cancellationToken = default);
    Task<int> CreateActivityAsync(int leadId, Guid gymId, string activityType, string description, Guid? createdBy, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeadActivityDto>> GetActivitiesAsync(int leadId, Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeadFollowUpDto>> GetFollowUpsAsync(int leadId, Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeadTrialDto>> GetTrialsAsync(int leadId, Guid gymId, CancellationToken cancellationToken = default);
    Task<LeadDashboardDto> GetDashboardAsync(Guid? gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NamedCountDto>> GetSourceAnalyticsAsync(Guid? gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NamedCountDto>> GetStatusAnalyticsAsync(Guid? gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeadConversionPointDto>> GetConversionReportAsync(Guid? gymId, int months, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainerLeadPerformanceDto>> GetTrainerPerformanceAsync(Guid? gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeadFollowUpDto>> GetPendingFollowUpsAsync(Guid? gymId, int? trainerId, int topN, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeadTrialDto>> GetTodaysTrialsAsync(Guid? gymId, int? trainerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeadReminderCandidateDto>> GetReminderCandidatesAsync(int hoursAhead, CancellationToken cancellationToken = default);
}

public interface ILeadService
{
    Task<LeadDto> CreateAsync(CreateLeadDto dto, CancellationToken cancellationToken = default);
    Task<LeadDto> UpdateAsync(int id, UpdateLeadDto dto, CancellationToken cancellationToken = default);
    Task<LeadDto> UpdateStatusAsync(int id, UpdateLeadStatusDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<LeadDetailDto> GetDetailAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<PagedResultDto<LeadDto>> GetPagedAsync(LeadSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<LeadDto> AssignTrainerAsync(AssignTrainerToLeadDto dto, CancellationToken cancellationToken = default);
    Task<LeadTrialDto> ScheduleTrialAsync(ScheduleTrialDto dto, CancellationToken cancellationToken = default);
    Task RecordTrialFeedbackAsync(RecordTrialFeedbackDto dto, CancellationToken cancellationToken = default);
    Task<LeadFollowUpDto> CreateFollowUpAsync(CreateLeadFollowUpDto dto, CancellationToken cancellationToken = default);
    Task<LeadFollowUpDto> UpdateFollowUpAsync(int followUpId, UpdateLeadFollowUpDto dto, Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<ConvertLeadResultDto> ConvertToMemberAsync(ConvertLeadToMemberDto dto, CancellationToken cancellationToken = default);
    Task<LeadDashboardDto> GetDashboardAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<LeadAnalyticsDto> GetAnalyticsAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeadFollowUpDto>> GetPendingFollowUpsAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeadTrialDto>> GetTodaysTrialsAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<byte[]> ExportPdfAsync(string reportType, LeadSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<byte[]> ExportExcelAsync(string reportType, LeadSearchQueryDto query, CancellationToken cancellationToken = default);
    Task ProcessRemindersAsync(CancellationToken cancellationToken = default);
}

public interface ILeadReportExporter
{
    byte[] ExportLeadSummaryPdf(IReadOnlyList<LeadDto> leads, string title);
    byte[] ExportLeadSummaryExcel(IReadOnlyList<LeadDto> leads, string title);
    byte[] ExportConversionReportPdf(LeadAnalyticsDto analytics, string title);
    byte[] ExportConversionReportExcel(LeadAnalyticsDto analytics, string title);
    byte[] ExportFollowUpReportExcel(IReadOnlyList<LeadFollowUpDto> followUps, string title);
}
