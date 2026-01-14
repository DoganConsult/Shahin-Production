"use client"

import { createContext, useContext, useEffect, useState, type ReactNode } from "react"

type Theme = "dark" | "light" | "system"

interface ThemeProviderProps {
  children: ReactNode
  defaultTheme?: Theme
  attribute?: string
  enableSystem?: boolean
  disableTransitionOnChange?: boolean
}

interface ThemeContextType {
  theme: Theme
  setTheme: (theme: Theme) => void
  resolvedTheme: "dark" | "light"
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined)

/**
 * ThemeProvider - Manages application theme (light/dark mode)
 *
 * Features:
 * - Persists theme preference to localStorage
 * - Supports system theme detection
 * - Applies theme class to document root
 */
export function ThemeProvider({
  children,
  defaultTheme = "light",
  attribute = "class",
  enableSystem = false,
  disableTransitionOnChange = false,
}: ThemeProviderProps) {
  const [theme, setThemeState] = useState<Theme>(defaultTheme)
  const [resolvedTheme, setResolvedTheme] = useState<"dark" | "light">("light")

  useEffect(() => {
    // Get stored theme or use default
    const storedTheme = localStorage.getItem("theme") as Theme | null
    if (storedTheme) {
      setThemeState(storedTheme)
    }
  }, [])

  useEffect(() => {
    const root = document.documentElement

    // Disable transitions during theme change if requested
    if (disableTransitionOnChange) {
      root.classList.add("[&_*]:!transition-none")
      setTimeout(() => {
        root.classList.remove("[&_*]:!transition-none")
      }, 0)
    }

    // Resolve theme (handle system preference)
    let resolved: "dark" | "light" = "light"
    if (theme === "system" && enableSystem) {
      resolved = window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light"
    } else {
      resolved = theme === "dark" ? "dark" : "light"
    }

    setResolvedTheme(resolved)

    // Apply theme to document
    if (attribute === "class") {
      root.classList.remove("light", "dark")
      root.classList.add(resolved)
    } else {
      root.setAttribute(attribute, resolved)
    }

    // Store preference
    localStorage.setItem("theme", theme)
  }, [theme, attribute, enableSystem, disableTransitionOnChange])

  // Listen for system theme changes
  useEffect(() => {
    if (!enableSystem || theme !== "system") return

    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)")
    const handleChange = (e: MediaQueryListEvent) => {
      setResolvedTheme(e.matches ? "dark" : "light")
    }

    mediaQuery.addEventListener("change", handleChange)
    return () => mediaQuery.removeEventListener("change", handleChange)
  }, [enableSystem, theme])

  const setTheme = (newTheme: Theme) => {
    setThemeState(newTheme)
  }

  return (
    <ThemeContext.Provider value={{ theme, setTheme, resolvedTheme }}>
      {children}
    </ThemeContext.Provider>
  )
}

/**
 * Hook to access theme context
 */
export function useTheme() {
  const context = useContext(ThemeContext)
  if (context === undefined) {
    throw new Error("useTheme must be used within a ThemeProvider")
  }
  return context
}

export default ThemeProvider
