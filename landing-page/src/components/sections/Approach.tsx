"use client"

import { useRef } from "react"
import { motion, useInView } from "framer-motion"
import { Settings, Search, FileCheck, Headphones } from "lucide-react"
import { useLocale, useTranslations } from "next-intl"
import { cn } from "@/lib/utils"

const stepIcons = [Search, Settings, FileCheck, Headphones]

interface Step {
  number: string
  title: string
  description: string
  icon: typeof Search
}

function StepCard({ step, index, totalSteps, isRTL }: { step: Step; index: number; totalSteps: number; isRTL: boolean }) {
  const cardRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(cardRef, { once: true, margin: "-50px" })

  return (
    <motion.div
      ref={cardRef}
      className="relative group"
      initial={{ opacity: 0, y: 30 }}
      animate={isInView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.5, delay: index * 0.15 }}
    >
      {/* Connector line (not for last item) */}
      {index < totalSteps - 1 && (
        <div className={cn(
          "hidden lg:block absolute top-8 w-[calc(100%-16px)] h-[2px]",
          isRTL ? "right-[calc(100%+8px)]" : "left-[calc(100%+8px)]"
        )}>
          <motion.div
            className={cn(
              "h-full",
              isRTL
                ? "bg-gradient-to-l from-accent-500/50 to-accent-500/10"
                : "bg-gradient-to-r from-accent-500/50 to-accent-500/10"
            )}
            initial={{ scaleX: 0 }}
            animate={isInView ? { scaleX: 1 } : {}}
            transition={{ duration: 0.8, delay: index * 0.15 + 0.3 }}
            style={{ transformOrigin: isRTL ? "right" : "left" }}
          />
        </div>
      )}

      {/* Card */}
      <div className={cn(
        "relative p-6 lg:p-8 rounded-2xl bg-white dark:bg-gray-900",
        "border border-gray-100 dark:border-gray-800",
        "transition-all duration-300",
        "hover:shadow-xl hover:shadow-accent-500/10 hover:border-accent-200 dark:hover:border-accent-800",
        "text-center lg:text-start"
      )}>
        {/* Number badge */}
        <motion.div
          className={cn(
            "w-16 h-16 rounded-2xl mx-auto lg:mx-0 mb-4",
            "bg-gradient-to-br from-accent-500 to-accent-400",
            "flex items-center justify-center",
            "shadow-lg shadow-accent-500/30",
            "transition-transform duration-300",
            "group-hover:scale-110 group-hover:rotate-[-5deg]"
          )}
          whileHover={{ rotate: -5, scale: 1.1 }}
        >
          <span className="text-xl font-extrabold text-primary-900">{step.number}</span>
        </motion.div>

        {/* Icon (hidden on mobile) */}
        <div className={cn(
          "hidden lg:flex absolute top-6 w-10 h-10 rounded-xl bg-gray-100 dark:bg-gray-800 items-center justify-center opacity-50 group-hover:opacity-100 transition-opacity",
          isRTL ? "left-6" : "right-6"
        )}>
          <step.icon className="w-5 h-5 text-accent-500" />
        </div>

        {/* Title */}
        <h4 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
          {step.title}
        </h4>

        {/* Description */}
        <p className="text-gray-600 dark:text-gray-400 text-sm leading-relaxed">
          {step.description}
        </p>
      </div>
    </motion.div>
  )
}

export function Approach() {
  const t = useTranslations('approach')
  const locale = useLocale()
  const isRTL = locale === 'ar'

  const sectionRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(sectionRef, { once: true, margin: "-100px" })

  const stepKeys = ['assess', 'design', 'assure', 'support'] as const
  const steps: Step[] = stepKeys.map((key, index) => ({
    number: `0${index + 1}`,
    title: t(`steps.${key}.title`),
    description: t(`steps.${key}.description`),
    icon: stepIcons[index],
  }))

  return (
    <section
      id="approach"
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
            <Settings className="w-4 h-4" />
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

        {/* Steps Grid */}
        <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6">
          {steps.map((step, index) => (
            <StepCard key={step.number} step={step} index={index} totalSteps={steps.length} isRTL={isRTL} />
          ))}
        </div>
      </div>
    </section>
  )
}
