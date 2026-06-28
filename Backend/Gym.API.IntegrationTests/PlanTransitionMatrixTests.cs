using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.Constants;
using Gym.Application.DTOs.Saas;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Gym.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace Gym.API.IntegrationTests;

/// <summary>
/// Full plan transition matrix: session, menus (sidebar proxy), API access, branding.
/// Route guards are Angular-only; menu codes + API 403/200 prove equivalent entitlement state.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public class PlanTransitionMatrixTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private readonly ITestOutputHelper _output;
    private HttpClient _client = null!;
    private Guid _gymId;

    public PlanTransitionMatrixTests(GymWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

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

    [Theory]
    [InlineData(SaasPlanCodes.Basic, SaasPlanCodes.Premium)]
    [InlineData(SaasPlanCodes.Basic, SaasPlanCodes.PremiumPro)]
    [InlineData(SaasPlanCodes.Premium, SaasPlanCodes.PremiumPro)]
    [InlineData(SaasPlanCodes.PremiumPro, SaasPlanCodes.Premium)]
    [InlineData(SaasPlanCodes.Premium, SaasPlanCodes.Basic)]
    [InlineData(SaasPlanCodes.PremiumPro, SaasPlanCodes.Basic)]
    public async Task PlanTransition_UpdatesEntitlementsImmediately(string fromPlanCode, string toPlanCode)
    {
        await SetPlanAsync(fromPlanCode);
        var beforeSession = await GetSessionWithoutReLoginAsync();

        await SetPlanAsync(toPlanCode);
        var afterSession = await GetSessionWithoutReLoginAsync();

        var expected = PlanProfile.For(toPlanCode);
        _output.WriteLine($"Transition {fromPlanCode} → {toPlanCode}");

        AssertPlanProfile(afterSession, expected, toPlanCode);
        await AssertApiAccessAsync(expected);
        await AssertBrandingAsync(expected);

        if (!string.Equals(fromPlanCode, toPlanCode, StringComparison.OrdinalIgnoreCase))
        {
            Assert.NotEqual(
                beforeSession.EnabledFeatureCodes.OrderBy(c => c).ToArray(),
                afterSession.EnabledFeatureCodes.OrderBy(c => c).ToArray());
        }
    }

    [Fact]
    public async Task SessionRefresh_DoesNotRequireReLogin_AfterPlanChange()
    {
        await SetPlanAsync(SaasPlanCodes.Basic);
        var session1 = await GetSessionWithoutReLoginAsync();

        await SetPlanAsync(SaasPlanCodes.PremiumPro);
        var session2 = await GetSessionWithoutReLoginAsync();

        Assert.Contains(session2.EnabledFeatureCodes, c => c.Equals("WEBSITE_BUILDER", StringComparison.OrdinalIgnoreCase));
        Assert.False(session2.ShowPoweredBy);
        Assert.False(string.IsNullOrWhiteSpace(session2.PlatformProductName));
        Assert.NotEmpty(session1.EnabledFeatureCodes);
    }

    private async Task SetPlanAsync(string planCode)
    {
        using var scope = _factory.Services.CreateScope();
        var saasRepository = scope.ServiceProvider.GetRequiredService<ISaasSubscriptionRepository>();
        var graceDays = scope.ServiceProvider.GetRequiredService<IOptions<SaasSubscriptionSettings>>().Value.GracePeriodDays;
        var plans = await saasRepository.GetAllPlansAsync();
        var plan = RequirePlan(plans, planCode);
        await saasRepository.UpdateSubscriptionPlanAsync(
            _gymId, plan.Id, SaasBillingCycles.Monthly, null, plan.MonthlyPrice,
            null, null, null, graceDays);
    }

    private static void AssertPlanProfile(SessionSnapshot session, PlanProfile expected, string planCode)
    {
        AssertFeaturePresence(session, "WHITE_LABEL", expected.HasWhiteLabel);
        AssertFeaturePresence(session, "WEBSITE_BUILDER", expected.HasWebsiteBuilder);
        Assert.Equal(expected.ShowPoweredBy, session.ShowPoweredBy);

        AssertMenuPresence(session, MenuCodes.WhiteLabel, expected.HasWhiteLabel);
        AssertMenuPresence(session, MenuCodes.WebsiteBuilder, expected.HasWebsiteBuilder);

        if (expected.HasWhiteLabel)
            Assert.Contains(session.EnabledMenuCodes, c => c.Equals(MenuCodes.WhiteLabel, StringComparison.OrdinalIgnoreCase));
        else
            Assert.DoesNotContain(session.EnabledMenuCodes, c => c.Equals(MenuCodes.WhiteLabel, StringComparison.OrdinalIgnoreCase));

        if (expected.HasWebsiteBuilder)
            Assert.Contains(session.EnabledMenuCodes, c => c.Equals(MenuCodes.WebsiteBuilder, StringComparison.OrdinalIgnoreCase));
        else
            Assert.DoesNotContain(session.EnabledMenuCodes, c => c.Equals(MenuCodes.WebsiteBuilder, StringComparison.OrdinalIgnoreCase));

        _ = planCode;
    }

    private async Task AssertApiAccessAsync(PlanProfile expected)
    {
        var whiteLabel = await _client.GetAsync("/api/white-label/settings");
        Assert.Equal(expected.WhiteLabelApiStatus, whiteLabel.StatusCode);

        var website = await _client.GetAsync("/api/website/settings");
        Assert.Equal(expected.WebsiteApiStatus, website.StatusCode);
    }

    private async Task AssertBrandingAsync(PlanProfile expected)
    {
        var response = await _client.GetAsync("/api/white-label/app-branding");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<BrandingPayload>>();
        Assert.NotNull(envelope?.Data);
        Assert.False(string.IsNullOrWhiteSpace(envelope.Data.BrandName), "Gym name must remain visible on all plans.");
        Assert.Equal(expected.ShowPoweredBy, envelope.Data.ShowPoweredBy);
    }

    private async Task<SessionSnapshot> GetSessionWithoutReLoginAsync()
    {
        var response = await _client.GetAsync("/api/auth/session");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<SessionSnapshot>>();
        Assert.NotNull(envelope?.Data);
        return envelope.Data;
    }

    private static void AssertFeaturePresence(SessionSnapshot session, string featureCode, bool shouldHave)
    {
        if (shouldHave)
            Assert.Contains(session.EnabledFeatureCodes, c => c.Equals(featureCode, StringComparison.OrdinalIgnoreCase));
        else
            Assert.DoesNotContain(session.EnabledFeatureCodes, c => c.Equals(featureCode, StringComparison.OrdinalIgnoreCase));
    }

    private static void AssertMenuPresence(SessionSnapshot session, string menuCode, bool shouldHave)
    {
        if (shouldHave)
            Assert.Contains(session.EnabledMenuCodes, c => c.Equals(menuCode, StringComparison.OrdinalIgnoreCase));
        else
            Assert.DoesNotContain(session.EnabledMenuCodes, c => c.Equals(menuCode, StringComparison.OrdinalIgnoreCase));
    }

    private static SaasPlanDto RequirePlan(IReadOnlyList<SaasPlanDto> plans, string planCode)
    {
        var plan = plans.FirstOrDefault(p => p.PlanCode.Equals(planCode, StringComparison.OrdinalIgnoreCase))
            ?? plans.FirstOrDefault(p => planCode == SaasPlanCodes.PremiumPro
                && p.PlanCode.Equals("Enterprise", StringComparison.OrdinalIgnoreCase));
        return plan ?? throw new InvalidOperationException($"Plan '{planCode}' was not found.");
    }

    private sealed class PlanProfile
    {
        public bool HasWhiteLabel { get; init; }
        public bool HasWebsiteBuilder { get; init; }
        public bool ShowPoweredBy { get; init; }
        public HttpStatusCode WhiteLabelApiStatus { get; init; }
        public HttpStatusCode WebsiteApiStatus { get; init; }

        public static PlanProfile For(string planCode) => planCode.ToUpperInvariant() switch
        {
            "BASIC" => new PlanProfile
            {
                HasWhiteLabel = false,
                HasWebsiteBuilder = false,
                ShowPoweredBy = true,
                WhiteLabelApiStatus = HttpStatusCode.Forbidden,
                WebsiteApiStatus = HttpStatusCode.Forbidden
            },
            "PREMIUM" => new PlanProfile
            {
                HasWhiteLabel = true,
                HasWebsiteBuilder = false,
                ShowPoweredBy = false,
                WhiteLabelApiStatus = HttpStatusCode.OK,
                WebsiteApiStatus = HttpStatusCode.Forbidden
            },
            "PREMIUMPRO" or "ENTERPRISE" => new PlanProfile
            {
                HasWhiteLabel = true,
                HasWebsiteBuilder = true,
                ShowPoweredBy = false,
                WhiteLabelApiStatus = HttpStatusCode.OK,
                WebsiteApiStatus = HttpStatusCode.OK
            },
            _ => throw new ArgumentException($"Unknown plan: {planCode}")
        };
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }

    private sealed class SessionSnapshot
    {
        public List<string> EnabledFeatureCodes { get; set; } = [];
        public List<string> EnabledMenuCodes { get; set; } = [];
        public bool ShowPoweredBy { get; set; }
        public string? PlatformProductName { get; set; }
    }

    private sealed class BrandingPayload
    {
        public string BrandName { get; set; } = string.Empty;
        public bool ShowPoweredBy { get; set; }
    }
}
