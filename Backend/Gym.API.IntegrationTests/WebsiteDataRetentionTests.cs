using System.Net;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.Constants;
using Gym.Application.DTOs.Saas;
using Gym.Application.DTOs.Website;
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
public class WebsiteDataRetentionTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private Guid _gymId;

    public WebsiteDataRetentionTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _gymId = _factory.DemoGymId;

        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IGymMenuService>().SeedMenusForGymAsync(_gymId, null);

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(
            _client,
            DemoDataSeeder.DemoGymAdminLoginIdentifier,
            DemoDataSeeder.DefaultDemoPassword,
            _gymId);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Downgrade_PreservesWebsiteAndWhiteLabelData_ReUpgradeRestoresApiAccess()
    {
        using var scope = _factory.Services.CreateScope();
        var saasRepository = scope.ServiceProvider.GetRequiredService<ISaasSubscriptionRepository>();
        var websiteRepository = scope.ServiceProvider.GetRequiredService<IWebsiteRepository>();
        var whiteLabelRepository = scope.ServiceProvider.GetRequiredService<IWhiteLabelRepository>();
        var graceDays = scope.ServiceProvider.GetRequiredService<IOptions<SaasSubscriptionSettings>>().Value.GracePeriodDays;
        var plans = await saasRepository.GetAllPlansAsync();
        var premiumPro = RequirePlan(plans, SaasPlanCodes.PremiumPro);
        var basic = RequirePlan(plans, SaasPlanCodes.Basic);

        await saasRepository.UpdateSubscriptionPlanAsync(
            _gymId, premiumPro.Id, SaasBillingCycles.Monthly, null, premiumPro.MonthlyPrice,
            null, null, null, graceDays);

        var pageSlug = $"retention-{Guid.NewGuid():N}"[..20];
        await websiteRepository.UpsertSettingsAsync(_gymId, new UpsertGymWebsiteSettingsDto
        {
            WebsiteTitle = "Retention Test Site",
            WebsiteSlug = pageSlug
        });
        await websiteRepository.SetPublishedAsync(_gymId, true);
        var cmsPageSlug = $"retention-page-{Guid.NewGuid():N}"[..24];
        var pageId = await websiteRepository.CreatePageAsync(_gymId, new CreateGymWebsitePageDto
        {
            PageName = "Retention Page",
            Slug = cmsPageSlug,
            PageContent = "<p>CMS content preserved</p>",
            DisplayOrder = 1
        });
        var sectionId = await websiteRepository.CreateSectionAsync(_gymId, new CreateGymWebsiteSectionDto
        {
            SectionType = "Hero",
            Title = "Retention Section",
            DisplayOrder = 1
        });
        await whiteLabelRepository.UpsertSettingsAsync(_gymId, new UpsertWhiteLabelSettingsDto
        {
            BrandName = "Retention Brand",
            IsWhiteLabelEnabled = true
        });

        var pagesBefore = await websiteRepository.GetPagesAsync(_gymId);
        var sectionsBefore = await websiteRepository.GetSectionsAsync(_gymId);
        var whiteLabelBefore = await whiteLabelRepository.GetSettingsAsync(_gymId);

        await saasRepository.UpdateSubscriptionPlanAsync(
            _gymId, basic.Id, SaasBillingCycles.Monthly, null, basic.MonthlyPrice,
            null, null, null, graceDays);

        Assert.Equal(HttpStatusCode.Forbidden, (await _client.GetAsync("/api/website/settings")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.GetAsync("/api/white-label/settings")).StatusCode);

        var pagesAfterDowngrade = await websiteRepository.GetPagesAsync(_gymId);
        var sectionsAfterDowngrade = await websiteRepository.GetSectionsAsync(_gymId);
        var whiteLabelAfterDowngrade = await whiteLabelRepository.GetSettingsAsync(_gymId);

        Assert.Equal(pagesBefore.Count, pagesAfterDowngrade.Count);
        Assert.Contains(pagesAfterDowngrade, p => p.Id == pageId);
        Assert.Equal(sectionsBefore.Count, sectionsAfterDowngrade.Count);
        Assert.Contains(sectionsAfterDowngrade, s => s.Id == sectionId);
        Assert.NotNull(whiteLabelAfterDowngrade);
        Assert.Equal(whiteLabelBefore!.BrandName, whiteLabelAfterDowngrade!.BrandName);

        await saasRepository.UpdateSubscriptionPlanAsync(
            _gymId, premiumPro.Id, SaasBillingCycles.Monthly, null, premiumPro.MonthlyPrice,
            null, null, null, graceDays);

        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/website/settings")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/white-label/settings")).StatusCode);

        var pagesAfterReUpgrade = await websiteRepository.GetPagesAsync(_gymId);
        Assert.Contains(pagesAfterReUpgrade, p => p.Id == pageId && p.PageContent!.Contains("CMS content"));
    }

    private static SaasPlanDto RequirePlan(IReadOnlyList<SaasPlanDto> plans, string planCode)
    {
        var plan = plans.FirstOrDefault(p => p.PlanCode.Equals(planCode, StringComparison.OrdinalIgnoreCase))
            ?? plans.FirstOrDefault(p => planCode == SaasPlanCodes.PremiumPro
                && p.PlanCode.Equals("Enterprise", StringComparison.OrdinalIgnoreCase));
        return plan ?? throw new InvalidOperationException($"Plan '{planCode}' was not found.");
    }
}
