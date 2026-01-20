"use client"

import { useState, useEffect } from "react"
import { ChevronDown, Loader2, Users } from "lucide-react"
import { useRoles } from "@/lib/hooks/use-roles"
import { useLocale } from "@/components/providers/locale-provider"

interface RoleSelectProps {
  value: string
  onChange: (value: string) => void
  required?: boolean
  disabled?: boolean
  placeholder?: string
  placeholderAr?: string
  className?: string
}

export function RoleSelect({
  value,
  onChange,
  required = false,
  disabled = false,
  placeholder = "Select your role",
  placeholderAr = "اختر دورك",
  className = ""
}: RoleSelectProps) {
  const { roles, isLoading, error } = useRoles()
  const { locale } = useLocale()
  const isArabic = locale === "ar"
  const [isOpen, setIsOpen] = useState(false)

  const selectedRole = roles.find(r => r.code === value)

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
        <Users className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
        <span className={`flex-1 text-right pr-6 ${!selectedRole ? "text-gray-400" : ""}`}>
          {isLoading ? (
            <span className="flex items-center gap-2">
              <Loader2 className="w-4 h-4 animate-spin" />
              {isArabic ? "جاري التحميل..." : "Loading..."}
            </span>
          ) : selectedRole ? (
            isArabic ? selectedRole.nameAr : selectedRole.name
          ) : (
            isArabic ? placeholderAr : placeholder
          )}
        </span>
        <ChevronDown className={`w-5 h-5 text-gray-400 transition-transform ${isOpen ? "rotate-180" : ""}`} />
      </button>

      {isOpen && !isLoading && (
        <div className="absolute z-50 w-full mt-2 bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-lg max-h-60 overflow-auto">
          {roles.map((role) => (
            <button
              key={role.id}
              type="button"
              onClick={() => {
                onChange(role.code)
                setIsOpen(false)
              }}
              className={`w-full px-4 py-3 text-right hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors ${
                value === role.code ? "bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400" : "text-gray-700 dark:text-gray-300"
              }`}
            >
              <div className="font-medium">
                {isArabic ? role.nameAr : role.name}
              </div>
              {role.description && (
                <div className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                  {isArabic ? role.descriptionAr : role.description}
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

export default RoleSelect
