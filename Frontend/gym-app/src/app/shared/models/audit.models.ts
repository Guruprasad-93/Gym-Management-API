export interface AuditLog {
  auditLogId: number;
  gymId?: string;
  gymName?: string;
  userId?: string;
  userName?: string;
  userEmail?: string;
  entityName: string;
  entityId: string;
  actionType: string;
  oldValueJson?: string;
  newValueJson?: string;
  ipAddress?: string;
  createdDate: string;
}

export interface AuditSearchQuery {
  userId?: string;
  entityName?: string;
  actionType?: string;
  entityId?: string;
  search?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface AuditDashboard {
  totalLogs: number;
  byEntity: AuditCountByKey[];
  byAction: AuditCountByKey[];
}

export interface AuditCountByKey {
  key: string;
  count: number;
}

export const AUDIT_ENTITIES = [
  'Member',
  'Trainer',
  'Membership',
  'MembershipPlan',
  'Payment',
  'MemberAttendance',
  'TrainerAttendance',
  'Auth',
] as const;

export const AUDIT_ACTIONS = [
  'Create',
  'Update',
  'Delete',
  'Login',
  'Logout',
  'CheckIn',
  'CheckOut',
  'Mark',
  'Renew',
  'Cancel',
] as const;
