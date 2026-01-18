"use client"

import { useRef } from "react"
import { motion, useInView } from "framer-motion"
import { Rocket, Brain, Shield, BarChart, CheckCircle2, ArrowRight, ArrowLeft, Play } from "lucide-react"
import { useLocale, useTranslations } from "next-intl"
import Link from "next/link"
import { Navigation } from "@/components/sections/Navigation"
import { Footer } from "@/components/sections/Footer"
import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"

const stepIcons = {
  Rocket,
  Brain,
  Shield,
  BarChart,
}

interface Step {
  number: string
  title: string
  description: string
  icon: keyof typeof stepIcons
}

function StepCard({ step, index, isRTL }: { step: Step; index: number; isRTL: boolean }) {
  const cardRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(cardRef, { once: true, margin: "-50px" })

  const Icon = stepIcons[step.icon]

  return (
    <motion.div
      ref={cardRef}
      className={cn(
        "relative p-8 rounded-2xl",
        "bg-white dark:bg-gray-900",
        "border border-gray-200 dark:border-gray-800",
        "hover:border-accent-300 dark:hover:border-accent-700",
        "hover:shadow-xl hover:shadow-accent-500/10",
        "transition-all duration-500"
      )}
      initial={{ opacity: 0, y: 40 }}
      animate={isInView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.6, delay: index * 0.2 }}
    >
      {/* Step Number */}
      <div className={cn(
        "absolute -top-6 -left-6 w-16 h-16 rounded-full",
        "bg-gradient-to-br from-accent-500 to-primary-500",
        "flex items-center justify-center",
        "text-white font-extrabold text-xl",
        "shadow-lg shadow-accent-500/30"
      )}>
        {step.number}
      </div>

      {/* Icon */}
      <div className={cn(
        "w-16 h-16 rounded-2xl mb-6 mt-4",
        "bg-gradient-to-br from-accent-100 to-accent-50 dark:from-accent-900/30 dark:to-accent-800/20",
        "flex items-center justify-center"
      )}>
        <Icon className="w-8 h-8 text-accent-600 dark:text-accent-400" />
      </div>

      {/* Content */}
      <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
        {step.title}
      </h3>
      <p className="text-gray-600 dark:text-gray-400 leading-relaxed">
        {step.description}
      </p>
    </motion.div>
  )
}

function FeatureCard({ title, description, index }: { title: string; description: string; index: number }) {
  const cardRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(cardRef, { once: true, margin: "-50px" })

  return (
    <motion.div
      ref={cardRef}
      className={cn(
        "p-6 rounded-xl",
        "bg-gray-50 dark:bg-gray-950",
        "border border-gray-200 dark:border-gray-800",
        "hover:border-accent-300 dark:hover:border-accent-700",
        "hover:shadow-lg transition-all duration-300"
      )}
      initial={{ opacity: 0, y: 30 }}
      animate={isInView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.5, delay: index * 0.1 }}
    >
      <CheckCircle2 className="w-6 h-6 text-accent-500 mb-4" />
      <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-2">
        {title}
      </h3>
      <p className="text-gray-600 dark:text-gray-400 text-sm leading-relaxed">
        {description}
      </p>
    </motion.div>
  )
}

export default function HowItWorksPage() {
  const t = useTranslations('howItWorks')
  const locale = useLocale()
  const isRTL = locale === 'ar'

  const stepsRef = useRef<HTMLDivElement>(null)
  const featuresRef = useRef<HTMLDivElement>(null)
  const isStepsInView = useInView(stepsRef, { once: true, margin: "-100px" })
  const isFeaturesInView = useInView(featuresRef, { once: true, margin: "-100px" })

  const steps = t.raw('steps') as Step[]
  const features = t.raw('features.items') as Array<{ title: string; description: string }>

  const ArrowIcon = isRTL ? ArrowLeft : ArrowRight

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
              <Play className="w-4 h-4" />
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

      {/* Steps Section */}
      <section ref={stepsRef} className="py-24 bg-white dark:bg-gray-900">
        <div className="container mx-auto px-6">
          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
            {steps.map((step, index) => (
              <StepCard
                key={step.number}
                step={step}
                index={index}
                isRTL={isRTL}
              />
            ))}
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
              {t('features.title')}
            </h2>
            <p className="text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
              {t('features.subtitle')}
            </p>
          </motion.div>

          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {features.map((feature, index) => (
              <FeatureCard
                key={index}
                title={feature.title}
                description={feature.description}
                index={index}
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
