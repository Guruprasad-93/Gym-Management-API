using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class AuthCookieTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public AuthCookieTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_SetsHttpOnlyCookies_AndValidateSucceeds()
    {
        var csrfResponse = await _client.GetAsync("/api/auth/csrf");
        csrfResponse.EnsureSuccessStatusCode();
        var csrf = CookieHelper.GetSetCookieValue(csrfResponse, "XSRF-TOKEN");

        var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email = "admin@fitzone-demo.com", password = "Demo@123" })
        };
        if (!string.IsNullOrEmpty(csrf))
            loginRequest.Headers.Add("X-XSRF-TOKEN", csrf);

        var loginResponse = await _client.SendAsync(loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Contains("gym_access_token", GetSetCookieNames(loginResponse), StringComparer.OrdinalIgnoreCase);

        var validateResponse = await _client.GetAsync("/api/auth/validate");
        Assert.Equal(HttpStatusCode.OK, validateResponse.StatusCode);
    }

    [Fact]
    public async Task MutatingRequest_WithoutCsrf_ReturnsForbidden()
    {
        var clientNoCsrf = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await clientNoCsrf.GetAsync("/api/auth/csrf");
        var loginResponse = await clientNoCsrf.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "admin@fitzone-demo.com", password = "Demo@123" });
        loginResponse.EnsureSuccessStatusCode();

        var changePassword = await clientNoCsrf.PostAsJsonAsync(
            "/api/auth/change-password",
            new { currentPassword = "wrong", newPassword = "NewPass@12345" });

        Assert.Equal(HttpStatusCode.Forbidden, changePassword.StatusCode);
    }

    [Fact]
    public async Task Session_Endpoint_RefreshesPermissions()
    {
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(
            _client,
            "admin@fitzone-demo.com",
            "Demo@123");

        var sessionResponse = await _client.GetAsync("/api/auth/session");
        sessionResponse.EnsureSuccessStatusCode();
        var json = await sessionResponse.Content.ReadAsStringAsync();
        Assert.Contains("permissions", json, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> GetSetCookieNames(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            yield break;

        foreach (var cookie in cookies)
            yield return cookie.Split('=', 2)[0];
    }
}
