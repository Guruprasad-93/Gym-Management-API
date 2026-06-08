using System.Security.Cryptography;
using Gym.Application.DTOs.Auth;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.Security;

public class AuthCookieService : IAuthCookieService
{
    private readonly AuthCookieSettings _settings;
    private readonly JwtSettings _jwt;

    public AuthCookieService(IOptions<AuthCookieSettings> settings, IOptions<JwtSettings> jwt)
    {
        _settings = settings.Value;
        _jwt = jwt.Value;
    }

    public bool UseCookieAuth => _settings.UseCookieAuth;

    public void SetAuthCookies(HttpResponse response, LoginResponseDto session, bool isProduction)
    {
        if (!_settings.UseCookieAuth)
            return;

        var cookieOptions = BuildCookieOptions(session.ExpiresAt, isProduction, _settings.CookiePath);
        response.Cookies.Append(_settings.AccessTokenCookieName, session.Token, cookieOptions);

        var refreshOptions = BuildCookieOptions(session.RefreshTokenExpiresAt, isProduction, _settings.RefreshCookiePath);
        response.Cookies.Append(_settings.RefreshTokenCookieName, session.RefreshToken, refreshOptions);

        IssueCsrfToken(response, isProduction);
    }

    public void ClearAuthCookies(HttpResponse response)
    {
        if (!_settings.UseCookieAuth)
            return;

        response.Cookies.Delete(_settings.AccessTokenCookieName, new CookieOptions { Path = _settings.CookiePath });
        response.Cookies.Delete(_settings.RefreshTokenCookieName, new CookieOptions { Path = _settings.RefreshCookiePath });
        response.Cookies.Delete(_settings.CsrfCookieName, new CookieOptions { Path = _settings.CookiePath });
    }

    public string? GetRefreshToken(HttpRequest request)
    {
        if (!_settings.UseCookieAuth)
            return null;

        return request.Cookies.TryGetValue(_settings.RefreshTokenCookieName, out var token) ? token : null;
    }

    public string IssueCsrfToken(HttpResponse response, bool isProduction)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var options = new CookieOptions
        {
            HttpOnly = false,
            Secure = isProduction,
            SameSite = SameSiteMode.Strict,
            Path = _settings.CookiePath,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays)
        };
        response.Cookies.Append(_settings.CsrfCookieName, token, options);
        return token;
    }

    public bool ValidateCsrf(HttpRequest request)
    {
        if (!_settings.UseCookieAuth)
            return true;

        if (!request.Cookies.TryGetValue(_settings.CsrfCookieName, out var cookieToken)
            || string.IsNullOrWhiteSpace(cookieToken))
            return false;

        if (!request.Headers.TryGetValue(_settings.CsrfHeaderName, out var headerValues))
            return false;

        var headerToken = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(headerToken))
            return false;

        var cookieBytes = System.Text.Encoding.UTF8.GetBytes(cookieToken);
        var headerBytes = System.Text.Encoding.UTF8.GetBytes(headerToken);
        return cookieBytes.Length == headerBytes.Length
            && CryptographicOperations.FixedTimeEquals(cookieBytes, headerBytes);
    }

    private static CookieOptions BuildCookieOptions(DateTime expiresUtc, bool isProduction, string path) => new()
    {
        HttpOnly = true,
        Secure = isProduction,
        SameSite = SameSiteMode.Strict,
        Path = path,
        IsEssential = true,
        Expires = new DateTimeOffset(expiresUtc, TimeSpan.Zero)
    };
}
