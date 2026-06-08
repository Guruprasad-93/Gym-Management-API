export interface NotificationTemplate {
  id: number;
  gymId: string;
  notificationType: string;
  templateName: string;
  bodyTemplate?: string;
  variablesJson?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateNotificationTemplateRequest {
  notificationType: string;
  templateName: string;
  bodyTemplate?: string;
  variablesJson?: string;
  isActive?: boolean;
}

export interface UpdateNotificationTemplateRequest extends CreateNotificationTemplateRequest {}

export interface NotificationSetting {
  id: number;
  gymId: string;
  notificationType: string;
  isEnabled: boolean;
  providerTemplateName?: string;
}

export interface UpdateNotificationSettingRequest {
  notificationType: string;
  isEnabled: boolean;
  providerTemplateName?: string;
}

export interface NotificationLog {
  id: number;
  gymId: string;
  notificationType: string;
  recipientPhone: string;
  recipientName?: string;
  memberId?: number;
  whatsAppTemplateName: string;
  variablesJson?: string;
  status: string;
  errorMessage?: string;
  providerMessageId?: string;
  sentAt?: string;
  createdAt: string;
}

export interface NotificationDashboard {
  totalLogs: number;
  sentCount: number;
  failedCount: number;
  pendingCount: number;
  activeTemplates: number;
  sentToday: number;
}

export interface NotificationSearchQuery {
  search?: string;
  notificationType?: string;
  status?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface SendTestNotificationRequest {
  phoneNumber: string;
  notificationType: string;
  templateName?: string;
  variables?: Record<string, string>;
}

export const NOTIFICATION_TYPES = [
  'MembershipExpiry7Days',
  'MembershipExpiry3Days',
  'MembershipExpiryToday',
  'PaymentSuccess',
  'MembershipRenewal',
  'NewMemberRegistration',
  'WorkoutPlanAssigned',
  'DietPlanAssigned',
] as const;

export const NOTIFICATION_STATUSES = ['Pending', 'Sent', 'Failed'] as const;
