"use client"

import { useRef } from "react"
import { motion, useInView } from "framer-motion"
import { CheckCircle2, Award } from "lucide-react"
import { useLocale, useTranslations } from "next-intl"
import { cn } from "@/lib/utils"

function ReasonItem({ reason, index }: { reason: string; index: number }) {
  const itemRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(itemRef, { once: true, margin: "-50px" })

  return (
    <motion.div
      ref={itemRef}
      className={cn(
        "flex items-center gap-4 p-5 rounded-xl",
        "bg-white dark:bg-gray-900 border border-gray-100 dark:border-gray-800",
        "transition-all duration-300",
        "hover:border-success-300 dark:hover:border-success-700",
        "hover:bg-gradient-to-r hover:from-success-50/50 hover:to-transparent",
        "dark:hover:from-success-900/20 dark:hover:to-transparent"
      )}
      initial={{ opacity: 0, y: 20 }}
      animate={isInView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.4, delay: index * 0.08 }}
    >
      <CheckCircle2 className="w-6 h-6 text-success-500 flex-shrink-0" />
      <p className="text-gray-700 dark:text-gray-300 font-medium">{reason}</p>
    </motion.div>
  )
}

export function WhyUs() {
  const t = useTranslations('whyus')
  const locale = useLocale()
  const isRTL = locale === 'ar'

  const sectionRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(sectionRef, { once: true, margin: "-100px" })

  const reasons = t.raw('reasons') as string[]

  return (
    <section
      ref={sectionRef}
      className="py-24 md:py-32 bg-white dark:bg-gray-900"
      dir={isRTL ? 'rtl' : 'ltr'}
    >
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-16"
          initial={{ opacity: 0, y: 30 }}
          animate={isInView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.6 }}
        >
          {/* Badge */}
          <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-accent-100 dark:bg-accent-900/30 text-accent-700 dark:text-accent-400 text-sm font-semibold mb-4">
            <Award className="w-4 h-4" />
            {t('badge')}
          </span>

          {/* Title */}
          <h2 className="text-3xl md:text-4xl lg:text-5xl font-extrabold text-gray-900 dark:text-white mb-4">
            {t('title')}
          </h2>

          {/* Subtitle */}
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
            {t('subtitle')}
          </p>
        </motion.div>

        {/* Reasons Grid */}
        <div className="grid md:grid-cols-2 gap-4 max-w-4xl mx-auto">
          {reasons.map((reason, index) => (
            <ReasonItem key={index} reason={reason} index={index} />
          ))}
        </div>
      </div>
    </section>
  )
}
