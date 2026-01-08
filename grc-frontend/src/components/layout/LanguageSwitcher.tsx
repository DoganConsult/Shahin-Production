"use client"

import { useLocale } from 'next-intl'
import { useRouter, usePathname } from 'next/navigation'
import { Globe } from 'lucide-react'
import { locales, localeNames, type Locale } from '@/i18n'

export function LanguageSwitcher() {
  const locale = useLocale() as Locale
  const router = useRouter()
  const pathname = usePathname()

  const switchLocale = (newLocale: Locale) => {
    // Remove current locale from pathname if present
    const segments = pathname.split('/')
    if (locales.includes(segments[1] as Locale)) {
      segments.splice(1, 1)
    }
    
    // Add new locale
    const newPath = newLocale === 'ar' 
      ? segments.join('/') || '/'
      : `/${newLocale}${segments.join('/') || ''}`
    
    router.push(newPath)
  }

  const otherLocale = locale === 'ar' ? 'en' : 'ar'

  return (
    <button
      onClick={() => switchLocale(otherLocale)}
      className="flex items-center gap-2 px-3 py-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
      title={`Switch to ${localeNames[otherLocale]}`}
    >
      <Globe className="w-5 h-5 text-gray-600 dark:text-gray-400" />
      <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
        {localeNames[otherLocale]}
      </span>
    </button>
  )
}

export function LanguageDropdown() {
  const locale = useLocale() as Locale
  const router = useRouter()
  const pathname = usePathname()

  const switchLocale = (newLocale: Locale) => {
    const segments = pathname.split('/')
    if (locales.includes(segments[1] as Locale)) {
      segments.splice(1, 1)
    }
    
    const newPath = newLocale === 'ar' 
      ? segments.join('/') || '/'
      : `/${newLocale}${segments.join('/') || ''}`
    
    router.push(newPath)
  }

  return (
    <div className="relative group">
      <button className="flex items-center gap-2 px-3 py-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors">
        <Globe className="w-5 h-5 text-gray-600 dark:text-gray-400" />
        <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
          {localeNames[locale]}
        </span>
      </button>
      <div className="absolute top-full right-0 mt-1 w-32 bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all">
        {locales.map((l) => (
          <button
            key={l}
            onClick={() => switchLocale(l)}
            className={`w-full px-4 py-2 text-right text-sm hover:bg-gray-100 dark:hover:bg-gray-700 first:rounded-t-lg last:rounded-b-lg ${
              locale === l ? 'text-emerald-600 font-medium' : 'text-gray-700 dark:text-gray-300'
            }`}
          >
            {localeNames[l]}
          </button>
        ))}
      </div>
    </div>
  )
}
