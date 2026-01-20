/**
 * Cities API Client
 * API methods for fetching cities
 */

import { api } from './client'

// ==================== Types ====================

export interface City {
  id: string
  code: string
  name: string
  nameAr: string
  countryCode: string
  countryId: string
  region?: string
  regionAr?: string
  isCapital: boolean
  isActive: boolean
  sortOrder: number
}

export interface CitiesResponse {
  items: City[]
  totalCount: number
}

// ==================== API Methods ====================

export const citiesApi = {
  /**
   * Get all cities
   */
  getAll: () =>
    api.get<CitiesResponse>('/api/app/cities'),

  /**
   * Get cities by country code
   */
  getByCountry: (countryCode: string) =>
    api.get<CitiesResponse>(`/api/app/cities/by-country/${countryCode}`),

  /**
   * Get active cities by country
   */
  getActiveByCountry: (countryCode: string) =>
    api.get<CitiesResponse>(`/api/app/cities/active/by-country/${countryCode}`),

  /**
   * Get city by code
   */
  getByCode: (code: string) =>
    api.get<City>(`/api/app/cities/by-code/${code}`),
}

export default citiesApi
