"use client"

import { useRef } from "react"
import { motion, useInView } from "framer-motion"
import { Radio, Server, Shield, Scale, CheckCircle2, ArrowRight, ArrowLeft, Phone } from "lucide-react"
import Link from "next/link"
import { useLocale, useTranslations } from "next-intl"
import { notFound } from "next/navigation"
import { Navigation } from "@/components/sections/Navigation"
import { Footer } from "@/components/sections/Footer"
import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"

const serviceConfig = {
  'telecommunications': {
    icon: Radio,
    color: 'accent',
  },
  'data-centers': {
    icon: Server,
    color: 'primary',
  },
  'cybersecurity': {
    icon: Shield,
    color: 'success',
  },
  'governance': {
    icon: Scale,
    color: 'accent',
  },
}

type ServiceSlug = keyof typeof serviceConfig

const slugToKey: Record<string, string> = {
  'telecommunications': 'telecommunications',
  'data-centers': 'dataCenters',
  'cybersecurity': 'cybersecurity',
  'governance': 'governance',
}

export default function ServiceDetailPage({ params }: { params: { slug: string; locale: string } }) {
  const t = useTranslations('serviceDetail')
  const locale = useLocale()
  const isRTL = locale === 'ar'

  const slug = params.slug as ServiceSlug
  const translationKey = slugToKey[slug]

  if (!serviceConfig[slug] || !translationKey) {
    notFound()
  }

  const config = serviceConfig[slug]
  const Icon = config.icon
  const ArrowIcon = isRTL ? ArrowLeft : ArrowRight

  const featuresRef = useRef<HTMLDivElement>(null)
  const benefitsRef = useRef<HTMLDivElement>(null)
  const isFeaturesInView = useInView(featuresRef, { once: true, margin: "-100px" })
  const isBenefitsInView = useInView(benefitsRef, { once: true, margin: "-100px" })

  const features = t.raw(`${translationKey}.features`) as string[]
  const benefits = t.raw(`${translationKey}.benefits`) as string[]

  return (
    <main className="min-h-screen" dir={isRTL ? 'rtl' : 'ltr'}>
      <Navigation />

      {/* Hero Section */}
      <section className="relative pt-32 pb-20 overflow-hidden">
        <div className="absolute inset-0 hero-gradient" />
        <div className="absolute inset-0 grid-pattern" />

        <div className="container mx-auto px-6 relative z-10">
          <motion.div
            className="max-w-3xl"
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
          >
            <div className={cn(
              "w-20 h-20 rounded-2xl mb-8",
              "bg-gradient-to-br from-accent-500 to-accent-400",
              "flex items-center justify-center"
            )}>
              <Icon className="w-10 h-10 text-white" />
            </div>

            <h1 className="text-4xl md:text-5xl lg:text-6xl font-extrabold text-white mb-6">
              {t(`${translationKey}.title`)}
            </h1>
            <p className="text-lg text-white/80 mb-8">
              {t(`${translationKey}.description`)}
            </p>

            <Link href={`/${locale}/contact`}>
              <Button variant="gradient" size="xl" className="group">
                <Phone className="w-5 h-5" />
                {t('cta.consult')}
                <ArrowIcon className={cn(
                  "w-5 h-5 transition-transform",
                  isRTL ? "group-hover:-translate-x-1" : "group-hover:translate-x-1"
                )} />
              </Button>
            </Link>
          </motion.div>
        </div>
      </section>

      {/* Overview Section */}
      <section className="py-24 bg-white dark:bg-gray-900">
        <div className="container mx-auto px-6">
          <div className="grid lg:grid-cols-2 gap-16 items-center">
            <motion.div
              initial={{ opacity: 0, x: isRTL ? 30 : -30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
            >
              <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-6">
                {t(`${translationKey}.overview.title`)}
              </h2>
              <div className="space-y-4 text-gray-600 dark:text-gray-400 leading-relaxed">
                <p>{t(`${translationKey}.overview.p1`)}</p>
                <p>{t(`${translationKey}.overview.p2`)}</p>
              </div>
            </motion.div>

            <motion.div
              className={cn(
                "aspect-video rounded-2xl overflow-hidden",
                "bg-gradient-to-br from-accent-100 to-primary-100 dark:from-accent-900/20 dark:to-primary-900/20",
                "flex items-center justify-center"
              )}
              initial={{ opacity: 0, x: isRTL ? -30 : 30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6, delay: 0.2 }}
            >
              <Icon className="w-24 h-24 text-accent-500/30" />
            </motion.div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section ref={featuresRef} className="py-24 bg-gray-50 dark:bg-gray-950">
        <div className="container mx-auto px-6">
          <motion.div
            className="text-center mb-16"
            initial={{ opacity: 0, y: 30 }}
            animate={isFeaturesInView ? { opacity: 1, y: 0 } : {}}
            transition={{ duration: 0.6 }}
          >
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
              {t('sections.features')}
            </h2>
          </motion.div>

          <div className="grid md:grid-cols-2 gap-4 max-w-4xl mx-auto">
            {features.map((feature, index) => (
              <motion.div
                key={index}
                className={cn(
                  "flex items-center gap-4 p-5 rounded-xl",
                  "bg-white dark:bg-gray-900 border border-gray-100 dark:border-gray-800",
                  "hover:border-accent-300 dark:hover:border-accent-700",
                  "transition-all duration-300"
                )}
                initial={{ opacity: 0, y: 20 }}
                animate={isFeaturesInView ? { opacity: 1, y: 0 } : {}}
                transition={{ duration: 0.4, delay: index * 0.08 }}
              >
                <CheckCircle2 className="w-6 h-6 text-accent-500 flex-shrink-0" />
                <p className="text-gray-700 dark:text-gray-300 font-medium">{feature}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Benefits Section */}
      <section ref={benefitsRef} className="py-24 bg-white dark:bg-gray-900">
        <div className="container mx-auto px-6">
          <motion.div
            className="text-center mb-16"
            initial={{ opacity: 0, y: 30 }}
            animate={isBenefitsInView ? { opacity: 1, y: 0 } : {}}
            transition={{ duration: 0.6 }}
          >
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
              {t('sections.benefits')}
            </h2>
          </motion.div>

          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6 max-w-5xl mx-auto">
            {benefits.map((benefit, index) => (
              <motion.div
                key={index}
                className={cn(
                  "p-6 rounded-xl text-center",
                  "bg-gray-50 dark:bg-gray-800",
                  "border border-gray-200 dark:border-gray-700",
                  "hover:border-accent-300 dark:hover:border-accent-700",
                  "transition-all duration-300"
                )}
                initial={{ opacity: 0, scale: 0.95 }}
                animate={isBenefitsInView ? { opacity: 1, scale: 1 } : {}}
                transition={{ duration: 0.4, delay: index * 0.1 }}
              >
                <div className="w-12 h-12 rounded-full bg-accent-100 dark:bg-accent-900/30 flex items-center justify-center mx-auto mb-4">
                  <CheckCircle2 className="w-6 h-6 text-accent-500" />
                </div>
                <p className="text-gray-700 dark:text-gray-300 font-medium">{benefit}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-24 relative overflow-hidden">
        <div className="absolute inset-0 hero-gradient" />
        <div className="absolute inset-0 grid-pattern" />

        <div className="container mx-auto px-6 relative z-10">
          <motion.div
            className="text-center max-w-2xl mx-auto"
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            <h2 className="text-3xl md:text-4xl font-bold text-white mb-6">
              {t('cta.title')}
            </h2>
            <p className="text-white/80 mb-8">
              {t('cta.description')}
            </p>
            <Link href={`/${locale}/contact`}>
              <Button variant="gradient" size="xl">
                {t('cta.button')}
                <ArrowIcon className="w-5 h-5" />
              </Button>
            </Link>
          </motion.div>
        </div>
      </section>

      <Footer />
    </main>
  )
}
