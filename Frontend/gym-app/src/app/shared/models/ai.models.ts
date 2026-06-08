export interface AiRecommendation {
  id: number;
  gymId: string;
  memberId: number;
  memberName: string;
  recommendationType: string;
  recommendationText: string;
  confidenceScore: number;
  isAccepted: boolean;
  acceptedDate?: string;
  generatedDate: string;
}

export interface AiInsight {
  id: number;
  gymId: string;
  insightType: string;
  insightText: string;
  severity: string;
  generatedDate: string;
}

export interface MemberRiskScore {
  id: number;
  gymId: string;
  memberId: number;
  memberName: string;
  churnRisk: string;
  attendanceRisk: string;
  renewalProbability: number;
  healthScore: number;
  lastCalculatedDate: string;
}

export interface LeadScore {
  leadId: number;
  gymId: string;
  fullName: string;
  mobileNumber?: string;
  email?: string;
  status: string;
  leadSource?: string;
  createdDate: string;
  followUpCount: number;
  completedTrials: number;
  scoreCategory: string;
  engagementScore: number;
}

export interface AiChartPoint {
  label: string;
  count: number;
}

export interface AiDashboard {
  highRiskMembers: number;
  predictedRenewals: number;
  hotLeads: number;
  recentRecommendations: number;
  actionableInsights: number;
  churnRiskDistribution: AiChartPoint[];
  renewalProbabilityDistribution: AiChartPoint[];
  topRecommendations: AiRecommendation[];
  highRiskMemberList: MemberRiskScore[];
}

export interface AiAnalytics {
  totalRecommendations: number;
  acceptedRecommendations: number;
  acceptanceRate: number;
  highChurnPredictions: number;
  recentHighChurnPredictions: number;
  totalTokensUsed: number;
  totalGenerations: number;
  totalInsights: number;
}
