using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Ai;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Mobile;
using Gym.Application.DTOs.Notifications;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gym.Application.Services;

public class AiRecommendationService : IAiRecommendationService
{
    private readonly IAiRecommendationRepository _repository;
    private readonly IAiProvider _aiProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    private readonly IMobilePushService _mobilePushService;
    private readonly AiSettings _settings;
    private readonly ILogger<AiRecommendationService> _logger;

    public AiRecommendationService(
        IAiRecommendationRepository repository,
        IAiProvider aiProvider,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        IMobilePushService mobilePushService,
        IOptions<AiSettings> settings,
        ILogger<AiRecommendationService> logger)
    {
        _repository = repository;
        _aiProvider = aiProvider;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _mobilePushService = mobilePushService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AiDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanViewInsights();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetDashboardAsync(gymId, cancellationToken);
    }

    public async Task<PagedResultDto<AiRecommendationDto>> GetRecommendationsAsync(AiRecommendationsQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewRecommendations();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetRecommendationsPagedAsync(gymId, query, cancellationToken);
    }

    public async Task<PagedResultDto<MemberRiskScoreDto>> GetMemberRiskAsync(MemberRiskQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewInsights();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetMemberRiskScoresPagedAsync(gymId, query, cancellationToken);
    }

    public async Task<PagedResultDto<LeadScoreDto>> GetLeadScoringAsync(LeadScoringQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewInsights();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetLeadScoringPagedAsync(gymId, query, cancellationToken);
    }

    public async Task<PagedResultDto<AiInsightDto>> GetBusinessInsightsAsync(AiInsightsQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewInsights();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetInsightsPagedAsync(gymId, query, cancellationToken);
    }

    public async Task<AiAnalyticsDto> GetAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanViewInsights();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetAnalyticsAsync(gymId, cancellationToken);
    }

    public async Task AcceptRecommendationAsync(AcceptAiRecommendationDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanViewRecommendations();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.MarkRecommendationAcceptedAsync(gymId, dto.RecommendationId, cancellationToken);
    }

    public async Task GenerateForMemberAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        var context = await _repository.GetMemberAnalysisContextAsync(gymId, memberId, cancellationToken);
        if (context is null)
            return;

        var risk = CalculateMemberRisk(context);
        await _repository.UpsertMemberRiskScoreAsync(gymId, memberId, risk, cancellationToken);

        if (!_settings.Enabled)
            return;

        var result = await _aiProvider.GenerateMemberRecommendationsAsync(context, cancellationToken);
        foreach (var recommendation in result.Recommendations)
            await _repository.CreateRecommendationAsync(gymId, memberId, recommendation, cancellationToken);

        await _repository.LogGenerationAsync(gymId, "Member", memberId.ToString(), result.TokensUsed, _settings.Provider, cancellationToken);
    }

    public async Task RunDailyGenerationAsync(CancellationToken cancellationToken = default)
    {
        var gyms = await _repository.GetGymsForJobAsync(cancellationToken);
        foreach (var gym in gyms)
        {
            try
            {
                var members = await _repository.GetActiveMembersForJobAsync(gym.GymId, cancellationToken);
                var count = 0;
                foreach (var member in members.Take(_settings.MaxMembersPerRun))
                {
                    await GenerateForMemberAsync(gym.GymId, member.MemberId, cancellationToken);
                    count++;
                }

                if (_settings.Enabled)
                {
                    var businessContext = await _repository.GetBusinessContextAsync(gym.GymId, cancellationToken);
                    var branches = await _repository.GetBranchAttendanceContextAsync(gym.GymId, cancellationToken);
                    if (businessContext is not null)
                    {
                        var insightResult = await _aiProvider.GenerateBusinessInsightsAsync(businessContext, branches, cancellationToken);
                        foreach (var insight in insightResult.Insights)
                            await _repository.CreateInsightAsync(gym.GymId, insight, cancellationToken);
                        await _repository.LogGenerationAsync(gym.GymId, "Business", null, insightResult.TokensUsed, _settings.Provider, cancellationToken);
                    }
                }

                await SendHighRiskAlertsAsync(gym.GymId, cancellationToken);
                _logger.LogInformation("AI generation completed for gym {GymId}. Members processed: {Count}", gym.GymId, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI generation failed for gym {GymId}", gym.GymId);
            }
        }
    }

    public static CalculatedMemberRiskDto CalculateMemberRisk(MemberAiAnalysisContextDto context)
    {
        var attendanceScore = context.AttendanceLast30Days;
        var attendanceDrop = context.AttendancePrev30Days > 0
            && context.AttendanceLast30Days < context.AttendancePrev30Days * 0.7;
        var workoutInactive = context.LastWorkoutDate is null || context.LastWorkoutDate < DateTime.UtcNow.AddDays(-14);
        var expirySoon = context.MembershipEndDate.HasValue && context.MembershipEndDate.Value <= DateTime.UtcNow.AddDays(30);
        var lowPayments = context.PaymentsLast6Months == 0;

        var churnPoints = 0;
        if (attendanceDrop) churnPoints += 3;
        if (workoutInactive) churnPoints += 2;
        if (expirySoon) churnPoints += 2;
        if (lowPayments) churnPoints += 1;
        if (attendanceScore < 4) churnPoints += 2;

        var churnRisk = churnPoints >= 5 ? AiRiskLevels.High : churnPoints >= 3 ? AiRiskLevels.Medium : AiRiskLevels.Low;
        var attendanceRisk = attendanceDrop || attendanceScore < 4
            ? attendanceScore < 2 ? AiRiskLevels.High : AiRiskLevels.Medium
            : AiRiskLevels.Low;

        var renewalBase = 75m;
        if (attendanceDrop) renewalBase -= 20;
        if (workoutInactive) renewalBase -= 15;
        if (expirySoon && attendanceScore < 6) renewalBase -= 20;
        if (lowPayments) renewalBase -= 10;
        if (context.CompletedGoals > 0) renewalBase += 10;
        renewalBase = Math.Clamp(renewalBase, 5, 98);

        var workoutScore = (decimal)(context.AvgWorkoutCompletion ?? 0);
        var dietScore = (decimal)(context.AvgDietCompliance ?? 0);
        var goalScore = context.TotalGoals == 0 ? 50 : (decimal)context.CompletedGoals / context.TotalGoals * 100;
        var attendancePct = Math.Min(100, attendanceScore * 3.3m);
        var healthScore = Math.Round((attendancePct * 0.3m) + (workoutScore * 0.25m) + (dietScore * 0.25m) + (goalScore * 0.2m), 2);

        return new CalculatedMemberRiskDto
        {
            ChurnRisk = churnRisk,
            AttendanceRisk = attendanceRisk,
            RenewalProbability = renewalBase,
            HealthScore = healthScore
        };
    }

    private async Task SendHighRiskAlertsAsync(Guid gymId, CancellationToken cancellationToken)
    {
        var highRiskMembers = await _repository.GetHighRiskMembersForNotificationAsync(gymId, cancellationToken);
        foreach (var member in highRiskMembers)
        {
            await _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
            {
                NotificationType = "ChurnRiskAlert",
                PhoneNumber = member.PhoneNumber ?? string.Empty,
                RecipientUserId = member.UserId,
                MemberId = member.MemberId,
                Variables = new Dictionary<string, string>
                {
                    ["memberName"] = member.FullName,
                    ["renewalProbability"] = member.RenewalProbability.ToString("0")
                }
            }, cancellationToken);

            await _mobilePushService.SendEventPushAsync(gymId, new SendEventPushRequest
            {
                UserId = member.UserId,
                NotificationType = PushNotificationTypes.ChurnRiskAlert,
                Title = "Membership Attention Needed",
                Message = $"Hi {member.FullName}, we'd love to help you stay on track. Your membership may need attention — contact your gym."
            }, cancellationToken);

            if (member.RenewalProbability < 40)
            {
                await _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
                {
                    NotificationType = "RenewalRiskAlert",
                    PhoneNumber = member.PhoneNumber ?? string.Empty,
                    RecipientUserId = member.UserId,
                    MemberId = member.MemberId,
                    Variables = new Dictionary<string, string>
                    {
                        ["memberName"] = member.FullName,
                        ["renewalProbability"] = member.RenewalProbability.ToString("0")
                    }
                }, cancellationToken);

                await _mobilePushService.SendEventPushAsync(gymId, new SendEventPushRequest
                {
                    UserId = member.UserId,
                    NotificationType = PushNotificationTypes.RenewalRiskAlert,
                    Title = "Renewal Reminder",
                    Message = $"Hi {member.FullName}, your membership renewal is approaching. Renew now to keep your progress going."
                }, cancellationToken);
            }
        }
    }

    private void EnsureCanViewInsights()
    {
        if (!_currentUser.HasPermission(Permissions.ViewAiInsights))
            throw new UnauthorizedAccessException("Missing VIEW_AI_INSIGHTS permission.");
    }

    private void EnsureCanViewRecommendations()
    {
        if (!_currentUser.HasPermission(Permissions.ViewAiRecommendations))
            throw new UnauthorizedAccessException("Missing VIEW_AI_RECOMMENDATIONS permission.");
    }
}
