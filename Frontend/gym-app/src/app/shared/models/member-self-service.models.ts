export interface MemberGoal {
  goalId: number;
  goalType: string;
  targetValue: number;
  currentValue: number;
  targetDate: string;
  status: string;
  progressPercent: number;
}

export interface CreateMemberGoalDto {
  goalType: string;
  targetValue: number;
  currentValue?: number;
  targetDate: string;
}

export interface UpdateMemberGoalDto {
  goalType: string;
  targetValue: number;
  targetDate: string;
}

export interface UpdateGoalProgressDto {
  currentValue: number;
}

export interface MemberProgressEntry {
  progressId: number;
  weight?: number;
  bmi?: number;
  bodyFatPercentage?: number;
  chest?: number;
  waist?: number;
  arms?: number;
  thighs?: number;
  notes?: string;
  progressDate: string;
}

export interface CreateMemberProgressDto {
  weight?: number;
  bmi?: number;
  bodyFatPercentage?: number;
  chest?: number;
  waist?: number;
  arms?: number;
  thighs?: number;
  notes?: string;
  progressDate: string;
}

export interface MemberProgressPhoto {
  progressPhotoId: number;
  fileId: number;
  photoType: string;
  uploadedDate: string;
  originalFileName?: string;
}

export interface CreateProgressPhotoDto {
  fileId: number;
  photoType: string;
}

export interface WaterIntake {
  waterIntakeId: number;
  targetLitres: number;
  consumedLitres: number;
  logDate: string;
  completionPercent: number;
}

export interface UpsertWaterIntakeDto {
  targetLitres: number;
  consumedLitres: number;
  logDate?: string;
}

export interface WorkoutTracking {
  workoutTrackingId: number;
  workoutPlanId: number;
  workoutPlanName?: string;
  exerciseCompleted?: string;
  completionPercentage: number;
  workoutDate: string;
}

export interface UpsertWorkoutTrackingDto {
  workoutPlanId: number;
  exerciseCompleted?: string;
  completionPercentage: number;
  workoutDate?: string;
}

export interface DietTracking {
  dietTrackingId: number;
  dietPlanId: number;
  dietPlanName?: string;
  compliancePercentage: number;
  mealsCompleted: number;
  trackingDate: string;
}

export interface UpsertDietTrackingDto {
  dietPlanId: number;
  compliancePercentage: number;
  mealsCompleted: number;
  trackingDate?: string;
}

export interface DietComplianceSummary {
  dailyCompliance: number;
  weeklyCompliance: number;
  monthlyCompliance: number;
}

export interface ReferralDto {
  referralId: number;
  referralCode: string;
  referredMemberName?: string;
  rewardPoints: number;
  status: string;
}

export interface ReferralStats {
  referralCode: string;
  totalReferrals: number;
  convertedReferrals: number;
  totalRewardPoints: number;
  referrals: ReferralDto[];
}

export interface MemberFeedback {
  feedbackId: number;
  rating: number;
  comments?: string;
  trainerId?: number;
  trainerName?: string;
  feedbackType: string;
  createdDate: string;
}

export interface CreateMemberFeedbackDto {
  rating: number;
  comments?: string;
  trainerId?: number;
  feedbackType: string;
}

export interface MemberQrCode {
  payload: string;
  qrCodeBase64: string;
  memberId: number;
}

export interface MemberDashboardMembership {
  membershipId: number;
  planName: string;
  startDate: string;
  endDate: string;
  status: string;
  remainingDays: number;
}

export interface MemberDashboardPayment {
  paymentId: number;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  status: string;
  invoiceNumber?: string;
}

export interface MemberSelfServiceDashboard {
  activeMembership?: MemberDashboardMembership;
  attendancePercentage: number;
  currentGoal?: MemberGoal;
  todayWorkout?: WorkoutTracking;
  todayDiet?: DietTracking;
  todayWater?: WaterIntake;
  recentPayments: MemberDashboardPayment[];
  referralStats: ReferralStats;
  workoutStreakDays: number;
}

export interface MemberSelfServiceAnalytics {
  goalCompletionRate: number;
  workoutCompliance: number;
  dietCompliance: DietComplianceSummary;
  waterCompliance: number;
  referralConversion: number;
}

export interface ProgressTrend {
  entries: MemberProgressEntry[];
  waterHistory: WaterIntake[];
  workoutHistory: WorkoutTracking[];
  dietHistory: DietTracking[];
}

export const GoalTypes = {
  WeightLoss: 'WeightLoss',
  WeightGain: 'WeightGain',
  MuscleGain: 'MuscleGain',
  FatLoss: 'FatLoss',
} as const;

export const FeedbackTypes = {
  Trainer: 'Trainer',
  Gym: 'Gym',
} as const;
