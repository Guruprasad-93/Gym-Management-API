using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Gym.API.IntegrationTests;

public class PlanCatalogPricingTests : IClassFixture<GymWebApplicationFactory>
{
    private readonly GymWebApplicationFactory _factory;

    public PlanCatalogPricingTests(GymWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetPlanCatalog_ActivePricingOptions_AreMarkedActive()
    {
        await _factory.EnsureDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPlanManagementRepository>();

        var catalog = await repository.GetPlanCatalogAsync(publicOnly: true);

        Assert.NotEmpty(catalog.Plans);

        var purchasable = catalog.Plans.Where(p => !p.PlanCode.Contains("Trial", StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.NotEmpty(purchasable);

        foreach (var plan in purchasable)
        {
            Assert.NotEmpty(plan.PricingOptions);
            Assert.All(plan.PricingOptions, option => Assert.True(option.IsActive, $"Plan {plan.PlanCode} pricing option {option.PricingOptionId} should be active."));
        }
    }
}
