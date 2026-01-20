"use client"

import { useState } from "react"
import { ChevronDown, Loader2, Globe } from "lucide-react"
import { useCountries } from "@/lib/hooks/use-countries"
import { useLocale } from "@/components/providers/locale-provider"

interface CountrySelectProps {
  value: string
  onChange: (value: string) => void
  required?: boolean
  disabled?: boolean
  placeholder?: string
  placeholderAr?: string
  className?: string
}

export function CountrySelect({
  value,
  onChange,
  required = false,
  disabled = false,
  placeholder = "Select country",
  placeholderAr = "اختر الدولة",
  className = ""
}: CountrySelectProps) {
  const { countries, isLoading, error } = useCountries()
  const { locale } = useLocale()
  const isArabic = locale === "ar"
  const [isOpen, setIsOpen] = useState(false)

  const selectedCountry = countries.find(c => c.code === value)

  // Group countries by region
  const gccCountries = countries.filter(c => c.region === 'GCC')
  const menaCountries = countries.filter(c => c.region === 'MENA')
  const internationalCountries = countries.filter(c => c.region === 'International')

  return (
    <div className={`relative ${className}`}>
      <button
        type="button"
        onClick={() => !disabled && setIsOpen(!isOpen)}
        disabled={disabled || isLoading}
        className={`w-full h-12 pr-10 pl-10 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-emerald-500 text-right flex items-center justify-between ${
          disabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer"
        }`}
      >
        <Globe className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
        <span className={`flex-1 text-right pr-6 ${!selectedCountry ? "text-gray-400" : ""}`}>
          {isLoading ? (
            <span className="flex items-center gap-2">
              <Loader2 className="w-4 h-4 animate-spin" />
              {isArabic ? "جاري التحميل..." : "Loading..."}
            </span>
          ) : selectedCountry ? (
            <span className="flex items-center gap-2">
              <span>{selectedCountry.flag}</span>
              <span>{isArabic ? selectedCountry.nameAr : selectedCountry.name}</span>
            </span>
          ) : (
            isArabic ? placeholderAr : placeholder
          )}
        </span>
        <ChevronDown className={`w-5 h-5 text-gray-400 transition-transform ${isOpen ? "rotate-180" : ""}`} />
      </button>

      {isOpen && !isLoading && (
        <div className="absolute z-50 w-full mt-2 bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-lg max-h-72 overflow-auto">
          {/* GCC Countries */}
          {gccCountries.length > 0 && (
            <>
              <div className="px-4 py-2 text-xs font-semibold text-gray-500 dark:text-gray-400 bg-gray-50 dark:bg-gray-900 sticky top-0">
                {isArabic ? "دول مجلس التعاون الخليجي" : "GCC Countries"}
              </div>
              {gccCountries.map((country) => (
                <button
                  key={country.id}
                  type="button"
                  onClick={() => {
                    onChange(country.code)
                    setIsOpen(false)
                  }}
                  className={`w-full px-4 py-3 text-right hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors flex items-center gap-3 ${
                    value === country.code ? "bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400" : "text-gray-700 dark:text-gray-300"
                  }`}
                >
                  <span className="text-lg">{country.flag}</span>
                  <span className="font-medium">{isArabic ? country.nameAr : country.name}</span>
                </button>
              ))}
            </>
          )}

          {/* MENA Countries */}
          {menaCountries.length > 0 && (
            <>
              <div className="px-4 py-2 text-xs font-semibold text-gray-500 dark:text-gray-400 bg-gray-50 dark:bg-gray-900 sticky top-0">
                {isArabic ? "الشرق الأوسط وشمال أفريقيا" : "MENA Region"}
              </div>
              {menaCountries.map((country) => (
                <button
                  key={country.id}
                  type="button"
                  onClick={() => {
                    onChange(country.code)
                    setIsOpen(false)
                  }}
                  className={`w-full px-4 py-3 text-right hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors flex items-center gap-3 ${
                    value === country.code ? "bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400" : "text-gray-700 dark:text-gray-300"
                  }`}
                >
                  <span className="text-lg">{country.flag}</span>
                  <span className="font-medium">{isArabic ? country.nameAr : country.name}</span>
                </button>
              ))}
            </>
          )}

          {/* International */}
          {internationalCountries.length > 0 && (
            <>
              <div className="px-4 py-2 text-xs font-semibold text-gray-500 dark:text-gray-400 bg-gray-50 dark:bg-gray-900 sticky top-0">
                {isArabic ? "دولي" : "International"}
              </div>
              {internationalCountries.map((country) => (
                <button
                  key={country.id}
                  type="button"
                  onClick={() => {
                    onChange(country.code)
                    setIsOpen(false)
                  }}
                  className={`w-full px-4 py-3 text-right hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors flex items-center gap-3 ${
                    value === country.code ? "bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400" : "text-gray-700 dark:text-gray-300"
                  }`}
                >
                  <span className="text-lg">{country.flag}</span>
                  <span className="font-medium">{isArabic ? country.nameAr : country.name}</span>
                </button>
              ))}
            </>
          )}
        </div>
      )}

      {/* Hidden input for form validation */}
      <input
        type="hidden"
        value={value}
        required={required}
      />
    </div>
  )
}

export default CountrySelect
