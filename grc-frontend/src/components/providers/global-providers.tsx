"use client"

import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { SessionProvider } from "next-auth/react"
import { useState, useEffect, type ReactNode } from "react"
import { ThemeProvider } from "./theme-provider"
import { ToastProvider } from "./toast-provider"
import { ApiProvider } from "./api-provider"
import { CommandPalette } from "@/components/ui/command-palette"

interface GlobalProvidersProps {
  children: ReactNode
  locale?: string
}

/**
 * GlobalProviders - Centralized provider configuration
 *
 * Wraps the application with all necessary context providers:
 * - SessionProvider: Authentication state from next-auth
 * - QueryClientProvider: React Query for API data fetching
 * - ApiProvider: Centralized API client with auth headers
 * - ThemeProvider: Theme switching (light/dark mode)
 * - ToastProvider: Toast notifications
 */
export function GlobalProviders({ children, locale = "ar" }: GlobalProvidersProps) {
  // Track if we're mounted on client to avoid hydration issues
  const [isMounted, setIsMounted] = useState(false)

  useEffect(() => {
    setIsMounted(true)
  }, [])

  // Create query client with default options
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            // Disable refetch on window focus in development
            refetchOnWindowFocus: process.env.NODE_ENV === "production",
            // Retry failed requests once
            retry: 1,
            // Cache data for 5 minutes
            staleTime: 5 * 60 * 1000,
          },
          mutations: {
            // Show error notifications on mutation failure
            onError: (error) => {
              console.error("Mutation error:", error)
            },
          },
        },
      })
  )

  // During SSR/SSG, render without SessionProvider to allow static generation
  // SessionProvider and ApiProvider will be enabled once the component mounts on client
  if (!isMounted) {
    return (
      <QueryClientProvider client={queryClient}>
        <ApiProvider>
          <ThemeProvider
            attribute="class"
            defaultTheme="light"
            enableSystem={false}
            disableTransitionOnChange
          >
            <ToastProvider>
              {children}
              <CommandPalette />
            </ToastProvider>
          </ThemeProvider>
        </ApiProvider>
      </QueryClientProvider>
    )
  }

  return (
    <SessionProvider>
      <QueryClientProvider client={queryClient}>
        <ApiProvider>
          <ThemeProvider
            attribute="class"
            defaultTheme="light"
            enableSystem={false}
            disableTransitionOnChange
          >
            <ToastProvider>
              {children}
              <CommandPalette />
            </ToastProvider>
          </ThemeProvider>
        </ApiProvider>
      </QueryClientProvider>
    </SessionProvider>
  )
}

export default GlobalProviders
