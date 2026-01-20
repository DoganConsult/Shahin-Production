/**
 * Health & System API Client
 * API methods for health checks and system status
 */

import { api } from './client'

// ==================== Types ====================

export interface HealthCheckResponse {
  status: 'healthy' | 'degraded' | 'unhealthy'
  timestamp: string
  version?: string
}

export interface SystemStatusResponse {
  status: string
  timestamp: string
  version: string
  environment: string
  uptime: number
  services: {
    database: boolean
    cache: boolean
    messageQueue: boolean
    storage: boolean
  }
}

export interface PingResponse {
  pong: boolean
  timestamp: string
  latency?: number
}

// ==================== API Methods ====================

export const healthApi = {
  /**
   * Basic health check
   */
  check: () =>
    api.get<HealthCheckResponse>('/api/health'),

  /**
   * Detailed system status
   */
  systemStatus: () =>
    api.get<SystemStatusResponse>('/api/system/status'),

  /**
   * Simple ping test
   */
  ping: () =>
    api.get<PingResponse>('/api/diagnostics/ping'),

  /**
   * Check API connectivity
   */
  isApiAvailable: async (): Promise<boolean> => {
    try {
      const response = await healthApi.ping()
      return response.pong === true
    } catch {
      return false
    }
  }
}

export default healthApi
