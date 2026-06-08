using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;

namespace Gym.API.IntegrationTests.Infrastructure;

public static class AuthenticatedClientHelper
{
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        HttpClient client,
        string email,
        string password)
    {
        var csrfResponse = await client.GetAsync("/api/auth/csrf");
        csrfResponse.EnsureSuccessStatusCode();
        var csrf = CookieHelper.GetSetCookieValue(csrfResponse, "XSRF-TOKEN");

        var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email, password })
        };
        if (!string.IsNullOrEmpty(csrf))
            loginRequest.Headers.Add("X-XSRF-TOKEN", csrf);

        var loginResponse = await client.SendAsync(loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        return client;
    }
}
