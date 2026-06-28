using System.Net;
using Gym.API.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Gym.Infrastructure.Persistence;
namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class SmokeTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public SmokeTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(
            _client,
            DemoDataSeeder.DemoGymAdminLoginIdentifier,
            "Demo@123");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Smoke_Login_And_ListMembers()
    {
        var members = await _client.GetAsync("/api/members?pageNumber=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, members.StatusCode);
    }

    [Fact]
    public async Task Smoke_AttendanceStatuses()
    {
        var response = await _client.GetAsync("/api/attendance/statuses");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Smoke_MembershipsList()
    {
        var response = await _client.GetAsync("/api/memberships");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Smoke_PaymentsHistory()
    {
        var response = await _client.GetAsync("/api/payments");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Smoke_AuditSearch()
    {
        var response = await _client.GetAsync("/api/audit-logs?pageNumber=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
