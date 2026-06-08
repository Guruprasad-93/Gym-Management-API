export interface Member {
  id: number;
  gymId: string;
  userId: string;
  fullName: string;
  email: string;
  trainerId?: number;
  trainerName?: string;
  dateOfBirth?: string;
  age?: number;
  gender?: string;
  height?: number;
  weight?: number;
  phone?: string;
  address?: string;
  emergencyContact?: string;
  joinDate: string;
  isActive: boolean;
  isDeleted: boolean;
  membershipStatus?: string;
  membershipPlanName?: string;
  membershipEndDate?: string;
  createdDate: string;
  updatedDate?: string;
}

export interface MemberDetails extends Member {
  paymentHistory: MemberPaymentHistory[];
  progress: MemberProgress[];
}

export interface MemberPaymentHistory {
  id: number;
  membershipId?: number;
  amount: number;
  paymentDate: string;
  paymentMethod?: string;
  transactionReference?: string;
  status: string;
  notes?: string;
}

export interface MemberProgress {
  progressType: string;
  recordedDate: string;
  detail?: string;
  weightKg?: number;
  createdAt: string;
}

export interface CreateMemberRequest {
  name: string;
  email: string;
  password: string;
  trainerId?: number;
  dateOfBirth?: string;
  gender?: string;
  height?: number;
  weight?: number;
  phone?: string;
  address?: string;
  emergencyContact?: string;
  joinDate: string;
}

export interface UpdateMemberRequest {
  fullName?: string;
  email?: string;
  trainerId?: number;
  dateOfBirth?: string;
  gender?: string;
  height?: number;
  weight?: number;
  phone?: string;
  address?: string;
  emergencyContact?: string;
  isActive: boolean;
}

export interface AssignTrainerRequest {
  trainerId: number;
}
