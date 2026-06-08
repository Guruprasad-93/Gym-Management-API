using Gym.Application.DTOs.Ai;
using Gym.Application.DTOs.Common;

namespace Gym.Application.Interfaces;

public interface IAiRecommendationRepository
{
    Task<int> CreateRecommendationAsync(Guid gymId, int memberId, AiGeneratedRecommendation recommendation, CancellationToken cancellationToken = default);
    Task<PagedResultDto<AiRecommendationDto>> GetRecommendationsPagedAsync(Guid gymId, AiRecommendationsQueryDto query, CancellationToken cancellationToken = default);
    Task MarkRecommendationAcceptedAsync(Guid gymId, int recommendationId, CancellationToken cancellationToken = default);
    Task<int> CreateInsightAsync(Guid gymId, AiGeneratedInsight insight, CancellationToken cancellationToken = default);
    Task<PagedResultDto<AiInsightDto>> GetInsightsPagedAsync(Guid gymId, AiInsightsQueryDto query, CancellationToken cancellationToken = default);
    Task<int> UpsertMemberRiskScoreAsync(Guid gymId, int memberId, CalculatedMemberRiskDto risk, CancellationToken cancellationToken = default);
    Task<PagedResultDto<MemberRiskScoreDto>> GetMemberRiskScoresPagedAsync(Guid gymId, MemberRiskQueryDto query, CancellationToken cancellationToken = default);
    Task<MemberRiskScoreDto?> GetMemberRiskScoreAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default);
    Task LogGenerationAsync(Guid gymId, string entityType, string? entityId, int tokensUsed, string provider, CancellationToken cancellationToken = default);
    Task<AiAnalyticsDto> GetAnalyticsAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<AiDashboardDto> GetDashboardAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<LeadScoreDto>> GetLeadScoringPagedAsync(Guid gymId, LeadScoringQueryDto query, CancellationToken cancellationToken = default);
    Task<MemberAiAnalysisContextDto?> GetMemberAnalysisContextAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default);
    Task<BusinessAiContextDto?> GetBusinessContextAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchAttendanceAiDto>> GetBranchAttendanceContextAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiMemberJobRowDto>> GetActiveMembersForJobAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiGymJobRowDto>> GetGymsForJobAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HighRiskMemberNotificationDto>> GetHighRiskMembersForNotificationAsync(Guid gymId, CancellationToken cancellationToken = default);
}

public interface IAiProvider
{
    Task<AiProviderResult> GenerateMemberRecommendationsAsync(MemberAiAnalysisContextDto context, CancellationToken cancellationToken = default);
    Task<AiProviderResult> GenerateBusinessInsightsAsync(BusinessAiContextDto context, IReadOnlyList<BranchAttendanceAiDto> branches, CancellationToken cancellationToken = default);
}

public interface IAiRecommendationService
{
    Task<AiDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<AiRecommendationDto>> GetRecommendationsAsync(AiRecommendationsQueryDto query, CancellationToken cancellationToken = default);
    Task<PagedResultDto<MemberRiskScoreDto>> GetMemberRiskAsync(MemberRiskQueryDto query, CancellationToken cancellationToken = default);
    Task<PagedResultDto<LeadScoreDto>> GetLeadScoringAsync(LeadScoringQueryDto query, CancellationToken cancellationToken = default);
    Task<PagedResultDto<AiInsightDto>> GetBusinessInsightsAsync(AiInsightsQueryDto query, CancellationToken cancellationToken = default);
    Task<AiAnalyticsDto> GetAnalyticsAsync(CancellationToken cancellationToken = default);
    Task AcceptRecommendationAsync(AcceptAiRecommendationDto dto, CancellationToken cancellationToken = default);
    Task GenerateForMemberAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default);
    Task RunDailyGenerationAsync(CancellationToken cancellationToken = default);
}
