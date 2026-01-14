/**
 * Trial API Client
 * API methods for trial lifecycle, team engagement, and ecosystem features
 */

import { api } from './client'

// ==================== Types ====================

export interface TrialSignupRequest {
  email: string
  firstName: string
  lastName: string
  companyName: string
  phone?: string
  sector?: string
  source?: string
  referralCode?: string
}

export interface TrialSignupResponse {
  success: boolean
  signupId?: string
  message: string
}

export interface TrialStatus {
  tenantId: string
  status: string
  startedAt: string
  endsAt: string
  daysRemaining: number
  canExtend: boolean
  hasExtended: boolean
  extensionDaysUsed: number
  currentPlan: string
  availableFeatures: string[]
  lockedFeatures: string[]
}

export interface TrialUsage {
  tenantId: string
  totalLogins: number
  uniqueUsers: number
  featuresUsed: number
  documentsUploaded: number
  controlsReviewed: number
  reportsGenerated: number
  engagementScore: number
  lastActiveAt: string
  featureUsage: Record<string, number>
  recentActivity: ActivitySummary[]
}

export interface ActivitySummary {
  date: string
  type: string
  description: string
  userId: string
}

// Team Types
export interface TeamMember {
  userId: string
  email: string
  fullName: string
  role: string
  status: string
  joinedAt: string
  lastActiveAt?: string
  actionsCompleted: number
  contributionScore: number
}

export interface TeamInviteRequest {
  email: string
  firstName?: string
  lastName?: string
  roleCode?: string
  welcomeMessage?: string
}

export interface TeamInviteResult {
  success: boolean
  inviteId?: string
  inviteLink?: string
  message: string
}

export interface BulkInviteResult {
  totalRequested: number
  successful: number
  failed: number
  results: InviteStatus[]
}

export interface InviteStatus {
  email: string
  success: boolean
  message: string
  inviteId?: string
}

export interface OnboardingStep {
  stepId: string
  title: string
  description: string
  isCompleted: boolean
  completedAt?: string
  completedBy?: string
}

export interface TeamOnboardingProgress {
  tenantId: string
  completedSteps: number
  totalSteps: number
  progressPercent: number
  steps: OnboardingStep[]
}

export interface MemberContribution {
  userId: string
  name: string
  role: string
  actionsCompleted: number
  contributionScore: number
  rank: number
}

export interface TeamEngagementDashboard {
  tenantId: string
  overallScore: number
  activeMembers: number
  totalActions: number
  leaderboard: MemberContribution[]
  activityByDay: Record<string, number>
  activityByType: Record<string, number>
}

// Ecosystem Types
export interface EcosystemPartner {
  partnerId: string
  name: string
  type: string
  sector: string
  description: string
  services: string[]
  certifications: string[]
  rating: number
  connections: number
}

export interface EcosystemPartnerRequest {
  partnerId?: string
  partnerType: string
  partnerEmail?: string
  partnerName?: string
  connectionPurpose: string
  sharedDataTypes?: string[]
  connectionExpiry?: string
}

export interface EcosystemConnection {
  connectionId: string
  partnerId: string
  partnerName: string
  partnerType: string
  status: string
  purpose: string
  connectedAt: string
  expiresAt?: string
  sharedDataTypes: string[]
  interactionsCount: number
}

export interface EcosystemConnectionResult {
  success: boolean
  connectionId?: string
  status?: string
  message: string
}

// Integration Types
export interface Integration {
  integrationCode: string
  name: string
  category: string
  description: string
  logoUrl: string
  isAvailableInTrial: boolean
  requiredScopes: string[]
}

export interface IntegrationResult {
  success: boolean
  integrationCode: string
  status: string
  message: string
  connectionDetails?: Record<string, string>
}

// ==================== API Methods ====================

export const trialApi = {
  // Signup & Activation
  signup: (data: TrialSignupRequest) =>
    api.post<TrialSignupResponse>('/api/trial/signup', data),

  provision: (signupId: string) =>
    api.post<{ success: boolean; tenantId: string; tenantSlug: string; loginUrl: string }>(
      '/api/trial/provision',
      { signupId }
    ),

  activate: (tenantId: string, token: string) =>
    api.post<{ success: boolean; message: string }>('/api/trial/activate', { tenantId, token }),

  // Status & Usage
  getStatus: () => api.get<TrialStatus>('/api/trial/status'),

  getUsage: () => api.get<TrialUsage>('/api/trial/usage'),

  // Extension & Conversion
  extend: (reason?: string) =>
    api.post<{ success: boolean; newEndDate: string; daysAdded: number; message: string }>(
      '/api/trial/extend',
      { reason }
    ),

  convert: (planCode: string) =>
    api.post<{ success: boolean; checkoutUrl: string; planCode: string; monthlyPrice: number }>(
      '/api/trial/convert',
      { planCode }
    ),

  // Team Collaboration
  team: {
    getMembers: () => api.get<{ total: number; members: TeamMember[] }>('/api/trial/team'),

    invite: (data: TeamInviteRequest) =>
      api.post<TeamInviteResult>('/api/trial/team/invite', data),

    inviteBulk: (requests: TeamInviteRequest[]) =>
      api.post<BulkInviteResult>('/api/trial/team/invite/bulk', requests),

    getOnboarding: () => api.get<TeamOnboardingProgress>('/api/trial/team/onboarding'),

    getEngagement: () => api.get<TeamEngagementDashboard>('/api/trial/team/engagement'),

    trackActivity: (userId: string, activityType: string, details: string) =>
      api.post<{ success: boolean }>('/api/trial/team/activity', {
        userId,
        activityType,
        details,
      }),
  },

  // Ecosystem Collaboration
  ecosystem: {
    getPartners: (sector?: string) =>
      api.get<{ total: number; partners: EcosystemPartner[] }>(
        `/api/trial/ecosystem/partners${sector ? `?sector=${sector}` : ''}`
      ),

    requestConnection: (data: EcosystemPartnerRequest) =>
      api.post<EcosystemConnectionResult>('/api/trial/ecosystem/connect', data),

    getConnections: () =>
      api.get<{ total: number; connections: EcosystemConnection[] }>(
        '/api/trial/ecosystem/connections'
      ),

    approveConnection: (connectionId: string) =>
      api.post<{ success: boolean; message: string }>(
        `/api/trial/ecosystem/approve/${connectionId}`
      ),

    rejectConnection: (connectionId: string, reason: string) =>
      api.post<{ success: boolean; message: string }>(
        `/api/trial/ecosystem/reject/${connectionId}`,
        { reason }
      ),

    trackInteraction: (connectionId: string, interactionType: string, details: string) =>
      api.post<{ success: boolean }>(`/api/trial/ecosystem/track/${connectionId}`, {
        interactionType,
        details,
      }),
  },

  // Integrations Marketplace
  integrations: {
    getAvailable: (category?: string) =>
      api.get<{ total: number; integrations: Integration[] }>(
        `/api/trial/integrations${category ? `?category=${category}` : ''}`
      ),

    connect: (integrationCode: string, config?: Record<string, string>) =>
      api.post<IntegrationResult>('/api/trial/integrations/connect', {
        integrationCode,
        config,
      }),
  },
}

export default trialApi
