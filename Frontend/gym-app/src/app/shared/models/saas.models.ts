export interface SaasPlan {
  id: number;
  planCode: string;
  planName: string;
  maxMembers: number;
  maxTrainers: number;
  storageLimitMb: number;
  whatsAppNotificationLimit: number;
  monthlyPrice: number;
  yearlyPrice: number;
  trialDays: number;
}

export interface GymSubscription {
  id: number;
  gymId: string;
  saasPlanId: number;
  planCode: string;
  planName: string;
  status: string;
  billingCycle?: string;
  amount: number;
  startDate: string;
  endDate: string;
  trialEndsAt?: string;
  currentPeriodEnd?: string;
  graceEndsAt?: string;
  remainingTrialDays?: number;
  hasAccess: boolean;
  cancelAtPeriodEnd: boolean;
  maxMembers: number;
  maxTrainers: number;
  storageLimitMb: number;
  whatsAppNotificationLimit: number;
  monthlyPrice: number;
  yearlyPrice: number;
}

export interface GymUsage {
  memberCount: number;
  trainerCount: number;
  storageUsedBytes: number;
  whatsAppSentThisMonth: number;
}

export interface SaasPaymentOrder {
  saasPaymentId: number;
  razorpayOrderId: string;
  amount: number;
  currency: string;
  keyId: string;
  planName: string;
  billingCycle: string;
}

export interface SaasPlatformDashboard {
  totalGyms: number;
  activeGyms: number;
  activeSubscriptions: number;
  expiredSubscriptions: number;
  trialSubscriptions: number;
  monthlyRecurringRevenue: number;
  annualRecurringRevenue: number;
}

export interface GymBranding {
  gymId: string;
  logoUrl?: string;
  primaryColor?: string;
  secondaryColor?: string;
  bannerFileId?: number;
  bannerUrl?: string;
  receiptHeaderText?: string;
  invoiceFooterText?: string;
}

export interface UpdateGymBranding {
  primaryColor?: string;
  secondaryColor?: string;
  bannerFileId?: number;
  receiptHeaderText?: string;
  invoiceFooterText?: string;
}
