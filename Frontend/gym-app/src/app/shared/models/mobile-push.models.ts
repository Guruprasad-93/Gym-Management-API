export interface PushNotificationAnalytics {
  totalSent: number;
  totalDelivered: number;
  totalFailed: number;
  totalOpened: number;
  totalClicked: number;
  totalPending: number;
  activeDevices: number;
  byType: PushNotificationTypeStat[];
}

export interface PushNotificationTypeStat {
  notificationType: string;
  count: number;
  failedCount: number;
}

export interface PushCampaignHistory {
  notificationType: string;
  title: string;
  message: string;
  sentDate: string;
  recipientCount: number;
  failedCount: number;
  sentCount: number;
}

export interface SendPushCampaign {
  title: string;
  message: string;
  branchId?: number;
}

export const PushTemplates = [
  { title: 'Membership Reminder', message: 'Your membership is expiring soon. Renew today to stay active!' },
  { title: 'New Workout Plan', message: 'Your trainer assigned a new workout plan. Open the app to view it.' },
  { title: 'Special Offer', message: 'Limited time offer — visit the front desk or renew in the app.' },
];
