/**
 * OpenID Connect (OIDC) Client for ABP OpenIddict
 * Handles OAuth2/OIDC flow with authorization code grant
 */

const OIDC_CONFIG = {
  clientId: 'grc-web',
  redirectUri: 'http://localhost:3003/api/auth/callback',
  scope: 'openid profile email roles offline_access',
  responseType: 'code',
  authorizationEndpoint: 'http://localhost:3006/connect/authorize',
  tokenEndpoint: 'http://localhost:3006/connect/token',
  logoutEndpoint: 'http://localhost:3006/connect/logout',
  userinfoEndpoint: 'http://localhost:3006/connect/userinfo',
}

export interface OIDCUser {
  sub: string
  name?: string
  email?: string
  email_verified?: boolean
  roles?: string[]
  tenant_id?: string
}

export interface OIDCTokens {
  access_token: string
  token_type: string
  expires_in?: number
  refresh_token?: string
  scope?: string
}

/**
 * Generate random state for OAuth2 flow
 */
function generateState(): string {
  return Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15)
}

/**
 * Store state in sessionStorage for CSRF protection
 */
function storeState(state: string): void {
  if (typeof window !== 'undefined') {
    sessionStorage.setItem('oidc_state', state)
  }
}

/**
 * Verify stored state matches returned state
 */
function verifyState(state: string): boolean {
  if (typeof window === 'undefined') return false
  const storedState = sessionStorage.getItem('oidc_state')
  sessionStorage.removeItem('oidc_state')
  return storedState === state
}

/**
 * Redirect to OpenIddict authorization endpoint
 */
export function signIn(returnUrl?: string): void {
  const state = generateState()
  storeState(state)
  
  // Store return URL for after authentication
  if (returnUrl) {
    sessionStorage.setItem('oidc_return_url', returnUrl)
  }

  const params = new URLSearchParams({
    response_type: OIDC_CONFIG.responseType,
    client_id: OIDC_CONFIG.clientId,
    redirect_uri: OIDC_CONFIG.redirectUri,
    scope: OIDC_CONFIG.scope,
    state: state,
  })

  window.location.href = `${OIDC_CONFIG.authorizationEndpoint}?${params.toString()}`
}

/**
 * Get current access token from cookies
 */
export async function getAccessToken(): Promise<string | null> {
  if (typeof window === 'undefined') return null
  
  // In browser, read from httpOnly cookie via API
  try {
    const response = await fetch('/api/auth/token', { credentials: 'include' })
    if (response.ok) {
      const data = await response.json()
      return data.access_token
    }
  } catch (error) {
    console.error('Failed to get access token:', error)
  }
  
  return null
}

/**
 * Refresh access token using refresh token
 */
export async function refreshAccessToken(): Promise<string | null> {
  try {
    const response = await fetch('/api/auth/refresh', { 
      method: 'POST',
      credentials: 'include' 
    })
    
    if (response.ok) {
      const data = await response.json()
      return data.access_token
    }
  } catch (error) {
    console.error('Failed to refresh token:', error)
  }
  
  return null
}

/**
 * Sign out user
 */
export function signOut(): void {
  // Clear local session
  if (typeof window !== 'undefined') {
    sessionStorage.clear()
  }
  
  // Redirect to OpenIddict logout
  const params = new URLSearchParams({
    post_logout_redirect_uri: 'http://localhost:3003',
  })
  
  window.location.href = `${OIDC_CONFIG.logoutEndpoint}?${params.toString()}`
}

/**
 * Get user info from OpenIddict userinfo endpoint
 */
export async function getUserInfo(): Promise<OIDCUser | null> {
  const token = await getAccessToken()
  if (!token) return null
  
  try {
    const response = await fetch(OIDC_CONFIG.userinfoEndpoint, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })
    
    if (response.ok) {
      return await response.json()
    }
  } catch (error) {
    console.error('Failed to get user info:', error)
  }
  
  return null
}

export default {
  signIn,
  signOut,
  getAccessToken,
  refreshAccessToken,
  getUserInfo,
}
