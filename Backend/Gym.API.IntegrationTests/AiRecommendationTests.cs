using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.DTOs.Ai;
using Gym.Application.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class AiRecommendationTests : IClassFixture<AiRecommendationFixture>
{
    private readonly GymWebApplicationFactory _factory;
    private readonly AiRecommendationFixture _fixture;

    public AiRecommendationTests(GymWebApplicationFactory factory, AiRecommendationFixture fixture)
    {
        _factory = factory;
        _fixture = fixture;
    }

    [Fact]
    public async Task GetDashboard_ReturnsOk()
    {
        var response = await _fixture.AdminClient.GetAsync("/api/ai/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRecommendations_ReturnsOk()
    {
        var response = await _fixture.AdminClient.GetAsync("/api/ai/recommendations?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMemberRisk_ReturnsOk()
    {
        var response = await _fixture.AdminClient.GetAsync("/api/ai/member-risk?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetLeadScoring_ReturnsOk()
    {
        var response = await _fixture.AdminClient.GetAsync("/api/ai/lead-scoring?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBusinessInsights_ReturnsOk()
    {
        var response = await _fixture.AdminClient.GetAsync("/api/ai/business-insights?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TrainerCanViewRecommendations_ReturnsOk()
    {
        var response = await _fixture.TrainerClient.GetAsync("/api/ai/recommendations?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_CannotAccessAiDashboard()
    {
        var anon = _factory.CreateClient();
        var response = await anon.GetAsync("/api/ai/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void RiskScoring_CalculatesHighChurnForInactiveMember()
    {
        var risk = AiRecommendationService.CalculateMemberRisk(new MemberAiAnalysisContextDto
        {
            FullName = "Test Member",
            AttendanceLast30Days = 1,
            AttendancePrev30Days = 10,
            LastWorkoutDate = DateTime.UtcNow.AddDays(-30),
            MembershipEndDate = DateTime.UtcNow.AddDays(10),
            PaymentsLast6Months = 0,
            TotalGoals = 2,
            CompletedGoals = 0
        });

        Assert.Equal("High", risk.ChurnRisk);
        Assert.True(risk.RenewalProbability < 50);
    }
}
