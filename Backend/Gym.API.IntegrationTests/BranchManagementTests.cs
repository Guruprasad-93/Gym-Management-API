using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.DTOs.Branches;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class BranchManagementTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public BranchManagementTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_client, "admin@fitzone-demo.com", "Demo@123");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetBranches_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/branches/list");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateBranch_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/branches", new CreateBranchDto
        {
            BranchName = "Integration Test Branch",
            BranchCode = "ITB",
            City = "Test City"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetBranchDashboard_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/branches/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBranchAnalytics_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/branches/analytics?months=6");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TransferMember_RequiresValidIds()
    {
        var branches = await _client.GetAsync("/api/branches/list");
        branches.EnsureSuccessStatusCode();
        var branchJson = await branches.Content.ReadFromJsonAsync<ApiEnvelope<List<BranchDto>>>();
        var branchId = branchJson?.Data?.FirstOrDefault()?.BranchId ?? 1;

        var response = await _client.PostAsJsonAsync("/api/branches/transfers/members", new TransferMemberBranchDto
        {
            MemberId = 99999,
            ToBranchId = branchId
        });
        Assert.True(response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound
            or HttpStatusCode.InternalServerError or HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GymAdmin_CannotAccessBranches_WithWrongGymId()
    {
        var wrongGymId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/branches?gymId={wrongGymId}&pageNumber=1&pageSize=10");
        Assert.True(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Anonymous_CannotAccessBranches()
    {
        var anon = _factory.CreateClient();
        var response = await anon.GetAsync("/api/branches/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed class ApiEnvelope<T> { public T? Data { get; set; } }
}
