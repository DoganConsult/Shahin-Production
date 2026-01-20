"use client"

import { useEffect } from "react"
import { motion } from "framer-motion"
import { Loader2 } from "lucide-react"

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5010'

export default function ForgotPasswordPage() {
  useEffect(() => {
    // Redirect to ABP's built-in forgot password page
    // Return to frontend login after password reset
    const frontendReturnUrl = `http://localhost:3003/login`
    window.location.href = `${API_BASE_URL}/Account/ForgotPassword?returnUrl=${encodeURIComponent(frontendReturnUrl)}`
  }, [])

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800 flex items-center justify-center p-8">
      <motion.div
        className="text-center"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
      >
        <Loader2 className="w-12 h-12 animate-spin text-emerald-600 mx-auto mb-4" />
        <p className="text-gray-600 dark:text-gray-400">
          جاري التحويل لصفحة إعادة تعيين كلمة المرور...
        </p>
      </motion.div>
    </div>
  )
}
