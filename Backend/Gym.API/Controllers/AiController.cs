using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Ai;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiRecommendationService _aiService;

    public AiController(IAiRecommendationService aiService) => _aiService = aiService;

    [HttpGet("dashboard")]
    [RequirePermission(Permissions.ViewAiInsights)]
    public async Task<ActionResult<ApiResponse<AiDashboardDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _aiService.GetDashboardAsync(cancellationToken);
        return Ok(ApiResponse<AiDashboardDto>.Ok(result));
    }

    [HttpGet("recommendations")]
    [RequirePermission(Permissions.ViewAiRecommendations)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<AiRecommendationDto>>>> GetRecommendations(
        [FromQuery] AiRecommendationsQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _aiService.GetRecommendationsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<AiRecommendationDto>>.Ok(result));
    }

    [HttpPut("recommendations/accept")]
    [RequirePermission(Permissions.ViewAiRecommendations)]
    public async Task<ActionResult<ApiResponse<object>>> AcceptRecommendation(
        [FromBody] AcceptAiRecommendationDto dto,
        CancellationToken cancellationToken)
    {
        await _aiService.AcceptRecommendationAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Recommendation accepted."));
    }

    [HttpGet("member-risk")]
    [RequirePermission(Permissions.ViewAiInsights)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<MemberRiskScoreDto>>>> GetMemberRisk(
        [FromQuery] MemberRiskQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _aiService.GetMemberRiskAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<MemberRiskScoreDto>>.Ok(result));
    }

    [HttpGet("lead-scoring")]
    [RequirePermission(Permissions.ViewAiInsights)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<LeadScoreDto>>>> GetLeadScoring(
        [FromQuery] LeadScoringQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _aiService.GetLeadScoringAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<LeadScoreDto>>.Ok(result));
    }

    [HttpGet("business-insights")]
    [RequirePermission(Permissions.ViewAiInsights)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<AiInsightDto>>>> GetBusinessInsights(
        [FromQuery] AiInsightsQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _aiService.GetBusinessInsightsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<AiInsightDto>>.Ok(result));
    }

    [HttpGet("analytics")]
    [RequirePermission(Permissions.ViewAiInsights)]
    public async Task<ActionResult<ApiResponse<AiAnalyticsDto>>> GetAnalytics(CancellationToken cancellationToken)
    {
        var result = await _aiService.GetAnalyticsAsync(cancellationToken);
        return Ok(ApiResponse<AiAnalyticsDto>.Ok(result));
    }
}
