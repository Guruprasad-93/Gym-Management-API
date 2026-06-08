using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.DTOs.WhiteLabel;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class WhiteLabelTests : IClassFixture<WhiteLabelFixture>
{
    private readonly WhiteLabelFixture _fixture;
    private string _subDomain = $"wl-{Guid.NewGuid():N}".Substring(0, 12);

    public WhiteLabelTests(WhiteLabelFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task UpsertSettings_ReturnsOk()
    {
        var response = await _fixture.AdminClient.PutAsJsonAsync("/api/white-label/settings", new UpsertWhiteLabelSettingsDto
        {
            BrandName = "FitZone White Label",
            PrimaryColor = "#1565c0",
            SecondaryColor = "#00838f",
            SubDomain = _subDomain,
            IsWhiteLabelEnabled = true
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSettings_ReturnsSavedBrand()
    {
        await UpsertSettings_ReturnsOk();
        var response = await _fixture.AdminClient.GetAsync("/api/white-label/settings");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<WhiteLabelSettingsDto>>();
        Assert.NotNull(body?.Data);
        Assert.Equal("FitZone White Label", body!.Data!.BrandName);
    }

    [Fact]
    public async Task InvalidBrandName_ReturnsBadRequest()
    {
        var response = await _fixture.AdminClient.PutAsJsonAsync("/api/white-label/settings", new UpsertWhiteLabelSettingsDto
        {
            BrandName = "   ",
            IsWhiteLabelEnabled = true
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InvalidColor_ReturnsBadRequest()
    {
        var response = await _fixture.AdminClient.PutAsJsonAsync("/api/white-label/settings", new UpsertWhiteLabelSettingsDto
        {
            BrandName = "Color Test Gym",
            PrimaryColor = "invalid",
            IsWhiteLabelEnabled = true
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDomain_ReturnsOk()
    {
        await UpsertSettings_ReturnsOk();
        var domain = $"custom-{Guid.NewGuid():N}.example.com";
        var response = await _fixture.AdminClient.PutAsJsonAsync("/api/white-label/domain", new UpdateWhiteLabelDomainDto
        {
            SubDomain = _subDomain,
            CustomDomain = domain
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPreview_ReturnsLoginWebsiteMobile()
    {
        await UpsertSettings_ReturnsOk();
        var response = await _fixture.AdminClient.GetAsync("/api/white-label/preview");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<WhiteLabelPreviewDto>>();
        Assert.NotNull(body?.Data?.Login);
        Assert.NotNull(body?.Data?.Website);
        Assert.NotNull(body?.Data?.Mobile);
    }

    [Fact]
    public async Task PublicLoginBranding_BySubDomain_ReturnsOk()
    {
        await UpsertSettings_ReturnsOk();
        var response = await _fixture.AnonClient.GetAsync($"/api/public/white-label/login-branding?subDomain={_subDomain}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MobileSettings_UpsertAndGet_ReturnsOk()
    {
        await UpsertSettings_ReturnsOk();
        var put = await _fixture.AdminClient.PutAsJsonAsync("/api/white-label/mobile-settings", new UpsertWhiteLabelMobileSettingsDto
        {
            AppName = "FitZone App",
            AndroidPackageName = "com.fitzone.app"
        });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        var get = await _fixture.AdminClient.GetAsync("/api/white-label/mobile-settings");
        get.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task EmailTemplates_CreateAndList_ReturnsOk()
    {
        await UpsertSettings_ReturnsOk();
        var create = await _fixture.AdminClient.PostAsJsonAsync("/api/white-label/email-templates", new UpsertWhiteLabelEmailTemplateDto
        {
            TemplateName = $"Test-{Guid.NewGuid():N}".Substring(0, 8),
            Subject = "Hello {{brandName}}",
            Body = "<p>Welcome</p>"
        });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var list = await _fixture.AdminClient.GetAsync("/api/white-label/email-templates");
        list.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task PlatformDashboard_SuperAdmin_ReturnsOk()
    {
        await UpsertSettings_ReturnsOk();
        var response = await _fixture.SuperAdminClient.GetAsync("/api/platform/white-label/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PlatformDashboard_GymAdmin_Forbidden()
    {
        var response = await _fixture.AdminClient.GetAsync("/api/platform/white-label/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }
}
