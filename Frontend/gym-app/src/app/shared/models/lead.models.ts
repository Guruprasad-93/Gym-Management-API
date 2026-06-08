export interface Lead {
  id: number;
  gymId: string;
  fullName: string;
  mobileNumber: string;
  email?: string;
  gender?: string;
  age?: number;
  address?: string;
  leadSource: string;
  interestedPlanId?: number;
  interestedPlanName?: string;
  status: string;
  assignedTrainerId?: number;
  assignedTrainerName?: string;
  notes?: string;
  convertedMemberId?: number;
  createdDate: string;
  createdBy?: string;
  updatedDate?: string;
}

export interface LeadActivity {
  id: number;
  leadId: number;
  activityType: string;
  description: string;
  createdDate: string;
  createdBy?: string;
}

export interface LeadFollowUp {
  id: number;
  leadId: number;
  followUpDate: string;
  followUpType: string;
  remarks?: string;
  status: string;
  nextFollowUpDate?: string;
  createdDate: string;
  leadName?: string;
  mobileNumber?: string;
}

export interface LeadTrial {
  id: number;
  leadId: number;
  trainerId?: number;
  trainerName?: string;
  trialDate: string;
  attendanceStatus: string;
  feedback?: string;
  rating?: number;
  leadName?: string;
  mobileNumber?: string;
}

export interface LeadDetail {
  lead: Lead;
  activities: LeadActivity[];
  followUps: LeadFollowUp[];
  trials: LeadTrial[];
}

export interface LeadDashboard {
  totalLeads: number;
  newLeadsToday: number;
  conversionRate: number;
  trialConversionRate: number;
  lostLeads: number;
  pendingFollowUps: number;
  todaysTrials: number;
  convertedLeads: number;
}

export interface NamedCount {
  name: string;
  count: number;
}

export interface LeadConversionPoint {
  year: number;
  month: number;
  monthLabel: string;
  conversions: number;
  newLeads: number;
}

export interface TrainerLeadPerformance {
  trainerId?: number;
  trainerName: string;
  totalLeads: number;
  convertedLeads: number;
}

export interface LeadAnalytics {
  dashboard: LeadDashboard;
  leadsBySource: NamedCount[];
  leadsByStatus: NamedCount[];
  monthlyConversions: LeadConversionPoint[];
  trainerPerformance: TrainerLeadPerformance[];
  pendingFollowUps: LeadFollowUp[];
  todaysTrials: LeadTrial[];
}

export interface LeadSearchQuery {
  gymId?: string;
  search?: string;
  status?: string;
  leadSource?: string;
  pageNumber?: number;
  pageSize?: number;
  sortColumn?: string;
  sortDirection?: string;
}

export interface CreateLeadRequest {
  gymId?: string;
  fullName: string;
  mobileNumber: string;
  email?: string;
  gender?: string;
  age?: number;
  address?: string;
  leadSource: string;
  interestedPlanId?: number;
  status?: string;
  assignedTrainerId?: number;
  notes?: string;
}

export interface UpdateLeadRequest extends CreateLeadRequest {
  status: string;
}

export interface AssignTrainerToLeadRequest {
  leadId: number;
  gymId?: string;
  trainerId: number;
}

export interface ScheduleTrialRequest {
  leadId: number;
  gymId?: string;
  trainerId?: number;
  trialDate: string;
}

export interface CreateFollowUpRequest {
  leadId: number;
  gymId?: string;
  followUpDate: string;
  followUpType: string;
  remarks?: string;
  nextFollowUpDate?: string;
}

export interface RecordTrialFeedbackRequest {
  trialId: number;
  leadId: number;
  gymId?: string;
  attendanceStatus: string;
  feedback?: string;
  rating?: number;
}

export interface ConvertLeadRequest {
  leadId: number;
  gymId?: string;
  membershipPlanId: number;
  startDate: string;
  email?: string;
  password?: string;
}

export interface ConvertLeadResult {
  lead: Lead;
  memberId: number;
  membershipId: number;
  temporaryPassword?: string;
}

export const LEAD_STATUSES = [
  'New',
  'Contacted',
  'TrialScheduled',
  'TrialCompleted',
  'FollowUpPending',
  'Converted',
  'Lost',
] as const;

export const KANBAN_STATUSES = [
  'New',
  'Contacted',
  'TrialScheduled',
  'FollowUpPending',
  'Converted',
  'Lost',
] as const;

export const LEAD_SOURCES = [
  'WalkIn',
  'Referral',
  'Facebook',
  'Instagram',
  'Google',
  'Website',
  'WhatsApp',
  'Other',
] as const;

export const LEAD_STATUS_LABELS: Record<string, string> = {
  New: 'New',
  Contacted: 'Contacted',
  TrialScheduled: 'Trial Scheduled',
  TrialCompleted: 'Trial Completed',
  FollowUpPending: 'Follow Up',
  Converted: 'Converted',
  Lost: 'Lost',
};
