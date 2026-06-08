export interface MembershipPlan {
  id: number;
  gymId: string;
  planName: string;
  description?: string;
  durationInMonths: number;
  price: number;
  isActive: boolean;
}

export interface Membership {
  id: number;
  gymId: string;
  memberId: number;
  memberName: string;
  memberEmail: string;
  membershipPlanId: number;
  planName: string;
  planPrice: number;
  durationInMonths: number;
  startDate: string;
  endDate: string;
  amount?: number;
  status: string;
  notes?: string;
}

export interface CreateMembershipPlanRequest {
  planName: string;
  durationInMonths: number;
  price: number;
  description?: string;
}

export interface UpdateMembershipPlanRequest {
  planName: string;
  durationInMonths: number;
  price: number;
  description?: string;
  isActive: boolean;
}

export interface CreateMembershipRequest {
  memberId: number;
  membershipPlanId: number;
  startDate: string;
  notes?: string;
}

export interface RenewMembershipRequest {
  notes?: string;
}

export interface Payment {
  id: number;
  gymId: string;
  memberId?: number;
  memberName?: string;
  memberEmail?: string;
  membershipId?: number;
  membershipPlanName?: string;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  transactionReference?: string;
  razorpayOrderId?: string;
  razorpayPaymentId?: string;
  status: string;
  notes?: string;
}

export interface CreatePaymentRequest {
  memberId: number;
  membershipId: number;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  transactionReference?: string;
  notes?: string;
}

export interface Invoice {
  id: number;
  invoiceNumber: string;
  amount: number;
  issuedAt: string;
  gymName: string;
  memberName: string;
  memberEmail: string;
  paymentMethod: string;
  membershipPlanName?: string;
}

export interface RevenueDashboard {
  totalRevenue: number;
  monthlyRevenue: number;
  expiredMemberships: number;
  activeMemberships: number;
  pendingRenewals: number;
}

export interface MonthlyRevenue {
  year: number;
  month: number;
  monthLabel: string;
  revenue: number;
}

export interface CreateRazorpayOrderRequest {
  membershipId: number;
  memberId?: number;
  renewOnSuccess?: boolean;
  notes?: string;
}

export interface RazorpayOrder {
  paymentId: number;
  razorpayOrderId: string;
  keyId: string;
  amount: number;
  currency: string;
  amountInPaise: number;
  memberName?: string;
  memberEmail?: string;
  planName?: string;
}

export interface VerifyRazorpayPaymentRequest {
  razorpayOrderId: string;
  razorpayPaymentId: string;
  razorpaySignature: string;
}

export interface RazorpayCheckoutContext {
  memberId: number;
  membershipId: number;
  amount: number;
  planName: string;
  status: string;
  endDate?: string;
  isExpired: boolean;
}

export interface RefundPaymentRequest {
  amount?: number;
  reason?: string;
}

export interface RefundPaymentResult {
  paymentId: number;
  status: string;
  refundReference?: string;
}
