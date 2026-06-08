import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { permissionGuard } from '../../core/guards/permission.guard';
import { roleGuard } from '../../core/guards/role.guard';
import { Permissions } from '../../core/constants/permissions';
import { Roles } from '../../core/constants/roles';

export const GYM_ADMIN_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./gym-admin-layout.component').then((m) => m.GymAdminLayoutComponent),
    canActivate: [authGuard, roleGuard(Roles.GymAdmin)],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        canActivate: [permissionGuard(Permissions.ViewAnalytics)],
        loadComponent: () =>
          import('./dashboard/gym-admin-dashboard.component').then(
            (m) => m.GymAdminDashboardComponent
          ),
      },
      {
        path: 'analytics/revenue',
        canActivate: [permissionGuard(Permissions.ViewRevenueAnalytics)],
        loadComponent: () =>
          import('./analytics/analytics-revenue.component').then((m) => m.AnalyticsRevenueComponent),
      },
      {
        path: 'analytics/members',
        canActivate: [permissionGuard(Permissions.ViewMemberAnalytics)],
        loadComponent: () =>
          import('./analytics/analytics-members.component').then((m) => m.AnalyticsMembersComponent),
      },
      {
        path: 'analytics/attendance',
        canActivate: [permissionGuard(Permissions.ViewAnalytics)],
        loadComponent: () =>
          import('./analytics/analytics-attendance.component').then((m) => m.AnalyticsAttendanceComponent),
      },
      {
        path: 'analytics/trainers',
        canActivate: [permissionGuard(Permissions.ViewAnalytics)],
        loadComponent: () =>
          import('./analytics/analytics-trainers.component').then((m) => m.AnalyticsTrainersComponent),
      },
      {
        path: 'members',
        canActivate: [permissionGuard(Permissions.ViewMembers)],
        loadComponent: () =>
          import('./members/member-list.component').then((m) => m.MemberListComponent),
      },
      {
        path: 'leads',
        canActivate: [permissionGuard(Permissions.ViewLeads)],
        loadComponent: () =>
          import('./leads/lead-list.component').then((m) => m.LeadListComponent),
      },
      {
        path: 'leads/create',
        canActivate: [permissionGuard(Permissions.ManageLeads)],
        loadComponent: () =>
          import('./leads/lead-form.component').then((m) => m.LeadFormComponent),
      },
      {
        path: 'leads/edit/:id',
        canActivate: [permissionGuard(Permissions.ManageLeads)],
        loadComponent: () =>
          import('./leads/lead-form.component').then((m) => m.LeadFormComponent),
      },
      {
        path: 'leads/followups',
        canActivate: [permissionGuard(Permissions.ViewLeads)],
        loadComponent: () =>
          import('./leads/lead-followups.component').then((m) => m.LeadFollowupsComponent),
      },
      {
        path: 'leads/trials',
        canActivate: [permissionGuard(Permissions.ViewLeads)],
        loadComponent: () =>
          import('./leads/lead-trials.component').then((m) => m.LeadTrialsComponent),
      },
      {
        path: 'leads/analytics',
        canActivate: [permissionGuard(Permissions.ViewLeadAnalytics)],
        loadComponent: () =>
          import('./leads/lead-analytics.component').then((m) => m.LeadAnalyticsComponent),
      },
      {
        path: 'leads/:id',
        canActivate: [permissionGuard(Permissions.ViewLeads)],
        loadComponent: () =>
          import('./leads/lead-detail.component').then((m) => m.LeadDetailComponent),
      },
      {
        path: 'expenses',
        canActivate: [permissionGuard(Permissions.ViewExpenses)],
        loadComponent: () =>
          import('./financial/expense-list.component').then((m) => m.ExpenseListComponent),
      },
      {
        path: 'payroll',
        canActivate: [permissionGuard(Permissions.ViewPayroll)],
        loadComponent: () =>
          import('./financial/payroll-list.component').then((m) => m.PayrollListComponent),
      },
      {
        path: 'financial',
        canActivate: [permissionGuard(Permissions.ViewFinancialAnalytics)],
        loadComponent: () =>
          import('./financial/financial-dashboard.component').then((m) => m.FinancialDashboardComponent),
      },
      {
        path: 'branches',
        canActivate: [permissionGuard(Permissions.ViewBranches)],
        loadComponent: () =>
          import('./branches/branch-list.component').then((m) => m.BranchListComponent),
      },
      {
        path: 'branches/dashboard',
        canActivate: [permissionGuard(Permissions.ViewBranches)],
        loadComponent: () =>
          import('./branches/branch-dashboard.component').then((m) => m.BranchDashboardComponent),
      },
      {
        path: 'branches/analytics',
        canActivate: [permissionGuard(Permissions.ViewBranchAnalytics)],
        loadComponent: () =>
          import('./branches/branch-analytics.component').then((m) => m.BranchAnalyticsComponent),
      },
      {
        path: 'branches/transfers',
        canActivate: [permissionGuard(Permissions.ViewBranches)],
        loadComponent: () =>
          import('./branches/branch-transfers.component').then((m) => m.BranchTransfersComponent),
      },
      {
        path: 'branches/targets',
        canActivate: [permissionGuard(Permissions.ViewBranches)],
        loadComponent: () =>
          import('./branches/branch-targets.component').then((m) => m.BranchTargetsComponent),
      },
      {
        path: 'mobile-notifications',
        canActivate: [permissionGuard(Permissions.SendNotifications)],
        loadComponent: () =>
          import('./mobile/mobile-notifications.component').then((m) => m.MobileNotificationsComponent),
      },
      {
        path: 'mobile-analytics',
        canActivate: [permissionGuard(Permissions.ViewNotifications)],
        loadComponent: () =>
          import('./mobile/mobile-analytics.component').then((m) => m.MobileAnalyticsComponent),
      },
      {
        path: 'ai',
        canActivate: [permissionGuard(Permissions.ViewAiInsights)],
        loadComponent: () =>
          import('./ai/ai-dashboard.component').then((m) => m.AiDashboardComponent),
      },
      {
        path: 'ai/insights',
        canActivate: [permissionGuard(Permissions.ViewAiInsights)],
        loadComponent: () =>
          import('./ai/ai-insights.component').then((m) => m.AiInsightsComponent),
      },
      {
        path: 'bookings',
        canActivate: [permissionGuard(Permissions.ViewBookings)],
        loadComponent: () =>
          import('./booking/gym-admin-bookings.component').then((m) => m.GymAdminBookingsComponent),
      },
      {
        path: 'schedules',
        canActivate: [permissionGuard(Permissions.ManageSchedules)],
        loadComponent: () =>
          import('./booking/schedule-list.component').then((m) => m.ScheduleListComponent),
      },
      {
        path: 'booking-analytics',
        canActivate: [permissionGuard(Permissions.ViewBookingAnalytics)],
        loadComponent: () =>
          import('./booking/booking-analytics.component').then((m) => m.BookingAnalyticsComponent),
      },
      {
        path: 'website-builder',
        canActivate: [permissionGuard(Permissions.ViewWebsiteBuilder)],
        loadComponent: () =>
          import('./website/website-builder.component').then((m) => m.WebsiteBuilderComponent),
      },
      {
        path: 'website-builder/pages',
        canActivate: [permissionGuard(Permissions.ManageWebsiteBuilder)],
        loadComponent: () =>
          import('./website/website-pages.component').then((m) => m.WebsitePagesComponent),
      },
      {
        path: 'website-builder/gallery',
        canActivate: [permissionGuard(Permissions.ManageWebsiteBuilder)],
        loadComponent: () =>
          import('./website/website-gallery.component').then((m) => m.WebsiteGalleryComponent),
      },
      {
        path: 'website-builder/testimonials',
        canActivate: [permissionGuard(Permissions.ManageWebsiteBuilder)],
        loadComponent: () =>
          import('./website/website-testimonials.component').then((m) => m.WebsiteTestimonialsComponent),
      },
      {
        path: 'website-builder/analytics',
        canActivate: [permissionGuard(Permissions.ViewWebsiteAnalytics)],
        loadComponent: () =>
          import('./website/website-analytics.component').then((m) => m.WebsiteAnalyticsComponent),
      },
      {
        path: 'branding',
        canActivate: [permissionGuard(Permissions.ViewWhiteLabel)],
        loadComponent: () =>
          import('./white-label/white-label-settings.component').then((m) => m.WhiteLabelSettingsComponent),
        data: { title: 'Branding' },
      },
      {
        path: 'white-label',
        canActivate: [permissionGuard(Permissions.ViewWhiteLabel)],
        loadComponent: () =>
          import('./white-label/white-label-settings.component').then((m) => m.WhiteLabelSettingsComponent),
      },
      {
        path: 'white-label/preview',
        canActivate: [permissionGuard(Permissions.ViewWhiteLabel)],
        loadComponent: () =>
          import('./white-label/white-label-preview.component').then((m) => m.WhiteLabelPreviewComponent),
      },
      {
        path: 'members/:id',
        canActivate: [permissionGuard(Permissions.ViewMemberDetails)],
        loadComponent: () =>
          import('./members/member-detail.component').then((m) => m.MemberDetailComponent),
      },
      {
        path: 'trainers',
        canActivate: [permissionGuard(Permissions.ViewTrainers)],
        loadComponent: () =>
          import('./trainers/trainer-list.component').then((m) => m.TrainerListComponent),
      },
      {
        path: 'trainers/:id',
        canActivate: [permissionGuard(Permissions.ViewTrainers)],
        loadComponent: () =>
          import('./trainers/trainer-detail.component').then((m) => m.TrainerDetailComponent),
      },
      {
        path: 'membership-plans',
        canActivate: [permissionGuard(Permissions.ViewMemberships)],
        loadComponent: () =>
          import('./memberships/membership-plan-list.component').then((m) => m.MembershipPlanListComponent),
      },
      {
        path: 'memberships',
        canActivate: [permissionGuard(Permissions.ViewMemberships)],
        loadComponent: () =>
          import('./memberships/membership-list.component').then((m) => m.MembershipListComponent),
      },
      {
        path: 'memberships/expired',
        canActivate: [permissionGuard(Permissions.ViewMemberships)],
        loadComponent: () =>
          import('./memberships/expired-membership-list.component').then((m) => m.ExpiredMembershipListComponent),
      },
      {
        path: 'payments',
        canActivate: [permissionGuard(Permissions.ViewPayments)],
        loadComponent: () =>
          import('./payments/payment-list.component').then((m) => m.PaymentListComponent),
      },
      {
        path: 'revenue',
        canActivate: [permissionGuard(Permissions.ViewRevenue)],
        loadComponent: () =>
          import('./payments/revenue-dashboard.component').then((m) => m.RevenueDashboardComponent),
      },
      {
        path: 'attendance',
        canActivate: [permissionGuard(Permissions.ViewAttendance)],
        loadComponent: () =>
          import('./attendance/attendance-dashboard.component').then((m) => m.AttendanceDashboardComponent),
      },
      {
        path: 'attendance/list',
        canActivate: [permissionGuard(Permissions.ViewAttendance)],
        loadComponent: () =>
          import('./attendance/attendance-list.component').then((m) => m.AttendanceListComponent),
      },
      {
        path: 'attendance/check-in',
        canActivate: [permissionGuard(Permissions.ManageAttendance)],
        loadComponent: () =>
          import('./attendance/attendance-check-in.component').then((m) => m.AttendanceCheckInComponent),
      },
      {
        path: 'attendance/check-out',
        canActivate: [permissionGuard(Permissions.ManageAttendance)],
        loadComponent: () =>
          import('./attendance/attendance-check-out.component').then((m) => m.AttendanceCheckOutComponent),
      },
      {
        path: 'attendance/members/:id/history',
        canActivate: [permissionGuard(Permissions.ViewAttendance)],
        loadComponent: () =>
          import('./attendance/member-attendance-history.component').then((m) => m.MemberAttendanceHistoryComponent),
      },
      {
        path: 'attendance/reports',
        canActivate: [permissionGuard(Permissions.ViewAttendance)],
        loadComponent: () =>
          import('./attendance/attendance-reports.component').then((m) => m.AttendanceReportsComponent),
      },
      {
        path: 'attendance/trainers',
        canActivate: [permissionGuard(Permissions.ViewTrainerAttendance)],
        loadComponent: () =>
          import('./attendance/trainer-attendance.component').then((m) => m.TrainerAttendanceComponent),
      },
      {
        path: 'audit',
        canActivate: [permissionGuard(Permissions.ViewAuditLogs)],
        loadComponent: () =>
          import('./audit/audit-dashboard.component').then((m) => m.AuditDashboardComponent),
      },
      {
        path: 'notifications',
        canActivate: [permissionGuard(Permissions.ViewNotifications)],
        loadComponent: () =>
          import('./notifications/notification-dashboard.component').then((m) => m.NotificationDashboardComponent),
      },
      {
        path: 'notifications/templates',
        canActivate: [permissionGuard(Permissions.ViewNotifications)],
        loadComponent: () =>
          import('./notifications/notification-template-list.component').then((m) => m.NotificationTemplateListComponent),
      },
      {
        path: 'notifications/history',
        canActivate: [permissionGuard(Permissions.ViewNotifications)],
        loadComponent: () =>
          import('./notifications/notification-history.component').then((m) => m.NotificationHistoryComponent),
      },
      {
        path: 'notifications/test',
        canActivate: [permissionGuard(Permissions.SendNotifications)],
        loadComponent: () =>
          import('./notifications/notification-test.component').then((m) => m.NotificationTestComponent),
      },
      {
        path: 'diet-plans',
        canActivate: [permissionGuard(Permissions.ViewDietPlans)],
        loadComponent: () =>
          import('./diet/diet-plan-list.component').then((m) => m.DietPlanListComponent),
      },
      {
        path: 'diet-plans/new',
        canActivate: [permissionGuard(Permissions.ManageDietPlans)],
        loadComponent: () =>
          import('./diet/diet-plan-editor.component').then((m) => m.DietPlanEditorComponent),
      },
      {
        path: 'diet-plans/:id/edit',
        canActivate: [permissionGuard(Permissions.ManageDietPlans)],
        loadComponent: () =>
          import('./diet/diet-plan-editor.component').then((m) => m.DietPlanEditorComponent),
      },
      {
        path: 'members/:id/diet',
        canActivate: [permissionGuard(Permissions.ViewMemberDiet)],
        loadComponent: () =>
          import('./diet/member-diet-view.component').then((m) => m.MemberDietViewComponent),
      },
      {
        path: 'subscription',
        canActivate: [permissionGuard(Permissions.ViewSaasSubscription)],
        loadComponent: () =>
          import('./subscription/gym-subscription.component').then((m) => m.GymSubscriptionComponent),
      },
      {
        path: 'settings/branding',
        canActivate: [permissionGuard(Permissions.ManageGymBranding)],
        loadComponent: () =>
          import('./settings/gym-branding.component').then((m) => m.GymBrandingComponent),
      },
      {
        path: 'workout-plans',
        canActivate: [permissionGuard(Permissions.ViewWorkoutPlans)],
        loadComponent: () =>
          import('./workout/workout-plan-list.component').then((m) => m.WorkoutPlanListComponent),
      },
      {
        path: 'workout-plans/exercises',
        canActivate: [permissionGuard(Permissions.ViewWorkoutPlans)],
        loadComponent: () =>
          import('./workout/exercise-library.component').then((m) => m.ExerciseLibraryComponent),
      },
      {
        path: 'workout-plans/new',
        canActivate: [permissionGuard(Permissions.ManageWorkoutPlans)],
        loadComponent: () =>
          import('./workout/workout-plan-editor.component').then((m) => m.WorkoutPlanEditorComponent),
      },
      {
        path: 'workout-plans/:id/edit',
        canActivate: [permissionGuard(Permissions.ManageWorkoutPlans)],
        loadComponent: () =>
          import('./workout/workout-plan-editor.component').then((m) => m.WorkoutPlanEditorComponent),
      },
      {
        path: 'members/:id/workout',
        canActivate: [permissionGuard(Permissions.ViewMemberWorkout)],
        loadComponent: () =>
          import('./workout/member-workout-view.component').then((m) => m.MemberWorkoutViewComponent),
      },
    ],
  },
];
