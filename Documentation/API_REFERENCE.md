# API Reference

Total endpoints: 364

| HTTP Method | Route | Controller |
|-------------|-------|------------|

| GET | `/api/[controller]` | RolesController |

| POST | `/api/[controller]` | RolesController |

| DELETE | `/api/[controller]/{id:guid}` | GymsController |

| GET | `/api/[controller]/{id:guid}` | GymsController |

| PUT | `/api/[controller]/{id:guid}` | GymsController |

| PATCH | `/api/[controller]/{id:guid}/activate` | GymsController |

| PATCH | `/api/[controller]/{id:guid}/deactivate` | GymsController |

| DELETE | `/api/[controller]/{id:int}` | RolesController |

| GET | `/api/[controller]/{id:int}` | PrivilegesController |

| PUT | `/api/[controller]/{id:int}` | PrivilegesController |

| POST | `/api/[controller]/change-password` | AuthController |

| GET | `/api/[controller]/csrf` | AuthController |

| POST | `/api/[controller]/forgot-password` | AuthController |

| POST | `/api/[controller]/login` | AuthController |

| POST | `/api/[controller]/logout` | AuthController |

| POST | `/api/[controller]/refresh` | AuthController |

| POST | `/api/[controller]/register` | AuthController |

| POST | `/api/[controller]/reset-password` | AuthController |

| GET | `/api/[controller]/session` | AuthController |

| GET | `/api/[controller]/stats` | DashboardController |

| GET | `/api/[controller]/validate` | AuthController |

| GET | `/api/ai/analytics` | AiController |

| GET | `/api/ai/business-insights` | AiController |

| GET | `/api/ai/dashboard` | AiController |

| GET | `/api/ai/lead-scoring` | AiController |

| GET | `/api/ai/member-risk` | AiController |

| GET | `/api/ai/recommendations` | AiController |

| PUT | `/api/ai/recommendations/accept` | AiController |

| GET | `/api/analytics/attendance` | AnalyticsController |

| GET | `/api/analytics/dashboard` | AnalyticsController |

| GET | `/api/analytics/diets` | AnalyticsController |

| GET | `/api/analytics/export/excel` | AnalyticsController |

| GET | `/api/analytics/export/pdf` | AnalyticsController |

| GET | `/api/analytics/members` | AnalyticsController |

| GET | `/api/analytics/revenue` | AnalyticsController |

| GET | `/api/analytics/trainers` | AnalyticsController |

| GET | `/api/analytics/workouts` | AnalyticsController |

| GET | `/api/attendance` | AttendanceController |

| POST | `/api/attendance/check-in` | AttendanceController |

| POST | `/api/attendance/check-out` | AttendanceController |

| GET | `/api/attendance/dashboard` | AttendanceController |

| POST | `/api/attendance/mark` | AttendanceController |

| GET | `/api/attendance/members/{memberid:int}/history` | AttendanceController |

| GET | `/api/attendance/members/{memberid:int}/history/export/excel` | AttendanceController |

| GET | `/api/attendance/reports/daily` | AttendanceController |

| GET | `/api/attendance/reports/daily/export/excel` | AttendanceController |

| GET | `/api/attendance/reports/daily/export/pdf` | AttendanceController |

| GET | `/api/attendance/reports/monthly` | AttendanceController |

| GET | `/api/attendance/reports/monthly/export/excel` | AttendanceController |

| GET | `/api/attendance/reports/monthly/export/pdf` | AttendanceController |

| GET | `/api/attendance/statuses` | AttendanceController |

| GET | `/api/attendance/today` | AttendanceController |

| GET | `/api/attendance/trainers` | AttendanceController |

| POST | `/api/attendance/trainers/check-in` | AttendanceController |

| POST | `/api/attendance/trainers/check-out` | AttendanceController |

| GET | `/api/audit-logs` | AuditLogsController |

| GET | `/api/audit-logs/dashboard` | AuditLogsController |

| GET | `/api/audit-logs/export/excel` | AuditLogsController |

| GET | `/api/audit-logs/export/pdf` | AuditLogsController |

| GET | `/api/booking-analytics` | BookingAnalyticsController |

| GET | `/api/booking-analytics/export/{format}` | BookingAnalyticsController |

| POST | `/api/booking-checkin` | BookingCheckInController |

| GET | `/api/bookings` | BookingsController |

| GET | `/api/bookings/available-slots` | BookingsController |

| POST | `/api/bookings/book` | BookingsController |

| POST | `/api/bookings/cancel` | BookingsController |

| GET | `/api/bookings/settings` | BookingsController |

| PUT | `/api/bookings/settings` | BookingsController |

| POST | `/api/bookings/waitlist` | BookingsController |

| GET | `/api/branches` | BranchesController |

| POST | `/api/branches` | BranchesController |

| DELETE | `/api/branches/{id:int}` | BranchesController |

| GET | `/api/branches/{id:int}` | BranchesController |

| PUT | `/api/branches/{id:int}` | BranchesController |

| PATCH | `/api/branches/{id:int}/activate` | BranchesController |

| PATCH | `/api/branches/{id:int}/deactivate` | BranchesController |

| POST | `/api/branches/{id:int}/manager` | BranchesController |

| GET | `/api/branches/analytics` | BranchesController |

| GET | `/api/branches/announcements` | BranchesController |

| POST | `/api/branches/announcements` | BranchesController |

| DELETE | `/api/branches/announcements/{id:int}` | BranchesController |

| GET | `/api/branches/dashboard` | BranchesController |

| GET | `/api/branches/list` | BranchesController |

| GET | `/api/branches/targets` | BranchesController |

| POST | `/api/branches/targets` | BranchesController |

| GET | `/api/branches/transfers` | BranchesController |

| POST | `/api/branches/transfers/members` | BranchesController |

| POST | `/api/branches/transfers/trainers` | BranchesController |

| GET | `/api/diet-plans` | DietPlansController |

| POST | `/api/diet-plans` | DietPlansController |

| DELETE | `/api/diet-plans/{id:int}` | DietPlansController |

| GET | `/api/diet-plans/{id:int}` | DietPlansController |

| PUT | `/api/diet-plans/{id:int}` | DietPlansController |

| PATCH | `/api/diet-plans/{id:int}/active` | DietPlansController |

| POST | `/api/diet-plans/{id:int}/clone` | DietPlansController |

| GET | `/api/diet-plans/{id:int}/export/excel` | DietPlansController |

| GET | `/api/diet-plans/{id:int}/export/pdf` | DietPlansController |

| POST | `/api/diet-plans/assign` | DietPlansController |

| DELETE | `/api/diet-plans/assignments/{assignedid:int}` | DietPlansController |

| GET | `/api/diet-plans/categories` | DietPlansController |

| POST | `/api/diet-plans/categories` | DietPlansController |

| GET | `/api/diet-plans/members/{memberid:int}` | DietPlansController |

| GET | `/api/diet-plans/members/{memberid:int}/assignments` | DietPlansController |

| GET | `/api/diet-plans/members/me` | DietPlansController |

| GET | `/api/expenses` | ExpensesController |

| POST | `/api/expenses` | ExpensesController |

| DELETE | `/api/expenses/{id:int}` | ExpensesController |

| GET | `/api/expenses/{id:int}` | ExpensesController |

| PUT | `/api/expenses/{id:int}` | ExpensesController |

| GET | `/api/expenses/categories` | ExpensesController |

| GET | `/api/expenses/export/excel` | ExpensesController |

| GET | `/api/expenses/export/pdf` | ExpensesController |

| DELETE | `/api/files/{fileid:long}` | FilesController |

| GET | `/api/files/{fileid:long}` | FilesController |

| GET | `/api/files/{fileid:long}/content` | FilesController |

| GET | `/api/files/gym/logo` | FilesController |

| GET | `/api/files/members/{memberid:int}` | FilesController |

| GET | `/api/files/trainers/{trainerid:int}` | FilesController |

| POST | `/api/files/upload` | FilesController |

| GET | `/api/financial/dashboard` | FinancialController |

| GET | `/api/financial/export/excel` | FinancialController |

| GET | `/api/financial/export/pdf` | FinancialController |

| GET | `/api/financial/profit-loss` | FinancialController |

| GET | `/api/gym-admins` | GymAdminsController |

| POST | `/api/gym-admins` | GymAdminsController |

| GET | `/api/gym-admins/{userid:guid}` | GymAdminsController |

| PUT | `/api/gym-admins/{userid:guid}` | GymAdminsController |

| PATCH | `/api/gym-admins/{userid:guid}/activate` | GymAdminsController |

| PATCH | `/api/gym-admins/{userid:guid}/deactivate` | GymAdminsController |

| POST | `/api/gym-admins/{userid:guid}/resend-temporary-password` | GymAdminsController |

| GET | `/api/leads` | LeadsController |

| POST | `/api/leads` | LeadsController |

| DELETE | `/api/leads/{id:int}` | LeadsController |

| GET | `/api/leads/{id:int}` | LeadsController |

| PUT | `/api/leads/{id:int}` | LeadsController |

| PATCH | `/api/leads/{id:int}/status` | LeadsController |

| GET | `/api/leads/analytics` | LeadsController |

| POST | `/api/leads/assign-trainer` | LeadsController |

| POST | `/api/leads/convert-member` | LeadsController |

| GET | `/api/leads/dashboard` | LeadsController |

| GET | `/api/leads/export/excel` | LeadsController |

| GET | `/api/leads/export/pdf` | LeadsController |

| POST | `/api/leads/followup` | LeadsController |

| PUT | `/api/leads/followup/{followupid:int}` | LeadsController |

| GET | `/api/leads/followups/pending` | LeadsController |

| POST | `/api/leads/schedule-trial` | LeadsController |

| POST | `/api/leads/trial-feedback` | LeadsController |

| GET | `/api/leads/trials/today` | LeadsController |

| GET | `/api/member/analytics` | MemberSelfServiceController |

| GET | `/api/member/attendance/export/pdf` | MemberSelfServiceController |

| POST | `/api/member/attendance/qr-scan` | MemberSelfServiceController |

| GET | `/api/member/dashboard` | MemberSelfServiceController |

| GET | `/api/member/diets` | MemberSelfServiceController |

| POST | `/api/member/diets` | MemberSelfServiceController |

| GET | `/api/member/diets/compliance` | MemberSelfServiceController |

| GET | `/api/member/feedback` | MemberSelfServiceController |

| POST | `/api/member/feedback` | MemberSelfServiceController |

| GET | `/api/member/goals` | MemberSelfServiceController |

| POST | `/api/member/goals` | MemberSelfServiceController |

| PUT | `/api/member/goals/{goalid:int}` | MemberSelfServiceController |

| POST | `/api/member/goals/{goalid:int}/complete` | MemberSelfServiceController |

| PATCH | `/api/member/goals/{goalid:int}/progress` | MemberSelfServiceController |

| GET | `/api/member/goals/export/pdf` | MemberSelfServiceController |

| GET | `/api/member/progress` | MemberSelfServiceController |

| POST | `/api/member/progress` | MemberSelfServiceController |

| GET | `/api/member/progress/export/pdf` | MemberSelfServiceController |

| GET | `/api/member/progress/history` | MemberSelfServiceController |

| GET | `/api/member/progress/photos` | MemberSelfServiceController |

| POST | `/api/member/progress/photos` | MemberSelfServiceController |

| GET | `/api/member/qr-code` | MemberSelfServiceController |

| GET | `/api/member/referrals` | MemberSelfServiceController |

| GET | `/api/member/water` | MemberSelfServiceController |

| POST | `/api/member/water` | MemberSelfServiceController |

| GET | `/api/member/workouts` | MemberSelfServiceController |

| POST | `/api/member/workouts` | MemberSelfServiceController |

| GET | `/api/member/workouts/streak` | MemberSelfServiceController |

| GET | `/api/members` | MembersController |

| POST | `/api/members` | MembersController |

| DELETE | `/api/members/{id:int}` | MembersController |

| GET | `/api/members/{id:int}` | MembersController |

| PUT | `/api/members/{id:int}` | MembersController |

| PATCH | `/api/members/{id:int}/activate` | MembersController |

| POST | `/api/members/{id:int}/assign-trainer` | MembersController |

| PATCH | `/api/members/{id:int}/deactivate` | MembersController |

| GET | `/api/members/{id:int}/details` | MembersController |

| DELETE | `/api/members/{id:int}/trainer-assignment` | MembersController |

| GET | `/api/members/me` | MembersController |

| GET | `/api/membership-plans` | MembershipPlansController |

| POST | `/api/membership-plans` | MembershipPlansController |

| DELETE | `/api/membership-plans/{id:int}` | MembershipPlansController |

| PUT | `/api/membership-plans/{id:int}` | MembershipPlansController |

| GET | `/api/memberships` | MembershipsController |

| POST | `/api/memberships` | MembershipsController |

| GET | `/api/memberships/{id:int}` | MembershipsController |

| POST | `/api/memberships/{id:int}/cancel` | MembershipsController |

| POST | `/api/memberships/{id:int}/renew` | MembershipsController |

| GET | `/api/memberships/expired` | MembershipsController |

| GET | `/api/menus` | MenusController |

| GET | `/api/menus/my-menus` | MenusController |

| GET | `/api/mobile/admin/analytics` | MobileAdminController |

| GET | `/api/mobile/admin/campaigns` | MobileAdminController |

| POST | `/api/mobile/admin/send` | MobileAdminController |

| GET | `/api/mobile/dashboard` | MobileController |

| POST | `/api/mobile/device/register` | MobileController |

| POST | `/api/mobile/device/unregister` | MobileController |

| GET | `/api/mobile/notifications` | MobileController |

| POST | `/api/mobile/notifications/{id:int}/engagement` | MobileController |

| PUT | `/api/mobile/notifications/read` | MobileController |

| GET | `/api/mobile/preferences` | MobileController |

| PUT | `/api/mobile/preferences` | MobileController |

| GET | `/api/mobile/sync` | MobileController |

| GET | `/api/notifications/dashboard` | NotificationsController |

| GET | `/api/notifications/history` | NotificationsController |

| POST | `/api/notifications/send` | NotificationsController |

| GET | `/api/notifications/settings` | NotificationsController |

| PUT | `/api/notifications/settings` | NotificationsController |

| GET | `/api/notifications/templates` | NotificationsController |

| POST | `/api/notifications/templates` | NotificationsController |

| DELETE | `/api/notifications/templates/{id:int}` | NotificationsController |

| PUT | `/api/notifications/templates/{id:int}` | NotificationsController |

| POST | `/api/notifications/test` | NotificationsController |

| GET | `/api/onboarding/plans` | GymOnboardingController |

| POST | `/api/onboarding/register` | GymOnboardingController |

| GET | `/api/payments` | PaymentsController |

| POST | `/api/payments` | PaymentsController |

| POST | `/api/payments/{paymentid:int}/invoice` | PaymentsController |

| POST | `/api/payments/{paymentid:int}/refund` | PaymentsController |

| GET | `/api/payments/invoices/{invoiceid:int}/download` | PaymentsController |

| GET | `/api/payments/member/{memberid:int}` | PaymentsController |

| GET | `/api/payments/razorpay/checkout-context` | PaymentsController |

| POST | `/api/payments/razorpay/order` | PaymentsController |

| POST | `/api/payments/razorpay/verify` | PaymentsController |

| GET | `/api/payments/revenue/dashboard` | PaymentsController |

| GET | `/api/payments/revenue/monthly` | PaymentsController |

| GET | `/api/payroll` | PayrollController |

| GET | `/api/payroll/{id:int}` | PayrollController |

| PUT | `/api/payroll/{id:int}` | PayrollController |

| POST | `/api/payroll/{id:int}/approve` | PayrollController |

| POST | `/api/payroll/{id:int}/pay` | PayrollController |

| GET | `/api/payroll/commissions` | PayrollController |

| POST | `/api/payroll/commissions` | PayrollController |

| GET | `/api/payroll/commissions/me` | PayrollController |

| GET | `/api/payroll/export/excel` | PayrollController |

| GET | `/api/payroll/export/pdf` | PayrollController |

| POST | `/api/payroll/generate` | PayrollController |

| GET | `/api/platform/subscription-plans` | PlatformSubscriptionPlansController |

| POST | `/api/platform/subscription-plans` | PlatformSubscriptionPlansController |

| DELETE | `/api/platform/subscription-plans/{id:int}` | PlatformSubscriptionPlansController |

| GET | `/api/platform/subscription-plans/{id:int}` | PlatformSubscriptionPlansController |

| PUT | `/api/platform/subscription-plans/{id:int}` | PlatformSubscriptionPlansController |

| POST | `/api/platform/subscription-plans/{id:int}/clone` | PlatformSubscriptionPlansController |

| POST | `/api/platform/subscription-plans/{planid:int}/pricing-options` | PlatformSubscriptionPlansController |

| PUT | `/api/platform/subscription-plans/{planid:int}/pricing-options/reorder` | PlatformSubscriptionPlansController |

| GET | `/api/platform/subscription-plans/feature-dependencies` | PlatformSubscriptionPlansController |

| GET | `/api/platform/subscription-plans/features` | PlatformSubscriptionPlansController |

| DELETE | `/api/platform/subscription-plans/pricing-options/{pricingoptionid:int}` | PlatformSubscriptionPlansController |

| PUT | `/api/platform/subscription-plans/pricing-options/{pricingoptionid:int}` | PlatformSubscriptionPlansController |

| POST | `/api/platform/subscription-plans/validate-features` | PlatformSubscriptionPlansController |

| GET | `/api/platform/tenant-menus/{gymid:guid}` | TenantMenuController |

| PUT | `/api/platform/tenant-menus/{gymid:guid}/{menuid:int}/disable` | TenantMenuController |

| PUT | `/api/platform/tenant-menus/{gymid:guid}/{menuid:int}/enable` | TenantMenuController |

| PUT | `/api/platform/tenant-menus/{gymid:guid}/bulk` | TenantMenuController |

| GET | `/api/platform/tenant-menus/gyms` | TenantMenuController |

| GET | `/api/platform/white-label/dashboard` | WhiteLabelPlatformController |

| GET | `/api/public/website/{gymslug}` | PublicWebsiteController |

| GET | `/api/public/website/{gymslug}/robots.txt` | PublicWebsiteController |

| GET | `/api/public/website/{gymslug}/sitemap` | PublicWebsiteController |

| POST | `/api/public/website/lead` | PublicWebsiteController |

| POST | `/api/public/website/trial-booking` | PublicWebsiteController |

| GET | `/api/public/white-label/login-branding` | PublicWhiteLabelController |

| POST | `/api/role-privileges` | RolePrivilegesController |

| GET | `/api/role-privileges/matrix` | RolePrivilegesController |

| GET | `/api/role-privileges/role/{roleid:int}` | RolePrivilegesController |

| DELETE | `/api/role-privileges/role/{roleid:int}/privilege/{privilegeid:int}` | RolePrivilegesController |

| GET | `/api/saas/branding` | SaasSubscriptionsController |

| PUT | `/api/saas/branding` | SaasSubscriptionsController |

| GET | `/api/saas/my-features` | SaasSubscriptionsController |

| POST | `/api/saas/payments/order` | SaasSubscriptionsController |

| POST | `/api/saas/payments/verify` | SaasSubscriptionsController |

| GET | `/api/saas/plans` | SaasSubscriptionsController |

| GET | `/api/saas/plans/catalog` | SaasSubscriptionsController |

| GET | `/api/saas/platform/dashboard` | SaasPlatformController |

| GET | `/api/saas/subscription` | SaasSubscriptionsController |

| POST | `/api/saas/subscription/cancel` | SaasSubscriptionsController |

| GET | `/api/saas/usage` | SaasSubscriptionsController |

| GET | `/api/schedules` | SchedulesController |

| POST | `/api/schedules` | SchedulesController |

| DELETE | `/api/schedules/{id:int}` | SchedulesController |

| GET | `/api/schedules/{id:int}` | SchedulesController |

| PUT | `/api/schedules/{id:int}` | SchedulesController |

| GET | `/api/subscription-notifications/my` | SubscriptionNotificationsController |

| PUT | `/api/subscription-notifications/read` | SubscriptionNotificationsController |

| GET | `/api/trainers` | TrainersController |

| POST | `/api/trainers` | TrainersController |

| DELETE | `/api/trainers/{id:int}` | TrainersController |

| GET | `/api/trainers/{id:int}` | TrainersController |

| PUT | `/api/trainers/{id:int}` | TrainersController |

| POST | `/api/trainers/{id:int}/assign-members` | TrainersController |

| GET | `/api/trainers/{id:int}/dashboard` | TrainersController |

| GET | `/api/trainers/{id:int}/members` | TrainersController |

| GET | `/api/trainers/{id:int}/unassigned-members` | TrainersController |

| GET | `/api/trainers/me` | TrainersController |

| DELETE | `/api/trainers/members/{memberid:int}/assignment` | TrainersController |

| GET | `/api/trainer-schedule` | TrainerScheduleController |

| POST | `/api/user-roles` | UserRolesController |

| GET | `/api/user-roles/user/{userid:guid}` | UserRolesController |

| DELETE | `/api/user-roles/user/{userid:guid}/role/{roleid:int}` | UserRolesController |

| GET | `/api/website/analytics` | WebsiteAnalyticsController |

| GET | `/api/website/analytics/export/{format}` | WebsiteAnalyticsController |

| GET | `/api/website/gallery` | WebsiteGalleryController |

| POST | `/api/website/gallery` | WebsiteGalleryController |

| DELETE | `/api/website/gallery/{id:int}` | WebsiteGalleryController |

| PUT | `/api/website/gallery/{id:int}` | WebsiteGalleryController |

| GET | `/api/website/leads` | WebsiteLeadsController |

| GET | `/api/website/pages` | WebsitePagesController |

| POST | `/api/website/pages` | WebsitePagesController |

| DELETE | `/api/website/pages/{id:int}` | WebsitePagesController |

| PUT | `/api/website/pages/{id:int}` | WebsitePagesController |

| GET | `/api/website/preview/{gymslug}` | WebsiteSettingsController |

| GET | `/api/website/sections` | WebsiteSectionsController |

| POST | `/api/website/sections` | WebsiteSectionsController |

| DELETE | `/api/website/sections/{id:int}` | WebsiteSectionsController |

| PUT | `/api/website/sections/{id:int}` | WebsiteSectionsController |

| GET | `/api/website/settings` | WebsiteSettingsController |

| PUT | `/api/website/settings` | WebsiteSettingsController |

| POST | `/api/website/settings/publish` | WebsiteSettingsController |

| POST | `/api/website/settings/unpublish` | WebsiteSettingsController |

| GET | `/api/website/testimonials` | WebsiteTestimonialsController |

| POST | `/api/website/testimonials` | WebsiteTestimonialsController |

| DELETE | `/api/website/testimonials/{id:int}` | WebsiteTestimonialsController |

| PUT | `/api/website/testimonials/{id:int}` | WebsiteTestimonialsController |

| GET | `/api/white-label/app-branding` | WhiteLabelController |

| PUT | `/api/white-label/domain` | WhiteLabelController |

| GET | `/api/white-label/email-templates` | WhiteLabelController |

| POST | `/api/white-label/email-templates` | WhiteLabelController |

| DELETE | `/api/white-label/email-templates/{id:int}` | WhiteLabelController |

| PUT | `/api/white-label/email-templates/{id:int}` | WhiteLabelController |

| GET | `/api/white-label/mobile-settings` | WhiteLabelController |

| PUT | `/api/white-label/mobile-settings` | WhiteLabelController |

| GET | `/api/white-label/preview` | WhiteLabelController |

| GET | `/api/white-label/settings` | WhiteLabelController |

| PUT | `/api/white-label/settings` | WhiteLabelController |

| POST | `/api/white-label/settings/disable` | WhiteLabelController |

| POST | `/api/white-label/settings/enable` | WhiteLabelController |

| GET | `/api/workout-plans` | WorkoutPlansController |

| POST | `/api/workout-plans` | WorkoutPlansController |

| DELETE | `/api/workout-plans/{id:int}` | WorkoutPlansController |

| GET | `/api/workout-plans/{id:int}` | WorkoutPlansController |

| PUT | `/api/workout-plans/{id:int}` | WorkoutPlansController |

| PATCH | `/api/workout-plans/{id:int}/active` | WorkoutPlansController |

| POST | `/api/workout-plans/{id:int}/clone` | WorkoutPlansController |

| GET | `/api/workout-plans/{id:int}/export/excel` | WorkoutPlansController |

| GET | `/api/workout-plans/{id:int}/export/pdf` | WorkoutPlansController |

| POST | `/api/workout-plans/assign` | WorkoutPlansController |

| GET | `/api/workout-plans/exercise-categories` | WorkoutPlansController |

| POST | `/api/workout-plans/exercise-categories` | WorkoutPlansController |

| GET | `/api/workout-plans/exercises` | WorkoutPlansController |

| POST | `/api/workout-plans/exercises` | WorkoutPlansController |

| DELETE | `/api/workout-plans/exercises/{id:int}` | WorkoutPlansController |

| GET | `/api/workout-plans/exercises/{id:int}` | WorkoutPlansController |

| PUT | `/api/workout-plans/exercises/{id:int}` | WorkoutPlansController |

| GET | `/api/workout-plans/members/{memberid:int}` | WorkoutPlansController |

| GET | `/api/workout-plans/members/me` | WorkoutPlansController |

| POST | `/api/workout-plans/progress` | WorkoutPlansController |

