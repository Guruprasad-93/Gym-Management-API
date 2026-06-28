using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class LoginIdentifierFormatTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _adminClient = null!;
    private Guid _gymId;

    public LoginIdentifierFormatTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _gymId = _factory.DemoGymId;
        _adminClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(
            _adminClient,
            DemoDataSeeder.DemoGymAdminLoginIdentifier,
            DemoDataSeeder.DefaultDemoPassword,
            _gymId);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_WithUsernameIdentifier_Succeeds()
    {
        await AssertLoginSucceedsAsync(DemoDataSeeder.DemoGymAdminLoginIdentifier);
    }

    [Fact]
    public async Task Login_WithEmailIdentifier_Succeeds()
    {
        var loginId = $"admin.{Guid.NewGuid():N}"[..12] + "@fitzone-demo.com";
        await CreateMemberLoginAsync(loginId);
        await AssertLoginSucceedsAsync(loginId);
    }

    [Fact]
    public async Task Login_WithPhoneIdentifier_Succeeds()
    {
        var loginId = $"9{Random.Shared.Next(100000000, 999999999)}";
        await CreateMemberLoginAsync(loginId);
        await AssertLoginSucceedsAsync(loginId);
    }

    [Fact]
    public async Task Login_WithEmployeeIdIdentifier_Succeeds()
    {
        var loginId = $"EMP{Random.Shared.Next(1000, 9999)}";
        await CreateMemberLoginAsync(loginId);
        await AssertLoginSucceedsAsync(loginId);
    }

    [Fact]
    public async Task Login_WithMemberCodeIdentifier_Succeeds()
    {
        var loginId = $"MEM{Random.Shared.Next(100000, 999999):D6}";
        await CreateMemberLoginAsync(loginId);
        await AssertLoginSucceedsAsync(loginId);
    }

    private async Task CreateMemberLoginAsync(string loginIdentifier)
    {
        var response = await _adminClient.PostAsJsonAsync("/api/members", new
        {
            name = $"Login Test {loginIdentifier}",
            loginIdentifier,
            password = DemoDataSeeder.DefaultDemoPassword,
            joinDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private async Task AssertLoginSucceedsAsync(string loginIdentifier)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(
            client,
            loginIdentifier,
            DemoDataSeeder.DefaultDemoPassword,
            _gymId);

        var session = await client.GetAsync("/api/auth/session");
        Assert.Equal(HttpStatusCode.OK, session.StatusCode);
    }
}
