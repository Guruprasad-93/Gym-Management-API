using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class SubscriptionExpiryNotificationTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;

    public SubscriptionExpiryNotificationTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync() => await _factory.EnsureDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GenerateDailyNotifications_DoesNotCreateDuplicates()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ISubscriptionNotificationService>();

        var first = await service.GenerateDailyNotificationsAsync();
        var second = await service.GenerateDailyNotificationsAsync();

        Assert.True(first.SubscriptionsProcessed >= 0);
        Assert.True(second.NotificationsCreated == 0);
        Assert.True(second.NotificationsSkippedDuplicate >= 0);
    }
}
