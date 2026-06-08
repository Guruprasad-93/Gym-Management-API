using System.Data;
using Dapper;
using Gym.Application.DTOs.Ai;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class AiRecommendationRepository : IAiRecommendationRepository
{
    private readonly IStoredProcedureExecutor _sp;
    private readonly ISqlConnectionFactory _connectionFactory;

    public AiRecommendationRepository(IStoredProcedureExecutor sp, ISqlConnectionFactory connectionFactory)
    {
        _sp = sp;
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateRecommendationAsync(Guid gymId, int memberId, AiGeneratedRecommendation recommendation, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@RecommendationType", recommendation.RecommendationType);
        parameters.Add("@RecommendationText", recommendation.RecommendationText);
        parameters.Add("@ConfidenceScore", recommendation.ConfidenceScore);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateAiRecommendation, parameters, "@Id", cancellationToken);
    }

    public async Task<PagedResultDto<AiRecommendationDto>> GetRecommendationsPagedAsync(Guid gymId, AiRecommendationsQueryDto query, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", query.MemberId);
        parameters.Add("@RecommendationType", query.RecommendationType);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var rows = await _sp.QueryAsync<AiRecommendationRow>(StoredProcedureNames.GetAiRecommendationsPaged, parameters, cancellationToken);
        return new PagedResultDto<AiRecommendationDto>
        {
            Items = rows.Select(MapRecommendation).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public Task MarkRecommendationAcceptedAsync(Guid gymId, int recommendationId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.MarkAiRecommendationAccepted, new { GymId = gymId, RecommendationId = recommendationId }, cancellationToken);

    public async Task<int> CreateInsightAsync(Guid gymId, AiGeneratedInsight insight, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@InsightType", insight.InsightType);
        parameters.Add("@InsightText", insight.InsightText);
        parameters.Add("@Severity", insight.Severity);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateAiInsight, parameters, "@Id", cancellationToken);
    }

    public async Task<PagedResultDto<AiInsightDto>> GetInsightsPagedAsync(Guid gymId, AiInsightsQueryDto query, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@Severity", query.Severity);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var rows = await _sp.QueryAsync<AiInsightRow>(StoredProcedureNames.GetAiInsightsPaged, parameters, cancellationToken);
        return new PagedResultDto<AiInsightDto>
        {
            Items = rows.Select(MapInsight).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<int> UpsertMemberRiskScoreAsync(Guid gymId, int memberId, CalculatedMemberRiskDto risk, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@ChurnRisk", risk.ChurnRisk);
        parameters.Add("@AttendanceRisk", risk.AttendanceRisk);
        parameters.Add("@RenewalProbability", risk.RenewalProbability);
        parameters.Add("@HealthScore", risk.HealthScore);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.UpsertMemberRiskScore, parameters, "@Id", cancellationToken);
    }

    public async Task<PagedResultDto<MemberRiskScoreDto>> GetMemberRiskScoresPagedAsync(Guid gymId, MemberRiskQueryDto query, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@ChurnRisk", query.ChurnRisk);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var rows = await _sp.QueryAsync<MemberRiskScoreRow>(StoredProcedureNames.GetMemberRiskScoresPaged, parameters, cancellationToken);
        return new PagedResultDto<MemberRiskScoreDto>
        {
            Items = rows.Select(MapRisk).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<MemberRiskScoreDto?> GetMemberRiskScoreAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<MemberRiskScoreRow>(
            StoredProcedureNames.GetMemberRiskScoreByMemberId, new { GymId = gymId, MemberId = memberId }, cancellationToken);
        return row is null ? null : MapRisk(row);
    }

    public async Task LogGenerationAsync(Guid gymId, string entityType, string? entityId, int tokensUsed, string provider, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@EntityType", entityType);
        parameters.Add("@EntityId", entityId);
        parameters.Add("@TokensUsed", tokensUsed);
        parameters.Add("@Provider", provider);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateAiGenerationLog, parameters, "@Id", cancellationToken);
    }

    public async Task<AiAnalyticsDto> GetAnalyticsAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<AiAnalyticsRow>(StoredProcedureNames.GetAiAnalytics, new { GymId = gymId }, cancellationToken);
        return row is null ? new AiAnalyticsDto() : new AiAnalyticsDto
        {
            TotalRecommendations = row.TotalRecommendations,
            AcceptedRecommendations = row.AcceptedRecommendations,
            AcceptanceRate = row.AcceptanceRate,
            HighChurnPredictions = row.HighChurnPredictions,
            RecentHighChurnPredictions = row.RecentHighChurnPredictions,
            TotalTokensUsed = row.TotalTokensUsed,
            TotalGenerations = row.TotalGenerations,
            TotalInsights = row.TotalInsights
        };
    }

    public async Task<AiDashboardDto> GetDashboardAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            StoredProcedureNames.GetAiDashboard,
            new { GymId = gymId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        var summary = await multi.ReadSingleOrDefaultAsync<AiDashboardSummaryRow>();
        var churnDist = (await multi.ReadAsync<AiChartPointRow>()).ToList();
        var renewalDist = (await multi.ReadAsync<AiChartPointRow>()).ToList();

        var recommendations = await GetRecommendationsPagedAsync(gymId, new AiRecommendationsQueryDto { PageNumber = 1, PageSize = 5 }, cancellationToken);
        var highRisk = await GetMemberRiskScoresPagedAsync(gymId, new MemberRiskQueryDto { ChurnRisk = "High", PageNumber = 1, PageSize = 10 }, cancellationToken);

        return new AiDashboardDto
        {
            HighRiskMembers = summary?.HighRiskMembers ?? 0,
            PredictedRenewals = summary?.PredictedRenewals ?? 0,
            HotLeads = summary?.HotLeads ?? 0,
            RecentRecommendations = summary?.RecentRecommendations ?? 0,
            ActionableInsights = summary?.ActionableInsights ?? 0,
            ChurnRiskDistribution = churnDist.Select(r => new AiChartPointDto { Label = r.Label, Count = r.Count }).ToList(),
            RenewalProbabilityDistribution = renewalDist.Select(r => new AiChartPointDto { Label = r.Label, Count = r.Count }).ToList(),
            TopRecommendations = recommendations.Items,
            HighRiskMemberList = highRisk.Items
        };
    }

    public async Task<PagedResultDto<LeadScoreDto>> GetLeadScoringPagedAsync(Guid gymId, LeadScoringQueryDto query, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@ScoreCategory", query.ScoreCategory);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var rows = await _sp.QueryAsync<LeadScoreRow>(StoredProcedureNames.GetLeadScoringPaged, parameters, cancellationToken);
        return new PagedResultDto<LeadScoreDto>
        {
            Items = rows.Select(MapLeadScore).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<MemberAiAnalysisContextDto?> GetMemberAnalysisContextAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<MemberAiContextRow>(
            StoredProcedureNames.GetMemberAiAnalysisContext, new { GymId = gymId, MemberId = memberId }, cancellationToken);
        return row is null ? null : MapContext(row);
    }

    public async Task<BusinessAiContextDto?> GetBusinessContextAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<BusinessAiContextRow>(
            StoredProcedureNames.GetBusinessAiContext, new { GymId = gymId }, cancellationToken);
        return row is null ? null : new BusinessAiContextDto
        {
            RevenueThisMonth = row.RevenueThisMonth,
            RevenuePrevMonth = row.RevenuePrevMonth,
            ActiveMembersLast30Days = row.ActiveMembersLast30Days,
            ActiveMembersPrev30Days = row.ActiveMembersPrev30Days,
            HighChurnMembers = row.HighChurnMembers,
            ActiveTrainers = row.ActiveTrainers,
            MembersWithTrainer = row.MembersWithTrainer
        };
    }

    public async Task<IReadOnlyList<BranchAttendanceAiDto>> GetBranchAttendanceContextAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<BranchAttendanceAiRow>(StoredProcedureNames.GetBranchAttendanceForAi, new { GymId = gymId }, cancellationToken);
        return rows.Select(r => new BranchAttendanceAiDto
        {
            BranchId = r.BranchId,
            BranchName = r.BranchName,
            AttendanceLast30Days = r.AttendanceLast30Days,
            AttendancePrev30Days = r.AttendancePrev30Days
        }).ToList();
    }

    public async Task<IReadOnlyList<AiMemberJobRowDto>> GetActiveMembersForJobAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<AiMemberJobRow>(StoredProcedureNames.GetActiveMembersForAiJob, new { GymId = gymId }, cancellationToken);
        return rows.Select(r => new AiMemberJobRowDto
        {
            MemberId = r.MemberId,
            GymId = r.GymId,
            UserId = r.UserId,
            FullName = r.FullName
        }).ToList();
    }

    public async Task<IReadOnlyList<AiGymJobRowDto>> GetGymsForJobAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<AiGymJobRow>(StoredProcedureNames.GetGymsForAiJob, cancellationToken: cancellationToken);
        return rows.Select(r => new AiGymJobRowDto { GymId = r.GymId, GymName = r.GymName }).ToList();
    }

    public async Task<IReadOnlyList<HighRiskMemberNotificationDto>> GetHighRiskMembersForNotificationAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<HighRiskMemberRow>(StoredProcedureNames.GetHighRiskMembersForNotification, new { GymId = gymId }, cancellationToken);
        return rows.Select(r => new HighRiskMemberNotificationDto
        {
            MemberId = r.MemberId,
            UserId = r.UserId,
            FullName = r.FullName,
            PhoneNumber = r.PhoneNumber,
            ChurnRisk = r.ChurnRisk,
            RenewalProbability = r.RenewalProbability
        }).ToList();
    }

    private static AiRecommendationDto MapRecommendation(AiRecommendationRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        MemberId = row.MemberId,
        MemberName = row.MemberName,
        RecommendationType = row.RecommendationType,
        RecommendationText = row.RecommendationText,
        ConfidenceScore = row.ConfidenceScore,
        IsAccepted = row.IsAccepted,
        AcceptedDate = row.AcceptedDate,
        GeneratedDate = row.GeneratedDate
    };

    private static AiInsightDto MapInsight(AiInsightRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        InsightType = row.InsightType,
        InsightText = row.InsightText,
        Severity = row.Severity,
        GeneratedDate = row.GeneratedDate
    };

    private static MemberRiskScoreDto MapRisk(MemberRiskScoreRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        MemberId = row.MemberId,
        MemberName = row.MemberName,
        ChurnRisk = row.ChurnRisk,
        AttendanceRisk = row.AttendanceRisk,
        RenewalProbability = row.RenewalProbability,
        HealthScore = row.HealthScore,
        LastCalculatedDate = row.LastCalculatedDate
    };

    private static LeadScoreDto MapLeadScore(LeadScoreRow row) => new()
    {
        LeadId = row.LeadId,
        GymId = row.GymId,
        FullName = row.FullName,
        MobileNumber = row.MobileNumber,
        Email = row.Email,
        Status = row.Status,
        LeadSource = row.LeadSource,
        CreatedDate = row.CreatedDate,
        FollowUpCount = row.FollowUpCount,
        CompletedTrials = row.CompletedTrials,
        ScoreCategory = row.ScoreCategory,
        EngagementScore = row.EngagementScore
    };

    private static MemberAiAnalysisContextDto MapContext(MemberAiContextRow row) => new()
    {
        MemberId = row.MemberId,
        GymId = row.GymId,
        FullName = row.FullName,
        Weight = row.Weight,
        Height = row.Height,
        JoinDate = row.JoinDate,
        PrimaryGoal = row.PrimaryGoal,
        AttendanceLast30Days = row.AttendanceLast30Days,
        AttendancePrev30Days = row.AttendancePrev30Days,
        AvgWorkoutCompletion = row.AvgWorkoutCompletion,
        AvgDietCompliance = row.AvgDietCompliance,
        LatestWeight = row.LatestWeight,
        Weight30DaysAgo = row.Weight30DaysAgo,
        CompletedGoals = row.CompletedGoals,
        TotalGoals = row.TotalGoals,
        MembershipEndDate = row.MembershipEndDate,
        PaymentsLast6Months = row.PaymentsLast6Months,
        LastWorkoutDate = row.LastWorkoutDate
    };

    private sealed class AiRecommendationRow
    {
        public int Id { get; set; }
        public Guid GymId { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string RecommendationType { get; set; } = string.Empty;
        public string RecommendationText { get; set; } = string.Empty;
        public decimal ConfidenceScore { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public DateTime GeneratedDate { get; set; }
    }

    private sealed class AiInsightRow
    {
        public int Id { get; set; }
        public Guid GymId { get; set; }
        public string InsightType { get; set; } = string.Empty;
        public string InsightText { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
    }

    private sealed class MemberRiskScoreRow
    {
        public int Id { get; set; }
        public Guid GymId { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string ChurnRisk { get; set; } = string.Empty;
        public string AttendanceRisk { get; set; } = string.Empty;
        public decimal RenewalProbability { get; set; }
        public decimal HealthScore { get; set; }
        public DateTime LastCalculatedDate { get; set; }
    }

    private sealed class LeadScoreRow
    {
        public int LeadId { get; set; }
        public Guid GymId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? LeadSource { get; set; }
        public DateTime CreatedDate { get; set; }
        public int FollowUpCount { get; set; }
        public int CompletedTrials { get; set; }
        public string ScoreCategory { get; set; } = string.Empty;
        public int EngagementScore { get; set; }
    }

    private sealed class AiAnalyticsRow
    {
        public int TotalRecommendations { get; set; }
        public int AcceptedRecommendations { get; set; }
        public decimal AcceptanceRate { get; set; }
        public int HighChurnPredictions { get; set; }
        public int RecentHighChurnPredictions { get; set; }
        public int TotalTokensUsed { get; set; }
        public int TotalGenerations { get; set; }
        public int TotalInsights { get; set; }
    }

    private sealed class AiDashboardSummaryRow
    {
        public int HighRiskMembers { get; set; }
        public int PredictedRenewals { get; set; }
        public int HotLeads { get; set; }
        public int RecentRecommendations { get; set; }
        public int ActionableInsights { get; set; }
    }

    private sealed class AiChartPointRow
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    private sealed class MemberAiContextRow
    {
        public int MemberId { get; set; }
        public Guid GymId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public DateTime JoinDate { get; set; }
        public string? PrimaryGoal { get; set; }
        public int AttendanceLast30Days { get; set; }
        public int AttendancePrev30Days { get; set; }
        public double? AvgWorkoutCompletion { get; set; }
        public double? AvgDietCompliance { get; set; }
        public decimal? LatestWeight { get; set; }
        public decimal? Weight30DaysAgo { get; set; }
        public int CompletedGoals { get; set; }
        public int TotalGoals { get; set; }
        public DateTime? MembershipEndDate { get; set; }
        public int PaymentsLast6Months { get; set; }
        public DateTime? LastWorkoutDate { get; set; }
    }

    private sealed class BusinessAiContextRow
    {
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenuePrevMonth { get; set; }
        public int ActiveMembersLast30Days { get; set; }
        public int ActiveMembersPrev30Days { get; set; }
        public int HighChurnMembers { get; set; }
        public int ActiveTrainers { get; set; }
        public int MembersWithTrainer { get; set; }
    }

    private sealed class BranchAttendanceAiRow
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int AttendanceLast30Days { get; set; }
        public int AttendancePrev30Days { get; set; }
    }

    private sealed class AiMemberJobRow
    {
        public int MemberId { get; set; }
        public Guid GymId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    private sealed class AiGymJobRow
    {
        public Guid GymId { get; set; }
        public string GymName { get; set; } = string.Empty;
    }

    private sealed class HighRiskMemberRow
    {
        public int MemberId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string ChurnRisk { get; set; } = string.Empty;
        public decimal RenewalProbability { get; set; }
    }
}
