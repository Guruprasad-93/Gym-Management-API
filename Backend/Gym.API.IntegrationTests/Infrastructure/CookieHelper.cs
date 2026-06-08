namespace Gym.API.IntegrationTests.Infrastructure;

public static class CookieHelper
{
    public static string? GetSetCookieValue(HttpResponseMessage response, string cookieName)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            return null;

        foreach (var cookie in cookies)
        {
            if (!cookie.StartsWith(cookieName + "=", StringComparison.OrdinalIgnoreCase))
                continue;

            var valuePart = cookie.Split(';')[0];
            return valuePart[(cookieName.Length + 1)..];
        }

        return null;
    }
}
