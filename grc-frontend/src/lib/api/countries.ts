/**
 * Countries API Client
 * API methods for fetching countries
 */

import { api } from './client'

// ==================== Types ====================

export interface Country {
  id: string
  code: string
  name: string
  nameAr: string
  iso2: string
  iso3: string
  phoneCode: string
  currency: string
  currencyAr: string
  region: string
  regionAr: string
  flag: string
  isActive: boolean
  sortOrder: number
}

export interface CountriesResponse {
  items: Country[]
  totalCount: number
}

// ==================== API Methods ====================

export const countriesApi = {
  /**
   * Get all countries
   */
  getAll: () =>
    api.get<CountriesResponse>('/api/app/countries'),

  /**
   * Get active countries
   */
  getActive: () =>
    api.get<CountriesResponse>('/api/app/countries/active'),

  /**
   * Get GCC countries
   */
  getGCC: () =>
    api.get<CountriesResponse>('/api/app/countries/gcc'),

  /**
   * Get MENA region countries
   */
  getMENA: () =>
    api.get<CountriesResponse>('/api/app/countries/mena'),

  /**
   * Get country by code
   */
  getByCode: (code: string) =>
    api.get<Country>(`/api/app/countries/by-code/${code}`),
}

export default countriesApi
