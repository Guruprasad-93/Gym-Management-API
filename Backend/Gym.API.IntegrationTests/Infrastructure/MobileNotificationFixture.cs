using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests.Infrastructure;

public class MobileNotificationFixture : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;

    public HttpClient MemberClient { get; private set; } = null!;
    public HttpClient AdminClient { get; private set; } = null!;

    public MobileNotificationFixture(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        MemberClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        AdminClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(MemberClient, "member1@fitzone-demo.com", "Demo@123");
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(AdminClient, "admin@fitzone-demo.com", "Demo@123");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
