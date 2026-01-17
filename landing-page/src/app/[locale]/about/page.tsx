"use client"

import { useRef } from "react"
import { motion, useInView } from "framer-motion"
import { Target, Eye, Award, Users, Globe, CheckCircle2, Building2 } from "lucide-react"
import { useLocale, useTranslations } from "next-intl"
import { Navigation } from "@/components/sections/Navigation"
import { Footer } from "@/components/sections/Footer"
import { cn } from "@/lib/utils"

function ValueCard({ icon: Icon, title, description, index }: {
  icon: React.ElementType
  title: string
  description: string
  index: number
}) {
  const cardRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(cardRef, { once: true, margin: "-50px" })

  return (
    <motion.div
      ref={cardRef}
      className={cn(
        "p-6 rounded-2xl",
        "bg-white dark:bg-gray-800",
        "border border-gray-200 dark:border-gray-700",
        "hover:border-accent-300 dark:hover:border-accent-700",
        "hover:shadow-lg hover:shadow-accent-500/10",
        "transition-all duration-300"
      )}
      initial={{ opacity: 0, y: 30 }}
      animate={isInView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.5, delay: index * 0.1 }}
    >
      <div className={cn(
        "w-12 h-12 rounded-xl mb-4",
        "bg-gradient-to-br from-accent-100 to-accent-50 dark:from-accent-900/30 dark:to-accent-800/20",
        "flex items-center justify-center"
      )}>
        <Icon className="w-6 h-6 text-accent-600 dark:text-accent-400" />
      </div>
      <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-2">{title}</h3>
      <p className="text-gray-600 dark:text-gray-400 text-sm leading-relaxed">{description}</p>
    </motion.div>
  )
}

function StatCard({ value, label, index }: { value: string; label: string; index: number }) {
  const cardRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(cardRef, { once: true, margin: "-50px" })

  return (
    <motion.div
      ref={cardRef}
      className="text-center"
      initial={{ opacity: 0, scale: 0.9 }}
      animate={isInView ? { opacity: 1, scale: 1 } : {}}
      transition={{ duration: 0.5, delay: index * 0.1 }}
    >
      <div className="text-4xl md:text-5xl font-extrabold text-gradient mb-2">{value}</div>
      <div className="text-gray-600 dark:text-gray-400">{label}</div>
    </motion.div>
  )
}

export default function AboutPage() {
  const t = useTranslations('about')
  const locale = useLocale()
  const isRTL = locale === 'ar'

  const missionRef = useRef<HTMLDivElement>(null)
  const valuesRef = useRef<HTMLDivElement>(null)
  const statsRef = useRef<HTMLDivElement>(null)

  const isMissionInView = useInView(missionRef, { once: true, margin: "-100px" })
  const isValuesInView = useInView(valuesRef, { once: true, margin: "-100px" })
  const isStatsInView = useInView(statsRef, { once: true, margin: "-100px" })

  const values = [
    { icon: Award, titleKey: 'values.excellence.title', descKey: 'values.excellence.description' },
    { icon: Users, titleKey: 'values.partnership.title', descKey: 'values.partnership.description' },
    { icon: Globe, titleKey: 'values.innovation.title', descKey: 'values.innovation.description' },
    { icon: CheckCircle2, titleKey: 'values.integrity.title', descKey: 'values.integrity.description' },
  ]

  const stats = [
    { value: '20+', labelKey: 'stats.years' },
    { value: '50+', labelKey: 'stats.clients' },
    { value: '100+', labelKey: 'stats.projects' },
    { value: '15+', labelKey: 'stats.countries' },
  ]

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
              <Building2 className="w-4 h-4" />
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

      {/* Story Section */}
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
                {t('story.title')}
              </h2>
              <div className="space-y-4 text-gray-600 dark:text-gray-400 leading-relaxed">
                <p>{t('story.p1')}</p>
                <p>{t('story.p2')}</p>
                <p>{t('story.p3')}</p>
              </div>
            </motion.div>

            <motion.div
              className={cn(
                "aspect-square rounded-2xl overflow-hidden",
                "bg-gradient-to-br from-accent-100 to-primary-100 dark:from-accent-900/20 dark:to-primary-900/20",
                "flex items-center justify-center"
              )}
              initial={{ opacity: 0, x: isRTL ? -30 : 30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6, delay: 0.2 }}
            >
              <Building2 className="w-32 h-32 text-accent-500/30" />
            </motion.div>
          </div>
        </div>
      </section>

      {/* Mission & Vision Section */}
      <section ref={missionRef} className="py-24 bg-gray-50 dark:bg-gray-950">
        <div className="container mx-auto px-6">
          <div className="grid md:grid-cols-2 gap-8">
            <motion.div
              className={cn(
                "p-8 rounded-2xl",
                "bg-white dark:bg-gray-900",
                "border border-gray-200 dark:border-gray-800"
              )}
              initial={{ opacity: 0, y: 30 }}
              animate={isMissionInView ? { opacity: 1, y: 0 } : {}}
              transition={{ duration: 0.6 }}
            >
              <div className={cn(
                "w-14 h-14 rounded-xl mb-6",
                "bg-gradient-to-br from-accent-500 to-accent-400",
                "flex items-center justify-center"
              )}>
                <Target className="w-7 h-7 text-white" />
              </div>
              <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
                {t('mission.title')}
              </h3>
              <p className="text-gray-600 dark:text-gray-400 leading-relaxed">
                {t('mission.description')}
              </p>
            </motion.div>

            <motion.div
              className={cn(
                "p-8 rounded-2xl",
                "bg-white dark:bg-gray-900",
                "border border-gray-200 dark:border-gray-800"
              )}
              initial={{ opacity: 0, y: 30 }}
              animate={isMissionInView ? { opacity: 1, y: 0 } : {}}
              transition={{ duration: 0.6, delay: 0.2 }}
            >
              <div className={cn(
                "w-14 h-14 rounded-xl mb-6",
                "bg-gradient-to-br from-primary-500 to-primary-400",
                "flex items-center justify-center"
              )}>
                <Eye className="w-7 h-7 text-white" />
              </div>
              <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
                {t('vision.title')}
              </h3>
              <p className="text-gray-600 dark:text-gray-400 leading-relaxed">
                {t('vision.description')}
              </p>
            </motion.div>
          </div>
        </div>
      </section>

      {/* Values Section */}
      <section ref={valuesRef} className="py-24 bg-white dark:bg-gray-900">
        <div className="container mx-auto px-6">
          <motion.div
            className="text-center mb-16"
            initial={{ opacity: 0, y: 30 }}
            animate={isValuesInView ? { opacity: 1, y: 0 } : {}}
            transition={{ duration: 0.6 }}
          >
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
              {t('values.title')}
            </h2>
            <p className="text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
              {t('values.subtitle')}
            </p>
          </motion.div>

          <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {values.map((value, index) => (
              <ValueCard
                key={value.titleKey}
                icon={value.icon}
                title={t(value.titleKey)}
                description={t(value.descKey)}
                index={index}
              />
            ))}
          </div>
        </div>
      </section>

      {/* Stats Section */}
      <section ref={statsRef} className="py-24 bg-gray-50 dark:bg-gray-950">
        <div className="container mx-auto px-6">
          <motion.div
            className="text-center mb-16"
            initial={{ opacity: 0, y: 30 }}
            animate={isStatsInView ? { opacity: 1, y: 0 } : {}}
            transition={{ duration: 0.6 }}
          >
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
              {t('stats.title')}
            </h2>
          </motion.div>

          <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
            {stats.map((stat, index) => (
              <StatCard
                key={stat.labelKey}
                value={stat.value}
                label={t(stat.labelKey)}
                index={index}
              />
            ))}
          </div>
        </div>
      </section>

      <Footer />
    </main>
  )
}
