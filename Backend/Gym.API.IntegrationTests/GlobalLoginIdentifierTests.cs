using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class GlobalLoginIdentifierTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public GlobalLoginIdentifierTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_WithoutGymId_Succeeds_ForGymAdmin()
    {
        var response = await PostLoginAsync(DemoDataSeeder.DemoGymAdminLoginIdentifier, DemoDataSeeder.DefaultDemoPassword);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var gymId = body.GetProperty("data").GetProperty("gymId").GetString();
        Assert.Equal(_factory.DemoGymId.ToString(), gymId, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WithoutGymId_Succeeds_ForSuperAdmin()
    {
        var response = await PostLoginAsync("superadmin", "SuperAdmin@123");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("data").TryGetProperty("gymId", out var gymIdProp));
        Assert.Equal(JsonValueKind.Null, gymIdProp.ValueKind);
    }

    [Fact]
    public async Task CreateMember_WithDuplicateLoginIdentifier_ReturnsBadRequest()
    {
        var adminClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(
            adminClient,
            DemoDataSeeder.DemoGymAdminLoginIdentifier,
            DemoDataSeeder.DefaultDemoPassword);

        var duplicateId = $"dup_{Guid.NewGuid():N}"[..12];
        var first = await adminClient.PostAsJsonAsync("/api/members", new
        {
            name = "Duplicate Test A",
            loginIdentifier = duplicateId,
            password = DemoDataSeeder.DefaultDemoPassword,
            joinDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
        });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await adminClient.PostAsJsonAsync("/api/members", new
        {
            name = "Duplicate Test B",
            loginIdentifier = duplicateId,
            password = DemoDataSeeder.DefaultDemoPassword,
            joinDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
        });
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    private async Task<HttpResponseMessage> PostLoginAsync(string loginIdentifier, string password)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var csrfResponse = await client.GetAsync("/api/auth/csrf");
        csrfResponse.EnsureSuccessStatusCode();
        var csrf = CookieHelper.GetSetCookieValue(csrfResponse, "XSRF-TOKEN");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { loginIdentifier, password })
        };
        if (!string.IsNullOrEmpty(csrf))
            request.Headers.Add("X-XSRF-TOKEN", csrf);

        return await client.SendAsync(request);
    }
}
