/**
 * Auth API Client
 * API methods for authentication using ABP OpenID Connect
 * No JWT - uses ABP built-in identity server
 */

import { api } from './client'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5010'

// ==================== Types ====================

export interface RegisterRequest {
  email: string
  password: string
  firstName: string
  lastName: string
  companyName: string
  phone?: string
  sector?: string
}

export interface RegisterResponse {
  success: boolean
  userId?: string
  message: string
  requiresVerification?: boolean
}

export interface ForgotPasswordRequest {
  email: string
}

export interface ForgotPasswordResponse {
  success: boolean
  message: string
}

export interface ResetPasswordRequest {
  token: string
  newPassword: string
  confirmPassword: string
}

export interface ResetPasswordResponse {
  success: boolean
  message: string
}

export interface InvitationAcceptRequest {
  token: string
  firstName: string
  lastName: string
  password: string
}

export interface InvitationAcceptResponse {
  success: boolean
  message: string
  loginUrl?: string
}

export interface VerifyEmailRequest {
  token: string
}

export interface VerifyEmailResponse {
  success: boolean
  message: string
}

export interface UserInfo {
  id: string
  email: string
  firstName: string
  lastName: string
  tenantId?: string
  roles: string[]
}

// ==================== API Methods ====================

export const authApi = {
  /**
   * Redirect to ABP OpenID Connect login
   * Uses ABP built-in identity server
   */
  login: (returnUrl?: string): void => {
    const redirect = returnUrl || '/dashboard'
    // Redirect to ABP MVC login page which handles OpenID Connect
    // Include frontend URL in return URL so user comes back to port 3003
    const frontendReturnUrl = `http://localhost:3003${redirect}`
    window.location.href = `${API_BASE_URL}/Account/Login?returnUrl=${encodeURIComponent(frontendReturnUrl)}`
  },

  /**
   * Register a new user account via ABP
   */
  register: (data: RegisterRequest) =>
    api.post<RegisterResponse>('/api/auth/register', data),

  /**
   * Redirect to ABP register page
   */
  redirectToRegister: (returnUrl?: string): void => {
    const redirect = returnUrl || '/dashboard'
    // Include frontend URL in return URL so user comes back to port 3003
    const frontendReturnUrl = `http://localhost:3003${redirect}`
    window.location.href = `${API_BASE_URL}/Account/Register?returnUrl=${encodeURIComponent(frontendReturnUrl)}`
  },

  /**
   * Request password reset email
   */
  forgotPassword: (data: ForgotPasswordRequest) =>
    api.post<ForgotPasswordResponse>('/api/auth/forgot-password', data),

  /**
   * Redirect to ABP forgot password page
   */
  redirectToForgotPassword: (): void => {
    // Include frontend URL in return URL for after password reset
    const frontendReturnUrl = `http://localhost:3003/login`
    window.location.href = `${API_BASE_URL}/Account/ForgotPassword?returnUrl=${encodeURIComponent(frontendReturnUrl)}`
  },

  /**
   * Reset password with token
   */
  resetPassword: (data: ResetPasswordRequest) =>
    api.post<ResetPasswordResponse>('/api/auth/reset-password', data),

  /**
   * Accept team invitation
   */
  acceptInvitation: (data: InvitationAcceptRequest) =>
    api.post<InvitationAcceptResponse>('/api/invitation/accept', data),

  /**
   * Verify email address
   */
  verifyEmail: (data: VerifyEmailRequest) =>
    api.post<VerifyEmailResponse>('/api/auth/verify-email', data),

  /**
   * Logout - redirects to ABP logout
   */
  logout: (): void => {
    window.location.href = `${API_BASE_URL}/Account/Logout`
  },

  /**
   * Get current user info from ABP session
   */
  getCurrentUser: () =>
    api.get<UserInfo>('/api/abp/application-configuration').then(config => {
      const currentUser = (config as { currentUser?: UserInfo }).currentUser
      return currentUser
    }),

  /**
   * Check if user is authenticated via ABP session
   */
  checkSession: () =>
    api.get<{ isAuthenticated: boolean; user?: UserInfo }>('/api/auth/session'),

  /**
   * Get ABP application configuration (includes auth state)
   */
  getAppConfig: () =>
    api.get('/api/abp/application-configuration')
}

export default authApi
