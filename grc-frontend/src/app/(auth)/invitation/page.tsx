"use client"

import { useEffect, Suspense } from "react"
import { useSearchParams } from "next/navigation"
import { motion } from "framer-motion"
import { Loader2, AlertCircle } from "lucide-react"
import Link from "next/link"
import { Button } from "@/components/ui/button"

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5010'

function InvitationContent() {
  const searchParams = useSearchParams()
  const token = searchParams.get('token')

  useEffect(() => {
    if (token) {
      // Redirect to ABP's invitation accept page with token
      window.location.href = `${API_BASE_URL}/Account/AcceptInvitation?token=${encodeURIComponent(token)}`
    }
  }, [token])

  // No token - show error
  if (!token) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800 flex items-center justify-center p-8">
        <motion.div
          className="w-full max-w-md text-center"
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
        >
          <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-700 p-8">
            <div className="w-16 h-16 rounded-full bg-red-100 dark:bg-red-900/30 flex items-center justify-center mx-auto mb-6">
              <AlertCircle className="w-8 h-8 text-red-600" />
            </div>
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
              رابط غير صالح
            </h1>
            <p className="text-gray-600 dark:text-gray-400 mb-6">
              رابط الدعوة غير صالح أو منتهي الصلاحية. يرجى التواصل مع مسؤول النظام للحصول على رابط جديد.
            </p>
            <Link href="/">
              <Button variant="outline" className="w-full">
                العودة للصفحة الرئيسية
              </Button>
            </Link>
          </div>
        </motion.div>
      </div>
    )
  }

  // Show loading while redirecting
  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800 flex items-center justify-center p-8">
      <motion.div
        className="text-center"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
      >
        <Loader2 className="w-12 h-12 animate-spin text-emerald-600 mx-auto mb-4" />
        <p className="text-gray-600 dark:text-gray-400">
          جاري التحويل لصفحة قبول الدعوة...
        </p>
      </motion.div>
    </div>
  )
}

function LoadingFallback() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800 flex items-center justify-center p-8">
      <div className="text-center">
        <Loader2 className="w-12 h-12 animate-spin text-emerald-600 mx-auto mb-4" />
        <p className="text-gray-600 dark:text-gray-400">
          جاري التحميل...
        </p>
      </div>
    </div>
  )
}

export default function InvitationPage() {
  return (
    <Suspense fallback={<LoadingFallback />}>
      <InvitationContent />
    </Suspense>
  )
}
