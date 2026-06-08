using Gym.Application.DTOs.Auth;
using Microsoft.AspNetCore.Http;

namespace Gym.Application.Interfaces;

public interface IAuthCookieService
{
    bool UseCookieAuth { get; }

    void SetAuthCookies(HttpResponse response, LoginResponseDto session, bool isProduction);

    void ClearAuthCookies(HttpResponse response);

    string? GetRefreshToken(HttpRequest request);

    string IssueCsrfToken(HttpResponse response, bool isProduction);

    bool ValidateCsrf(HttpRequest request);
}
