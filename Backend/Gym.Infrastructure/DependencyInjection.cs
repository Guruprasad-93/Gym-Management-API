using Gym.Application.Authorization;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Gym.Infrastructure.Authorization;
using Gym.Infrastructure.Persistence;
using Gym.Infrastructure.Repositories;
using Gym.Infrastructure.Security;
using Gym.Infrastructure.Services;
using Gym.Infrastructure.StoredProcedures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gym.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        DapperTypeHandlers.Register();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<PasswordResetSettings>(configuration.GetSection(PasswordResetSettings.SectionName));
        services.Configure<FileStorageSettings>(configuration.GetSection(FileStorageSettings.SectionName));
        services.Configure<CorsSettings>(configuration.GetSection(CorsSettings.SectionName));
        services.Configure<RateLimitSettings>(configuration.GetSection(RateLimitSettings.SectionName));
        services.Configure<AuthCookieSettings>(configuration.GetSection(AuthCookieSettings.SectionName));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<RazorpaySettings>(configuration.GetSection(RazorpaySettings.SectionName));
        if (environment?.IsDevelopment() == true)
        {
            services.PostConfigure<RazorpaySettings>(options =>
            {
                if (!options.Enabled && string.IsNullOrWhiteSpace(options.KeyId))
                {
                    options.Enabled = true;
                    options.UseMockGateway = true;
                    options.KeyId = "rzp_test_mock";
                    options.KeySecret = MockRazorpayGateway.DevKeySecret;
                }
            });
        }
        services.Configure<SaasSubscriptionSettings>(configuration.GetSection(SaasSubscriptionSettings.SectionName));

        services.AddScoped<IAuthCookieService, AuthCookieService>();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IStoredProcedureExecutor, StoredProcedureExecutor>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
        services.AddScoped<IRolePrivilegeRepository, RolePrivilegeRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IGymRepository, GymRepository>();
        services.AddScoped<IGymAdminRepository, GymAdminRepository>();
        services.AddScoped<ITrainerRepository, TrainerRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IMembershipPlanRepository, MembershipPlanRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IInvoicePdfGenerator, InvoicePdfGenerator>();
        services.AddHttpClient<IWhatsAppProvider, WhatsAppProvider>();
        var razorpaySettings = configuration.GetSection(RazorpaySettings.SectionName).Get<RazorpaySettings>() ?? new RazorpaySettings();
        if (environment?.IsDevelopment() == true
            && !razorpaySettings.Enabled
            && string.IsNullOrWhiteSpace(razorpaySettings.KeyId))
        {
            razorpaySettings.Enabled = true;
            razorpaySettings.UseMockGateway = true;
            razorpaySettings.KeyId = "rzp_test_mock";
            razorpaySettings.KeySecret = MockRazorpayGateway.DevKeySecret;
        }

        if (razorpaySettings.UseMockGateway)
        {
            services.AddSingleton<IRazorpayGateway>(_ => new MockRazorpayGateway());
        }
        else
        {
            services.AddHttpClient<IRazorpayGateway, RazorpayGateway>();
        }
        services.Configure<WhatsAppSettings>(configuration.GetSection(WhatsAppSettings.SectionName));
        services.AddHostedService<BackgroundJobs.NotificationBackgroundJob>();
        services.AddHostedService<BackgroundJobs.SaasSubscriptionBackgroundJob>();
        services.AddHostedService<BackgroundJobs.SubscriptionExpiryNotificationBackgroundJob>();
        services.AddHostedService<BackgroundJobs.LeadReminderBackgroundJob>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<ILeadReportExporter, LeadReportExporter>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IPayrollRepository, PayrollRepository>();
        services.AddScoped<IFinancialAnalyticsRepository, FinancialAnalyticsRepository>();
        services.AddScoped<IFinancialReportExporter, FinancialReportExporter>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IClientIpProvider, ClientIpProvider>();
        services.AddScoped<IAuditReportExporter, AuditReportExporter>();
        services.AddScoped<IAttendanceReportExporter, AttendanceReportExporter>();
        services.AddScoped<IDietPlanRepository, DietPlanRepository>();
        services.AddScoped<IDietPlanReportExporter, DietPlanReportExporter>();
        services.AddScoped<IWorkoutPlanRepository, WorkoutPlanRepository>();
        services.AddScoped<IWorkoutPlanReportExporter, WorkoutPlanReportExporter>();
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
        services.AddScoped<IAnalyticsReportExporter, AnalyticsReportExporter>();
        services.AddScoped<ISaasSubscriptionRepository, SaasSubscriptionRepository>();
        services.AddScoped<ISubscriptionNotificationRepository, SubscriptionNotificationRepository>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IMemberSelfServiceRepository, MemberSelfServiceRepository>();
        services.AddScoped<IMemberSelfServiceReportExporter, MemberSelfServiceReportExporter>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IMobilePushRepository, MobilePushRepository>();
        services.AddScoped<IAiRecommendationRepository, AiRecommendationRepository>();
        services.AddHttpClient<IFirebasePushService, FirebasePushService>();
        services.Configure<FirebaseSettings>(configuration.GetSection(FirebaseSettings.SectionName));
        services.AddHostedService<BackgroundJobs.PushNotificationBackgroundJob>();
        services.Configure<AiSettings>(configuration.GetSection(AiSettings.SectionName));
        services.AddHttpClient<Infrastructure.Services.Ai.OpenAiProvider>();
        services.AddScoped<Infrastructure.Services.Ai.MockAiProvider>();
        services.AddScoped<IAiProvider>(sp =>
        {
            var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiSettings>>().Value;
            if (settings.Enabled && string.Equals(settings.Provider, "OpenAI", StringComparison.OrdinalIgnoreCase))
                return sp.GetRequiredService<Infrastructure.Services.Ai.OpenAiProvider>();
            return sp.GetRequiredService<Infrastructure.Services.Ai.MockAiProvider>();
        });
        services.AddHostedService<BackgroundJobs.AiRecommendationBackgroundJob>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingReportExporter, BookingReportExporter>();
        services.AddScoped<IWebsiteRepository, WebsiteRepository>();
        services.AddScoped<IWebsiteReportExporter, WebsiteReportExporter>();
        services.AddScoped<IWhiteLabelRepository, WhiteLabelRepository>();
        services.AddScoped<IGymMenuRepository, GymMenuRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<IPlanManagementRepository, PlanManagementRepository>();
        services.AddHostedService<BackgroundJobs.BookingReminderBackgroundJob>();
        services.AddHostedService<BackgroundJobs.AttendanceAutoCheckoutBackgroundJob>();
        services.AddScoped<IQrCodeGenerator, QrCodeGeneratorService>();
        services.AddScoped<IFileValidator, FileValidator>();
        services.AddScoped<IImageProcessor, ImageProcessor>();
        services.AddSingleton<IFileDownloadUrlSigner, FileDownloadUrlSigner>();

        var fileProvider = configuration.GetSection(FileStorageSettings.SectionName).Get<FileStorageSettings>()?.Provider ?? "Local";
        if (string.Equals(fileProvider, "Azure", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IFileStorageProvider, AzureBlobFileStorageProvider>();
        else
            services.AddSingleton<IFileStorageProvider, LocalFileStorageProvider>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPermissionResolver, PermissionResolver>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, AnyPermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, FeatureAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>();

        return services;
    }
}
