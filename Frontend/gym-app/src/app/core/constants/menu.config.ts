import { Permissions } from './permissions';
import { Roles } from './roles';

export interface AppMenuItem {
  label: string;
  icon: string;
  route: string;
  roles?: string[];
  permissions?: string[];
}

export const SUPER_ADMIN_MENU: AppMenuItem[] = [
  { label: 'Dashboard', icon: 'dashboard', route: '/super-admin', permissions: [Permissions.ViewDashboard] },
  { label: 'Gyms', icon: 'fitness_center', route: '/super-admin/gyms', permissions: [Permissions.ViewGyms] },
  { label: 'Gym Admins', icon: 'manage_accounts', route: '/super-admin/gym-admins', permissions: [Permissions.ViewGymAdmins] },
  { label: 'Roles', icon: 'admin_panel_settings', route: '/super-admin/roles', permissions: [Permissions.ViewRoles] },
  { label: 'Privileges', icon: 'security', route: '/super-admin/privileges', permissions: [Permissions.ViewPrivileges] },
  { label: 'Role Matrix', icon: 'grid_on', route: '/super-admin/role-matrix', permissions: [Permissions.ViewPermissionMatrix] },
  { label: 'Audit Logs', icon: 'history', route: '/super-admin/audit', permissions: [Permissions.ViewAuditLogs] },
  { label: 'White Label', icon: 'palette', route: '/super-admin/white-label', permissions: [Permissions.ViewPlatformSaas] },
];

export const GYM_ADMIN_MENU: AppMenuItem[] = [
  { label: 'Dashboard', icon: 'dashboard', route: '/gym-admin/dashboard', permissions: [Permissions.ViewAnalytics, Permissions.ViewDashboard] },
  { label: 'Revenue Analytics', icon: 'trending_up', route: '/gym-admin/analytics/revenue', permissions: [Permissions.ViewRevenueAnalytics] },
  { label: 'Member Analytics', icon: 'groups', route: '/gym-admin/analytics/members', permissions: [Permissions.ViewMemberAnalytics] },
  { label: 'Attendance Analytics', icon: 'bar_chart', route: '/gym-admin/analytics/attendance', permissions: [Permissions.ViewAnalytics] },
  { label: 'Trainer Analytics', icon: 'sports', route: '/gym-admin/analytics/trainers', permissions: [Permissions.ViewAnalytics] },
  { label: 'Members', icon: 'groups', route: '/gym-admin/members', permissions: [Permissions.ViewMembers] },
  { label: 'Leads & CRM', icon: 'contact_page', route: '/gym-admin/leads', permissions: [Permissions.ViewLeads] },
  { label: 'Expenses', icon: 'receipt_long', route: '/gym-admin/expenses', permissions: [Permissions.ViewExpenses] },
  { label: 'Payroll', icon: 'payments', route: '/gym-admin/payroll', permissions: [Permissions.ViewPayroll] },
  { label: 'Financial', icon: 'account_balance', route: '/gym-admin/financial', permissions: [Permissions.ViewFinancialAnalytics] },
  { label: 'Trainers', icon: 'sports', route: '/gym-admin/trainers', permissions: [Permissions.ViewTrainers] },
  { label: 'Membership Plans', icon: 'card_membership', route: '/gym-admin/membership-plans', permissions: [Permissions.ViewMemberships] },
  { label: 'Memberships', icon: 'event_note', route: '/gym-admin/memberships', permissions: [Permissions.ViewMemberships] },
  { label: 'Payments', icon: 'payments', route: '/gym-admin/payments', permissions: [Permissions.ViewPayments] },
  { label: 'Notifications', icon: 'notifications', route: '/gym-admin/notifications', permissions: [Permissions.ViewNotifications] },
  { label: 'Revenue', icon: 'trending_up', route: '/gym-admin/revenue', permissions: [Permissions.ViewRevenue] },
  { label: 'Attendance', icon: 'event_available', route: '/gym-admin/attendance', permissions: [Permissions.ViewAttendance] },
  { label: 'Attendance Reports', icon: 'assessment', route: '/gym-admin/attendance/reports', permissions: [Permissions.ViewAttendance] },
  { label: 'Audit Logs', icon: 'history', route: '/gym-admin/audit', permissions: [Permissions.ViewAuditLogs] },
  { label: 'Diet Plans', icon: 'restaurant_menu', route: '/gym-admin/diet-plans', permissions: [Permissions.ViewDietPlans] },
  { label: 'Workout Plans', icon: 'fitness_center', route: '/gym-admin/workout-plans', permissions: [Permissions.ViewWorkoutPlans] },
  { label: 'Subscription', icon: 'subscriptions', route: '/gym-admin/subscription', permissions: [Permissions.ViewSaasSubscription] },
  { label: 'Branding', icon: 'palette', route: '/gym-admin/settings/branding', permissions: [Permissions.ManageGymBranding] },
  { label: 'Branches', icon: 'store', route: '/gym-admin/branches', permissions: [Permissions.ViewBranches] },
  { label: 'Branch Dashboard', icon: 'dashboard', route: '/gym-admin/branches/dashboard', permissions: [Permissions.ViewBranches] },
  { label: 'Branch Analytics', icon: 'leaderboard', route: '/gym-admin/branches/analytics', permissions: [Permissions.ViewBranchAnalytics] },
  { label: 'Branch Transfers', icon: 'swap_horiz', route: '/gym-admin/branches/transfers', permissions: [Permissions.ViewBranches] },
  { label: 'Branch Targets', icon: 'flag', route: '/gym-admin/branches/targets', permissions: [Permissions.ViewBranches] },
  { label: 'Mobile Push', icon: 'phonelink_ring', route: '/gym-admin/mobile-notifications', permissions: [Permissions.SendNotifications] },
  { label: 'Mobile Analytics', icon: 'insights', route: '/gym-admin/mobile-analytics', permissions: [Permissions.ViewNotifications] },
  { label: 'AI Dashboard', icon: 'psychology', route: '/gym-admin/ai', permissions: [Permissions.ViewAiInsights] },
  { label: 'AI Insights', icon: 'lightbulb', route: '/gym-admin/ai/insights', permissions: [Permissions.ViewAiInsights] },
  { label: 'Bookings', icon: 'event_available', route: '/gym-admin/bookings', permissions: [Permissions.ViewBookings] },
  { label: 'Class Schedules', icon: 'calendar_month', route: '/gym-admin/schedules', permissions: [Permissions.ManageSchedules] },
  { label: 'Booking Analytics', icon: 'insights', route: '/gym-admin/booking-analytics', permissions: [Permissions.ViewBookingAnalytics] },
  { label: 'Website Builder', icon: 'language', route: '/gym-admin/website-builder', permissions: [Permissions.ViewWebsiteBuilder] },
  { label: 'Website Analytics', icon: 'public', route: '/gym-admin/website-builder/analytics', permissions: [Permissions.ViewWebsiteAnalytics] },
  { label: 'Branding', icon: 'palette', route: '/gym-admin/branding', permissions: [Permissions.ViewWhiteLabel] },
  { label: 'White Label', icon: 'branding_watermark', route: '/gym-admin/white-label', permissions: [Permissions.ViewWhiteLabel] },
];

export function filterMenuItems(
  items: AppMenuItem[],
  roles: string[],
  permissions: string[]
): AppMenuItem[] {
  return items.filter((item) => {
    if (item.roles?.length && !item.roles.some((r) => roles.includes(r))) return false;
    if (item.permissions?.length && !item.permissions.some((p) => permissions.includes(p))) return false;
    return true;
  });
}

export const TRAINER_MENU: AppMenuItem[] = [
  { label: 'Dashboard', icon: 'dashboard', route: '/trainer', permissions: [Permissions.ViewDashboard] },
  { label: 'My Members', icon: 'groups', route: '/trainer/members', permissions: [Permissions.ViewMembers] },
  { label: 'Attendance', icon: 'event_available', route: '/trainer/attendance', permissions: [Permissions.ViewAttendance] },
  { label: 'Workout Plans', icon: 'fitness_center', route: '/trainer/workout-plans', permissions: [Permissions.ViewWorkoutPlans] },
  { label: 'AI Recommendations', icon: 'psychology', route: '/trainer/ai-recommendations', permissions: [Permissions.ViewAiRecommendations] },
  { label: 'My Schedule', icon: 'calendar_month', route: '/trainer/schedule', permissions: [Permissions.ViewBookings] },
  { label: 'Class Bookings', icon: 'event_note', route: '/trainer/bookings', permissions: [Permissions.ViewBookings] },
];

export const MEMBER_MENU: AppMenuItem[] = [
  { label: 'Dashboard', icon: 'dashboard', route: '/member/dashboard', permissions: [Permissions.ViewMemberDashboard] },
  { label: 'Goals', icon: 'flag', route: '/member/goals', permissions: [Permissions.ManageMemberGoals] },
  { label: 'Progress', icon: 'monitor_weight', route: '/member/progress', permissions: [Permissions.TrackMemberProgress] },
  { label: 'Workouts', icon: 'fitness_center', route: '/member/workouts', permissions: [Permissions.TrackMemberProgress] },
  { label: 'Diet Tracker', icon: 'restaurant', route: '/member/diets', permissions: [Permissions.TrackMemberProgress] },
  { label: 'Water', icon: 'water_drop', route: '/member/water', permissions: [Permissions.TrackMemberProgress] },
  { label: 'Referrals', icon: 'card_giftcard', route: '/member/referrals', permissions: [Permissions.ViewMemberDashboard] },
  { label: 'Feedback', icon: 'rate_review', route: '/member/feedback', permissions: [Permissions.SubmitMemberFeedback] },
  { label: 'My Profile', icon: 'person', route: '/member/profile', permissions: [Permissions.ViewMemberDetails] },
  { label: 'My Diet Plan', icon: 'restaurant_menu', route: '/member/diet', permissions: [Permissions.ViewMemberDiet] },
  { label: 'My Workout Plan', icon: 'sports_gymnastics', route: '/member/workout', permissions: [Permissions.ViewMemberWorkout] },
  { label: 'Pay Membership', icon: 'payments', route: '/member/checkout', permissions: [Permissions.InitiateOnlinePayment] },
  { label: 'Book a Class', icon: 'event_available', route: '/member/bookings', permissions: [Permissions.ViewBookings] },
];

export function getDefaultRouteForUser(roles: string[]): string {
  if (roles.includes(Roles.SuperAdmin)) return '/super-admin';
  if (roles.includes(Roles.GymAdmin)) return '/gym-admin/dashboard';
  if (roles.includes(Roles.Trainer)) return '/trainer';
  if (roles.includes(Roles.Member)) return '/member/dashboard';
  return '/auth/login';
}