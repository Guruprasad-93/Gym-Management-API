using System.Text.Json;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace Gym.API.Middleware;

/// <summary>
/// Enforces subscription feature entitlements on API routes (plan features ∩ gym menu settings).
/// RBAC is enforced separately via RequirePermission on controllers.
/// </summary>
public class FeatureAccessMiddleware
{
    private readonly RequestDelegate _next;

    public FeatureAccessMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        IFeatureResolverService featureResolver,
        ICurrentUserService currentUser)
    {
        if (context.Items.ContainsKey(SubscriptionAccessMiddleware.SkipMenuAccessKey))
        {
            await _next(context);
            return;
        }

        if (ShouldCheckFeatureAccess(context, currentUser))
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (!IsGymIdentityBrandingPath(path))
            {
                var featureCode = await featureResolver.ResolveFeatureCodeForPathAsync(
                    path, context.RequestAborted);

                if (!string.IsNullOrWhiteSpace(featureCode))
                {
                    var gymId = currentUser.GymId!.Value;
                    if (!await featureResolver.HasFeatureAsync(gymId, featureCode, context.RequestAborted))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            success = false,
                            message = $"The '{featureCode}' feature is not included in your subscription plan.",
                            data = (object?)null
                        }));
                        return;
                    }
                }
            }
        }

        await _next(context);
    }

    private static bool ShouldCheckFeatureAccess(HttpContext context, ICurrentUserService currentUser)
    {
        if (!currentUser.IsAuthenticated)
            return false;

        if (!currentUser.GymId.HasValue)
            return false;

        if (currentUser.HasRole(RoleNames.SuperAdmin))
            return false;

        return HttpMethods.IsGet(context.Request.Method)
               || HttpMethods.IsPost(context.Request.Method)
               || HttpMethods.IsPut(context.Request.Method)
               || HttpMethods.IsPatch(context.Request.Method)
               || HttpMethods.IsDelete(context.Request.Method);
    }

    private static bool IsGymIdentityBrandingPath(string path) =>
        path.StartsWith("/api/white-label/app-branding", StringComparison.OrdinalIgnoreCase);
}
