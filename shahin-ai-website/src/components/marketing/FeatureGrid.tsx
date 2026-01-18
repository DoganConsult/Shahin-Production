"use client"

import { useRef } from "react"
import { motion, useInView } from "framer-motion"
import { LucideIcon, ArrowRight, ArrowLeft } from "lucide-react"
import { useLocale, useTranslations } from "next-intl"
import Link from "next/link"
import { cn } from "@/lib/utils"

export interface FeatureItem {
  icon: LucideIcon
  title: string
  description: string
  href?: string
  color?: string
}

interface FeatureGridProps {
  features?: FeatureItem[]
  title?: string
  subtitle?: string
  badge?: string
  className?: string
}

export function FeatureGrid({ features, title, subtitle, badge, className }: FeatureGridProps) {
  const sectionRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(sectionRef, { once: true, margin: "-100px" })
  const t = useTranslations('expertise')
  const locale = useLocale()
  const isRTL = locale === 'ar'

  // Use provided features or fallback to translations
  const items = features || []

  if (items.length === 0) {
    return null
  }

  const sectionTitle = title || t('title')
  const sectionSubtitle = subtitle || t('subtitle')
  const sectionBadge = badge || t('badge')

  return (
    <section
      ref={sectionRef}
      className={cn("py-24 md:py-32 bg-gray-50 dark:bg-gray-950", className)}
    >
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-16"
          initial={{ opacity: 0, y: 30 }}
          animate={isInView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.6 }}
        >
          {sectionBadge && (
            <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-accent-100 dark:bg-accent-900/30 text-accent-700 dark:text-accent-400 text-sm font-semibold mb-4">
              {sectionBadge}
            </span>
          )}
          {sectionTitle && (
            <h2 className="text-3xl md:text-4xl lg:text-5xl font-extrabold text-gray-900 dark:text-white mb-4">
              {sectionTitle}
            </h2>
          )}
          {sectionSubtitle && (
            <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
              {sectionSubtitle}
            </p>
          )}
        </motion.div>

        {/* Features Grid */}
        <div className="grid md:grid-cols-2 gap-6 lg:gap-8">
          {items.map((item, index) => (
            <FeatureCard key={index} item={item} index={index} isRTL={isRTL} locale={locale} />
          ))}
        </div>
      </div>
    </section>
  )
}

function FeatureCard({
  item,
  index,
  isRTL,
  locale
}: {
  item: FeatureItem
  index: number
  isRTL: boolean
  locale: string
}) {
  const cardRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(cardRef, { once: true, margin: "-100px" })
  const tItems = useTranslations('expertise.items')
  const ArrowIcon = isRTL ? ArrowLeft : ArrowRight
  const colorClass = item.color || "from-blue-500 to-cyan-500"

  const cardContent = (
    <motion.div
      ref={cardRef}
      className="group relative"
      initial={{ opacity: 0, y: 40 }}
      animate={isInView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.6, delay: index * 0.1 }}
    >
      {/* Card */}
      <div className={cn(
        "relative p-8 rounded-2xl bg-white dark:bg-gray-900 border border-gray-100 dark:border-gray-800",
        "transition-all duration-500 ease-out",
        "hover:shadow-2xl hover:shadow-gray-200/50 dark:hover:shadow-gray-900/50",
        "hover:-translate-y-2 hover:border-transparent",
        "overflow-hidden"
      )}>
        {/* Gradient border on hover */}
        <div className={cn(
          "absolute inset-0 rounded-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-500",
          "bg-gradient-to-r p-[2px]",
          colorClass
        )}>
          <div className="absolute inset-[2px] rounded-[14px] bg-white dark:bg-gray-900" />
        </div>

        {/* Top gradient line */}
        <motion.div
          className={cn(
            "absolute top-0 left-0 right-0 h-1 bg-gradient-to-r rounded-t-2xl",
            colorClass
          )}
          initial={{ scaleX: 0 }}
          whileInView={{ scaleX: 1 }}
          transition={{ duration: 0.6, delay: index * 0.1 + 0.3 }}
          style={{ transformOrigin: "left" }}
        />

        {/* Content */}
        <div className="relative z-10">
          {/* Icon */}
          <div className={cn(
            "w-14 h-14 rounded-xl flex items-center justify-center mb-6",
            "bg-gradient-to-br transition-all duration-300",
            "group-hover:scale-110 group-hover:rotate-3",
            colorClass
          )}>
            <item.icon className="w-7 h-7 text-white" />
          </div>

          {/* Title */}
          <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-3 group-hover:text-accent-600 dark:group-hover:text-accent-400 transition-colors">
            {item.title}
          </h3>

          {/* Description */}
          <p className="text-gray-600 dark:text-gray-400 leading-relaxed mb-6">
            {item.description}
          </p>

          {/* Link */}
          {item.href && (
            <a
              href={`/${locale}${item.href}`}
              className="inline-flex items-center gap-2 text-sm font-semibold text-accent-600 dark:text-accent-400 group/link"
            >
              {tItems('learn_more') || 'Learn more'}
              <ArrowIcon className="w-4 h-4 transition-transform group-hover/link:translate-x-1" />
            </a>
          )}
        </div>

        {/* Background glow on hover */}
        <div className={cn(
          "absolute -bottom-20 -right-20 w-40 h-40 rounded-full blur-3xl",
          "opacity-0 group-hover:opacity-20 transition-opacity duration-500",
          "bg-gradient-to-br",
          colorClass
        )} />
      </div>
    </motion.div>
  )

  if (item.href) {
    return (
      <Link href={`/${locale}${item.href}`} className="block">
        {cardContent}
      </Link>
    )
  }

  return cardContent
}
