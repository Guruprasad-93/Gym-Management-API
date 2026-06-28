using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests;

public class MenuPermissionMapInitializationTests
{
    [Fact]
    public void GetMenuCodeForPermission_DoesNotThrow_AndResolvesKnownPermission()
    {
        var menuCode = MenuPermissionMap.GetMenuCodeForPermission(Permissions.ViewMembers);

        Assert.Equal(MenuCodes.Members, menuCode);
    }

    [Theory]
    [InlineData("/api/files/2/content")]
    [InlineData("/api/files/99/content?g=00000000-0000-0000-0000-000000000001&exp=1&sig=abc")]
    public void ResolveMenuCode_FileContentDownload_IsExcluded(string path)
    {
        Assert.Null(ApiRouteMenuMap.ResolveMenuCode(path));
    }

    [Fact]
    public void ResolveMenuCode_GymLogoMetadata_UsesGymBrandingMenu()
    {
        Assert.Equal(MenuCodes.GymBranding, ApiRouteMenuMap.ResolveMenuCode("/api/files/gym/logo"));
    }
}

[Collection(nameof(IntegrationTestCollection))]
public class LoginIdentifierTenantMenuRegressionTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _tenantClient = null!;
    private HttpClient _superAdminClient = null!;

    public LoginIdentifierTenantMenuRegressionTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();

        _tenantClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        _superAdminClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(
            _tenantClient,
            DemoDataSeeder.DemoGymAdminLoginIdentifier,
            DemoDataSeeder.DefaultDemoPassword,
            _factory.DemoGymId);

        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(
            _superAdminClient,
            "superadmin",
            "SuperAdmin@123",
            gymId: null);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task MyMenus_ReturnsOk()
    {
        var response = await _tenantClient.GetAsync("/api/menus/my-menus");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("enabledMenuCodes", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateMember_WithoutEmail_ReturnsCreated()
    {
        var response = await _tenantClient.PostAsJsonAsync("/api/members", new
        {
            name = $"Member {Guid.NewGuid():N}"[..18],
            loginIdentifier = $"m{Guid.NewGuid():N}"[..12],
            password = "Test@12345",
            joinDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateTrainer_WithoutEmail_ReturnsCreated()
    {
        var response = await _tenantClient.PostAsJsonAsync("/api/trainers", new
        {
            name = $"Trainer {Guid.NewGuid():N}"[..19],
            loginIdentifier = $"t{Guid.NewGuid():N}"[..12],
            password = "Test@12345"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateGymAdmin_WithoutEmail_ReturnsCreated()
    {
        var response = await _superAdminClient.PostAsJsonAsync("/api/gym-admins", new
        {
            gymId = DemoDataSeeder.DemoGymId,
            name = $"Gym Admin {Guid.NewGuid():N}"[..20],
            loginIdentifier = $"g{Guid.NewGuid():N}"[..12],
            generateTemporaryPassword = true
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
