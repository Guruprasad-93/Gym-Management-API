namespace Gym.Application.Options;

public class AuthCookieSettings
{
    public const string SectionName = "AuthCookies";

    public bool UseCookieAuth { get; set; } = true;
    public string AccessTokenCookieName { get; set; } = "gym_access_token";
    public string RefreshTokenCookieName { get; set; } = "gym_refresh_token";
    public string CsrfCookieName { get; set; } = "XSRF-TOKEN";
    public string CsrfHeaderName { get; set; } = "X-XSRF-TOKEN";
    public string CookiePath { get; set; } = "/";
    public string RefreshCookiePath { get; set; } = "/api/auth";
}
