using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.DTOs.Leads;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Gym.Infrastructure.Persistence;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class LeadManagementTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public LeadManagementTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_client, DemoDataSeeder.DemoGymAdminLoginIdentifier, "Demo@123");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateLead_ReturnsCreated()
    {
        var dto = new CreateLeadDto
        {
            FullName = "Integration Test Lead",
            MobileNumber = "9999900001",
            LeadSource = "Website",
            Email = $"lead.test.{Guid.NewGuid():N}@example.com"
        };

        var response = await _client.PostAsJsonAsync("/api/leads", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetLeadsPaged_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/leads?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetLeadDashboard_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/leads/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetLeadAnalytics_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/leads/analytics");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateFollowUp_And_GetPending_ReturnsOk()
    {
        var create = await _client.PostAsJsonAsync("/api/leads", new CreateLeadDto
        {
            FullName = "Follow Up Lead",
            MobileNumber = "9999900002",
            LeadSource = "WalkIn"
        });
        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<ApiEnvelope<LeadDto>>();
        Assert.NotNull(created?.Data);

        var followUp = await _client.PostAsJsonAsync("/api/leads/followup", new CreateLeadFollowUpDto
        {
            LeadId = created!.Data!.Id,
            FollowUpDate = DateTime.UtcNow.AddDays(1),
            FollowUpType = "Call",
            Remarks = "Integration test follow-up"
        });
        Assert.Equal(HttpStatusCode.OK, followUp.StatusCode);

        var pending = await _client.GetAsync("/api/leads/followups/pending");
        Assert.Equal(HttpStatusCode.OK, pending.StatusCode);
    }

    [Fact]
    public async Task ScheduleTrial_ReturnsOk()
    {
        var create = await _client.PostAsJsonAsync("/api/leads", new CreateLeadDto
        {
            FullName = "Trial Lead",
            MobileNumber = "9999900003",
            LeadSource = "Referral"
        });
        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<ApiEnvelope<LeadDto>>();

        var trial = await _client.PostAsJsonAsync("/api/leads/schedule-trial", new ScheduleTrialDto
        {
            LeadId = created!.Data!.Id,
            TrialDate = DateTime.UtcNow.AddDays(2)
        });
        Assert.Equal(HttpStatusCode.OK, trial.StatusCode);

        var today = await _client.GetAsync("/api/leads/trials/today");
        Assert.Equal(HttpStatusCode.OK, today.StatusCode);
    }

    [Fact]
    public async Task GymAdmin_CannotAccessLeads_WithWrongGymId()
    {
        var wrongGymId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/leads?gymId={wrongGymId}&pageNumber=1&pageSize=10");
        Assert.True(
            response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                or HttpStatusCode.BadRequest or HttpStatusCode.NotFound,
            $"Expected tenant rejection but got {response.StatusCode}");
    }

    [Fact]
    public async Task Anonymous_CannotAccessLeads()
    {
        var anon = _factory.CreateClient();
        var response = await anon.GetAsync("/api/leads?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed class ApiEnvelope<T>
    {
        public T? Data { get; set; }
    }
}
