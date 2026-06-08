using System.Net;
using Gym.API.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class TenantIsolationTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _gymAdminClient = null!;

    public TenantIsolationTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _gymAdminClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
    }

    private async Task EnsureGymAdminAuthenticatedAsync()
    {
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(
            _gymAdminClient,
            "admin@fitzone-demo.com",
            "Demo@123");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GymAdmin_CannotAccessMembers_WithWrongGymId()
    {
        await EnsureGymAdminAuthenticatedAsync();
        var wrongGymId = Guid.NewGuid();
        var response = await _gymAdminClient.GetAsync($"/api/members?gymId={wrongGymId}&pageNumber=1&pageSize=10");
        Assert.True(
            response.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden
                or HttpStatusCode.BadRequest
                or HttpStatusCode.NotFound,
            $"Expected tenant rejection but got {response.StatusCode}");
    }

    [Fact]
    public async Task GymAdmin_GetMembers_WithoutGymId_UsesJwtScope()
    {
        await EnsureGymAdminAuthenticatedAsync();
        var response = await _gymAdminClient.GetAsync("/api/members?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("success", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Anonymous_CannotAccessMembers()
    {
        var anon = _factory.CreateClient();
        var response = await anon.GetAsync("/api/members?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
