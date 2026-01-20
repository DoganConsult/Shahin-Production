"use client"

import { motion } from "framer-motion"
import { Shield, Award, Sparkles } from "lucide-react"
import { useLocale } from "@/components/providers/locale-provider"

const certifications = [
  { key: "iso27001", icon: "🔒", name: "ISO 27001", nameAr: "آيزو 27001" },
  { key: "soc2", icon: "✓", name: "SOC 2 Type II", nameAr: "SOC 2 النوع الثاني" },
  { key: "ncaCertified", icon: "🛡️", name: "NCA Certified", nameAr: "معتمد من NCA" },
  { key: "samaApproved", icon: "🏦", name: "SAMA Approved", nameAr: "معتمد من ساما" },
]

const content = {
  badge: {
    en: "New Platform - Launching in Saudi Arabia",
    ar: "منصة جديدة - الإطلاق في المملكة العربية السعودية"
  },
  description: {
    en: "Built with enterprise-grade security and designed to meet the highest compliance standards from day one.",
    ar: "مبنية بأمان على مستوى المؤسسات ومصممة لتلبية أعلى معايير الامتثال منذ اليوم الأول."
  },
  trustIndicators: {
    enterprise: { en: "Enterprise Security", ar: "أمان المؤسسات" },
    saudiMarket: { en: "Saudi Market Focus", ar: "التركيز على السوق السعودي" },
    arabicSupport: { en: "Arabic Language Support", ar: "دعم اللغة العربية" }
  }
}

export function TrustStrip() {
  const { locale } = useLocale()
  const isArabic = locale === "ar"

  return (
    <section className="py-16 border-y border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-900/50">
      <div className="container mx-auto px-6">
        {/* New Platform Badge */}
        <motion.div
          className="text-center mb-10"
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
        >
          <div className="inline-flex items-center gap-3 px-6 py-3 bg-gradient-to-r from-emerald-500/10 to-teal-500/10 border border-emerald-200 dark:border-emerald-800 rounded-full mb-4">
            <Sparkles className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
            <span className="text-emerald-700 dark:text-emerald-300 font-semibold">
              {isArabic ? content.badge.ar : content.badge.en}
            </span>
            <Sparkles className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
          </div>
          <p className="text-sm text-gray-500 dark:text-gray-400 max-w-lg mx-auto">
            {isArabic ? content.description.ar : content.description.en}
          </p>
        </motion.div>

        {/* Security & Compliance Standards */}
        <motion.div
          className="flex flex-wrap items-center justify-center gap-4"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.6 }}
        >
          {certifications.map((cert) => (
            <div
              key={cert.key}
              className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-full shadow-sm hover:shadow-md transition-shadow"
            >
              <span>{cert.icon}</span>
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                {isArabic ? cert.nameAr : cert.name}
              </span>
            </div>
          ))}
        </motion.div>

        {/* Trust Indicators */}
        <motion.div
          className="flex flex-wrap justify-center gap-8 mt-10"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ delay: 0.2 }}
        >
          <div className="flex items-center gap-2 text-gray-600 dark:text-gray-400">
            <Shield className="w-5 h-5 text-emerald-600" />
            <span className="text-sm">{isArabic ? content.trustIndicators.enterprise.ar : content.trustIndicators.enterprise.en}</span>
          </div>
          <div className="flex items-center gap-2 text-gray-600 dark:text-gray-400">
            <Award className="w-5 h-5 text-emerald-600" />
            <span className="text-sm">{isArabic ? content.trustIndicators.saudiMarket.ar : content.trustIndicators.saudiMarket.en}</span>
          </div>
          <div className="flex items-center gap-2 text-gray-600 dark:text-gray-400">
            <Shield className="w-5 h-5 text-emerald-600" />
            <span className="text-sm">{isArabic ? content.trustIndicators.arabicSupport.ar : content.trustIndicators.arabicSupport.en}</span>
          </div>
        </motion.div>
      </div>
    </section>
  )
}
