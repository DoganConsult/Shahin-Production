"use client"

import { useRef } from "react"
import { motion, useInView } from "framer-motion"
import { Landmark, Radio, Zap, Building2, Building } from "lucide-react"
import { useLocale, useTranslations } from "next-intl"
import { cn } from "@/lib/utils"

const sectorIcons = [Landmark, Radio, Zap, Building2]

interface Sector {
  icon: typeof Landmark
  title: string
  description: string
}

function SectorCard({ sector, index, isRTL }: { sector: Sector; index: number; isRTL: boolean }) {
  const cardRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(cardRef, { once: true, margin: "-50px" })

  return (
    <motion.div
      ref={cardRef}
      className={cn(
        "group flex items-start gap-5 p-6 rounded-2xl",
        "bg-white dark:bg-gray-900 border border-gray-100 dark:border-gray-800",
        "transition-all duration-300",
        "hover:border-accent-300 dark:hover:border-accent-700",
        "hover:shadow-lg hover:shadow-accent-500/10",
        isRTL ? "hover:-translate-x-2" : "hover:translate-x-2"
      )}
      initial={{ opacity: 0, x: isRTL ? 30 : -30 }}
      animate={isInView ? { opacity: 1, x: 0 } : {}}
      transition={{ duration: 0.5, delay: index * 0.1 }}
    >
      {/* Icon */}
      <div className={cn(
        "w-12 h-12 rounded-xl flex-shrink-0",
        "bg-gradient-to-br from-accent-100 to-accent-50 dark:from-accent-900/30 dark:to-accent-800/20",
        "flex items-center justify-center",
        "group-hover:from-accent-500 group-hover:to-accent-400",
        "transition-all duration-300"
      )}>
        <sector.icon className={cn(
          "w-6 h-6 text-accent-600 dark:text-accent-400",
          "group-hover:text-white transition-colors duration-300"
        )} />
      </div>

      {/* Content */}
      <div>
        <h4 className="text-lg font-bold text-gray-900 dark:text-white mb-1 group-hover:text-accent-600 dark:group-hover:text-accent-400 transition-colors">
          {sector.title}
        </h4>
        <p className="text-sm text-gray-600 dark:text-gray-400 leading-relaxed">
          {sector.description}
        </p>
      </div>
    </motion.div>
  )
}

export function Sectors() {
  const t = useTranslations('sectors')
  const locale = useLocale()
  const isRTL = locale === 'ar'

  const sectionRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(sectionRef, { once: true, margin: "-100px" })

  const sectorKeys = ['government', 'telecom', 'infrastructure', 'enterprise'] as const
  const sectors: Sector[] = sectorKeys.map((key, index) => ({
    icon: sectorIcons[index],
    title: t(`items.${key}.title`),
    description: t(`items.${key}.description`),
  }))

  return (
    <section
      id="sectors"
      ref={sectionRef}
      className="py-24 md:py-32 bg-gray-50 dark:bg-gray-950"
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
            <Building className="w-4 h-4" />
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

        {/* Sectors Grid */}
        <div className="grid md:grid-cols-2 gap-4 lg:gap-6">
          {sectors.map((sector, index) => (
            <SectorCard key={index} sector={sector} index={index} isRTL={isRTL} />
          ))}
        </div>
      </div>
    </section>
  )
}
