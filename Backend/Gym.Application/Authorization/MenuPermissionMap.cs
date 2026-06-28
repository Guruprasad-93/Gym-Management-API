using Gym.Application.Constants;

namespace Gym.Application.Authorization;

/// <summary>
/// Maps RBAC permissions to tenant menu module codes.
/// </summary>
public static class MenuPermissionMap
{
    private static readonly Dictionary<string, string[]> MenuToPermissions = new(StringComparer.OrdinalIgnoreCase)
    {
        [MenuCodes.Dashboard] = [Permissions.ViewDashboard, Permissions.ViewAnalytics],
        [MenuCodes.Leads] = [Permissions.ViewLeads, Permissions.ManageLeads, Permissions.ConvertLeads, Permissions.ViewLeadAnalytics],
        [MenuCodes.Crm] = [Permissions.ViewLeads, Permissions.ManageLeads, Permissions.ConvertLeads, Permissions.ViewLeadAnalytics],
        [MenuCodes.Members] = [Permissions.ViewMembers, Permissions.CreateMember, Permissions.UpdateMember, Permissions.DeleteMember, Permissions.ViewMemberDetails, Permissions.ViewMemberDashboard, Permissions.ManageMemberGoals, Permissions.TrackMemberProgress, Permissions.SubmitMemberFeedback, Permissions.ViewMemberDiet, Permissions.ViewMemberWorkout, Permissions.InitiateOnlinePayment],
        [MenuCodes.Memberships] = [Permissions.ViewMemberships, Permissions.CreateMembership, Permissions.UpdateMembership, Permissions.RenewMembership],
        [MenuCodes.MembershipPlans] = [Permissions.ViewMemberships, Permissions.UpdateMembership],
        [MenuCodes.Attendance] = [Permissions.ViewAttendance, Permissions.ManageAttendance, Permissions.ViewTrainerAttendance, Permissions.ManageTrainerAttendance],
        [MenuCodes.AttendanceReports] = [Permissions.ViewAttendance, Permissions.ExportAttendanceReports],
        [MenuCodes.Payments] = [Permissions.ViewPayments, Permissions.CreatePayment, Permissions.DownloadInvoice, Permissions.RefundPayment],
        [MenuCodes.Revenue] = [Permissions.ViewRevenue],
        [MenuCodes.DietPlans] = [Permissions.ViewDietPlans, Permissions.ManageDietPlans, Permissions.AssignDietPlan, Permissions.ExportDietPlans],
        [MenuCodes.WorkoutPlans] = [Permissions.ViewWorkoutPlans, Permissions.ManageWorkoutPlans, Permissions.AssignWorkoutPlan, Permissions.ExportWorkoutPlans],
        [MenuCodes.Bookings] = [Permissions.ViewBookings, Permissions.ManageBookings],
        [MenuCodes.ClassSchedules] = [Permissions.ManageSchedules, Permissions.ViewBookings],
        [MenuCodes.BookingAnalytics] = [Permissions.ViewBookingAnalytics],
        [MenuCodes.Notifications] = [Permissions.ViewNotifications, Permissions.ManageNotifications, Permissions.SendNotifications],
        [MenuCodes.MobilePush] = [Permissions.SendNotifications, Permissions.ViewMobileNotifications],
        [MenuCodes.MobileAnalytics] = [Permissions.ViewNotifications, Permissions.ViewMobileNotifications],
        [MenuCodes.RevenueAnalytics] = [Permissions.ViewRevenueAnalytics],
        [MenuCodes.MemberAnalytics] = [Permissions.ViewMemberAnalytics],
        [MenuCodes.AttendanceAnalytics] = [Permissions.ViewAnalytics, Permissions.ViewAttendance],
        [MenuCodes.TrainerAnalytics] = [Permissions.ViewAnalytics, Permissions.ViewTrainers],
        [MenuCodes.Financial] = [Permissions.ViewFinancialAnalytics],
        [MenuCodes.Reports] = [Permissions.ViewAnalytics, Permissions.ViewRevenueAnalytics, Permissions.ViewMemberAnalytics, Permissions.ViewFinancialAnalytics],
        [MenuCodes.Analytics] = [Permissions.ViewAnalytics, Permissions.ViewRevenueAnalytics, Permissions.ViewMemberAnalytics],
        [MenuCodes.Trainers] = [Permissions.ViewTrainers, Permissions.CreateTrainer, Permissions.UpdateTrainer, Permissions.DeleteTrainer, Permissions.AssignMemberToTrainer],
        [MenuCodes.Staff] = [Permissions.ViewGymAdmins, Permissions.CreateGymAdmin, Permissions.UpdateGymAdmin],
        [MenuCodes.Branches] = [Permissions.ViewBranches, Permissions.ManageBranches, Permissions.TransferMembers, Permissions.TransferTrainers],
        [MenuCodes.BranchDashboard] = [Permissions.ViewBranches],
        [MenuCodes.BranchAnalytics] = [Permissions.ViewBranchAnalytics, Permissions.ViewBranches],
        [MenuCodes.BranchTransfers] = [Permissions.TransferMembers, Permissions.TransferTrainers, Permissions.ViewBranches],
        [MenuCodes.BranchTargets] = [Permissions.ManageBranches, Permissions.ViewBranches],
        [MenuCodes.Inventory] = [Permissions.ViewFiles, Permissions.ManageFiles],
        [MenuCodes.Pos] = [Permissions.CreatePayment, Permissions.ViewPayments],
        [MenuCodes.AiInsights] = [Permissions.ViewAiInsights],
        [MenuCodes.AiDashboard] = [Permissions.ViewAiInsights],
        [MenuCodes.AiRecommendations] = [Permissions.ViewAiRecommendations],
        [MenuCodes.WebsiteBuilder] = [Permissions.ViewWebsiteBuilder, Permissions.ManageWebsiteBuilder],
        [MenuCodes.WebsiteAnalytics] = [Permissions.ViewWebsiteAnalytics],
        [MenuCodes.PublicWebsite] = [Permissions.ViewWebsiteBuilder, Permissions.ManageWebsiteBuilder, Permissions.ViewWebsiteAnalytics],
        [MenuCodes.WhiteLabel] = [Permissions.ViewWhiteLabel, Permissions.ManageWhiteLabel],
        [MenuCodes.Subscriptions] = [Permissions.ViewSaasSubscription, Permissions.ManageSaasSubscription],
        [MenuCodes.AuditLogs] = [Permissions.ViewAuditLogs, Permissions.ExportAuditLogs],
        [MenuCodes.Expenses] = [Permissions.ViewExpenses, Permissions.ManageExpenses],
        [MenuCodes.Payroll] = [Permissions.ViewPayroll, Permissions.ManagePayroll],
        [MenuCodes.GymBranding] = [Permissions.ManageGymBranding],
        [MenuCodes.Settings] = [Permissions.ViewExpenses, Permissions.ViewPayroll, Permissions.ManageGymBranding, Permissions.ManageNotifications],
    };

    private static readonly Dictionary<string, string> PermissionToMenu = BuildPermissionToMenu();

    public static string? GetMenuCodeForPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return null;

        return PermissionToMenu.TryGetValue(permission, out var menuCode) ? menuCode : null;
    }

    public static bool UserCanSeeMenu(string menuCode, IEnumerable<string> userPermissions)
    {
        if (!MenuToPermissions.TryGetValue(menuCode, out var required) || required.Length == 0)
            return true;

        var set = userPermissions as HashSet<string> ?? userPermissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return required.Any(set.Contains);
    }

    public static bool IsParentMenu(string menuCode) =>
        menuCode is MenuCodes.Crm or MenuCodes.Memberships or MenuCodes.Attendance or MenuCodes.Bookings
            or MenuCodes.Notifications or MenuCodes.Reports or MenuCodes.Branches or MenuCodes.AiInsights
            or MenuCodes.PublicWebsite or MenuCodes.Settings or MenuCodes.Analytics;

    private static Dictionary<string, string> BuildPermissionToMenu()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (menuCode, permissions) in MenuToPermissions)
        {
            foreach (var permission in permissions)
                map.TryAdd(permission, menuCode);
        }

        return map;
    }
}
