using System.Text.Json;
using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace Gym.API.Middleware;

public class SubscriptionAccessMiddleware
{
    public const string SkipMenuAccessKey = "SkipMenuAccessForSubscriptionRenewal";

    private readonly RequestDelegate _next;

    public SubscriptionAccessMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        ISubscriptionAccessService subscriptionAccess,
        ICurrentUserService currentUser)
    {
        if (!ShouldEnforce(context, currentUser))
        {
            await _next(context);
            return;
        }

        var roles = currentUser.Roles.ToList();
        var access = await subscriptionAccess.ResolveAsync(
            currentUser.GymId!.Value,
            roles,
            context.RequestAborted);

        if (access.AccessMode is SubscriptionAccessModes.Active or SubscriptionAccessModes.GracePeriod)
        {
            await _next(context);
            return;
        }

        if (access.AccessMode == SubscriptionAccessModes.ExpiredAdminRenewal)
        {
            if (SubscriptionRenewalApiPaths.IsRenewalPath(context.Request.Path.Value))
            {
                context.Items[SkipMenuAccessKey] = true;
                await _next(context);
                return;
            }

            await WriteForbiddenAsync(
                context,
                access.AccessMode,
                "Your gym subscription has expired. Please renew your subscription to continue.");
            return;
        }

        if (!SubscriptionRenewalApiPaths.IsAuthPath(context.Request.Path.Value))
        {
            await WriteForbiddenAsync(
                context,
                access.AccessMode,
                "Your gym subscription has expired. Please contact your gym administrator.");
            return;
        }

        context.Items[SkipMenuAccessKey] = true;
        await _next(context);
    }

    private static bool ShouldEnforce(HttpContext context, ICurrentUserService currentUser)
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

    private static async Task WriteForbiddenAsync(HttpContext context, string accessMode, string message)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            success = false,
            message,
            data = new { subscriptionAccessMode = accessMode }
        }));
    }
}
