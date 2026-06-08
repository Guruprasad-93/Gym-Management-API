export interface AttendanceStatus {
  attendanceStatusId: number;
  code: string;
  name: string;
  description?: string;
}

export interface MemberAttendance {
  memberAttendanceId: number;
  gymId: string;
  memberId: number;
  memberName: string;
  memberEmail?: string;
  trainerId?: number;
  trainerName?: string;
  attendanceStatusId: number;
  statusCode: string;
  statusName: string;
  attendanceDate: string;
  checkInAt?: string;
  checkOutAt?: string;
  notes?: string;
  createdAt: string;
}

export interface TrainerAttendance {
  trainerAttendanceId: number;
  gymId: string;
  trainerId: number;
  trainerName: string;
  attendanceStatusId: number;
  statusName: string;
  attendanceDate: string;
  checkInAt?: string;
  checkOutAt?: string;
  notes?: string;
  createdAt: string;
}

export interface AttendanceDashboard {
  totalActiveMembers: number;
  membersPresentToday: number;
  currentlyCheckedIn: number;
  absentToday: number;
}

export interface AttendanceQuery {
  fromDate?: string;
  toDate?: string;
  memberId?: number;
  statusId?: number;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface DailyAttendanceReport {
  reportDate: string;
  statusCounts: { attendanceStatusId: number; statusCode: string; statusName: string; recordCount: number; reportDate: string }[];
  details: { memberAttendanceId: number; memberId: number; memberName: string; statusName: string; checkInAt?: string; checkOutAt?: string }[];
}

export interface MonthlyAttendanceReport {
  year: number;
  month: number;
  members: { memberId: number; memberName: string; presentDays: number; absentDays: number; lateDays: number; excusedDays: number; totalRecords: number }[];
}
