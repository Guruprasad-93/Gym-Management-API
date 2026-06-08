using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests.Infrastructure;

public class WhiteLabelFixture : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;

    public HttpClient AdminClient { get; private set; } = null!;
    public HttpClient SuperAdminClient { get; private set; } = null!;
    public HttpClient AnonClient { get; private set; } = null!;

    public WhiteLabelFixture(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        AdminClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        SuperAdminClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        AnonClient = _factory.CreateClient();
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(AdminClient, "admin@fitzone-demo.com", "Demo@123");
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(SuperAdminClient, "superadmin@gym.com", "SuperAdmin@123");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
