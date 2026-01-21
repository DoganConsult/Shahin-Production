"use client"

import { useState } from "react"
import Link from "next/link"
import { motion } from "framer-motion"
import { ArrowRight, Check, Shield, Mail, User, Building2, Phone, Lock, Eye, EyeOff } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"

const benefits = [
  "تجربة مجانية لمدة 14 يوم",
  "جميع الميزات متاحة",
  "بدون بطاقة ائتمان",
  "دعم فني باللغة العربية",
  "إعداد سريع خلال دقائق",
  "بيانات آمنة 100%",
]

export default function TrialPage() {
  const [showPassword, setShowPassword] = useState(false)
  const [isLoading, setIsLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 2000))
    setIsLoading(false)
    // Redirect to onboarding
    window.location.href = "/onboarding"
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800 flex">
      {/* Left Side - Form */}
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

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  الاسم الأول
                </label>
                <div className="relative">
                  <User className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                  <input
                    type="text"
                    required
                    className="w-full h-12 pr-10 pl-4 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-emerald-500"
                    placeholder="أحمد"
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  الاسم الأخير
                </label>
                <input
                  type="text"
                  required
                  className="w-full h-12 px-4 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-emerald-500"
                  placeholder="محمد"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                اسم الشركة
              </label>
              <div className="relative">
                <Building2 className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                  type="text"
                  required
                  className="w-full h-12 pr-10 pl-4 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-emerald-500"
                  placeholder="شركة المثال"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                البريد الإلكتروني
              </label>
              <div className="relative">
                <Mail className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                  type="email"
                  required
                  className="w-full h-12 pr-10 pl-4 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-emerald-500"
                  placeholder="ahmed@example.com"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                رقم الجوال
              </label>
              <div className="relative">
                <Phone className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                  type="tel"
                  required
                  className="w-full h-12 pr-10 pl-4 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-emerald-500"
                  placeholder="+966 50 000 0000"
                  dir="ltr"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                كلمة المرور
              </label>
              <div className="relative">
                <Lock className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                  type={showPassword ? "text" : "password"}
                  required
                  minLength={8}
                  className="w-full h-12 pr-10 pl-12 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-emerald-500"
                  placeholder="********"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                >
                  {showPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                </button>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <input
                type="checkbox"
                required
                className="w-5 h-5 mt-0.5 rounded border-gray-300 text-emerald-600 focus:ring-emerald-500"
              />
              <label className="text-sm text-gray-600 dark:text-gray-400">
                أوافق على{" "}
                <Link href="/terms" className="text-emerald-600 hover:underline">الشروط والأحكام</Link>
                {" "}و{" "}
                <Link href="/privacy" className="text-emerald-600 hover:underline">سياسة الخصوصية</Link>
              </label>
            </div>

            <Button
              type="submit"
              variant="gradient"
              size="xl"
              className="w-full"
              isLoading={isLoading}
            >
              ابدأ التجربة المجانية
              <ArrowRight className="w-5 h-5" />
            </Button>
          </form>

          {/* Login Link */}
          <p className="text-center text-gray-600 dark:text-gray-400 mt-6">
            لديك حساب بالفعل؟{" "}
            <Link href="/login" className="text-emerald-600 font-medium hover:underline">
              تسجيل الدخول
            </Link>
          </p>
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

          {/* Testimonial */}
          <Card className="mt-12 bg-white/10 border-white/20">
            <CardContent className="p-6">
              <p className="text-white/90 mb-4">
                "منصة شاهين ساعدتنا في تحقيق الامتثال الكامل مع متطلبات NCA في وقت قياسي"
              </p>
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center font-bold">
                  أ
                </div>
                <div>
                  <div className="font-medium">أحمد الغامدي</div>
                  <div className="text-sm text-emerald-200">مدير أمن المعلومات</div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </motion.div>
    </div>
  )
}
