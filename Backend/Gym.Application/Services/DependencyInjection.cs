using System.Reflection;
using FluentValidation;
using Gym.Application.Behaviors;
using Gym.Application.Interfaces;
using Gym.Application.Mappings;
using Gym.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Gym.Application.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddAutoMapper(typeof(MappingProfile));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPrivilegeService, PrivilegeService>();
        services.AddScoped<IRolePrivilegeService, RolePrivilegeService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IGymService, GymService>();
        services.AddScoped<IGymAdminService, GymAdminService>();
        services.AddScoped<ITrainerService, TrainerService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IMembershipPlanService, MembershipPlanService>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IDietPlanService, DietPlanService>();
        services.AddScoped<IWorkoutPlanService, WorkoutPlanService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IGymOnboardingService, GymOnboardingService>();
        services.AddScoped<ISaasSubscriptionService, SaasSubscriptionService>();
        services.AddScoped<ITenantLimitService, TenantLimitService>();
        services.AddScoped<IGymBrandingService, GymBrandingService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IFinancialAnalyticsService, FinancialAnalyticsService>();
        services.AddScoped<IMemberSelfService, MemberSelfService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IMobilePushService, MobilePushService>();
        services.AddScoped<IAiRecommendationService, AiRecommendationService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IWebsiteService, WebsiteService>();
        services.AddScoped<IWhiteLabelService, WhiteLabelService>();
        services.AddScoped<IGymMenuService, GymMenuService>();
        services.AddScoped<ISubscriptionAccessService, SubscriptionAccessService>();
        services.AddScoped<ISubscriptionNotificationService, SubscriptionNotificationService>();
        services.AddScoped<IFeatureResolverService, FeatureResolverService>();
        services.AddScoped<IFeatureDependencyService, FeatureDependencyService>();
        services.AddScoped<IPlanManagementService, PlanManagementService>();

        return services;
    }
}
