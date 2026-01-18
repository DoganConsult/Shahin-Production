import { notFound } from 'next/navigation'
import { getRequestConfig } from 'next-intl/server'

export const locales = ['ar', 'en'] as const
export const defaultLocale = 'ar' as const

export type Locale = (typeof locales)[number]

export default getRequestConfig(async ({ locale }) => {
  const validLocale = locale ?? defaultLocale
  if (!locales.includes(validLocale as Locale)) notFound()

  return {
    locale: validLocale,
    messages: (await import(`../messages/${validLocale}.json`)).default
  }
})
