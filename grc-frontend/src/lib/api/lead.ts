/**
 * Lead API Client
 * API methods for lead capture and contact forms
 */

import { api } from './client'

// ==================== Types ====================

export interface LeadSubmitRequest {
  email: string
  companySize?: string
  sector?: string
  erpSystem?: string
  mainPain?: string
  firstName?: string
  lastName?: string
  companyName?: string
  phone?: string
  message?: string
  source?: string
  utmSource?: string
  utmMedium?: string
  utmCampaign?: string
}

export interface LeadSubmitResponse {
  success: boolean
  leadId?: string
  message: string
}

export interface ContactFormRequest {
  name: string
  email: string
  company?: string
  phone?: string
  subject: string
  message: string
}

export interface ContactFormResponse {
  success: boolean
  ticketId?: string
  message: string
}

export interface NewsletterSubscribeRequest {
  email: string
  source?: string
}

export interface NewsletterSubscribeResponse {
  success: boolean
  message: string
}

// ==================== API Methods ====================

export const leadApi = {
  /**
   * Submit lead form (demo request)
   */
  submit: (data: LeadSubmitRequest) =>
    api.post<LeadSubmitResponse>('/api/lead/submit', data),

  /**
   * Submit contact form
   */
  contact: (data: ContactFormRequest) =>
    api.post<ContactFormResponse>('/api/contact/submit', data),

  /**
   * Subscribe to newsletter
   */
  subscribeNewsletter: (data: NewsletterSubscribeRequest) =>
    api.post<NewsletterSubscribeResponse>('/api/newsletter/subscribe', data),

  /**
   * Track lead activity
   */
  trackActivity: (leadId: string, activityType: string, details?: string) =>
    api.post('/api/lead/track', { leadId, activityType, details }),
}

export default leadApi
