using System.Data;
using Dapper;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Leads;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public LeadRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<LeadDto> CreateAsync(Guid gymId, Guid? createdBy, CreateLeadDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@FullName", dto.FullName.Trim());
        parameters.Add("@MobileNumber", dto.MobileNumber.Trim());
        parameters.Add("@Email", dto.Email?.Trim());
        parameters.Add("@Gender", dto.Gender);
        parameters.Add("@Age", dto.Age);
        parameters.Add("@Address", dto.Address);
        parameters.Add("@LeadSource", dto.LeadSource);
        parameters.Add("@InterestedPlanId", dto.InterestedPlanId);
        parameters.Add("@Status", dto.Status ?? "New");
        parameters.Add("@AssignedTrainerId", dto.AssignedTrainerId);
        parameters.Add("@Notes", dto.Notes);
        parameters.Add("@CreatedBy", createdBy);
        parameters.Add("@LeadId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var leadId = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CreateLead, parameters, "@LeadId", cancellationToken);
        return (await GetByIdAsync(leadId, gymId, null, cancellationToken))!;
    }

    public async Task UpdateAsync(int leadId, Guid gymId, UpdateLeadDto dto, CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(StoredProcedureNames.UpdateLead, new
        {
            LeadId = leadId,
            GymId = gymId,
            FullName = dto.FullName.Trim(),
            MobileNumber = dto.MobileNumber.Trim(),
            dto.Email,
            dto.Gender,
            dto.Age,
            dto.Address,
            dto.LeadSource,
            dto.InterestedPlanId,
            dto.Status,
            dto.AssignedTrainerId,
            dto.Notes
        }, cancellationToken);
    }

    public Task UpdateStatusAsync(int leadId, Guid gymId, string status, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateLeadStatus, new { LeadId = leadId, GymId = gymId, Status = status }, cancellationToken);

    public Task DeleteAsync(int leadId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteLead, new { LeadId = leadId, GymId = gymId }, cancellationToken);

    public async Task<LeadDto?> GetByIdAsync(int leadId, Guid? gymId, int? trainerId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<LeadRow>(
            StoredProcedureNames.GetLeadById, new { LeadId = leadId, GymId = gymId, TrainerId = trainerId }, cancellationToken);
        return row is null ? null : MapLead(row);
    }

    public async Task<PagedResultDto<LeadDto>> GetPagedAsync(
        Guid? gymId, int? trainerId, LeadSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@TrainerId", trainerId);
        parameters.Add("@Search", query.Search);
        parameters.Add("@Status", query.Status);
        parameters.Add("@LeadSource", query.LeadSource);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@SortColumn", query.SortColumn);
        parameters.Add("@SortDirection", query.SortDirection);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rows = await _sp.QueryAsync<LeadRow>(StoredProcedureNames.GetLeadsPaged, parameters, cancellationToken);
        return new PagedResultDto<LeadDto>
        {
            Items = rows.Select(MapLead).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<IReadOnlyList<LeadDto>> SearchAllAsync(
        Guid? gymId, int? trainerId, LeadSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        query.PageNumber = 1;
        query.PageSize = 5000;
        var result = await GetPagedAsync(gymId, trainerId, query, cancellationToken);
        return result.Items;
    }

    public Task AssignTrainerAsync(int leadId, Guid gymId, int trainerId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.AssignTrainerToLead,
            new { LeadId = leadId, GymId = gymId, TrainerId = trainerId }, cancellationToken);

    public Task MarkConvertedAsync(int leadId, Guid gymId, int memberId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.ConvertLeadToMember,
            new { LeadId = leadId, GymId = gymId, ConvertedMemberId = memberId }, cancellationToken);

    public async Task<int> ScheduleTrialAsync(
        int leadId, Guid gymId, int? trainerId, DateTime trialDate, Guid? createdBy, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@LeadId", leadId);
        parameters.Add("@GymId", gymId);
        parameters.Add("@TrainerId", trainerId);
        parameters.Add("@TrialDate", trialDate);
        parameters.Add("@CreatedBy", createdBy);
        parameters.Add("@TrialId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.ScheduleTrialSession, parameters, cancellationToken);
        return parameters.Get<int>("@TrialId");
    }

    public Task RecordTrialFeedbackAsync(int trialId, Guid gymId, RecordTrialFeedbackDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.RecordTrialFeedback, new
        {
            TrialId = trialId,
            GymId = gymId,
            dto.AttendanceStatus,
            dto.Feedback,
            dto.Rating
        }, cancellationToken);

    public async Task<int> CreateFollowUpAsync(
        int leadId, Guid gymId, Guid? createdBy, CreateLeadFollowUpDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@LeadId", leadId);
        parameters.Add("@GymId", gymId);
        parameters.Add("@FollowUpDate", dto.FollowUpDate);
        parameters.Add("@FollowUpType", dto.FollowUpType);
        parameters.Add("@Remarks", dto.Remarks);
        parameters.Add("@NextFollowUpDate", dto.NextFollowUpDate);
        parameters.Add("@CreatedBy", createdBy);
        parameters.Add("@FollowUpId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.CreateLeadFollowUp, parameters, cancellationToken);
        return parameters.Get<int>("@FollowUpId");
    }

    public Task UpdateFollowUpAsync(int followUpId, Guid gymId, UpdateLeadFollowUpDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateLeadFollowUp, new
        {
            FollowUpId = followUpId,
            GymId = gymId,
            dto.FollowUpDate,
            dto.FollowUpType,
            dto.Remarks,
            dto.Status,
            dto.NextFollowUpDate
        }, cancellationToken);

    public async Task<int> CreateActivityAsync(
        int leadId, Guid gymId, string activityType, string description, Guid? createdBy, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@LeadId", leadId);
        parameters.Add("@GymId", gymId);
        parameters.Add("@ActivityType", activityType);
        parameters.Add("@Description", description);
        parameters.Add("@CreatedBy", createdBy);
        parameters.Add("@ActivityId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.CreateLeadActivity, parameters, cancellationToken);
        return parameters.Get<int>("@ActivityId");
    }

    public async Task<IReadOnlyList<LeadActivityDto>> GetActivitiesAsync(int leadId, Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<LeadActivityRow>(
            StoredProcedureNames.GetLeadActivities, new { LeadId = leadId, GymId = gymId }, cancellationToken);
        return rows.Select(r => new LeadActivityDto
        {
            Id = r.ActivityId, LeadId = r.LeadId, ActivityType = r.ActivityType,
            Description = r.Description, CreatedDate = r.CreatedDate, CreatedBy = r.CreatedBy
        }).ToList();
    }

    public async Task<IReadOnlyList<LeadFollowUpDto>> GetFollowUpsAsync(int leadId, Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<LeadFollowUpRow>(
            StoredProcedureNames.GetLeadFollowUps, new { LeadId = leadId, GymId = gymId }, cancellationToken);
        return rows.Select(MapFollowUp).ToList();
    }

    public async Task<IReadOnlyList<LeadTrialDto>> GetTrialsAsync(int leadId, Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<LeadTrialRow>(
            StoredProcedureNames.GetLeadTrials, new { LeadId = leadId, GymId = gymId }, cancellationToken);
        return rows.Select(MapTrial).ToList();
    }

    public async Task<LeadDashboardDto> GetDashboardAsync(Guid? gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<LeadDashboardRow>(
            StoredProcedureNames.GetLeadDashboard, new { GymId = gymId }, cancellationToken);
        return new LeadDashboardDto
        {
            TotalLeads = row?.TotalLeads ?? 0,
            NewLeadsToday = row?.NewLeadsToday ?? 0,
            ConvertedLeads = row?.ConvertedLeads ?? 0,
            LostLeads = row?.LostLeads ?? 0,
            PendingFollowUps = row?.PendingFollowUps ?? 0,
            TodaysTrials = row?.TodaysTrials ?? 0,
            ConversionRate = row?.ConversionRate ?? 0,
            TrialConversionRate = row?.TrialConversionRate ?? 0
        };
    }

    public async Task<IReadOnlyList<NamedCountDto>> GetSourceAnalyticsAsync(Guid? gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<NamedCountRow2>(
            StoredProcedureNames.GetLeadSourceAnalytics, new { GymId = gymId }, cancellationToken);
        return rows.Select(r => new NamedCountDto { Name = r.Name, Count = r.Count }).ToList();
    }

    public async Task<IReadOnlyList<NamedCountDto>> GetStatusAnalyticsAsync(Guid? gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<NamedCountRow2>(
            StoredProcedureNames.GetLeadStatusAnalytics, new { GymId = gymId }, cancellationToken);
        return rows.Select(r => new NamedCountDto { Name = r.Name, Count = r.Count }).ToList();
    }

    public async Task<IReadOnlyList<LeadConversionPointDto>> GetConversionReportAsync(
        Guid? gymId, int months, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<LeadConversionRow>(
            StoredProcedureNames.GetLeadConversionReport, new { GymId = gymId, Months = months }, cancellationToken);
        return rows.Select(r => new LeadConversionPointDto
        {
            Year = r.Year, Month = r.Month, MonthLabel = r.MonthLabel,
            Conversions = r.Conversions, NewLeads = r.NewLeads
        }).Reverse().ToList();
    }

    public async Task<IReadOnlyList<TrainerLeadPerformanceDto>> GetTrainerPerformanceAsync(
        Guid? gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<TrainerLeadPerfRow>(
            StoredProcedureNames.GetTrainerLeadConversion, new { GymId = gymId }, cancellationToken);
        return rows.Select(r => new TrainerLeadPerformanceDto
        {
            TrainerId = r.TrainerId, TrainerName = r.TrainerName,
            TotalLeads = r.TotalLeads, ConvertedLeads = r.ConvertedLeads
        }).ToList();
    }

    public async Task<IReadOnlyList<LeadFollowUpDto>> GetPendingFollowUpsAsync(
        Guid? gymId, int? trainerId, int topN, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<LeadFollowUpRow>(
            StoredProcedureNames.GetPendingFollowUps, new { GymId = gymId, TrainerId = trainerId, TopN = topN }, cancellationToken);
        return rows.Select(MapFollowUp).ToList();
    }

    public async Task<IReadOnlyList<LeadTrialDto>> GetTodaysTrialsAsync(
        Guid? gymId, int? trainerId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<LeadTrialRow>(
            StoredProcedureNames.GetTodaysTrials, new { GymId = gymId, TrainerId = trainerId }, cancellationToken);
        return rows.Select(MapTrial).ToList();
    }

    public async Task<IReadOnlyList<LeadReminderCandidateDto>> GetReminderCandidatesAsync(
        int hoursAhead, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<LeadReminderRow>(
            StoredProcedureNames.GetLeadReminderCandidates, new { HoursAhead = hoursAhead }, cancellationToken);
        return rows.Select(r => new LeadReminderCandidateDto
        {
            ReminderType = r.ReminderType, EntityId = r.EntityId, GymId = r.GymId, LeadId = r.LeadId,
            LeadName = r.LeadName, MobileNumber = r.MobileNumber, ScheduledAt = r.ScheduledAt
        }).ToList();
    }

    private static LeadDto MapLead(LeadRow r) => new()
    {
        Id = r.LeadId, GymId = r.GymId, FullName = r.FullName, MobileNumber = r.MobileNumber,
        Email = r.Email, Gender = r.Gender, Age = r.Age, Address = r.Address, LeadSource = r.LeadSource,
        InterestedPlanId = r.InterestedPlanId, InterestedPlanName = r.InterestedPlanName,
        Status = r.Status, AssignedTrainerId = r.AssignedTrainerId, AssignedTrainerName = r.AssignedTrainerName,
        Notes = r.Notes, ConvertedMemberId = r.ConvertedMemberId,
        CreatedDate = r.CreatedDate, CreatedBy = r.CreatedBy, UpdatedDate = r.UpdatedDate
    };

    private static LeadFollowUpDto MapFollowUp(LeadFollowUpRow r) => new()
    {
        Id = r.FollowUpId, LeadId = r.LeadId, FollowUpDate = r.FollowUpDate, FollowUpType = r.FollowUpType,
        Remarks = r.Remarks, Status = r.Status, NextFollowUpDate = r.NextFollowUpDate,
        CreatedDate = r.CreatedDate, LeadName = r.LeadName, MobileNumber = r.MobileNumber
    };

    private static LeadTrialDto MapTrial(LeadTrialRow r) => new()
    {
        Id = r.TrialId, LeadId = r.LeadId, TrainerId = r.TrainerId, TrainerName = r.TrainerName,
        TrialDate = r.TrialDate, AttendanceStatus = r.AttendanceStatus, Feedback = r.Feedback, Rating = r.Rating,
        LeadName = r.LeadName, MobileNumber = r.MobileNumber
    };
}
