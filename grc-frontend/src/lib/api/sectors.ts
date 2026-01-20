/**
 * Sectors API Client
 * API methods for fetching organization sectors from the database
 */

import { api } from './client'

// ==================== Types ====================

export interface Sector {
  id: string
  code: string
  name: string
  nameAr: string
  description?: string
  descriptionAr?: string
  icon?: string
  isActive: boolean
  sortOrder: number
  parentId?: string
  children?: Sector[]
}

export interface SectorsResponse {
  items: Sector[]
  totalCount: number
}

// ==================== API Methods ====================

export const sectorsApi = {
  /**
   * Get all sectors
   */
  getAll: () =>
    api.get<SectorsResponse>('/api/app/sectors'),

  /**
   * Get active sectors only (for dropdowns)
   */
  getActive: () =>
    api.get<SectorsResponse>('/api/app/sectors/active'),

  /**
   * Get sectors as tree structure
   */
  getTree: () =>
    api.get<Sector[]>('/api/app/sectors/tree'),

  /**
   * Get a specific sector by ID
   */
  getById: (id: string) =>
    api.get<Sector>(`/api/app/sectors/${id}`),

  /**
   * Get sector by code
   */
  getByCode: (code: string) =>
    api.get<Sector>(`/api/app/sectors/by-code/${code}`),
}

// Predefined sector codes for type safety
export const SECTOR_CODES = {
  FINANCIAL: 'financial',
  HEALTHCARE: 'healthcare',
  GOVERNMENT: 'government',
  INDUSTRIAL: 'industrial',
  RETAIL: 'retail',
  TELECOM: 'telecom',
  ENERGY: 'energy',
  EDUCATION: 'education',
  REAL_ESTATE: 'real_estate',
  LOGISTICS: 'logistics',
  OTHER: 'other'
} as const

export type SectorCode = typeof SECTOR_CODES[keyof typeof SECTOR_CODES]

export default sectorsApi
