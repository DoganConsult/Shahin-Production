"use client"

import { useRef } from "react"
import { motion, useInView } from "framer-motion"
import { Check, ArrowRight, ArrowLeft } from "lucide-react"
import { useLocale } from "next-intl"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"

export interface PricingPlan {
  name: string
  price: string
  period: string
  description: string
  features: string[]
  cta: string
  ctaLink?: string
  popular?: boolean
}

interface PricingCardProps {
  plan: PricingPlan
  index?: number
}

export function PricingCard({ plan, index = 0 }: PricingCardProps) {
  const locale = useLocale()
  const isRTL = locale === 'ar'
  const cardRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(cardRef, { once: true, margin: "-50px" })

  const ArrowIcon = isRTL ? ArrowLeft : ArrowRight
  const ctaLink = plan.ctaLink || (plan.cta === "Contact Sales" ? `/${locale}/contact` : `/${locale}/trial`)

  return (
    <motion.div
      ref={cardRef}
      className={cn(
        "relative p-8 rounded-2xl",
        plan.popular
          ? "bg-gradient-to-br from-accent-500 to-primary-500 text-white border-2 border-accent-400"
          : "bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800",
        "hover:shadow-xl transition-all duration-300"
      )}
      initial={{ opacity: 0, y: 40 }}
      animate={isInView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.6, delay: index * 0.15 }}
    >
      {plan.popular && (
        <div className="absolute -top-4 left-1/2 -translate-x-1/2 px-4 py-1 rounded-full bg-white dark:bg-gray-900 text-accent-600 dark:text-accent-400 text-sm font-semibold">
          {isRTL ? "الأكثر شعبية" : "Most Popular"}
        </div>
      )}

      <div className="mb-6">
        <h3 className={cn(
          "text-2xl font-bold mb-2",
          plan.popular ? "text-white" : "text-gray-900 dark:text-white"
        )}>
          {plan.name}
        </h3>
        <p className={cn(
          "text-sm mb-4",
          plan.popular ? "text-white/80" : "text-gray-600 dark:text-gray-400"
        )}>
          {plan.description}
        </p>
        <div className="flex items-baseline gap-2">
          <span className={cn(
            "text-4xl font-extrabold",
            plan.popular ? "text-white" : "text-gray-900 dark:text-white"
          )}>
            {plan.price}
          </span>
          <span className={cn(
            "text-sm",
            plan.popular ? "text-white/70" : "text-gray-500 dark:text-gray-400"
          )}>
            {plan.period}
          </span>
        </div>
      </div>

      <ul className="space-y-4 mb-8">
        {plan.features.map((feature, i) => (
          <li key={i} className="flex items-start gap-3">
            <Check className={cn(
              "w-5 h-5 flex-shrink-0 mt-0.5",
              plan.popular ? "text-white" : "text-success-500"
            )} />
            <span className={cn(
              "text-sm",
              plan.popular ? "text-white/90" : "text-gray-600 dark:text-gray-400"
            )}>
              {feature}
            </span>
          </li>
        ))}
      </ul>

      <Link href={ctaLink}>
        <Button
          variant={plan.popular ? "outline" : "gradient"}
          size="lg"
          className={cn(
            "w-full",
            plan.popular && "bg-white dark:bg-gray-900 text-accent-600 dark:text-accent-400 hover:bg-gray-50 dark:hover:bg-gray-800"
          )}
        >
          {plan.cta}
          {ArrowIcon && <ArrowIcon className="w-4 h-4" />}
        </Button>
      </Link>
    </motion.div>
  )
}
