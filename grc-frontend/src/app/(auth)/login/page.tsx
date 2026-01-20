"use client"

import { useEffect } from "react"
import { Loader2 } from "lucide-react"

export default function LoginPage() {
  useEffect(() => {
    // Get the API URL (ABP backend)
    const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5010'

    // Get the frontend URL for return after login
    const frontendUrl = process.env.NEXT_PUBLIC_FRONTEND_URL || 'http://localhost:3003'

    // Get return URL from query params or default to dashboard
    const params = new URLSearchParams(window.location.search)
    const returnPath = params.get('returnUrl') || '/dashboard'

    // Build full return URL (frontend URL + path)
    const fullReturnUrl = returnPath.startsWith('http')
      ? returnPath
      : `${frontendUrl}${returnPath}`

    // Redirect to ABP built-in login form
    const loginUrl = `${apiUrl}/Account/Login?returnUrl=${encodeURIComponent(fullReturnUrl)}`

    window.location.href = loginUrl
  }, [])

  // Show loading while redirecting
  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800 flex items-center justify-center">
      <div className="text-center">
        <Loader2 className="w-12 h-12 animate-spin text-emerald-600 mx-auto mb-4" />
        <p className="text-gray-600 dark:text-gray-400 text-lg">
          جاري التحويل لصفحة تسجيل الدخول...
        </p>
        <p className="text-gray-500 dark:text-gray-500 text-sm mt-2">
          Redirecting to login page...
        </p>
      </div>
    </div>
  )
}
