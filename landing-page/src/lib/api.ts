/**
 * API Client for GrcMvc Backend
 * Handles CSRF tokens, error handling, and all API calls
 */

import type {
  CsrfTokenResponse,
  ContactRequest,
  ContactResponse,
  NewsletterRequest,
  NewsletterResponse,
  TrialSignupRequest,
  TrialSignupResponse,
  LandingPageData,
  ApiResponse,
} from '@/types/api';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5137';

// CSRF Token Management
let csrfToken: string | null = null;
let csrfTokenExpiry: number | null = null;
const CSRF_TOKEN_LIFETIME = 30 * 60 * 1000; // 30 minutes

/**
 * Fetches a new CSRF token from the backend
 */
async function fetchCsrfToken(): Promise<string> {
  try {
    const response = await fetch(`${API_URL}/api/Landing/csrf-token`, {
      method: 'GET',
      credentials: 'include',
      headers: {
        'Accept': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch CSRF token: ${response.status}`);
    }

    const data: CsrfTokenResponse = await response.json();

    if (data.success && data.token) {
      csrfToken = data.token;
      csrfTokenExpiry = Date.now() + CSRF_TOKEN_LIFETIME;
      return data.token;
    }

    throw new Error('Invalid CSRF token response');
  } catch (error) {
    console.error('Error fetching CSRF token:', error);
    throw error;
  }
}

/**
 * Gets a valid CSRF token, fetching a new one if needed
 */
async function getCsrfToken(): Promise<string> {
  if (csrfToken && csrfTokenExpiry && Date.now() < csrfTokenExpiry) {
    return csrfToken;
  }
  return fetchCsrfToken();
}

/**
 * Makes an API request with optional CSRF token
 */
async function apiRequest<T>(
  endpoint: string,
  options: RequestInit & { requiresCsrf?: boolean } = {}
): Promise<T> {
  const { requiresCsrf = false, ...fetchOptions } = options;

  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
    ...(fetchOptions.headers || {}),
  };

  // Add CSRF token for protected endpoints
  if (requiresCsrf) {
    try {
      const token = await getCsrfToken();
      (headers as Record<string, string>)['X-CSRF-Token'] = token;
    } catch {
      console.warn('Failed to get CSRF token, proceeding without it');
    }
  }

  const response = await fetch(`${API_URL}${endpoint}`, {
    ...fetchOptions,
    headers,
    credentials: 'include', // Required for CORS with credentials
  });

  // Handle CSRF token expiry - retry once with new token
  if (response.status === 400 && requiresCsrf) {
    const errorData = await response.json().catch(() => ({}));
    if (errorData.message?.toLowerCase().includes('token') ||
        errorData.message?.toLowerCase().includes('antiforgery')) {
      // Token might be expired, fetch a new one and retry
      csrfToken = null;
      csrfTokenExpiry = null;
      const newToken = await fetchCsrfToken();
      (headers as Record<string, string>)['X-CSRF-Token'] = newToken;

      const retryResponse = await fetch(`${API_URL}${endpoint}`, {
        ...fetchOptions,
        headers,
        credentials: 'include',
      });

      if (!retryResponse.ok) {
        const retryError = await retryResponse.json().catch(() => ({ message: 'Request failed' }));
        throw new Error(retryError.message || `API error: ${retryResponse.status}`);
      }

      return retryResponse.json();
    }
  }

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({ message: 'Request failed' }));
    throw new Error(errorData.message || `API error: ${response.status}`);
  }

  return response.json();
}

// ============ API Methods ============

/**
 * Submit contact form
 */
export async function submitContact(data: ContactRequest): Promise<ContactResponse> {
  return apiRequest<ContactResponse>('/api/Landing/Contact', {
    method: 'POST',
    body: JSON.stringify(data),
    requiresCsrf: true,
  });
}

/**
 * Subscribe to newsletter
 */
export async function subscribeNewsletter(data: NewsletterRequest): Promise<NewsletterResponse> {
  return apiRequest<NewsletterResponse>('/api/Landing/SubscribeNewsletter', {
    method: 'POST',
    body: JSON.stringify(data),
    requiresCsrf: true,
  });
}

/**
 * Start trial signup (no CSRF required)
 */
export async function startTrial(data: TrialSignupRequest): Promise<TrialSignupResponse> {
  return apiRequest<TrialSignupResponse>('/api/Landing/StartTrial', {
    method: 'POST',
    body: JSON.stringify(data),
    requiresCsrf: false, // This endpoint has IgnoreAntiforgeryToken
  });
}

/**
 * Get all landing page data
 */
export async function getLandingPageData(): Promise<LandingPageData> {
  return apiRequest<LandingPageData>('/api/landing/all', {
    method: 'GET',
    requiresCsrf: false,
  });
}

/**
 * Unsubscribe from newsletter
 */
export async function unsubscribeNewsletter(email: string): Promise<ApiResponse> {
  return apiRequest<ApiResponse>('/api/Landing/UnsubscribeNewsletter', {
    method: 'POST',
    body: JSON.stringify({ email }),
    requiresCsrf: false, // This endpoint doesn't require CSRF
  });
}

/**
 * Send chat message (AI chat widget)
 */
export async function sendChatMessage(message: string, context?: string): Promise<ApiResponse<{ response: string }>> {
  return apiRequest<ApiResponse<{ response: string }>>('/api/Landing/ChatMessage', {
    method: 'POST',
    body: JSON.stringify({ message, context }),
    requiresCsrf: false,
  });
}

// Export the base API URL for use in components
export { API_URL };
