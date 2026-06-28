using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gym.Infrastructure.Persistence;

/// <summary>
/// Clears tenant business data while preserving migrations, roles, privileges, menus catalog, and SaaS plan catalog.
/// </summary>
public static class DemoDatabaseReset
{
    public static async Task ResetBusinessDataAsync(
        IServiceProvider serviceProvider,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        logger.LogWarning("Resetting all tenant business data (preserving platform master data)...");

        await db.Database.ExecuteSqlRawAsync(BusinessResetSql, cancellationToken);

        logger.LogInformation("Tenant business data reset completed.");
    }

    private const string BusinessResetSql = """
        SET NOCOUNT ON;
        SET XACT_ABORT ON;
        BEGIN TRY
            BEGIN TRANSACTION;

            /* Child / junction tables first */
            IF OBJECT_ID('dbo.SaasSubscriptionPayments') IS NOT NULL DELETE FROM dbo.SaasSubscriptionPayments;
            IF OBJECT_ID('dbo.GymSubscriptions') IS NOT NULL DELETE FROM dbo.GymSubscriptions;
            IF OBJECT_ID('dbo.Invoices') IS NOT NULL DELETE FROM dbo.Invoices;
            IF OBJECT_ID('dbo.TrainerCommissions') IS NOT NULL DELETE FROM dbo.TrainerCommissions;
            IF OBJECT_ID('dbo.Payments') IS NOT NULL DELETE FROM dbo.Payments;
            IF OBJECT_ID('dbo.Memberships') IS NOT NULL DELETE FROM dbo.Memberships;
            IF OBJECT_ID('dbo.SlotBookings') IS NOT NULL DELETE FROM dbo.SlotBookings;
            IF OBJECT_ID('dbo.BookingWaitlist') IS NOT NULL DELETE FROM dbo.BookingWaitlist;
            IF OBJECT_ID('dbo.ClassSchedules') IS NOT NULL DELETE FROM dbo.ClassSchedules;
            IF OBJECT_ID('dbo.TrainerAvailability') IS NOT NULL DELETE FROM dbo.TrainerAvailability;
            IF OBJECT_ID('dbo.BookingSettings') IS NOT NULL DELETE FROM dbo.BookingSettings;
            IF OBJECT_ID('dbo.MemberAttendance') IS NOT NULL DELETE FROM dbo.MemberAttendance;
            IF OBJECT_ID('dbo.TrainerAttendance') IS NOT NULL DELETE FROM dbo.TrainerAttendance;
            IF OBJECT_ID('dbo.LeadActivities') IS NOT NULL DELETE FROM dbo.LeadActivities;
            IF OBJECT_ID('dbo.LeadTrials') IS NOT NULL DELETE FROM dbo.LeadTrials;
            IF OBJECT_ID('dbo.LeadFollowUps') IS NOT NULL DELETE FROM dbo.LeadFollowUps;
            IF OBJECT_ID('dbo.Leads') IS NOT NULL DELETE FROM dbo.Leads;
            IF OBJECT_ID('dbo.AssignedDietPlans') IS NOT NULL DELETE FROM dbo.AssignedDietPlans;
            IF OBJECT_ID('dbo.DietPlanItems') IS NOT NULL DELETE FROM dbo.DietPlanItems;
            IF OBJECT_ID('dbo.DietPlans') IS NOT NULL DELETE FROM dbo.DietPlans;
            IF OBJECT_ID('dbo.DietCategories') IS NOT NULL DELETE FROM dbo.DietCategories;
            IF OBJECT_ID('dbo.AssignedWorkoutPlans') IS NOT NULL DELETE FROM dbo.AssignedWorkoutPlans;
            IF OBJECT_ID('dbo.MemberWorkoutProgress') IS NOT NULL DELETE FROM dbo.MemberWorkoutProgress;
            IF OBJECT_ID('dbo.WorkoutPlanExercises') IS NOT NULL DELETE FROM dbo.WorkoutPlanExercises;
            IF OBJECT_ID('dbo.WorkoutPlans') IS NOT NULL DELETE FROM dbo.WorkoutPlans;
            IF OBJECT_ID('dbo.ExerciseLibrary') IS NOT NULL DELETE FROM dbo.ExerciseLibrary;
            IF OBJECT_ID('dbo.ExerciseCategories') IS NOT NULL DELETE FROM dbo.ExerciseCategories;
            IF OBJECT_ID('dbo.MemberProgress') IS NOT NULL DELETE FROM dbo.MemberProgress;
            IF OBJECT_ID('dbo.MemberGoals') IS NOT NULL DELETE FROM dbo.MemberGoals;
            IF OBJECT_ID('dbo.MemberProgressPhotos') IS NOT NULL DELETE FROM dbo.MemberProgressPhotos;
            IF OBJECT_ID('dbo.WaterIntakeLogs') IS NOT NULL DELETE FROM dbo.WaterIntakeLogs;
            IF OBJECT_ID('dbo.WorkoutTracking') IS NOT NULL DELETE FROM dbo.WorkoutTracking;
            IF OBJECT_ID('dbo.DietTracking') IS NOT NULL DELETE FROM dbo.DietTracking;
            IF OBJECT_ID('dbo.MemberReferrals') IS NOT NULL DELETE FROM dbo.MemberReferrals;
            IF OBJECT_ID('dbo.MemberFeedback') IS NOT NULL DELETE FROM dbo.MemberFeedback;
            IF OBJECT_ID('dbo.MemberQrTokens') IS NOT NULL DELETE FROM dbo.MemberQrTokens;
            IF OBJECT_ID('dbo.MemberFiles') IS NOT NULL DELETE FROM dbo.MemberFiles;
            IF OBJECT_ID('dbo.TrainerFiles') IS NOT NULL DELETE FROM dbo.TrainerFiles;
            IF OBJECT_ID('dbo.GymWebsiteGallery') IS NOT NULL DELETE FROM dbo.GymWebsiteGallery;
            IF OBJECT_ID('dbo.GymWebsiteTestimonials') IS NOT NULL DELETE FROM dbo.GymWebsiteTestimonials;
            IF OBJECT_ID('dbo.GymWebsiteSections') IS NOT NULL DELETE FROM dbo.GymWebsiteSections;
            IF OBJECT_ID('dbo.GymWebsitePages') IS NOT NULL DELETE FROM dbo.GymWebsitePages;
            IF OBJECT_ID('dbo.GymWebsiteSettings') IS NOT NULL DELETE FROM dbo.GymWebsiteSettings;
            IF OBJECT_ID('dbo.WhiteLabelEmailTemplates') IS NOT NULL DELETE FROM dbo.WhiteLabelEmailTemplates;
            IF OBJECT_ID('dbo.WhiteLabelMobileSettings') IS NOT NULL DELETE FROM dbo.WhiteLabelMobileSettings;
            IF OBJECT_ID('dbo.WhiteLabelSettings') IS NOT NULL DELETE FROM dbo.WhiteLabelSettings;
            IF OBJECT_ID('dbo.Files') IS NOT NULL DELETE FROM dbo.Files;
            IF OBJECT_ID('dbo.NotificationLogs') IS NOT NULL DELETE FROM dbo.NotificationLogs;
            IF OBJECT_ID('dbo.NotificationTemplates') IS NOT NULL DELETE FROM dbo.NotificationTemplates;
            IF OBJECT_ID('dbo.NotificationSettings') IS NOT NULL DELETE FROM dbo.NotificationSettings;
            IF OBJECT_ID('dbo.PushNotifications') IS NOT NULL DELETE FROM dbo.PushNotifications;
            IF OBJECT_ID('dbo.DeviceTokens') IS NOT NULL DELETE FROM dbo.DeviceTokens;
            IF OBJECT_ID('dbo.NotificationPreferences') IS NOT NULL DELETE FROM dbo.NotificationPreferences;
            IF OBJECT_ID('dbo.UserInAppNotifications') IS NOT NULL DELETE FROM dbo.UserInAppNotifications;
            IF OBJECT_ID('dbo.Expenses') IS NOT NULL DELETE FROM dbo.Expenses;
            IF OBJECT_ID('dbo.ExpenseCategories') IS NOT NULL DELETE FROM dbo.ExpenseCategories;
            IF OBJECT_ID('dbo.Payrolls') IS NOT NULL DELETE FROM dbo.Payrolls;
            IF OBJECT_ID('dbo.RevenueReports') IS NOT NULL DELETE FROM dbo.RevenueReports;
            IF OBJECT_ID('dbo.Coupons') IS NOT NULL DELETE FROM dbo.Coupons;
            IF OBJECT_ID('dbo.PaymentMethods') IS NOT NULL DELETE FROM dbo.PaymentMethods;
            IF OBJECT_ID('dbo.BranchTransferHistory') IS NOT NULL DELETE FROM dbo.BranchTransferHistory;
            IF OBJECT_ID('dbo.BranchTargets') IS NOT NULL DELETE FROM dbo.BranchTargets;
            IF OBJECT_ID('dbo.BranchAnnouncements') IS NOT NULL DELETE FROM dbo.BranchAnnouncements;
            IF OBJECT_ID('dbo.BranchManagers') IS NOT NULL DELETE FROM dbo.BranchManagers;
            IF OBJECT_ID('dbo.AiRecommendations') IS NOT NULL DELETE FROM dbo.AiRecommendations;
            IF OBJECT_ID('dbo.AiInsights') IS NOT NULL DELETE FROM dbo.AiInsights;
            IF OBJECT_ID('dbo.MemberRiskScores') IS NOT NULL DELETE FROM dbo.MemberRiskScores;
            IF OBJECT_ID('dbo.AiGenerationLogs') IS NOT NULL DELETE FROM dbo.AiGenerationLogs;
            IF OBJECT_ID('dbo.Members') IS NOT NULL DELETE FROM dbo.Members;
            IF OBJECT_ID('dbo.Trainers') IS NOT NULL DELETE FROM dbo.Trainers;
            IF OBJECT_ID('dbo.Branches') IS NOT NULL DELETE FROM dbo.Branches;
            IF OBJECT_ID('dbo.GymBranches') IS NOT NULL DELETE FROM dbo.GymBranches;
            IF OBJECT_ID('dbo.AnalyticsDashboardCache') IS NOT NULL DELETE FROM dbo.AnalyticsDashboardCache;
            IF OBJECT_ID('dbo.AuditLogs') IS NOT NULL DELETE FROM dbo.AuditLogs;
            IF OBJECT_ID('dbo.ActivityLogs') IS NOT NULL DELETE FROM dbo.ActivityLogs;
            IF OBJECT_ID('dbo.GymMenus') IS NOT NULL DELETE FROM dbo.GymMenus;
            IF OBJECT_ID('dbo.MembershipPlans') IS NOT NULL DELETE FROM dbo.MembershipPlans;

            IF OBJECT_ID('dbo.RefreshTokens') IS NOT NULL
                DELETE FROM dbo.RefreshTokens
                WHERE UserId IN (SELECT Id FROM dbo.Users WHERE GymId IS NOT NULL);

            IF OBJECT_ID('dbo.UserLoginSessions') IS NOT NULL
                DELETE FROM dbo.UserLoginSessions
                WHERE UserId IN (SELECT Id FROM dbo.Users WHERE GymId IS NOT NULL);

            IF OBJECT_ID('dbo.UserRoles') IS NOT NULL
                DELETE FROM dbo.UserRoles
                WHERE UserId IN (SELECT Id FROM dbo.Users WHERE GymId IS NOT NULL);

            IF OBJECT_ID('dbo.Users') IS NOT NULL
                DELETE FROM dbo.Users WHERE GymId IS NOT NULL;

            IF OBJECT_ID('dbo.Gyms') IS NOT NULL DELETE FROM dbo.Gyms;

            COMMIT TRANSACTION;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
            THROW;
        END CATCH
        """;
}
