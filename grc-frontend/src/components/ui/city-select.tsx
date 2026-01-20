"use client"

import { useState } from "react"
import { ChevronDown, Loader2, MapPin } from "lucide-react"
import { useCities } from "@/lib/hooks/use-cities"
import { useLocale } from "@/components/providers/locale-provider"

interface CitySelectProps {
  value: string
  onChange: (value: string) => void
  countryCode?: string
  required?: boolean
  disabled?: boolean
  placeholder?: string
  placeholderAr?: string
  className?: string
}

export function CitySelect({
  value,
  onChange,
  countryCode,
  required = false,
  disabled = false,
  placeholder = "Select city",
  placeholderAr = "اختر المدينة",
  className = ""
}: CitySelectProps) {
  const { cities, isLoading, error } = useCities(countryCode)
  const { locale } = useLocale()
  const isArabic = locale === "ar"
  const [isOpen, setIsOpen] = useState(false)

  const selectedCity = cities.find(c => c.code === value)
  const isDisabled = disabled || !countryCode

  return (
    <div className={`relative ${className}`}>
      <button
        type="button"
        onClick={() => !isDisabled && setIsOpen(!isOpen)}
        disabled={isDisabled || isLoading}
        className={`w-full h-12 pr-10 pl-10 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-emerald-500 text-right flex items-center justify-between ${
          isDisabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer"
        }`}
      >
        <MapPin className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
        <span className={`flex-1 text-right pr-6 ${!selectedCity ? "text-gray-400" : ""}`}>
          {!countryCode ? (
            isArabic ? "اختر الدولة أولاً" : "Select country first"
          ) : isLoading ? (
            <span className="flex items-center gap-2">
              <Loader2 className="w-4 h-4 animate-spin" />
              {isArabic ? "جاري التحميل..." : "Loading..."}
            </span>
          ) : selectedCity ? (
            isArabic ? selectedCity.nameAr : selectedCity.name
          ) : (
            isArabic ? placeholderAr : placeholder
          )}
        </span>
        <ChevronDown className={`w-5 h-5 text-gray-400 transition-transform ${isOpen ? "rotate-180" : ""}`} />
      </button>

      {isOpen && !isLoading && countryCode && (
        <div className="absolute z-50 w-full mt-2 bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-lg max-h-60 overflow-auto">
          {cities.length === 0 ? (
            <div className="px-4 py-3 text-center text-gray-500 dark:text-gray-400">
              {isArabic ? "لا توجد مدن متاحة" : "No cities available"}
            </div>
          ) : (
            cities.map((city) => (
              <button
                key={city.id}
                type="button"
                onClick={() => {
                  onChange(city.code)
                  setIsOpen(false)
                }}
                className={`w-full px-4 py-3 text-right hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors ${
                  value === city.code ? "bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400" : "text-gray-700 dark:text-gray-300"
                }`}
              >
                <div className="font-medium flex items-center gap-2">
                  {city.isCapital && (
                    <span className="text-xs bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400 px-2 py-0.5 rounded">
                      {isArabic ? "العاصمة" : "Capital"}
                    </span>
                  )}
                  {isArabic ? city.nameAr : city.name}
                </div>
                {city.region && (
                  <div className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                    {isArabic ? city.regionAr : city.region}
                  </div>
                )}
              </button>
            ))
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

export default CitySelect
