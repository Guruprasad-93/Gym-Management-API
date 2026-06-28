using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.Constants;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class SubscriptionExpiryNotificationRepositoryTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;

    public SubscriptionExpiryNotificationRepositoryTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync() => await _factory.EnsureDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateNotification_WithSameKey_IsIdempotent()
    {
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISubscriptionNotificationRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var gymId = _factory.DemoGymId;
        var adminUser = await userRepository.GetByLoginIdentifierAsync(
            DemoDataSeeder.DemoGymAdminLoginIdentifier);
        Assert.NotNull(adminUser);
        var userId = adminUser!.Id;
        var key = $"integration-test-subscription-{Guid.NewGuid():N}";

        var first = await repository.CreateNotificationAsync(
            gymId,
            userId,
            key,
            SubscriptionNotificationTypes.Expiry7Days,
            "Subscription Reminder",
            "Your subscription expires in 7 days.",
            SubscriptionNotificationSeverity.Info,
            "/gym-admin/renew-subscription",
            false);

        var second = await repository.CreateNotificationAsync(
            gymId,
            userId,
            key,
            SubscriptionNotificationTypes.Expiry7Days,
            "Subscription Reminder",
            "Your subscription expires in 7 days.",
            SubscriptionNotificationSeverity.Info,
            "/gym-admin/renew-subscription",
            false);

        Assert.True(first);
        Assert.False(second);
    }
}
