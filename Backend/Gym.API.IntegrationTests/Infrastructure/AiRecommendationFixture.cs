using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests.Infrastructure;

public class AiRecommendationFixture : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;

    public HttpClient AdminClient { get; private set; } = null!;
    public HttpClient TrainerClient { get; private set; } = null!;

    public AiRecommendationFixture(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        AdminClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        TrainerClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(AdminClient, "admin@fitzone-demo.com", "Demo@123");
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(TrainerClient, "trainer1@fitzone-demo.com", "Demo@123");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
