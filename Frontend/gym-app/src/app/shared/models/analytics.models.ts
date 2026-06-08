export interface AnalyticsDashboard {
  overview: AnalyticsOverview;
  revenue: RevenueAnalytics;
  membership: MembershipAnalytics;
  attendance: AttendanceAnalytics;
  trainers: TrainerAnalytics;
  workouts: WorkoutAnalytics;
  diets: DietAnalytics;
  widgets: AnalyticsWidgets;
}

export interface AnalyticsOverview {
  totalMembers: number;
  activeMembers: number;
  revenueToday: number;
  revenueThisMonth: number;
  expiringMemberships: number;
  activeTrainers: number;
}

export interface RevenueAnalytics {
  revenueToday: number;
  revenueThisWeek: number;
  revenueThisMonth: number;
  revenueThisYear: number;
  failedPaymentsCount: number;
  revenueTrend: TrendPoint[];
  revenueByPlan: NamedValue[];
  revenueByPaymentMethod: NamedValue[];
}

export interface MembershipAnalytics {
  activeMembers: number;
  expiredMembers: number;
  expiringIn7Days: number;
  newRegistrationsThisMonth: number;
  activeMemberships: number;
  growthTrend: GrowthPoint[];
  planDistribution: NamedCount[];
}

export interface AttendanceAnalytics {
  todayAttendanceCount: number;
  uniqueMembersToday: number;
  weeklyTrend: AttendanceTrendPoint[];
  monthlyTrend: NamedCount[];
  mostActiveMembers: MemberActivity[];
  leastActiveMembers: MemberActivity[];
  attendancePercentage: MemberAttendancePercent[];
}

export interface TrainerAnalytics {
  activeTrainers: number;
  assignedMembers: number;
  performance: TrainerPerformance[];
}

export interface WorkoutAnalytics {
  activeWorkoutPlans: number;
  completedWorkoutPlans: number;
  completionPercentage: number;
}

export interface DietAnalytics {
  activeDietPlans: number;
  compliancePercentage: number;
}

export interface AnalyticsWidgets {
  recentPayments: RecentPaymentWidget[];
  expiringMemberships: ExpiringMembershipWidget[];
  newMembers: NewMemberWidget[];
  recentNotifications: RecentNotificationWidget[];
}

export interface TrendPoint {
  year: number;
  month: number;
  monthLabel: string;
  value: number;
}

export interface GrowthPoint {
  year: number;
  month: number;
  monthLabel: string;
  newMembers: number;
}

export interface NamedValue {
  name: string;
  value: number;
  count: number;
}

export interface NamedCount {
  name: string;
  count: number;
}

export interface AttendanceTrendPoint {
  date: string;
  dayLabel: string;
  count: number;
}

export interface MemberActivity {
  memberId: number;
  memberName: string;
  attendanceCount: number;
}

export interface MemberAttendancePercent {
  memberId: number;
  memberName: string;
  attendancePercentage: number;
}

export interface TrainerPerformance {
  trainerId: number;
  trainerName: string;
  assignedMembers: number;
  todayAttendance: number;
}

export interface RecentPaymentWidget {
  paymentId: number;
  amount: number;
  paymentMethod: string;
  status: string;
  paymentDate: string;
  memberName?: string;
}

export interface ExpiringMembershipWidget {
  membershipId: number;
  endDate: string;
  memberName: string;
  planName: string;
}

export interface NewMemberWidget {
  memberId: number;
  memberName: string;
  memberEmail: string;
  joinDate: string;
}

export interface RecentNotificationWidget {
  logId: number;
  notificationType: string;
  status: string;
  recipientPhone: string;
  createdAt: string;
}
