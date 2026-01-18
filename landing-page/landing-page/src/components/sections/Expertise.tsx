"use client"

import { useRef } from "react"
import { motion, useInView } from "framer-motion"
import { Network, Building2, ShieldCheck, ClipboardCheck, ArrowRight, Star } from "lucide-react"
import { useLocale, useTranslations } from "next-intl"
import { cn } from "@/lib/utils"

const expertiseItems = [
  {
    icon: Network,
    key: "telecom",
    color: "from-blue-500 to-cyan-500",
    href: "/services#telecommunications",
  },
  {
    icon: Building2,
    key: "datacenters",
    color: "from-purple-500 to-pink-500",
    href: "/services#data-centers",
  },
  {
    icon: ShieldCheck,
    key: "cybersecurity",
    color: "from-emerald-500 to-teal-500",
    href: "/services#cybersecurity",
  },
  {
    icon: ClipboardCheck,
    key: "governance",
    color: "from-orange-500 to-amber-500",
    href: "/services#governance",
  },
]

function ExpertiseCard({ item, index }: { item: typeof expertiseItems[0]; index: number }) {
  const cardRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(cardRef, { once: true, margin: "-100px" })
  const tItems = useTranslations('expertise.items')
  const locale = useLocale()

  return (
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
          item.color
        )}>
          <div className="absolute inset-[2px] rounded-[14px] bg-white dark:bg-gray-900" />
        </div>

        {/* Top gradient line */}
        <motion.div
          className={cn(
            "absolute top-0 left-0 right-0 h-1 bg-gradient-to-r rounded-t-2xl",
            item.color
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
            item.color
          )}>
            <item.icon className="w-7 h-7 text-white" />
          </div>

          {/* Title */}
          <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-3 group-hover:text-accent-600 dark:group-hover:text-accent-400 transition-colors">
            {tItems(`${item.key}.title`)}
          </h3>

          {/* Description */}
          <p className="text-gray-600 dark:text-gray-400 leading-relaxed mb-6">
            {tItems(`${item.key}.description`)}
          </p>

          {/* Link */}
          <a
            href={`/${locale}${item.href}`}
            className="inline-flex items-center gap-2 text-sm font-semibold text-accent-600 dark:text-accent-400 group/link"
          >
            {tItems('learn_more')}
            <ArrowRight className="w-4 h-4 transition-transform group-hover/link:translate-x-1" />
          </a>
        </div>

        {/* Background glow on hover */}
        <div className={cn(
          "absolute -bottom-20 -right-20 w-40 h-40 rounded-full blur-3xl",
          "opacity-0 group-hover:opacity-20 transition-opacity duration-500",
          "bg-gradient-to-br",
          item.color
        )} />
      </div>
    </motion.div>
  )
}

export function Expertise() {
  const sectionRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(sectionRef, { once: true, margin: "-100px" })
  const t = useTranslations('expertise')

  return (
    <section
      id="expertise"
      ref={sectionRef}
      className="py-24 md:py-32 bg-gray-50 dark:bg-gray-950"
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
            <Star className="w-4 h-4" />
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

        {/* Cards Grid */}
        <div className="grid md:grid-cols-2 gap-6 lg:gap-8">
          {expertiseItems.map((item, index) => (
            <ExpertiseCard key={item.key} item={item} index={index} />
          ))}
        </div>
      </div>
    </section>
  )
}
