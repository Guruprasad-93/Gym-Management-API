using Gym.Application.Interfaces;

namespace Gym.API.Middleware;

public class CsrfValidationMiddleware
{
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Get,
        HttpMethods.Head,
        HttpMethods.Options,
        HttpMethods.Trace
    };

    private readonly RequestDelegate _next;
    private readonly bool _disableCsrf;

    public CsrfValidationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _disableCsrf = configuration.GetValue<bool>("Testing:DisableCsrf");
    }

    public async Task InvokeAsync(HttpContext context, IAuthCookieService authCookies)
    {
        if (!authCookies.UseCookieAuth
            || _disableCsrf
            || SafeMethods.Contains(context.Request.Method)
            || IsExemptPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!authCookies.ValidateCsrf(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "CSRF validation failed."
            });
            return;
        }

        await _next(context);
    }

    private static bool IsExemptPath(PathString path)
    {
        var value = path.Value ?? string.Empty;
        return value.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/api/auth/refresh", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/api/auth/forgot-password", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/api/auth/reset-password", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/public/website", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/public/white-label", StringComparison.OrdinalIgnoreCase);
    }
}
