/**
 * Roles API Client
 * API methods for fetching GRC roles from the database
 */

import { api } from './client'

// ==================== Types ====================

export interface GrcRole {
  id: string
  code: string
  name: string
  nameAr: string
  description?: string
  descriptionAr?: string
  permissions: string[]
  isDefault?: boolean
  sortOrder: number
}

export interface RolesResponse {
  items: GrcRole[]
  totalCount: number
}

// ==================== API Methods ====================

export const rolesApi = {
  /**
   * Get all available GRC roles
   */
  getAll: () =>
    api.get<RolesResponse>('/api/app/roles'),

  /**
   * Get roles for registration/signup (public roles only)
   */
  getPublicRoles: () =>
    api.get<RolesResponse>('/api/app/roles/public'),

  /**
   * Get a specific role by ID
   */
  getById: (id: string) =>
    api.get<GrcRole>(`/api/app/roles/${id}`),

  /**
   * Get role by code
   */
  getByCode: (code: string) =>
    api.get<GrcRole>(`/api/app/roles/by-code/${code}`),
}

// Predefined GRC role codes for type safety
export const GRC_ROLE_CODES = {
  ADMIN: 'grc_admin',
  COMPLIANCE_OFFICER: 'compliance_officer',
  RISK_MANAGER: 'risk_manager',
  AUDITOR: 'auditor',
  IT_SECURITY: 'it_security',
  DATA_PROTECTION: 'data_protection',
  BUSINESS_OWNER: 'business_owner',
  VIEWER: 'viewer'
} as const

export type GrcRoleCode = typeof GRC_ROLE_CODES[keyof typeof GRC_ROLE_CODES]

export default rolesApi
