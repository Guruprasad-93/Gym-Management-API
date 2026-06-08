export interface ClassSchedule {
  id: number;
  gymId: string;
  branchId: number;
  branchName: string;
  className: string;
  description?: string;
  trainerId: number;
  trainerName: string;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  capacity: number;
  status: string;
  createdDate: string;
}

export interface AvailableSlot {
  classScheduleId: number;
  gymId: string;
  branchId: number;
  branchName: string;
  className: string;
  description?: string;
  trainerId: number;
  trainerName: string;
  bookingDate: string;
  startTime: string;
  endTime: string;
  capacity: number;
  remainingCapacity: number;
  waitlistCount: number;
}

export interface SlotBooking {
  id: number;
  gymId: string;
  branchId: number;
  branchName: string;
  memberId: number;
  memberName: string;
  classScheduleId: number;
  className: string;
  startTime: string;
  endTime: string;
  bookingDate: string;
  status: string;
  checkInTime?: string;
  createdDate: string;
  trainerName: string;
}

export interface BookingAnalytics {
  totalBookings: number;
  todaysBookings: number;
  occupancyPercent: number;
  noShowPercent: number;
  cancellationPercent: number;
  bookingTrend: { label: string; bookingCount: number }[];
  popularClasses: { label: string; bookingCount: number }[];
  peakHours: { label: string; bookingCount: number }[];
  branchComparison: { label: string; bookingCount: number }[];
}

export interface TrainerScheduleItem {
  classScheduleId: number;
  className: string;
  branchId: number;
  branchName: string;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  capacity: number;
  status: string;
  bookingCount: number;
}

export const DAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
