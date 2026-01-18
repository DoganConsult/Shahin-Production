"use client"

import { motion } from "framer-motion"
import { FileText } from "lucide-react"
import { useLocale, useTranslations } from "next-intl"
import { Navigation } from "@/components/sections/Navigation"
import { Footer } from "@/components/sections/Footer"
import { cn } from "@/lib/utils"

export default function TermsPage() {
  const t = useTranslations('legal.terms')
  const locale = useLocale()
  const isRTL = locale === 'ar'

  const sections = t.raw('sections') as Array<{ title: string; content: string }>

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
              <FileText className="w-4 h-4" />
              {t('badge')}
            </span>
            <h1 className="text-4xl md:text-5xl lg:text-6xl font-extrabold text-white mb-6">
              {t('title')}
            </h1>
            <p className="text-lg text-white/80">
              {t('lastUpdated')}
            </p>
          </motion.div>
        </div>
      </section>

      {/* Content */}
      <section className="py-24 bg-white dark:bg-gray-900">
        <div className="container mx-auto px-6">
          <div className="max-w-3xl mx-auto">
            <motion.div
              className="prose prose-lg dark:prose-invert max-w-none"
              initial={{ opacity: 0, y: 30 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6, delay: 0.2 }}
            >
              <p className="text-gray-600 dark:text-gray-400 mb-12 text-lg leading-relaxed">
                {t('intro')}
              </p>

              {sections.map((section, index) => (
                <motion.div
                  key={index}
                  className={cn(
                    "mb-10 p-6 rounded-xl",
                    "bg-gray-50 dark:bg-gray-800",
                    "border border-gray-200 dark:border-gray-700"
                  )}
                  initial={{ opacity: 0, y: 20 }}
                  whileInView={{ opacity: 1, y: 0 }}
                  viewport={{ once: true }}
                  transition={{ delay: index * 0.1 }}
                >
                  <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-4">
                    {section.title}
                  </h2>
                  <p className="text-gray-600 dark:text-gray-400 leading-relaxed whitespace-pre-line">
                    {section.content}
                  </p>
                </motion.div>
              ))}
            </motion.div>
          </div>
        </div>
      </section>

      <Footer />
    </main>
  )
}
