"use client"

import { useState } from "react"
import { ChevronDown, Loader2, Building2 } from "lucide-react"
import { useSectors } from "@/lib/hooks/use-sectors"
import { useLocale } from "@/components/providers/locale-provider"

interface SectorSelectProps {
  value: string
  onChange: (value: string) => void
  required?: boolean
  disabled?: boolean
  placeholder?: string
  placeholderAr?: string
  className?: string
}

export function SectorSelect({
  value,
  onChange,
  required = false,
  disabled = false,
  placeholder = "Select your sector",
  placeholderAr = "اختر القطاع",
  className = ""
}: SectorSelectProps) {
  const { sectors, isLoading, error } = useSectors()
  const { locale } = useLocale()
  const isArabic = locale === "ar"
  const [isOpen, setIsOpen] = useState(false)

  const selectedSector = sectors.find(s => s.code === value)

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
        <Building2 className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
        <span className={`flex-1 text-right pr-6 ${!selectedSector ? "text-gray-400" : ""}`}>
          {isLoading ? (
            <span className="flex items-center gap-2">
              <Loader2 className="w-4 h-4 animate-spin" />
              {isArabic ? "جاري التحميل..." : "Loading..."}
            </span>
          ) : selectedSector ? (
            <span className="flex items-center gap-2">
              {selectedSector.icon && <span>{selectedSector.icon}</span>}
              {isArabic ? selectedSector.nameAr : selectedSector.name}
            </span>
          ) : (
            isArabic ? placeholderAr : placeholder
          )}
        </span>
        <ChevronDown className={`w-5 h-5 text-gray-400 transition-transform ${isOpen ? "rotate-180" : ""}`} />
      </button>

      {isOpen && !isLoading && (
        <div className="absolute z-50 w-full mt-2 bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-lg max-h-60 overflow-auto">
          {sectors.map((sector) => (
            <button
              key={sector.id}
              type="button"
              onClick={() => {
                onChange(sector.code)
                setIsOpen(false)
              }}
              className={`w-full px-4 py-3 text-right hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors ${
                value === sector.code ? "bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400" : "text-gray-700 dark:text-gray-300"
              }`}
            >
              <div className="font-medium flex items-center gap-2">
                {sector.icon && <span>{sector.icon}</span>}
                {isArabic ? sector.nameAr : sector.name}
              </div>
              {sector.description && (
                <div className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                  {isArabic ? sector.descriptionAr : sector.description}
                </div>
              )}
            </button>
          ))}
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

export default SectorSelect
