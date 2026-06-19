using System.Net;
using System.Text.Json;
using FluentValidation;
using Gym.Application.Common;
using Gym.Application.DTOs.Common;
using Microsoft.Data.SqlClient;

namespace Gym.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ValidationException validationEx => (HttpStatusCode.BadRequest,
                string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))),
            BusinessException appEx => ((HttpStatusCode)appEx.StatusCode, appEx.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            InvalidOperationException => (HttpStatusCode.Conflict, exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            SqlException sqlEx when sqlEx.Number >= 50000 => (HttpStatusCode.BadRequest, sqlEx.Message),
            SqlException sqlEx when sqlEx.Number is 2627 or 2601 =>
                (HttpStatusCode.Conflict, UniqueConstraintMessage(sqlEx)),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static string UniqueConstraintMessage(SqlException sqlEx)
    {
        var text = sqlEx.Message;
        if (text.Contains("UX_Branches_Gym_Code", StringComparison.OrdinalIgnoreCase))
            return "A branch with this code already exists. The default branch uses code MAIN — choose a different code.";
        return "A record with this value already exists.";
    }
}
