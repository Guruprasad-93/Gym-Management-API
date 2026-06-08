export interface GymWebsiteSettings {
  gymId: string;
  websiteSlug: string;
  websiteTitle?: string;
  websiteDescription?: string;
  logoFileId?: number;
  bannerFileId?: number;
  logoUrl?: string;
  bannerUrl?: string;
  primaryColor?: string;
  secondaryColor?: string;
  contactPhone?: string;
  contactEmail?: string;
  whatsAppNumber?: string;
  address?: string;
  googleMapEmbedUrl?: string;
  facebookUrl?: string;
  instagramUrl?: string;
  youtubeUrl?: string;
  metaTitle?: string;
  metaDescription?: string;
  metaKeywords?: string;
  isPublished: boolean;
  publishedDate?: string;
}

export interface GymWebsitePage {
  id: number;
  pageName: string;
  slug: string;
  pageContent?: string;
  displayOrder: number;
  isActive: boolean;
}

export interface GymWebsiteSection {
  id: number;
  sectionType: string;
  title?: string;
  subtitle?: string;
  description?: string;
  imageFileId?: number;
  imageUrl?: string;
  displayOrder: number;
  isVisible: boolean;
}

export interface GymWebsiteTestimonial {
  id: number;
  memberName: string;
  rating: number;
  reviewText?: string;
  imageFileId?: number;
  imageUrl?: string;
  isApproved: boolean;
}

export interface GymWebsiteGalleryItem {
  id: number;
  fileId: number;
  caption?: string;
  displayOrder: number;
  publicUrl?: string;
  originalFileName?: string;
}

export interface WebsiteLeadCapture {
  id: number;
  name: string;
  mobileNumber: string;
  email?: string;
  source: string;
  interestedPlan?: string;
  notes?: string;
  status: string;
  createdDate: string;
}

export interface WebsiteAnalyticsOverview {
  totalWebsiteLeads: number;
  trialRequests: number;
  convertedLeads: number;
  leadConversionRate: number;
  leadsInPeriod: number;
  dailyLeads: { leadDate: string; leadCount: number }[];
  topSources: { name: string; count: number }[];
}

export interface PublicWebsite {
  settings: GymWebsiteSettings;
  gymName: string;
  sections: GymWebsiteSection[];
  pages: GymWebsitePage[];
  testimonials: GymWebsiteTestimonial[];
  gallery: GymWebsiteGalleryItem[];
  membershipPlans: { id: number; planName: string; description?: string; durationInMonths: number; price: number }[];
  trainers: { id: number; fullName: string; specialization?: string; bio?: string; profileImageUrl?: string }[];
}

export interface PublicWebsiteLead {
  websiteSlug: string;
  name: string;
  mobileNumber: string;
  email?: string;
  interestedPlan?: string;
  notes?: string;
}

export interface PublicTrialBooking {
  websiteSlug: string;
  name: string;
  mobileNumber: string;
  email?: string;
  preferredDate: string;
  preferredTime: string;
}
