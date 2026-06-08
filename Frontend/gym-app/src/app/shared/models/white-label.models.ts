export interface WhiteLabelSettings {
  id: number;
  gymId: string;
  brandName: string;
  logoFileId?: number | null;
  faviconFileId?: number | null;
  logoUrl?: string | null;
  faviconUrl?: string | null;
  primaryColor?: string | null;
  secondaryColor?: string | null;
  loginBackgroundFileId?: number | null;
  loginBackgroundUrl?: string | null;
  appDisplayName?: string | null;
  supportEmail?: string | null;
  supportPhone?: string | null;
  customDomain?: string | null;
  subDomain?: string | null;
  isWhiteLabelEnabled: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface UpsertWhiteLabelSettings {
  brandName: string;
  logoFileId?: number | null;
  faviconFileId?: number | null;
  primaryColor?: string | null;
  secondaryColor?: string | null;
  loginBackgroundFileId?: number | null;
  appDisplayName?: string | null;
  supportEmail?: string | null;
  supportPhone?: string | null;
  customDomain?: string | null;
  subDomain?: string | null;
  isWhiteLabelEnabled: boolean;
}

export interface UpdateWhiteLabelDomain {
  subDomain?: string | null;
  customDomain?: string | null;
}

export interface WhiteLabelEmailTemplate {
  id: number;
  gymId: string;
  templateName: string;
  subject: string;
  body: string;
  isActive: boolean;
}

export interface WhiteLabelMobileSettings {
  id: number;
  gymId: string;
  appName?: string | null;
  splashScreenFileId?: number | null;
  appIconFileId?: number | null;
  splashScreenUrl?: string | null;
  appIconUrl?: string | null;
  androidPackageName?: string | null;
  iosBundleId?: string | null;
}

export interface WhiteLabelLoginBranding {
  gymId: string;
  brandName: string;
  appDisplayName?: string | null;
  primaryColor?: string | null;
  secondaryColor?: string | null;
  supportEmail?: string | null;
  supportPhone?: string | null;
  logoUrl?: string | null;
  loginBackgroundUrl?: string | null;
}

export interface WhiteLabelPreview {
  login: WhiteLabelLoginBranding;
  website: {
    brandName: string;
    logoUrl?: string | null;
    primaryColor?: string | null;
    secondaryColor?: string | null;
    websiteTitle?: string | null;
  };
  mobile: WhiteLabelMobileSettings;
}

export interface WhiteLabelPlatformDashboard {
  totalWhiteLabelCustomers: number;
  subDomainCustomers: number;
  customDomainCustomers: number;
  whiteLabelMonthlyRevenue: number;
  expiringWhiteLabelPlans: number;
  premiumCustomers: WhiteLabelCustomerSummary[];
  adoptionTrend: WhiteLabelAdoptionPoint[];
}

export interface WhiteLabelCustomerSummary {
  brandName: string;
  subDomain?: string | null;
  customDomain?: string | null;
  subscriptionStatus?: string | null;
  currentPeriodEnd?: string | null;
}

export interface WhiteLabelAdoptionPoint {
  adoptionDate: string;
  enabledCount: number;
}
