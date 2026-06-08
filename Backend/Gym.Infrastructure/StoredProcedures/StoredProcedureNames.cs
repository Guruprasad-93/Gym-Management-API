namespace Gym.Infrastructure.StoredProcedures;

/// <summary>
/// Canonical stored procedure names (dbo schema).
/// Convention: sp_{Verb}{Entity} e.g. sp_CreateGym, sp_GetAllMembers.
/// </summary>
public static class StoredProcedureNames
{
    // Auth / User
    public const string LoginUser = "dbo.sp_LoginUser";
    public const string UserExistsByEmail = "dbo.sp_User_ExistsByEmail";
    public const string GetUserByEmail = "dbo.sp_User_GetByEmail";
    public const string GetUserById = "dbo.sp_User_GetById";
    public const string CreateUser = "dbo.sp_User_Insert";
    public const string UserAnyExists = "dbo.sp_User_AnyExists";
    public const string GetUserLoginContext = "dbo.sp_User_GetLoginContext";
    public const string ChangeUserPassword = "dbo.sp_User_ChangePassword";
    public const string SetPasswordResetToken = "dbo.sp_User_SetPasswordResetToken";
    public const string ResetUserPassword = "dbo.sp_User_ResetPassword";
    public const string IncrementTokenVersion = "dbo.sp_User_IncrementTokenVersion";
    public const string GetUserPermissions = "dbo.sp_User_GetPermissions";
    public const string GetUserRoles = "dbo.sp_User_GetRoles";

    // Sessions / refresh tokens
    public const string CreateUserLoginSession = "dbo.sp_UserLoginSession_Create";
    public const string EndUserLoginSession = "dbo.sp_UserLoginSession_End";
    public const string EndAllUserLoginSessions = "dbo.sp_UserLoginSession_EndAllForUser";
    public const string IsUserLoginSessionActive = "dbo.sp_UserLoginSession_IsActive";
    public const string InsertRefreshToken = "dbo.sp_RefreshToken_Insert";
    public const string GetRefreshTokenByToken = "dbo.sp_RefreshToken_GetByToken";
    public const string RevokeRefreshToken = "dbo.sp_RefreshToken_Revoke";
    public const string RevokeAllRefreshTokensForUser = "dbo.sp_RefreshToken_RevokeAllForUser";
    public const string GetRevokedRefreshTokenByToken = "dbo.sp_RefreshToken_GetRevokedByToken";

    // Gym
    public const string CreateGym = "dbo.sp_CreateGym";
    public const string UpdateGym = "dbo.sp_UpdateGym";
    public const string DeleteGym = "dbo.sp_DeleteGym";
    public const string GetGymById = "dbo.sp_GetGymById";
    public const string GetAllGyms = "dbo.sp_GetAllGyms";
    public const string SetGymActive = "dbo.sp_SetGymActive";

    // Gym admin
    public const string CreateGymAdmin = "dbo.sp_CreateGymAdmin";
    public const string GetAllGymAdmins = "dbo.sp_GetAllGymAdmins";
    public const string GetGymAdminById = "dbo.sp_GetGymAdminById";
    public const string UpdateGymAdmin = "dbo.sp_UpdateGymAdmin";
    public const string SetGymAdminActive = "dbo.sp_SetGymAdminActive";
    public const string ResetGymAdminPassword = "dbo.sp_ResetGymAdminPassword";

    // Trainer
    public const string CreateTrainer = "dbo.sp_CreateTrainer";
    public const string UpdateTrainer = "dbo.sp_UpdateTrainer";
    public const string DeleteTrainer = "dbo.sp_DeleteTrainer";
    public const string GetTrainerById = "dbo.sp_GetTrainerById";
    public const string GetTrainerByUserId = "dbo.sp_GetTrainerByUserId";
    public const string GetAllTrainers = "dbo.sp_GetAllTrainers";
    public const string SearchTrainers = "dbo.sp_SearchTrainers";
    public const string AssignMemberToTrainer = "dbo.sp_AssignMemberToTrainer";
    public const string RemoveTrainerAssignment = "dbo.sp_RemoveTrainerAssignment";
    public const string GetTrainerMembers = "dbo.sp_GetTrainerMembers";
    public const string GetUnassignedMembers = "dbo.sp_GetUnassignedMembers";
    public const string GetTrainerDashboard = "dbo.sp_GetTrainerDashboard";

    // Member
    public const string CreateMember = "dbo.sp_CreateMember";
    public const string UpdateMember = "dbo.sp_UpdateMember";
    public const string DeleteMember = "dbo.sp_DeleteMember";
    public const string GetMemberById = "dbo.sp_GetMemberById";
    public const string GetMemberByUserId = "dbo.sp_GetMemberByUserId";
    public const string GetAllMembers = "dbo.sp_GetAllMembers";
    public const string SearchMembers = "dbo.sp_SearchMembers";
    public const string AssignTrainerToMember = "dbo.sp_AssignTrainerToMember";
    public const string ActivateMember = "dbo.sp_ActivateMember";
    public const string DeactivateMember = "dbo.sp_DeactivateMember";
    public const string GetMemberDetails = "dbo.sp_GetMemberDetails";
    public const string GetMemberPaymentHistory = "dbo.sp_GetMemberPaymentHistory";
    public const string GetMemberProgress = "dbo.sp_GetMemberProgress";

    // Membership / payment
    public const string CreateMembershipPlan = "dbo.sp_CreateMembershipPlan";
    public const string UpdateMembershipPlan = "dbo.sp_UpdateMembershipPlan";
    public const string DeleteMembershipPlan = "dbo.sp_DeleteMembershipPlan";
    public const string GetMembershipPlans = "dbo.sp_GetMembershipPlans";
    public const string CreateMembership = "dbo.sp_CreateMembership";
    public const string RenewMembership = "dbo.sp_RenewMembership";
    public const string CancelMembership = "dbo.sp_CancelMembership";
    public const string GetMembershipById = "dbo.sp_GetMembershipById";
    public const string GetMembershipDetails = "dbo.sp_GetMembershipDetails";
    public const string GetAllMemberships = "dbo.sp_GetAllMemberships";
    public const string GetExpiredMemberships = "dbo.sp_GetExpiredMemberships";
    public const string CreatePayment = "dbo.sp_CreatePayment";
    public const string GetPaymentHistory = "dbo.sp_GetPaymentHistory";
    public const string GetPaymentsByMember = "dbo.sp_GetPaymentsByMember";
    public const string GetAllPayments = "dbo.sp_GetAllPayments";
    public const string GenerateInvoice = "dbo.sp_GenerateInvoice";
    public const string GetInvoiceById = "dbo.sp_GetInvoiceById";
    public const string GetRevenueDashboard = "dbo.sp_GetRevenueDashboard";
    public const string GetMonthlyRevenueSummary = "dbo.sp_GetMonthlyRevenueSummary";
    public const string CreateRazorpayPaymentOrder = "dbo.sp_CreateRazorpayPaymentOrder";
    public const string GetPaymentByRazorpayOrderId = "dbo.sp_GetPaymentByRazorpayOrderId";
    public const string ConfirmRazorpayPayment = "dbo.sp_ConfirmRazorpayPayment";
    public const string FailRazorpayPayment = "dbo.sp_FailRazorpayPayment";
    public const string RefundPayment = "dbo.sp_RefundPayment";
    public const string GetMemberPayableMembership = "dbo.sp_GetMemberPayableMembership";
    public const string CreateNotificationTemplate = "dbo.sp_CreateNotificationTemplate";
    public const string UpdateNotificationTemplate = "dbo.sp_UpdateNotificationTemplate";
    public const string DeleteNotificationTemplate = "dbo.sp_DeleteNotificationTemplate";
    public const string GetNotificationTemplates = "dbo.sp_GetNotificationTemplates";
    public const string UpsertNotificationSetting = "dbo.sp_UpsertNotificationSetting";
    public const string GetNotificationSettings = "dbo.sp_GetNotificationSettings";
    public const string LogNotification = "dbo.sp_LogNotification";
    public const string UpdateNotificationLogStatus = "dbo.sp_UpdateNotificationLogStatus";
    public const string SearchNotificationLogs = "dbo.sp_SearchNotificationLogs";
    public const string GetPendingNotifications = "dbo.sp_GetPendingNotifications";
    public const string GetNotificationDashboard = "dbo.sp_GetNotificationDashboard";
    public const string GetMembershipsExpiringForNotification = "dbo.sp_GetMembershipsExpiringForNotification";
    public const string GetAllActiveGymIds = "dbo.sp_GetAllActiveGymIds";
    public const string GetAnalyticsDashboardOverview = "dbo.sp_GetAnalyticsDashboardOverview";
    public const string GetAnalyticsRevenueSummary = "dbo.sp_GetAnalyticsRevenueSummary";
    public const string GetAnalyticsRevenueByPlan = "dbo.sp_GetAnalyticsRevenueByPlan";
    public const string GetAnalyticsRevenueByPaymentMethod = "dbo.sp_GetAnalyticsRevenueByPaymentMethod";
    public const string GetAnalyticsMembershipSummary = "dbo.sp_GetAnalyticsMembershipSummary";
    public const string GetAnalyticsMembershipGrowthTrend = "dbo.sp_GetAnalyticsMembershipGrowthTrend";
    public const string GetAnalyticsPlanDistribution = "dbo.sp_GetAnalyticsPlanDistribution";
    public const string GetAnalyticsAttendanceSummary = "dbo.sp_GetAnalyticsAttendanceSummary";
    public const string GetAnalyticsAttendanceWeeklyTrend = "dbo.sp_GetAnalyticsAttendanceWeeklyTrend";
    public const string GetAnalyticsAttendanceMonthlyTrend = "dbo.sp_GetAnalyticsAttendanceMonthlyTrend";
    public const string GetAnalyticsMostActiveMembers = "dbo.sp_GetAnalyticsMostActiveMembers";
    public const string GetAnalyticsLeastActiveMembers = "dbo.sp_GetAnalyticsLeastActiveMembers";
    public const string GetAnalyticsMemberAttendancePercentage = "dbo.sp_GetAnalyticsMemberAttendancePercentage";
    public const string GetAnalyticsTrainerSummary = "dbo.sp_GetAnalyticsTrainerSummary";
    public const string GetAnalyticsTrainerPerformance = "dbo.sp_GetAnalyticsTrainerPerformance";
    public const string GetAnalyticsWorkoutSummary = "dbo.sp_GetAnalyticsWorkoutSummary";
    public const string GetAnalyticsDietSummary = "dbo.sp_GetAnalyticsDietSummary";
    public const string GetAnalyticsRecentPayments = "dbo.sp_GetAnalyticsRecentPayments";
    public const string GetAnalyticsExpiringMemberships = "dbo.sp_GetAnalyticsExpiringMemberships";
    public const string GetAnalyticsNewMembers = "dbo.sp_GetAnalyticsNewMembers";
    public const string GetAnalyticsRecentNotifications = "dbo.sp_GetAnalyticsRecentNotifications";

    public const string SaasGetAllPlans = "dbo.sp_Saas_GetAllPlans";
    public const string SaasGetPlanById = "dbo.sp_Saas_GetPlanById";
    public const string SaasGetGymSubscription = "dbo.sp_Saas_GetGymSubscription";
    public const string SaasGetGymUsage = "dbo.sp_Saas_GetGymUsage";
    public const string SaasCheckTenantLimit = "dbo.sp_Saas_CheckTenantLimit";
    public const string SaasCreateTrialSubscription = "dbo.sp_Saas_CreateTrialSubscription";
    public const string SaasUpdateSubscriptionPlan = "dbo.sp_Saas_UpdateSubscriptionPlan";
    public const string SaasCancelSubscription = "dbo.sp_Saas_CancelSubscription";
    public const string SaasCreatePendingPayment = "dbo.sp_Saas_CreatePendingPayment";
    public const string SaasCompletePayment = "dbo.sp_Saas_CompletePayment";
    public const string SaasGetPendingPayment = "dbo.sp_Saas_GetPendingPayment";
    public const string SaasGetPlatformDashboard = "dbo.sp_Saas_GetPlatformDashboard";
    public const string SaasExpireSubscriptions = "dbo.sp_Saas_ExpireSubscriptions";
    public const string SaasSeedNotificationSettings = "dbo.sp_Saas_SeedNotificationSettings";
    public const string GymUpdateBranding = "dbo.sp_Gym_UpdateBranding";

    // Attendance
    public const string GetAttendanceStatuses = "dbo.sp_GetAttendanceStatuses";
    public const string MemberAttendanceCheckIn = "dbo.sp_MemberAttendance_CheckIn";
    public const string MemberAttendanceCheckOut = "dbo.sp_MemberAttendance_CheckOut";
    public const string MemberAttendanceMark = "dbo.sp_MemberAttendance_Mark";
    public const string GetMemberAttendanceById = "dbo.sp_GetMemberAttendanceById";
    public const string GetTodayMemberAttendance = "dbo.sp_GetTodayMemberAttendance";
    public const string GetMemberAttendanceByDateRange = "dbo.sp_GetMemberAttendanceByDateRange";
    public const string GetMemberAttendanceHistory = "dbo.sp_GetMemberAttendanceHistory";
    public const string GetDailyAttendanceReport = "dbo.sp_GetDailyAttendanceReport";
    public const string GetMonthlyAttendanceReport = "dbo.sp_GetMonthlyAttendanceReport";
    public const string TrainerAttendanceCheckIn = "dbo.sp_TrainerAttendance_CheckIn";
    public const string TrainerAttendanceCheckOut = "dbo.sp_TrainerAttendance_CheckOut";
    public const string GetTrainerAttendanceByDateRange = "dbo.sp_GetTrainerAttendanceByDateRange";
    public const string GetAttendanceDashboard = "dbo.sp_GetAttendanceDashboard";
    public const string AuditLogInsert = "dbo.sp_AuditLog_Insert";
    public const string SearchAuditLogs = "dbo.sp_SearchAuditLogs";
    public const string GetAuditLogSummary = "dbo.sp_GetAuditLogSummary";

    // Diet plans
    public const string GetDietCategories = "dbo.sp_GetDietCategories";
    public const string CreateDietCategory = "dbo.sp_CreateDietCategory";
    public const string GetDietPlans = "dbo.sp_GetDietPlans";
    public const string GetDietPlanById = "dbo.sp_GetDietPlanById";
    public const string CreateDietPlan = "dbo.sp_CreateDietPlan";
    public const string UpdateDietPlan = "dbo.sp_UpdateDietPlan";
    public const string DeleteDietPlan = "dbo.sp_DeleteDietPlan";
    public const string SetDietPlanActive = "dbo.sp_SetDietPlanActive";
    public const string ReplaceDietPlanItems = "dbo.sp_ReplaceDietPlanItems";
    public const string CloneDietPlan = "dbo.sp_CloneDietPlan";
    public const string AssignDietPlanToMember = "dbo.sp_AssignDietPlanToMember";
    public const string UnassignDietPlan = "dbo.sp_UnassignDietPlan";
    public const string GetMemberAssignedDietPlan = "dbo.sp_GetMemberAssignedDietPlan";
    public const string GetMemberDietAssignments = "dbo.sp_GetMemberDietAssignments";

    // Workout plans
    public const string GetExerciseCategories = "dbo.sp_GetExerciseCategories";
    public const string CreateExerciseCategory = "dbo.sp_CreateExerciseCategory";
    public const string GetExerciseLibrary = "dbo.sp_GetExerciseLibrary";
    public const string GetExerciseById = "dbo.sp_GetExerciseById";
    public const string CreateExercise = "dbo.sp_CreateExercise";
    public const string UpdateExercise = "dbo.sp_UpdateExercise";
    public const string DeleteExercise = "dbo.sp_DeleteExercise";
    public const string GetWorkoutPlans = "dbo.sp_GetWorkoutPlans";
    public const string GetWorkoutPlanById = "dbo.sp_GetWorkoutPlanById";
    public const string CreateWorkoutPlan = "dbo.sp_CreateWorkoutPlan";
    public const string UpdateWorkoutPlan = "dbo.sp_UpdateWorkoutPlan";
    public const string DeleteWorkoutPlan = "dbo.sp_DeleteWorkoutPlan";
    public const string SetWorkoutPlanActive = "dbo.sp_SetWorkoutPlanActive";
    public const string ReplaceWorkoutPlanExercises = "dbo.sp_ReplaceWorkoutPlanExercises";
    public const string CloneWorkoutPlan = "dbo.sp_CloneWorkoutPlan";
    public const string AssignWorkoutPlanToMember = "dbo.sp_AssignWorkoutPlanToMember";
    public const string UnassignWorkoutPlan = "dbo.sp_UnassignWorkoutPlan";
    public const string GetMemberWorkoutPlan = "dbo.sp_GetMemberWorkoutPlan";
    public const string UpsertMemberWorkoutProgress = "dbo.sp_UpsertMemberWorkoutProgress";

    // Files
    public const string FileCreate = "dbo.sp_File_Create";
    public const string FileGetById = "dbo.sp_File_GetById";
    public const string FileSoftDelete = "dbo.sp_File_SoftDelete";
    public const string FileUpdatePublicUrl = "dbo.sp_File_UpdatePublicUrl";
    public const string MemberFileCreate = "dbo.sp_MemberFile_Create";
    public const string TrainerFileCreate = "dbo.sp_TrainerFile_Create";
    public const string GymSetLogoFile = "dbo.sp_Gym_SetLogoFile";
    public const string MemberFilesGetByMember = "dbo.sp_MemberFiles_GetByMember";
    public const string TrainerFilesGetByTrainer = "dbo.sp_TrainerFiles_GetByTrainer";
    public const string FileGetGymLogo = "dbo.sp_File_GetGymLogo";

    // Production hardening
    public const string MemberGetGymId = "dbo.sp_Member_GetGymId";
    public const string TrainerGetGymId = "dbo.sp_Trainer_GetGymId";

    // Dashboard
    public const string GetDashboardStatistics = "dbo.sp_GetDashboardStatistics";
    public const string GetGymDashboardStatistics = "dbo.sp_GetGymDashboardStatistics";

    // Roles / privileges
    public const string GetRoleById = "dbo.sp_Role_GetById";
    public const string GetRoleByName = "dbo.sp_Role_GetByName";
    public const string GetAllRoles = "dbo.sp_Role_GetAll";
    public const string CreateRole = "dbo.sp_Role_Insert";
    public const string UpdateRole = "dbo.sp_Role_Update";
    public const string DeleteRole = "dbo.sp_Role_Delete";
    public const string RoleAnyExists = "dbo.sp_Role_AnyExists";
    public const string RoleIsAssignedToUsers = "dbo.sp_Role_IsAssignedToUsers";

    public const string GetPrivilegeById = "dbo.sp_Privilege_GetById";
    public const string GetPrivilegeByName = "dbo.sp_Privilege_GetByName";
    public const string GetAllPrivileges = "dbo.sp_Privilege_GetAll";
    public const string CreatePrivilege = "dbo.sp_Privilege_Insert";
    public const string UpdatePrivilege = "dbo.sp_Privilege_Update";
    public const string DeletePrivilege = "dbo.sp_Privilege_Delete";
    public const string PrivilegeIsAssignedToRoles = "dbo.sp_Privilege_IsAssignedToRoles";

    public const string AssignPrivilegeToRole = "dbo.sp_AssignPrivilegeToRole";
    public const string RemovePrivilegeFromRole = "dbo.sp_RemovePrivilegeFromRole";
    public const string GetPrivilegesByRoleId = "dbo.sp_RolePrivilege_GetByRoleId";
    public const string GetRolePrivilege = "dbo.sp_RolePrivilege_Get";
    public const string GetAllRolePrivileges = "dbo.sp_RolePrivilege_GetAll";

    public const string GetUserRolesByUserId = "dbo.sp_UserRole_GetByUserId";
    public const string GetUserRole = "dbo.sp_UserRole_Get";
    public const string AssignUserRole = "dbo.sp_UserRole_Insert";
    public const string RemoveUserRole = "dbo.sp_UserRole_Delete";

    // CRM / Leads
    public const string CreateLead = "dbo.sp_CreateLead";
    public const string UpdateLead = "dbo.sp_UpdateLead";
    public const string UpdateLeadStatus = "dbo.sp_UpdateLeadStatus";
    public const string DeleteLead = "dbo.sp_DeleteLead";
    public const string GetLeadById = "dbo.sp_GetLeadById";
    public const string GetLeadsPaged = "dbo.sp_GetLeadsPaged";
    public const string SearchLeads = "dbo.sp_SearchLeads";
    public const string AssignTrainerToLead = "dbo.sp_AssignTrainerToLead";
    public const string ConvertLeadToMember = "dbo.sp_ConvertLeadToMember";
    public const string ScheduleTrialSession = "dbo.sp_ScheduleTrialSession";
    public const string RecordTrialFeedback = "dbo.sp_RecordTrialFeedback";
    public const string CreateLeadFollowUp = "dbo.sp_CreateLeadFollowUp";
    public const string UpdateLeadFollowUp = "dbo.sp_UpdateLeadFollowUp";
    public const string CreateLeadActivity = "dbo.sp_CreateLeadActivity";
    public const string GetLeadActivities = "dbo.sp_GetLeadActivities";
    public const string GetLeadFollowUps = "dbo.sp_GetLeadFollowUps";
    public const string GetLeadTrials = "dbo.sp_GetLeadTrials";
    public const string GetLeadDashboard = "dbo.sp_GetLeadDashboard";
    public const string GetLeadSourceAnalytics = "dbo.sp_GetLeadSourceAnalytics";
    public const string GetLeadStatusAnalytics = "dbo.sp_GetLeadStatusAnalytics";
    public const string GetLeadConversionReport = "dbo.sp_GetLeadConversionReport";
    public const string GetTrainerLeadConversion = "dbo.sp_GetTrainerLeadConversion";
    public const string GetPendingFollowUps = "dbo.sp_GetPendingFollowUps";
    public const string GetTodaysTrials = "dbo.sp_GetTodaysTrials";
    public const string GetLeadReminderCandidates = "dbo.sp_GetLeadReminderCandidates";

    // Financial / Payroll / Expenses
    public const string SeedExpenseCategories = "dbo.sp_SeedExpenseCategories";
    public const string GetExpenseCategories = "dbo.sp_GetExpenseCategories";
    public const string CreateExpense = "dbo.sp_CreateExpense";
    public const string UpdateExpense = "dbo.sp_UpdateExpense";
    public const string DeleteExpense = "dbo.sp_DeleteExpense";
    public const string GetExpenseById = "dbo.sp_GetExpenseById";
    public const string GetExpensesPaged = "dbo.sp_GetExpensesPaged";
    public const string CreatePayroll = "dbo.sp_CreatePayroll";
    public const string UpdatePayroll = "dbo.sp_UpdatePayroll";
    public const string GetPayrollById = "dbo.sp_GetPayrollById";
    public const string GetPayrollsPaged = "dbo.sp_GetPayrollsPaged";
    public const string GenerateMonthlyPayroll = "dbo.sp_GenerateMonthlyPayroll";
    public const string ApprovePayroll = "dbo.sp_ApprovePayroll";
    public const string PayPayroll = "dbo.sp_PayPayroll";
    public const string CreateTrainerCommission = "dbo.sp_CreateTrainerCommission";
    public const string GetExpenseDashboard = "dbo.sp_GetExpenseDashboard";
    public const string GetPayrollDashboard = "dbo.sp_GetPayrollDashboard";
    public const string GetProfitLossSummary = "dbo.sp_GetProfitLossSummary";
    public const string GetMonthlyProfitTrend = "dbo.sp_GetMonthlyProfitTrend";
    public const string GetExpenseCategoryBreakdown = "dbo.sp_GetExpenseCategoryBreakdown";
    public const string GetTrainerCommissionReport = "dbo.sp_GetTrainerCommissionReport";
    public const string GetPayrollCostTrend = "dbo.sp_GetPayrollCostTrend";
    public const string GetCommissionTrend = "dbo.sp_GetCommissionTrend";
    public const string GetFinancialRevenueSummary = "dbo.sp_GetFinancialRevenueSummary";

    // Member Self-Service
    public const string CreateMemberGoal = "dbo.sp_CreateMemberGoal";
    public const string UpdateMemberGoal = "dbo.sp_UpdateMemberGoal";
    public const string UpdateMemberGoalProgress = "dbo.sp_UpdateMemberGoalProgress";
    public const string CompleteMemberGoal = "dbo.sp_CompleteMemberGoal";
    public const string GetMemberGoals = "dbo.sp_GetMemberGoals";
    public const string GetMemberGoalById = "dbo.sp_GetMemberGoalById";
    public const string CreateMemberProgress = "dbo.sp_CreateMemberProgress";
    public const string GetMemberProgressHistory = "dbo.sp_GetMemberProgressHistory";
    public const string CreateMemberProgressPhoto = "dbo.sp_CreateMemberProgressPhoto";
    public const string GetMemberProgressPhotos = "dbo.sp_GetMemberProgressPhotos";
    public const string UpsertWaterIntake = "dbo.sp_UpsertWaterIntake";
    public const string GetWaterIntake = "dbo.sp_GetWaterIntake";
    public const string GetWaterIntakeHistory = "dbo.sp_GetWaterIntakeHistory";
    public const string UpsertWorkoutTracking = "dbo.sp_UpsertWorkoutTracking";
    public const string GetWorkoutTrackingHistory = "dbo.sp_GetWorkoutTrackingHistory";
    public const string GetWorkoutStreak = "dbo.sp_GetWorkoutStreak";
    public const string UpsertDietTracking = "dbo.sp_UpsertDietTracking";
    public const string GetDietTrackingHistory = "dbo.sp_GetDietTrackingHistory";
    public const string GetDietComplianceSummary = "dbo.sp_GetDietComplianceSummary";
    public const string GetOrCreateReferralCode = "dbo.sp_GetOrCreateReferralCode";
    public const string RecordReferralConversion = "dbo.sp_RecordReferralConversion";
    public const string GetReferralStats = "dbo.sp_GetReferralStats";
    public const string CreateMemberFeedback = "dbo.sp_CreateMemberFeedback";
    public const string GetMemberFeedback = "dbo.sp_GetMemberFeedback";
    public const string GetOrCreateMemberQrToken = "dbo.sp_GetOrCreateMemberQrToken";
    public const string MemberAttendanceQrCheckIn = "dbo.sp_MemberAttendance_QrCheckIn";
    public const string GetMemberSelfServiceDashboard = "dbo.sp_GetMemberSelfServiceDashboard";
    public const string GetMemberSelfServiceAnalytics = "dbo.sp_GetMemberSelfServiceAnalytics";

    // Multi-Branch
    public const string EnsureDefaultBranch = "dbo.sp_EnsureDefaultBranch";
    public const string CreateBranch = "dbo.sp_CreateBranch";
    public const string UpdateBranch = "dbo.sp_UpdateBranch";
    public const string SetBranchActive = "dbo.sp_SetBranchActive";
    public const string DeleteBranch = "dbo.sp_DeleteBranch";
    public const string GetBranchById = "dbo.sp_GetBranchById";
    public const string GetBranchesPaged = "dbo.sp_GetBranchesPaged";
    public const string GetAllBranches = "dbo.sp_GetAllBranches";
    public const string AssignBranchManager = "dbo.sp_AssignBranchManager";
    public const string TransferMemberBranch = "dbo.sp_TransferMemberBranch";
    public const string TransferTrainerBranch = "dbo.sp_TransferTrainerBranch";
    public const string GetBranchTransferHistory = "dbo.sp_GetBranchTransferHistory";
    public const string UpsertBranchTarget = "dbo.sp_UpsertBranchTarget";
    public const string GetBranchTargets = "dbo.sp_GetBranchTargets";
    public const string CreateBranchAnnouncement = "dbo.sp_CreateBranchAnnouncement";
    public const string GetBranchAnnouncements = "dbo.sp_GetBranchAnnouncements";
    public const string DeleteBranchAnnouncement = "dbo.sp_DeleteBranchAnnouncement";
    public const string GetBranchAnnouncementRecipients = "dbo.sp_GetBranchAnnouncementRecipients";
    public const string GetBranchDashboard = "dbo.sp_GetBranchDashboard";
    public const string GetBranchAnalyticsComparison = "dbo.sp_GetBranchAnalyticsComparison";

    // Mobile Push
    public const string RegisterDeviceToken = "dbo.sp_RegisterDeviceToken";
    public const string UnregisterDeviceToken = "dbo.sp_UnregisterDeviceToken";
    public const string GetActiveDeviceTokensForUser = "dbo.sp_GetActiveDeviceTokensForUser";
    public const string CreatePushNotification = "dbo.sp_CreatePushNotification";
    public const string UpdatePushNotificationStatus = "dbo.sp_UpdatePushNotificationStatus";
    public const string MarkPushNotificationsRead = "dbo.sp_MarkPushNotificationsRead";
    public const string RecordPushNotificationEngagement = "dbo.sp_RecordPushNotificationEngagement";
    public const string GetPushNotificationsPaged = "dbo.sp_GetPushNotificationsPaged";
    public const string GetPendingPushNotifications = "dbo.sp_GetPendingPushNotifications";
    public const string GetOrCreateNotificationPreferences = "dbo.sp_GetOrCreateNotificationPreferences";
    public const string UpdateNotificationPreferences = "dbo.sp_UpdateNotificationPreferences";
    public const string IsPushCategoryEnabled = "dbo.sp_IsPushCategoryEnabled";
    public const string GetMobileDashboard = "dbo.sp_GetMobileDashboard";
    public const string GetMobileSyncProfile = "dbo.sp_GetMobileSyncProfile";
    public const string GetMobileSyncDelta = "dbo.sp_GetMobileSyncDelta";
    public const string GetPushNotificationAnalytics = "dbo.sp_GetPushNotificationAnalytics";
    public const string SearchPushNotificationCampaigns = "dbo.sp_SearchPushNotificationCampaigns";
    public const string GetMembershipsExpiringForPush = "dbo.sp_GetMembershipsExpiringForPush";
    public const string GetMembersForAttendancePushReminder = "dbo.sp_GetMembersForAttendancePushReminder";
    public const string GetMembersForWorkoutPushReminder = "dbo.sp_GetMembersForWorkoutPushReminder";
    public const string GetMembersForDietPushReminder = "dbo.sp_GetMembersForDietPushReminder";
    public const string GetMembersForGoalPushReminder = "dbo.sp_GetMembersForGoalPushReminder";
    public const string GetMobilePushRecipientUserIds = "dbo.sp_GetMobilePushRecipientUserIds";

    // AI Trainer Recommendations
    public const string CreateAiRecommendation = "dbo.sp_CreateAiRecommendation";
    public const string GetAiRecommendationsPaged = "dbo.sp_GetAiRecommendationsPaged";
    public const string MarkAiRecommendationAccepted = "dbo.sp_MarkAiRecommendationAccepted";
    public const string CreateAiInsight = "dbo.sp_CreateAiInsight";
    public const string GetAiInsightsPaged = "dbo.sp_GetAiInsightsPaged";
    public const string UpsertMemberRiskScore = "dbo.sp_UpsertMemberRiskScore";
    public const string GetMemberRiskScoresPaged = "dbo.sp_GetMemberRiskScoresPaged";
    public const string GetMemberRiskScoreByMemberId = "dbo.sp_GetMemberRiskScoreByMemberId";
    public const string CreateAiGenerationLog = "dbo.sp_CreateAiGenerationLog";
    public const string GetAiAnalytics = "dbo.sp_GetAiAnalytics";
    public const string GetAiDashboard = "dbo.sp_GetAiDashboard";
    public const string GetLeadScoringPaged = "dbo.sp_GetLeadScoringPaged";
    public const string GetMemberAiAnalysisContext = "dbo.sp_GetMemberAiAnalysisContext";
    public const string GetActiveMembersForAiJob = "dbo.sp_GetActiveMembersForAiJob";
    public const string GetGymsForAiJob = "dbo.sp_GetGymsForAiJob";
    public const string GetBusinessAiContext = "dbo.sp_GetBusinessAiContext";
    public const string GetBranchAttendanceForAi = "dbo.sp_GetBranchAttendanceForAi";
    public const string GetHighRiskMembersForNotification = "dbo.sp_GetHighRiskMembersForNotification";

    // Booking & Slot Reservation
    public const string UpsertBookingSettings = "dbo.sp_UpsertBookingSettings";
    public const string GetBookingSettings = "dbo.sp_GetBookingSettings";
    public const string CreateClassSchedule = "dbo.sp_CreateClassSchedule";
    public const string UpdateClassSchedule = "dbo.sp_UpdateClassSchedule";
    public const string GetClassScheduleById = "dbo.sp_GetClassScheduleById";
    public const string GetClassSchedulesPaged = "dbo.sp_GetClassSchedulesPaged";
    public const string DeleteClassSchedule = "dbo.sp_DeleteClassSchedule";
    public const string GetAvailableSlots = "dbo.sp_GetAvailableSlots";
    public const string CreateSlotBooking = "dbo.sp_CreateSlotBooking";
    public const string CancelSlotBooking = "dbo.sp_CancelSlotBooking";
    public const string JoinBookingWaitlist = "dbo.sp_JoinBookingWaitlist";
    public const string GetSlotBookingsPaged = "dbo.sp_GetSlotBookingsPaged";
    public const string BookingQrCheckIn = "dbo.sp_BookingQrCheckIn";
    public const string ProcessNoShowBookings = "dbo.sp_ProcessNoShowBookings";
    public const string GetBookingsForReminder = "dbo.sp_GetBookingsForReminder";
    public const string GetTrainerSchedule = "dbo.sp_GetTrainerSchedule";
    public const string CreateTrainerAvailability = "dbo.sp_CreateTrainerAvailability";
    public const string GetTrainerAvailability = "dbo.sp_GetTrainerAvailability";
    public const string GetBookingAnalytics = "dbo.sp_GetBookingAnalytics";
    public const string GetBookingAiContext = "dbo.sp_GetBookingAiContext";
    public const string GetGymsForBookingJob = "dbo.sp_GetGymsForBookingJob";

    // Public Gym Website Builder
    public const string UpsertGymWebsiteSettings = "dbo.sp_UpsertGymWebsiteSettings";
    public const string GetGymWebsiteSettings = "dbo.sp_GetGymWebsiteSettings";
    public const string SetGymWebsitePublished = "dbo.sp_SetGymWebsitePublished";
    public const string CreateGymWebsitePage = "dbo.sp_CreateGymWebsitePage";
    public const string UpdateGymWebsitePage = "dbo.sp_UpdateGymWebsitePage";
    public const string DeleteGymWebsitePage = "dbo.sp_DeleteGymWebsitePage";
    public const string GetGymWebsitePages = "dbo.sp_GetGymWebsitePages";
    public const string CreateGymWebsiteSection = "dbo.sp_CreateGymWebsiteSection";
    public const string UpdateGymWebsiteSection = "dbo.sp_UpdateGymWebsiteSection";
    public const string DeleteGymWebsiteSection = "dbo.sp_DeleteGymWebsiteSection";
    public const string GetGymWebsiteSections = "dbo.sp_GetGymWebsiteSections";
    public const string CreateGymWebsiteTestimonial = "dbo.sp_CreateGymWebsiteTestimonial";
    public const string UpdateGymWebsiteTestimonial = "dbo.sp_UpdateGymWebsiteTestimonial";
    public const string DeleteGymWebsiteTestimonial = "dbo.sp_DeleteGymWebsiteTestimonial";
    public const string GetGymWebsiteTestimonials = "dbo.sp_GetGymWebsiteTestimonials";
    public const string CreateGymWebsiteGalleryItem = "dbo.sp_CreateGymWebsiteGalleryItem";
    public const string UpdateGymWebsiteGalleryItem = "dbo.sp_UpdateGymWebsiteGalleryItem";
    public const string DeleteGymWebsiteGalleryItem = "dbo.sp_DeleteGymWebsiteGalleryItem";
    public const string GetGymWebsiteGallery = "dbo.sp_GetGymWebsiteGallery";
    public const string CreateWebsiteLeadCapture = "dbo.sp_CreateWebsiteLeadCapture";
    public const string SearchWebsiteLeadCaptures = "dbo.sp_SearchWebsiteLeadCaptures";
    public const string ConvertWebsiteLeadCapture = "dbo.sp_ConvertWebsiteLeadCapture";
    public const string GetPublicWebsiteBySlug = "dbo.sp_GetPublicWebsiteBySlug";
    public const string GetGymIdByWebsiteSlug = "dbo.sp_GetGymIdByWebsiteSlug";
    public const string GetWebsiteAnalyticsOverview = "dbo.sp_GetWebsiteAnalyticsOverview";
    public const string GetWebsiteNotificationRecipients = "dbo.sp_GetWebsiteNotificationRecipients";

    // White Label SaaS
    public const string UpsertWhiteLabelSettings = "dbo.sp_UpsertWhiteLabelSettings";
    public const string GetWhiteLabelSettings = "dbo.sp_GetWhiteLabelSettings";
    public const string SetWhiteLabelEnabled = "dbo.sp_SetWhiteLabelEnabled";
    public const string UpdateWhiteLabelDomain = "dbo.sp_UpdateWhiteLabelDomain";
    public const string GetWhiteLabelBySubDomain = "dbo.sp_GetWhiteLabelBySubDomain";
    public const string GetWhiteLabelLoginBranding = "dbo.sp_GetWhiteLabelLoginBranding";
    public const string CreateWhiteLabelEmailTemplate = "dbo.sp_CreateWhiteLabelEmailTemplate";
    public const string UpdateWhiteLabelEmailTemplate = "dbo.sp_UpdateWhiteLabelEmailTemplate";
    public const string DeleteWhiteLabelEmailTemplate = "dbo.sp_DeleteWhiteLabelEmailTemplate";
    public const string GetWhiteLabelEmailTemplates = "dbo.sp_GetWhiteLabelEmailTemplates";
    public const string UpsertWhiteLabelMobileSettings = "dbo.sp_UpsertWhiteLabelMobileSettings";
    public const string GetWhiteLabelMobileSettings = "dbo.sp_GetWhiteLabelMobileSettings";
    public const string GetWhiteLabelPlatformDashboard = "dbo.sp_GetWhiteLabelPlatformDashboard";
}
