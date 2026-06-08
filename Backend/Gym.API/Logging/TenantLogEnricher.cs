using Gym.Domain.Constants;
using Microsoft.AspNetCore.Authentication;
using Serilog.Core;
using Serilog.Events;

namespace Gym.API.Logging;

public class TenantLogEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantLogEnricher(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return;

        var userId = httpContext.User.FindFirst(AuthClaimTypes.UserId)?.Value;
        var gymId = httpContext.User.FindFirst(AuthClaimTypes.GymId)?.Value;
        var email = httpContext.User.FindFirst(AuthClaimTypes.Email)?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));

        if (!string.IsNullOrWhiteSpace(gymId))
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("GymId", gymId));

        if (!string.IsNullOrWhiteSpace(email))
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserEmail", email));
    }
}
