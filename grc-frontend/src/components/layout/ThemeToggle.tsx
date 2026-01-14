"use client"

import * as React from "react"
import { Sun, Moon } from "lucide-react"

/**
 * ThemeToggle Component - Design System 2.0
 *
 * Toggles between light and dark mode.
 * - Defaults to LIGHT mode
 * - Persists preference in localStorage
 * - Accessible with ARIA labels
 *
 * @example
 * <ThemeToggle />
 * <ThemeToggle forceLight /> // Always start with light mode
 */

export interface ThemeToggleProps {
  /** Force light mode on mount (ignores localStorage/system preference) */
  forceLight?: boolean
  /** Custom class name */
  className?: string
}

export function ThemeToggle({ forceLight = true, className }: ThemeToggleProps) {
  const [theme, setTheme] = React.useState<"light" | "dark">("light")
  const [mounted, setMounted] = React.useState(false)

  React.useEffect(() => {
    setMounted(true)

    // If forceLight is true, always use light mode
    if (forceLight) {
      setTheme("light")
      document.documentElement.classList.remove("dark")
      localStorage.setItem("theme", "light")
      return
    }

    // Check localStorage first
    const savedTheme = localStorage.getItem("theme") as "light" | "dark" | null

    if (savedTheme) {
      setTheme(savedTheme)
      document.documentElement.classList.toggle("dark", savedTheme === "dark")
    } else {
      // Default to light mode (don't check system preference)
      // This ensures consistent light mode experience
      setTheme("light")
      document.documentElement.classList.remove("dark")
    }
  }, [forceLight])

  const toggleTheme = React.useCallback(() => {
    const newTheme = theme === "light" ? "dark" : "light"
    setTheme(newTheme)
    localStorage.setItem("theme", newTheme)
    document.documentElement.classList.toggle("dark", newTheme === "dark")
  }, [theme])

  // Prevent hydration mismatch
  if (!mounted) {
    return (
      <button
        className={`w-10 h-10 rounded-lg bg-gray-100 flex items-center justify-center ${className || ""}`}
        aria-label="Toggle theme"
        disabled
      >
        <span className="w-5 h-5" />
      </button>
    )
  }

  const isLight = theme === "light"

  return (
    <button
      onClick={toggleTheme}
      className={`
        w-10 h-10 rounded-lg flex items-center justify-center
        transition-all duration-200
        ${isLight
          ? "bg-gray-100 hover:bg-gray-200 text-gray-600"
          : "bg-gray-800 hover:bg-gray-700 text-gray-400"
        }
        focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2
        ${className || ""}
      `}
      aria-label={isLight ? "Switch to dark mode" : "Switch to light mode"}
      aria-pressed={!isLight}
      title={isLight ? "الوضع الداكن" : "الوضع الفاتح"}
    >
      {isLight ? (
        <Moon className="w-5 h-5" aria-hidden="true" />
      ) : (
        <Sun className="w-5 h-5" aria-hidden="true" />
      )}
    </button>
  )
}

/**
 * Hook to get current theme
 */
export function useTheme() {
  const [theme, setTheme] = React.useState<"light" | "dark">("light")
  const [mounted, setMounted] = React.useState(false)

  React.useEffect(() => {
    setMounted(true)
    const savedTheme = localStorage.getItem("theme") as "light" | "dark" | null
    if (savedTheme) {
      setTheme(savedTheme)
    }

    // Listen for theme changes
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.attributeName === "class") {
          const isDark = document.documentElement.classList.contains("dark")
          setTheme(isDark ? "dark" : "light")
        }
      })
    })

    observer.observe(document.documentElement, { attributes: true })

    return () => observer.disconnect()
  }, [])

  const setThemeValue = React.useCallback((newTheme: "light" | "dark") => {
    setTheme(newTheme)
    localStorage.setItem("theme", newTheme)
    document.documentElement.classList.toggle("dark", newTheme === "dark")
  }, [])

  return {
    theme,
    setTheme: setThemeValue,
    isDark: theme === "dark",
    isLight: theme === "light",
    mounted,
  }
}

export default ThemeToggle
