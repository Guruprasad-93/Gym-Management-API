using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Gym.Application.DTOs.Ai;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.Services.Ai;

public class OpenAiProvider : IAiProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    private readonly HttpClient _httpClient;
    private readonly AiSettings _settings;
    private readonly ILogger<OpenAiProvider> _logger;

    public OpenAiProvider(HttpClient httpClient, IOptions<AiSettings> settings, ILogger<OpenAiProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AiProviderResult> GenerateMemberRecommendationsAsync(MemberAiAnalysisContextDto context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            return await new MockAiProvider().GenerateMemberRecommendationsAsync(context, cancellationToken);

        var prompt = $"""
            Generate concise gym recommendations for member {context.FullName}.
            Goal: {context.PrimaryGoal ?? "General Fitness"}
            Attendance last 30 days: {context.AttendanceLast30Days}, previous 30: {context.AttendancePrev30Days}
            Avg workout completion: {context.AvgWorkoutCompletion ?? 0}%
            Avg diet compliance: {context.AvgDietCompliance ?? 0}%
            Weight trend: {context.Weight30DaysAgo} -> {context.LatestWeight}
            Return JSON array with objects: recommendationType (WorkoutAdjustment|WorkoutIntensity|WeeklyWorkoutPlan|DietCalorie|DietMealAdjustment|ProteinTarget), recommendationText, confidenceScore (0-100).
            """;

        return await CallOpenAiAsync(prompt, cancellationToken);
    }

    public async Task<AiProviderResult> GenerateBusinessInsightsAsync(
        BusinessAiContextDto context,
        IReadOnlyList<BranchAttendanceAiDto> branches,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            return await new MockAiProvider().GenerateBusinessInsightsAsync(context, branches, cancellationToken);

        var branchSummary = string.Join("; ", branches.Select(b => $"{b.BranchName}: {b.AttendanceLast30Days}/{b.AttendancePrev30Days}"));
        var prompt = $"""
            Generate business insights for a gym.
            Revenue this month: {context.RevenueThisMonth}, previous: {context.RevenuePrevMonth}
            Active members 30d: {context.ActiveMembersLast30Days}, previous: {context.ActiveMembersPrev30Days}
            High churn members: {context.HighChurnMembers}
            Trainer utilization: {context.MembersWithTrainer}/{context.ActiveTrainers}
            Branches: {branchSummary}
            Return JSON array with objects: insightType, insightText, severity (Info|Warning|Critical).
            """;

        return await CallOpenAiAsync(prompt, cancellationToken);
    }

    private async Task<AiProviderResult> CallOpenAiAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            var body = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new { role = "system", content = "You are a fitness business AI assistant. Respond with valid JSON only." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.4
            };
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI call failed: {Status} {Body}", response.StatusCode, content);
                return new AiProviderResult();
            }

            using var doc = JsonDocument.Parse(content);
            var text = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "[]";
            var tokens = doc.RootElement.TryGetProperty("usage", out var usage) && usage.TryGetProperty("total_tokens", out var total)
                ? total.GetInt32()
                : text.Length / 4;

            if (text.Contains("insightType", StringComparison.OrdinalIgnoreCase))
            {
                var insights = JsonSerializer.Deserialize<List<AiGeneratedInsight>>(text, JsonOptions) ?? [];
                return new AiProviderResult { Insights = insights, TokensUsed = tokens };
            }

            var recommendations = JsonSerializer.Deserialize<List<AiGeneratedRecommendation>>(text, JsonOptions) ?? [];
            return new AiProviderResult { Recommendations = recommendations, TokensUsed = tokens };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI provider failed");
            return new AiProviderResult();
        }
    }
}
