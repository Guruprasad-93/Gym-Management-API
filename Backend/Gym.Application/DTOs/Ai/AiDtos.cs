using Gym.Application.DTOs.Common;

namespace Gym.Application.DTOs.Ai;

public sealed class AiRecommendationDto
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

public sealed class AiInsightDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public string InsightType { get; set; } = string.Empty;
    public string InsightText { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
}

public sealed class MemberRiskScoreDto
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

public sealed class LeadScoreDto
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

public sealed class AiDashboardDto
{
    public int HighRiskMembers { get; set; }
    public int PredictedRenewals { get; set; }
    public int HotLeads { get; set; }
    public int RecentRecommendations { get; set; }
    public int ActionableInsights { get; set; }
    public IReadOnlyList<AiChartPointDto> ChurnRiskDistribution { get; set; } = Array.Empty<AiChartPointDto>();
    public IReadOnlyList<AiChartPointDto> RenewalProbabilityDistribution { get; set; } = Array.Empty<AiChartPointDto>();
    public IReadOnlyList<AiRecommendationDto> TopRecommendations { get; set; } = Array.Empty<AiRecommendationDto>();
    public IReadOnlyList<MemberRiskScoreDto> HighRiskMemberList { get; set; } = Array.Empty<MemberRiskScoreDto>();
}

public sealed class AiChartPointDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class AiAnalyticsDto
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

public sealed class AiRecommendationsQueryDto : PagedRequestDto
{
    public int? MemberId { get; set; }
    public string? RecommendationType { get; set; }
}

public sealed class MemberRiskQueryDto : PagedRequestDto
{
    public string? ChurnRisk { get; set; }
}

public sealed class LeadScoringQueryDto : PagedRequestDto
{
    public string? ScoreCategory { get; set; }
}

public sealed class AiInsightsQueryDto : PagedRequestDto
{
    public string? Severity { get; set; }
}

public sealed class AcceptAiRecommendationDto
{
    public int RecommendationId { get; set; }
}

public sealed class MemberAiAnalysisContextDto
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

public sealed class BusinessAiContextDto
{
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenuePrevMonth { get; set; }
    public int ActiveMembersLast30Days { get; set; }
    public int ActiveMembersPrev30Days { get; set; }
    public int HighChurnMembers { get; set; }
    public int ActiveTrainers { get; set; }
    public int MembersWithTrainer { get; set; }
}

public sealed class BranchAttendanceAiDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int AttendanceLast30Days { get; set; }
    public int AttendancePrev30Days { get; set; }
}

public sealed class AiMemberJobRowDto
{
    public int MemberId { get; set; }
    public Guid GymId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
}

public sealed class AiGymJobRowDto
{
    public Guid GymId { get; set; }
    public string GymName { get; set; } = string.Empty;
}

public sealed class HighRiskMemberNotificationDto
{
    public int MemberId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string ChurnRisk { get; set; } = string.Empty;
    public decimal RenewalProbability { get; set; }
}

public sealed class AiGeneratedRecommendation
{
    public string RecommendationType { get; set; } = string.Empty;
    public string RecommendationText { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
}

public sealed class AiGeneratedInsight
{
    public string InsightType { get; set; } = string.Empty;
    public string InsightText { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}

public sealed class AiProviderResult
{
    public IReadOnlyList<AiGeneratedRecommendation> Recommendations { get; set; } = Array.Empty<AiGeneratedRecommendation>();
    public IReadOnlyList<AiGeneratedInsight> Insights { get; set; } = Array.Empty<AiGeneratedInsight>();
    public int TokensUsed { get; set; }
}

public sealed class CalculatedMemberRiskDto
{
    public string ChurnRisk { get; set; } = string.Empty;
    public string AttendanceRisk { get; set; } = string.Empty;
    public decimal RenewalProbability { get; set; }
    public decimal HealthScore { get; set; }
}
