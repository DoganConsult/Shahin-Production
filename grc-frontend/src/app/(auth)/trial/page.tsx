"use client"

import { useState } from "react"
import Link from "next/link"
import { motion } from "framer-motion"
import { ArrowRight, Check, Shield, Loader2 } from "lucide-react"
import { Button } from "@/components/ui/button"

const benefits = [
  "تجربة مجانية لمدة 14 يوم",
  "جميع الميزات متاحة",
  "بدون بطاقة ائتمان",
  "دعم فني باللغة العربية",
  "إعداد سريع خلال دقائق",
  "بيانات آمنة 100%",
]

export default function TrialPage() {
  const [isRedirecting, setIsRedirecting] = useState(false)

  const handleStartTrial = () => {
    setIsRedirecting(true)

    // Get the API URL (ABP backend)
    const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5010'

    // Get the frontend URL for return after registration
    const frontendUrl = process.env.NEXT_PUBLIC_FRONTEND_URL || 'http://localhost:3003'

    // Redirect to ABP's built-in registration page
    window.location.href = `${apiUrl}/Account/Register?returnUrl=${encodeURIComponent(frontendUrl + '/dashboard')}`
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800 flex">
      {/* Left Side - CTA */}
      <div className="flex-1 flex items-center justify-center p-8">
        <motion.div
          className="w-full max-w-md"
          initial={{ opacity: 0, x: -20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.5 }}
        >
          {/* Logo */}
          <Link href="/" className="flex items-center gap-3 mb-8">
            <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center shadow-lg">
              <span className="text-white font-bold text-2xl">ش</span>
            </div>
            <div>
              <span className="text-2xl font-bold text-gray-900 dark:text-white block">شاهين</span>
              <span className="text-sm text-gray-500 dark:text-gray-400">Shahin GRC</span>
            </div>
          </Link>

          {/* Title */}
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
            ابدأ تجربتك المجانية
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mb-8">
            أنشئ حسابك واستمتع بجميع الميزات لمدة 14 يوم مجاناً
          </p>

          {/* Main CTA */}
          <div className="space-y-4">
            <Button
              onClick={handleStartTrial}
              variant="gradient"
              size="xl"
              className="w-full"
              disabled={isRedirecting}
            >
              {isRedirecting ? (
                <>
                  <Loader2 className="w-5 h-5 animate-spin ml-2" />
                  جاري التحويل لصفحة التسجيل...
                </>
              ) : (
                <>
                  ابدأ التجربة المجانية
                  <ArrowRight className="w-5 h-5 mr-2" />
                </>
              )}
            </Button>

            {/* Trust badges */}
            <div className="flex items-center justify-center gap-6 text-sm text-gray-500 dark:text-gray-400">
              <span className="flex items-center gap-1">
                <Check className="w-4 h-4 text-emerald-500" />
                بدون بطاقة ائتمان
              </span>
              <span className="flex items-center gap-1">
                <Check className="w-4 h-4 text-emerald-500" />
                14 يوم مجاناً
              </span>
            </div>
          </div>

          {/* Login Link */}
          <p className="text-center text-gray-600 dark:text-gray-400 mt-8">
            لديك حساب بالفعل؟{" "}
            <Link href="/login" className="text-emerald-600 font-medium hover:underline">
              تسجيل الدخول
            </Link>
          </p>

          {/* Security Note */}
          <div className="mt-6 p-4 bg-emerald-50 dark:bg-emerald-900/20 rounded-xl border border-emerald-200 dark:border-emerald-800">
            <div className="flex items-center gap-3">
              <Shield className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
              <p className="text-sm text-emerald-700 dark:text-emerald-300">
                بياناتك محمية بتشفير SSL وتخزين آمن متوافق مع NCA
              </p>
            </div>
          </div>
        </motion.div>
      </div>

      {/* Right Side - Benefits */}
      <motion.div
        className="hidden lg:flex flex-1 bg-gradient-to-br from-emerald-600 to-teal-700 p-12 items-center justify-center"
        initial={{ opacity: 0, x: 20 }}
        animate={{ opacity: 1, x: 0 }}
        transition={{ duration: 0.5, delay: 0.2 }}
      >
        <div className="max-w-md text-white">
          <Shield className="w-16 h-16 mb-8 opacity-80" />
          <h2 className="text-3xl font-bold mb-6">
            منصة شاملة لإدارة الامتثال
          </h2>
          <p className="text-emerald-100 mb-8 text-lg">
            انضم لأكثر من 500 مؤسسة سعودية تثق في شاهين لإدارة الحوكمة والمخاطر والامتثال
          </p>

          <div className="space-y-4">
            {benefits.map((benefit, index) => (
              <motion.div
                key={benefit}
                className="flex items-center gap-3"
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.4 + index * 0.1 }}
              >
                <div className="w-6 h-6 rounded-full bg-white/20 flex items-center justify-center">
                  <Check className="w-4 h-4" />
                </div>
                <span>{benefit}</span>
              </motion.div>
            ))}
          </div>
        </div>
      </motion.div>
    </div>
  )
}
