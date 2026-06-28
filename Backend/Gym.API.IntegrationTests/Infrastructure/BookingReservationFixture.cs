using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Gym.Infrastructure.Persistence;

namespace Gym.API.IntegrationTests.Infrastructure;

public class BookingReservationFixture : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;

    public HttpClient AdminClient { get; private set; } = null!;
    public HttpClient MemberClient { get; private set; } = null!;
    public HttpClient TrainerClient { get; private set; } = null!;

    public BookingReservationFixture(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        AdminClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        MemberClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        TrainerClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(AdminClient, DemoDataSeeder.DemoGymAdminLoginIdentifier, "Demo@123");
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(MemberClient, DemoDataSeeder.DemoMember1LoginIdentifier, "Demo@123");
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(TrainerClient, DemoDataSeeder.DemoTrainer1LoginIdentifier, "Demo@123");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
