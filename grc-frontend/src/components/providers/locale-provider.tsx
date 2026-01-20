"use client"

import { createContext, useContext, useState, useEffect, type ReactNode } from "react"
import { NextIntlClientProvider } from "next-intl"
import arMessages from "../../../messages/ar.json"
import enMessages from "../../../messages/en.json"

type Locale = "ar" | "en"

interface LocaleContextType {
  locale: Locale
  setLocale: (locale: Locale) => void
  isRTL: boolean
}

const LocaleContext = createContext<LocaleContextType | undefined>(undefined)

export function useLocale() {
  const context = useContext(LocaleContext)
  if (!context) {
    throw new Error("useLocale must be used within a LocaleProvider")
  }
  return context
}

interface LocaleProviderProps {
  children: ReactNode
  defaultLocale?: Locale
}

export function LocaleProvider({ children, defaultLocale = "ar" }: LocaleProviderProps) {
  const [locale, setLocaleState] = useState<Locale>(defaultLocale)
  const [messages, setMessages] = useState<Record<string, unknown>>(
    defaultLocale === "ar" ? arMessages : enMessages
  )
  const [isHydrated, setIsHydrated] = useState(false)

  // Read locale from localStorage on mount
  useEffect(() => {
    const savedLocale = localStorage.getItem("locale") as Locale | null
    if (savedLocale && (savedLocale === "ar" || savedLocale === "en")) {
      setLocaleState(savedLocale)
      setMessages(savedLocale === "ar" ? arMessages : enMessages)
    }
    setIsHydrated(true)
  }, [])

  // Update document direction when locale changes
  useEffect(() => {
    if (isHydrated) {
      document.documentElement.lang = locale
      document.documentElement.dir = locale === "ar" ? "rtl" : "ltr"
    }
  }, [locale, isHydrated])

  const setLocale = (newLocale: Locale) => {
    setLocaleState(newLocale)
    setMessages(newLocale === "ar" ? arMessages : enMessages)
    localStorage.setItem("locale", newLocale)
    // Update document attributes immediately
    document.documentElement.lang = newLocale
    document.documentElement.dir = newLocale === "ar" ? "rtl" : "ltr"
  }

  const isRTL = locale === "ar"

  return (
    <LocaleContext.Provider value={{ locale, setLocale, isRTL }}>
      <NextIntlClientProvider
        locale={locale}
        messages={messages}
        timeZone="Asia/Riyadh"
      >
        {children}
      </NextIntlClientProvider>
    </LocaleContext.Provider>
  )
}
