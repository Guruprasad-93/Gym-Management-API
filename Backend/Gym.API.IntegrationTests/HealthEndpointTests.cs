using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class HealthEndpointTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public HealthEndpointTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Health_ReturnsHealthy_WithSqlCheck()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sqlserver", json, StringComparison.OrdinalIgnoreCase);
    }
}
