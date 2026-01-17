"use client"

import { useRef } from "react"
import { motion, useInView } from "framer-motion"
import { Radio, Server, Shield, Scale, ArrowRight, ArrowLeft, CheckCircle2, Briefcase } from "lucide-react"
import Link from "next/link"
import { useLocale, useTranslations } from "next-intl"
import { Navigation } from "@/components/sections/Navigation"
import { Footer } from "@/components/sections/Footer"
import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"

const serviceIcons = {
  telecommunications: Radio,
  dataCenters: Server,
  cybersecurity: Shield,
  governance: Scale,
}

interface ServiceCardProps {
  slug: string
  icon: React.ElementType
  title: string
  description: string
  features: string[]
  index: number
  isRTL: boolean
  locale: string
}

function ServiceCard({ slug, icon: Icon, title, description, features, index, isRTL, locale }: ServiceCardProps) {
  const cardRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(cardRef, { once: true, margin: "-50px" })

  const ArrowIcon = isRTL ? ArrowLeft : ArrowRight

  return (
    <motion.div
      ref={cardRef}
      className={cn(
        "group p-8 rounded-2xl",
        "bg-white dark:bg-gray-900",
        "border border-gray-200 dark:border-gray-800",
        "hover:border-accent-300 dark:hover:border-accent-700",
        "hover:shadow-xl hover:shadow-accent-500/10",
        "transition-all duration-500"
      )}
      initial={{ opacity: 0, y: 40 }}
      animate={isInView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.6, delay: index * 0.15 }}
    >
      {/* Icon */}
      <div className={cn(
        "w-16 h-16 rounded-2xl mb-6",
        "bg-gradient-to-br from-accent-100 to-accent-50 dark:from-accent-900/30 dark:to-accent-800/20",
        "flex items-center justify-center",
        "group-hover:from-accent-500 group-hover:to-accent-400",
        "transition-all duration-500"
      )}>
        <Icon className={cn(
          "w-8 h-8 text-accent-600 dark:text-accent-400",
          "group-hover:text-white transition-colors duration-500"
        )} />
      </div>

      {/* Content */}
      <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-4 group-hover:text-accent-600 dark:group-hover:text-accent-400 transition-colors">
        {title}
      </h3>
      <p className="text-gray-600 dark:text-gray-400 mb-6 leading-relaxed">
        {description}
      </p>

      {/* Features */}
      <ul className="space-y-3 mb-8">
        {features.map((feature, i) => (
          <li key={i} className="flex items-start gap-3">
            <CheckCircle2 className="w-5 h-5 text-success-500 flex-shrink-0 mt-0.5" />
            <span className="text-sm text-gray-600 dark:text-gray-400">{feature}</span>
          </li>
        ))}
      </ul>

      {/* CTA */}
      <Link href={`/${locale}/services/${slug}`}>
        <Button variant="outline" className="group/btn w-full">
          <span>{isRTL ? 'اكتشف المزيد' : 'Learn More'}</span>
          <ArrowIcon className={cn(
            "w-4 h-4 transition-transform",
            isRTL ? "group-hover/btn:-translate-x-1" : "group-hover/btn:translate-x-1"
          )} />
        </Button>
      </Link>
    </motion.div>
  )
}

export default function ServicesPage() {
  const t = useTranslations('services')
  const locale = useLocale()
  const isRTL = locale === 'ar'

  const sectionRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(sectionRef, { once: true, margin: "-100px" })

  const serviceKeys = ['telecommunications', 'dataCenters', 'cybersecurity', 'governance'] as const

  const services = serviceKeys.map(key => ({
    slug: key === 'dataCenters' ? 'data-centers' : key,
    icon: serviceIcons[key],
    title: t(`items.${key}.title`),
    description: t(`items.${key}.description`),
    features: t.raw(`items.${key}.features`) as string[],
  }))

  return (
    <main className="min-h-screen" dir={isRTL ? 'rtl' : 'ltr'}>
      <Navigation />

      {/* Hero Section */}
      <section className="relative pt-32 pb-20 overflow-hidden">
        <div className="absolute inset-0 hero-gradient" />
        <div className="absolute inset-0 grid-pattern" />

        <div className="container mx-auto px-6 relative z-10">
          <motion.div
            className="text-center max-w-3xl mx-auto"
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
          >
            <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-accent-500/10 border border-accent-500/30 text-accent-400 text-sm font-semibold mb-6">
              <Briefcase className="w-4 h-4" />
              {t('badge')}
            </span>
            <h1 className="text-4xl md:text-5xl lg:text-6xl font-extrabold text-white mb-6">
              {t('title')}
            </h1>
            <p className="text-lg text-white/80">
              {t('subtitle')}
            </p>
          </motion.div>
        </div>
      </section>

      {/* Services Grid */}
      <section ref={sectionRef} className="py-24 bg-gray-50 dark:bg-gray-950">
        <div className="container mx-auto px-6">
          <motion.div
            className="text-center mb-16"
            initial={{ opacity: 0, y: 30 }}
            animate={isInView ? { opacity: 1, y: 0 } : {}}
            transition={{ duration: 0.6 }}
          >
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
              {t('grid.title')}
            </h2>
            <p className="text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
              {t('grid.subtitle')}
            </p>
          </motion.div>

          <div className="grid md:grid-cols-2 gap-8">
            {services.map((service, index) => (
              <ServiceCard
                key={service.slug}
                {...service}
                index={index}
                isRTL={isRTL}
                locale={locale}
              />
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-24 bg-white dark:bg-gray-900">
        <div className="container mx-auto px-6">
          <motion.div
            className="text-center max-w-2xl mx-auto"
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-6">
              {t('cta.title')}
            </h2>
            <p className="text-gray-600 dark:text-gray-400 mb-8">
              {t('cta.description')}
            </p>
            <Link href={`/${locale}/contact`}>
              <Button variant="gradient" size="xl">
                {t('cta.button')}
                {isRTL ? <ArrowLeft className="w-5 h-5" /> : <ArrowRight className="w-5 h-5" />}
              </Button>
            </Link>
          </motion.div>
        </div>
      </section>

      <Footer />
    </main>
  )
}
