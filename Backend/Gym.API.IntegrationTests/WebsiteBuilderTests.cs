using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.DTOs.Website;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class WebsiteBuilderTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _adminClient = null!;
    private HttpClient _anonClient = null!;
    private string _slug = $"fitzone-{Guid.NewGuid():N}".Substring(0, 20);

    public WebsiteBuilderTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _adminClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        _anonClient = _factory.CreateClient();
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_adminClient, "admin@fitzone-demo.com", "Demo@123");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UpsertSettings_ReturnsOk()
    {
        var response = await _adminClient.PutAsJsonAsync("/api/website/settings", new UpsertGymWebsiteSettingsDto
        {
            WebsiteSlug = _slug,
            WebsiteTitle = "FitZone Public Site",
            WebsiteDescription = "Best gym in town",
            PrimaryColor = "#1565c0"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreatePage_ReturnsOk()
    {
        await UpsertSettings_ReturnsOk();
        var response = await _adminClient.PostAsJsonAsync("/api/website/pages", new CreateGymWebsitePageDto
        {
            PageName = "About Us",
            Slug = $"about-{Guid.NewGuid():N}".Substring(0, 12),
            PageContent = "Welcome",
            DisplayOrder = 1
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PublishWebsite_AllowsPublicAccess()
    {
        await UpsertSettings_ReturnsOk();
        await _adminClient.PostAsync("/api/website/settings/publish", null);
        var response = await _anonClient.GetAsync($"/api/public/website/{_slug}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnpublishedWebsite_ReturnsNotFound()
    {
        var slug = $"draft-{Guid.NewGuid():N}".Substring(0, 16);
        await _adminClient.PutAsJsonAsync("/api/website/settings", new UpsertGymWebsiteSettingsDto { WebsiteSlug = slug, WebsiteTitle = "Draft" });
        await _adminClient.PostAsync("/api/website/settings/unpublish", null);
        var response = await _anonClient.GetAsync($"/api/public/website/{slug}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CaptureLead_CreatesWebsiteLead()
    {
        await UpsertSettings_ReturnsOk();
        await _adminClient.PostAsync("/api/website/settings/publish", null);
        var response = await _anonClient.PostAsJsonAsync("/api/public/website/lead", new PublicWebsiteLeadDto
        {
            WebsiteSlug = _slug,
            Name = "Web Visitor",
            MobileNumber = "9999900101",
            Email = "visitor@example.com",
            InterestedPlan = "Gold Plan"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task BookTrial_ReturnsOk()
    {
        await UpsertSettings_ReturnsOk();
        await _adminClient.PostAsync("/api/website/settings/publish", null);
        var response = await _anonClient.PostAsJsonAsync("/api/public/website/trial-booking", new PublicTrialBookingDto
        {
            WebsiteSlug = _slug,
            Name = "Trial User",
            MobileNumber = "9999900102",
            PreferredDate = DateTime.UtcNow.AddDays(2).Date,
            PreferredTime = new TimeSpan(10, 0, 0)
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWebsiteAnalytics_ReturnsOk()
    {
        var response = await _adminClient.GetAsync("/api/website/analytics?days=30");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWebsiteLeads_ReturnsOk()
    {
        var response = await _adminClient.GetAsync("/api/website/leads?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_CannotAccessAdminWebsiteSettings()
    {
        var response = await _anonClient.GetAsync("/api/website/settings");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSitemap_ReturnsXml()
    {
        await UpsertSettings_ReturnsOk();
        await _adminClient.PostAsync("/api/website/settings/publish", null);
        var response = await _anonClient.GetAsync($"/api/public/website/{_slug}/sitemap");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("urlset", body);
    }
}
