using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.Constants;
using Gym.Application.DTOs.Saas;
using Gym.Application.DTOs.WhiteLabel;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Gym.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class FeatureEntitlementDowngradeTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private Guid _gymId;

    public FeatureEntitlementDowngradeTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _gymId = _factory.DemoGymId;

        using (var scope = _factory.Services.CreateScope())
        {
            var gymMenuService = scope.ServiceProvider.GetRequiredService<IGymMenuService>();
            await gymMenuService.SeedMenusForGymAsync(_gymId, null);
        }

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(
            _client,
            DemoDataSeeder.DemoGymAdminLoginIdentifier,
            DemoDataSeeder.DefaultDemoPassword,
            _gymId);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Downgrade_FromPremiumProToBasic_RemovesWhiteLabelAndWebsiteAccessImmediately()
    {
        using var scope = _factory.Services.CreateScope();
        var saasRepository = scope.ServiceProvider.GetRequiredService<ISaasSubscriptionRepository>();
        var graceDays = scope.ServiceProvider.GetRequiredService<IOptions<SaasSubscriptionSettings>>().Value.GracePeriodDays;
        var plans = await saasRepository.GetAllPlansAsync();

        var premiumPro = RequirePlan(plans, SaasPlanCodes.PremiumPro);
        var basic = RequirePlan(plans, SaasPlanCodes.Basic);

        await saasRepository.UpdateSubscriptionPlanAsync(
            _gymId, premiumPro.Id, SaasBillingCycles.Monthly, null, premiumPro.MonthlyPrice,
            null, null, null, graceDays);

        var upgradeSettings = await _client.GetAsync("/api/white-label/settings");
        Assert.Equal(HttpStatusCode.OK, upgradeSettings.StatusCode);

        var upgradeWebsite = await _client.GetAsync("/api/website/settings");
        Assert.Equal(HttpStatusCode.OK, upgradeWebsite.StatusCode);

        await saasRepository.UpdateSubscriptionPlanAsync(
            _gymId, basic.Id, SaasBillingCycles.Monthly, null, basic.MonthlyPrice,
            null, null, null, graceDays);

        var session = await GetSessionAsync();
        Assert.DoesNotContain(session.EnabledFeatureCodes, c => c.Equals("WHITE_LABEL", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(session.EnabledFeatureCodes, c => c.Equals("WEBSITE_BUILDER", StringComparison.OrdinalIgnoreCase));

        var downgradedSettings = await _client.GetAsync("/api/white-label/settings");
        Assert.Equal(HttpStatusCode.Forbidden, downgradedSettings.StatusCode);

        var downgradedWebsite = await _client.GetAsync("/api/website/settings");
        Assert.Equal(HttpStatusCode.Forbidden, downgradedWebsite.StatusCode);

        var brandingResponse = await _client.GetAsync("/api/white-label/app-branding");
        Assert.Equal(HttpStatusCode.OK, brandingResponse.StatusCode);
        var branding = await brandingResponse.Content.ReadFromJsonAsync<ApiEnvelope<BrandingPayload>>();
        Assert.NotNull(branding?.Data);
        Assert.True(branding.Data.ShowPoweredBy);
        Assert.False(string.IsNullOrWhiteSpace(branding.Data.BrandName));
    }

    [Fact]
    public async Task Upgrade_FromBasicToPremium_EnablesWhiteLabelButNotWebsiteBuilder()
    {
        using var scope = _factory.Services.CreateScope();
        var saasRepository = scope.ServiceProvider.GetRequiredService<ISaasSubscriptionRepository>();
        var graceDays = scope.ServiceProvider.GetRequiredService<IOptions<SaasSubscriptionSettings>>().Value.GracePeriodDays;
        var plans = await saasRepository.GetAllPlansAsync();

        var basic = RequirePlan(plans, SaasPlanCodes.Basic);
        var premium = RequirePlan(plans, SaasPlanCodes.Premium);

        await saasRepository.UpdateSubscriptionPlanAsync(
            _gymId, basic.Id, SaasBillingCycles.Monthly, null, basic.MonthlyPrice,
            null, null, null, graceDays);

        Assert.Equal(HttpStatusCode.Forbidden, (await _client.GetAsync("/api/white-label/settings")).StatusCode);

        await saasRepository.UpdateSubscriptionPlanAsync(
            _gymId, premium.Id, SaasBillingCycles.Monthly, null, premium.MonthlyPrice,
            null, null, null, graceDays);

        var session = await GetSessionAsync();
        Assert.Contains(session.EnabledFeatureCodes, c => c.Equals("WHITE_LABEL", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(session.EnabledFeatureCodes, c => c.Equals("WEBSITE_BUILDER", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/white-label/settings")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.GetAsync("/api/website/settings")).StatusCode);

        var brandingResponse = await _client.GetAsync("/api/white-label/app-branding");
        Assert.Equal(HttpStatusCode.OK, brandingResponse.StatusCode);
        var branding = await brandingResponse.Content.ReadFromJsonAsync<ApiEnvelope<BrandingPayload>>();
        Assert.NotNull(branding?.Data);
        Assert.False(branding.Data.ShowPoweredBy);
    }

    [Fact]
    public async Task Downgrade_DisablesWhiteLabelFlag_WithoutDeletingWebsiteData()
    {
        using var scope = _factory.Services.CreateScope();
        var saasRepository = scope.ServiceProvider.GetRequiredService<ISaasSubscriptionRepository>();
        var whiteLabelRepository = scope.ServiceProvider.GetRequiredService<IWhiteLabelRepository>();
        var graceDays = scope.ServiceProvider.GetRequiredService<IOptions<SaasSubscriptionSettings>>().Value.GracePeriodDays;
        var plans = await saasRepository.GetAllPlansAsync();

        var premiumPro = RequirePlan(plans, SaasPlanCodes.PremiumPro);
        var basic = RequirePlan(plans, SaasPlanCodes.Basic);

        await saasRepository.UpdateSubscriptionPlanAsync(
            _gymId, premiumPro.Id, SaasBillingCycles.Monthly, null, premiumPro.MonthlyPrice,
            null, null, null, graceDays);

        await whiteLabelRepository.UpsertSettingsAsync(_gymId, new UpsertWhiteLabelSettingsDto
        {
            BrandName = "Demo White Label",
            IsWhiteLabelEnabled = true
        });

        await whiteLabelRepository.SetEnabledAsync(_gymId, true);

        await saasRepository.UpdateSubscriptionPlanAsync(
            _gymId, basic.Id, SaasBillingCycles.Monthly, null, basic.MonthlyPrice,
            null, null, null, graceDays);

        var settings = await whiteLabelRepository.GetSettingsAsync(_gymId);
        Assert.NotNull(settings);
        Assert.False(settings.IsWhiteLabelEnabled);
    }

    private async Task<SessionPayload> GetSessionAsync()
    {
        var response = await _client.GetAsync("/api/auth/session");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<SessionPayload>>();
        Assert.NotNull(envelope?.Data);
        return envelope.Data;
    }

    private static SaasPlanDto RequirePlan(IReadOnlyList<SaasPlanDto> plans, string planCode)
    {
        var plan = plans.FirstOrDefault(p => p.PlanCode.Equals(planCode, StringComparison.OrdinalIgnoreCase))
            ?? plans.FirstOrDefault(p => planCode == SaasPlanCodes.PremiumPro
                && p.PlanCode.Equals("Enterprise", StringComparison.OrdinalIgnoreCase));
        return plan ?? throw new InvalidOperationException($"Plan '{planCode}' was not found.");
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }

    private sealed class BrandingPayload
    {
        public string BrandName { get; set; } = string.Empty;
        public bool ShowPoweredBy { get; set; }
    }

    private sealed class SessionPayload
    {
        public List<string> EnabledFeatureCodes { get; set; } = [];
        public List<string> EnabledMenuCodes { get; set; } = [];
        public bool ShowPoweredBy { get; set; }
    }
}
