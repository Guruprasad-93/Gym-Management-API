using System.Text.Json;
using Gym.Application.Authorization;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace Gym.API.Middleware;

/// <summary>
/// Enforces per-tenant menu/module access on API requests (403 when module disabled).
/// </summary>
public class GymMenuAccessMiddleware
{
    private readonly RequestDelegate _next;

    public GymMenuAccessMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IGymMenuService menuService, ICurrentUserService currentUser)
    {
        if (context.Items.ContainsKey(SubscriptionAccessMiddleware.SkipMenuAccessKey))
        {
            await _next(context);
            return;
        }

        if (ShouldCheckMenuAccess(context, currentUser))
        {
            var menuCode = ApiRouteMenuMap.ResolveMenuCode(context.Request.Path);
            if (!string.IsNullOrWhiteSpace(menuCode))
            {
                var gymId = currentUser.GymId!.Value;
                if (!await menuService.IsMenuEnabledAsync(gymId, menuCode, context.RequestAborted))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = $"The '{menuCode}' module is not enabled for your gym.",
                        data = (object?)null
                    }));
                    return;
                }
            }
        }

        await _next(context);
    }

    private static bool ShouldCheckMenuAccess(HttpContext context, ICurrentUserService currentUser)
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
}
