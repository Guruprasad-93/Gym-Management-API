using System.Security.Claims;
using System.Text;
using FluentValidation.AspNetCore;
using Gym.API.Json;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Gym.Application.Services;
using Gym.Domain.Constants;
using Gym.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Gym.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new UtcNullableDateTimeJsonConverter());
            });
        services.AddFluentValidationAutoValidation();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // Avoid schema ID clashes when different namespaces define the same DTO name (e.g. TrendPointDto).
            options.CustomSchemaIds(type => type.FullName?.Replace("+", ".", StringComparison.Ordinal) ?? type.Name);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT token (legacy header auth when AuthCookies:UseCookieAuth is false)"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
            options.OperationFilter<LoginRequestSwaggerFilter>();
        });

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<PasswordResetSettings>(configuration.GetSection(PasswordResetSettings.SectionName));
        services.Configure<CorsSettings>(configuration.GetSection(CorsSettings.SectionName));
        services.Configure<RateLimitSettings>(configuration.GetSection(RateLimitSettings.SectionName));
        services.Configure<AuthCookieSettings>(configuration.GetSection(AuthCookieSettings.SectionName));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<ApplicationInsightsSettings>(configuration.GetSection(ApplicationInsightsSettings.SectionName));

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are not configured.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Secret) || jwtSettings.Secret.Length < 32)
            throw new InvalidOperationException("JWT Secret must be at least 32 characters.");

        var cookieSettings = configuration.GetSection(AuthCookieSettings.SectionName).Get<AuthCookieSettings>()
            ?? new AuthCookieSettings();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    NameClaimType = AuthClaimTypes.UserId,
                    RoleClaimType = AuthClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (cookieSettings.UseCookieAuth
                            && context.Request.Cookies.TryGetValue(cookieSettings.AccessTokenCookieName, out var accessToken)
                            && !string.IsNullOrWhiteSpace(accessToken))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var authRepository = context.HttpContext.RequestServices
                            .GetRequiredService<IAuthRepository>();

                        var userIdClaim = context.Principal?.FindFirst(AuthClaimTypes.UserId)?.Value;
                        var sessionIdClaim = context.Principal?.FindFirst(AuthClaimTypes.SessionId)?.Value;
                        var tokenVersionClaim = context.Principal?.FindFirst(AuthClaimTypes.TokenVersion)?.Value;

                        if (!Guid.TryParse(userIdClaim, out var userId) ||
                            !Guid.TryParse(sessionIdClaim, out var sessionId) ||
                            !int.TryParse(tokenVersionClaim, out var tokenVersion))
                        {
                            context.Fail("Invalid token claims.");
                            return;
                        }

                        var isValid = await authRepository.IsSessionActiveAsync(userId, sessionId, tokenVersion);
                        if (!isValid)
                            context.Fail("Session is no longer active.");
                    }
                };
            });

        services.AddAuthorization();
        services.AddAzureForwardedHeaders();
        services.AddAuthRateLimiting(configuration);
        services.AddCorsFromConfiguration(configuration, cookieSettings);
        services.AddGymHealthChecks(configuration);

        var appInsights = configuration.GetSection(ApplicationInsightsSettings.SectionName).Get<ApplicationInsightsSettings>();
        if (appInsights?.Enabled == true && !string.IsNullOrWhiteSpace(appInsights.ConnectionString))
            services.AddApplicationInsightsTelemetry();

        return services;
    }

    private static void AddCorsFromConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        AuthCookieSettings cookieSettings)
    {
        var corsSettings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();
        var origins = corsSettings.AllowedOrigins
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (origins.Length == 0)
            origins = ["http://localhost:4200"];

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();

                if (cookieSettings.UseCookieAuth)
                    policy.AllowCredentials();
            });
        });
    }
}
