/**
 * Trial React Query Hooks
 * Custom hooks for trial lifecycle, team engagement, and ecosystem features
 * Integrated with ApiProvider for centralized auth handling
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useApi } from '@/components/providers/api-provider'
import type {
  TrialSignupRequest,
  TrialStatus,
  TrialUsage,
  TeamInviteRequest,
  TeamInviteResult,
  TeamOnboardingProgress,
  TeamEngagementDashboard,
  BulkInviteResult,
  TeamMember,
  EcosystemPartnerRequest,
  EcosystemConnectionResult,
  EcosystemPartner,
  EcosystemConnection,
  Integration,
  IntegrationResult,
} from '../api/trial'

// Re-export types
export type {
  TrialSignupRequest,
  TrialStatus,
  TrialUsage,
  TeamInviteRequest,
  TeamInviteResult,
  TeamOnboardingProgress,
  TeamEngagementDashboard,
  BulkInviteResult,
  TeamMember,
  EcosystemPartnerRequest,
  EcosystemConnectionResult,
  EcosystemPartner,
  EcosystemConnection,
  Integration,
  IntegrationResult,
}

// Query Keys
export const trialKeys = {
  all: ['trial'] as const,
  status: () => [...trialKeys.all, 'status'] as const,
  usage: () => [...trialKeys.all, 'usage'] as const,
  team: () => [...trialKeys.all, 'team'] as const,
  teamMembers: () => [...trialKeys.team(), 'members'] as const,
  teamOnboarding: () => [...trialKeys.team(), 'onboarding'] as const,
  teamEngagement: () => [...trialKeys.team(), 'engagement'] as const,
  ecosystem: () => [...trialKeys.all, 'ecosystem'] as const,
  partners: (sector?: string) => [...trialKeys.ecosystem(), 'partners', sector] as const,
  connections: () => [...trialKeys.ecosystem(), 'connections'] as const,
  integrations: (category?: string) => [...trialKeys.all, 'integrations', category] as const,
}

// ==================== Trial Status Hooks ====================

export function useTrialStatus() {
  const api = useApi()
  return useQuery({
    queryKey: trialKeys.status(),
    queryFn: () => api.get<TrialStatus>('/trial/status'),
  })
}

export function useTrialUsage() {
  const api = useApi()
  return useQuery({
    queryKey: trialKeys.usage(),
    queryFn: () => api.get<TrialUsage>('/trial/usage'),
  })
}

export function useTrialSignup() {
  const api = useApi()
  return useMutation({
    mutationFn: (data: TrialSignupRequest) =>
      api.post<{ success: boolean; signupId?: string; message: string }>('/trial/signup', data),
  })
}

export function useTrialExtend() {
  const api = useApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (reason?: string) =>
      api.post<{ success: boolean; newEndDate: string; daysAdded: number; message: string }>(
        '/trial/extend',
        { reason }
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: trialKeys.status() })
    },
  })
}

export function useTrialConvert() {
  const api = useApi()
  return useMutation({
    mutationFn: (planCode: string) =>
      api.post<{ success: boolean; checkoutUrl: string; planCode: string; monthlyPrice: number }>(
        '/trial/convert',
        { planCode }
      ),
  })
}

// ==================== Team Collaboration Hooks ====================

export function useTeamMembers() {
  const api = useApi()
  return useQuery({
    queryKey: trialKeys.teamMembers(),
    queryFn: () => api.get<{ total: number; members: TeamMember[] }>('/trial/team'),
  })
}

export function useTeamOnboarding() {
  const api = useApi()
  return useQuery({
    queryKey: trialKeys.teamOnboarding(),
    queryFn: () => api.get<TeamOnboardingProgress>('/trial/team/onboarding'),
  })
}

export function useTeamEngagement() {
  const api = useApi()
  return useQuery({
    queryKey: trialKeys.teamEngagement(),
    queryFn: () => api.get<TeamEngagementDashboard>('/trial/team/engagement'),
  })
}

export function useInviteTeamMember() {
  const api = useApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: TeamInviteRequest) =>
      api.post<TeamInviteResult>('/trial/team/invite', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: trialKeys.teamMembers() })
    },
  })
}

export function useBulkInviteTeamMembers() {
  const api = useApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (requests: TeamInviteRequest[]) =>
      api.post<BulkInviteResult>('/trial/team/invite/bulk', requests),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: trialKeys.teamMembers() })
    },
  })
}

export function useTrackTeamActivity() {
  const api = useApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      userId,
      activityType,
      details,
    }: {
      userId: string
      activityType: string
      details: string
    }) => api.post<{ success: boolean }>('/trial/team/activity', { userId, activityType, details }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: trialKeys.teamEngagement() })
      queryClient.invalidateQueries({ queryKey: trialKeys.teamOnboarding() })
    },
  })
}

// ==================== Ecosystem Collaboration Hooks ====================

export function useEcosystemPartners(sector?: string) {
  const api = useApi()
  return useQuery({
    queryKey: trialKeys.partners(sector),
    queryFn: () =>
      api.get<{ total: number; partners: EcosystemPartner[] }>(
        `/trial/ecosystem/partners${sector ? `?sector=${sector}` : ''}`
      ),
  })
}

export function useEcosystemConnections() {
  const api = useApi()
  return useQuery({
    queryKey: trialKeys.connections(),
    queryFn: () =>
      api.get<{ total: number; connections: EcosystemConnection[] }>('/trial/ecosystem/connections'),
  })
}

export function useRequestPartnerConnection() {
  const api = useApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: EcosystemPartnerRequest) =>
      api.post<EcosystemConnectionResult>('/trial/ecosystem/connect', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: trialKeys.connections() })
    },
  })
}

export function useApprovePartnerConnection() {
  const api = useApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (connectionId: string) =>
      api.post<{ success: boolean; message: string }>(`/trial/ecosystem/approve/${connectionId}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: trialKeys.connections() })
    },
  })
}

export function useRejectPartnerConnection() {
  const api = useApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ connectionId, reason }: { connectionId: string; reason: string }) =>
      api.post<{ success: boolean; message: string }>(`/trial/ecosystem/reject/${connectionId}`, {
        reason,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: trialKeys.connections() })
    },
  })
}

export function useTrackPartnerInteraction() {
  const api = useApi()
  return useMutation({
    mutationFn: ({
      connectionId,
      interactionType,
      details,
    }: {
      connectionId: string
      interactionType: string
      details: string
    }) =>
      api.post<{ success: boolean }>(`/trial/ecosystem/track/${connectionId}`, {
        interactionType,
        details,
      }),
  })
}

// ==================== Integrations Hooks ====================

export function useAvailableIntegrations(category?: string) {
  const api = useApi()
  return useQuery({
    queryKey: trialKeys.integrations(category),
    queryFn: () =>
      api.get<{ total: number; integrations: Integration[] }>(
        `/trial/integrations${category ? `?category=${category}` : ''}`
      ),
  })
}

export function useConnectIntegration() {
  const api = useApi()
  return useMutation({
    mutationFn: ({
      integrationCode,
      config,
    }: {
      integrationCode: string
      config?: Record<string, string>
    }) => api.post<IntegrationResult>('/trial/integrations/connect', { integrationCode, config }),
  })
}
