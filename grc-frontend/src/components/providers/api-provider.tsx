"use client"

import { createContext, useContext, useMemo, useState, useEffect, type ReactNode } from "react"

// API configuration
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5010"

interface ApiConfig {
  baseUrl: string
  headers: HeadersInit
}

interface ApiContextType {
  config: ApiConfig
  fetch: <T>(endpoint: string, options?: RequestInit) => Promise<T>
  get: <T>(endpoint: string) => Promise<T>
  post: <T>(endpoint: string, data?: unknown) => Promise<T>
  put: <T>(endpoint: string, data?: unknown) => Promise<T>
  patch: <T>(endpoint: string, data?: unknown) => Promise<T>
  delete: <T>(endpoint: string) => Promise<T>
}

const ApiContext = createContext<ApiContextType | undefined>(undefined)

/**
 * API Error class for typed error handling
 */
export class ApiError extends Error {
  constructor(
    public status: number,
    public statusText: string,
    public data?: unknown
  ) {
    super(`API Error: ${status} ${statusText}`)
    this.name = "ApiError"
  }
}

/**
 * ApiProvider - Centralized API client configuration
 *
 * Features:
 * - Automatic authentication header injection
 * - Type-safe fetch wrapper
 * - Convenience methods (get, post, put, patch, delete)
 * - Error handling
 */
export function ApiProvider({ children }: { children: ReactNode }) {
  // Store auth token - will be set from localStorage on mount
  const [authToken, setAuthToken] = useState<string | null>(null)

  // Check for auth token on mount
  useEffect(() => {
    const token = typeof window !== 'undefined' ? localStorage.getItem('auth_token') : null
    if (token) {
      setAuthToken(token)
    }
  }, [])

  const config = useMemo<ApiConfig>(() => ({
    baseUrl: API_BASE_URL,
    headers: {
      "Content-Type": "application/json",
      ...(authToken ? { Authorization: `Bearer ${authToken}` } : {}),
    },
  }), [authToken])

  // Base fetch function
  const apiFetch = async <T,>(endpoint: string, options: RequestInit = {}): Promise<T> => {
    const url = endpoint.startsWith("http") ? endpoint : `${config.baseUrl}${endpoint}`

    const response = await fetch(url, {
      ...options,
      headers: {
        ...config.headers,
        ...options.headers,
      },
    })

    // Handle non-OK responses
    if (!response.ok) {
      let errorData
      try {
        errorData = await response.json()
      } catch {
        errorData = null
      }
      throw new ApiError(response.status, response.statusText, errorData)
    }

    // Handle empty responses
    if (response.status === 204) {
      return null as T
    }

    return response.json()
  }

  // Convenience methods
  const get = <T,>(endpoint: string): Promise<T> => apiFetch<T>(endpoint, { method: "GET" })

  const post = <T,>(endpoint: string, data?: unknown): Promise<T> =>
    apiFetch<T>(endpoint, {
      method: "POST",
      body: data ? JSON.stringify(data) : undefined,
    })

  const put = <T,>(endpoint: string, data?: unknown): Promise<T> =>
    apiFetch<T>(endpoint, {
      method: "PUT",
      body: data ? JSON.stringify(data) : undefined,
    })

  const patch = <T,>(endpoint: string, data?: unknown): Promise<T> =>
    apiFetch<T>(endpoint, {
      method: "PATCH",
      body: data ? JSON.stringify(data) : undefined,
    })

  const del = <T,>(endpoint: string): Promise<T> => apiFetch<T>(endpoint, { method: "DELETE" })

  const value: ApiContextType = {
    config,
    fetch: apiFetch,
    get,
    post,
    put,
    patch,
    delete: del,
  }

  return <ApiContext.Provider value={value}>{children}</ApiContext.Provider>
}

/**
 * Hook to access API client
 */
export function useApi() {
  const context = useContext(ApiContext)
  if (context === undefined) {
    throw new Error("useApi must be used within an ApiProvider")
  }
  return context
}

export default ApiProvider
