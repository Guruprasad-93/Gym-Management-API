export interface Branch {
  branchId: number;
  gymId: string;
  branchName: string;
  branchCode?: string;
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  isActive: boolean;
  managerUserId?: string;
  managerName?: string;
  memberCount: number;
  trainerCount: number;
}

export interface CreateBranchDto {
  branchName: string;
  branchCode?: string;
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  managerUserId?: string;
}

export interface BranchDashboardItem {
  branchId: number;
  branchName: string;
  memberCount: number;
  trainerCount: number;
  revenueMonth: number;
  attendanceMonth: number;
  leadsOpen: number;
  expensesMonth: number;
  profitMonth: number;
}

export interface BranchTransfer {
  transferId: number;
  entityType: string;
  entityId: number;
  entityName?: string;
  fromBranchName?: string;
  toBranchName?: string;
  transferDate: string;
  notes?: string;
}

export interface TransferMemberDto {
  memberId: number;
  toBranchId: number;
  notes?: string;
}

export interface TransferTrainerDto {
  trainerId: number;
  toBranchId: number;
  notes?: string;
}

export interface BranchTarget {
  targetId: number;
  branchId: number;
  branchName: string;
  targetMonth: string;
  revenueTarget: number;
  newMembersTarget: number;
  leadConversionsTarget: number;
  actualRevenue: number;
  actualNewMembers: number;
  actualLeadConversions: number;
  revenueAchievementPercent: number;
  membersAchievementPercent: number;
  leadsAchievementPercent: number;
}

export interface UpsertBranchTargetDto {
  branchId: number;
  targetMonth: string;
  revenueTarget: number;
  newMembersTarget: number;
  leadConversionsTarget: number;
}

export interface BranchAnnouncement {
  announcementId: number;
  branchId?: number;
  branchName?: string;
  title: string;
  message: string;
  targetAudience: string;
  publishDate: string;
}

export interface CreateBranchAnnouncementDto {
  branchId?: number;
  title: string;
  message: string;
  targetAudience: string;
  sendWhatsApp?: boolean;
}

export interface BranchAnalyticsRanking {
  branchId: number;
  branchName: string;
  totalRevenue: number;
  totalExpenses: number;
  profit: number;
  memberCount: number;
  attendanceCount: number;
  leadConversions: number;
}

export interface BranchMonthlyRevenue {
  branchName: string;
  year: number;
  month: number;
  revenue: number;
}

export interface BranchAnalytics {
  rankings: BranchAnalyticsRanking[];
  monthlyRevenue: BranchMonthlyRevenue[];
}
