"use client"

import { motion } from "framer-motion"
import { useTranslations } from "next-intl"

const partnersConfig = [
  { key: "aramco", nameEn: "Aramco" },
  { key: "stc", nameEn: "STC" },
  { key: "snb", nameEn: "SNB" },
  { key: "sabic", nameEn: "SABIC" },
  { key: "alrajhi", nameEn: "Al Rajhi" },
  { key: "neom", nameEn: "NEOM" },
  { key: "acwaPower", nameEn: "ACWA Power" },
  { key: "maaden", nameEn: "Ma'aden" },
]

const certificationsConfig = [
  { key: "iso27001", icon: "🔒" },
  { key: "soc2", icon: "✓" },
  { key: "ncaCertified", icon: "🛡️" },
  { key: "samaApproved", icon: "🏦" },
]

export function TrustStrip() {
  const t = useTranslations("landing.trustStrip")

  return (
    <section className="py-16 border-y border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-900/50">
      <div className="container mx-auto px-6">
        {/* Title */}
        <motion.p
          className="text-center text-sm font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-10"
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
        >
          {t("title")}
        </motion.p>

        {/* Partner Logos */}
        <motion.div
          className="flex flex-wrap items-center justify-center gap-8 md:gap-12 mb-12"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.6 }}
        >
          {partnersConfig.map((partner, index) => (
            <motion.div
              key={partner.key}
              className="text-xl md:text-2xl font-bold text-gray-400 dark:text-gray-600 hover:text-gray-600 dark:hover:text-gray-400 transition-colors cursor-default"
              initial={{ opacity: 0 }}
              whileInView={{ opacity: 1 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.1 }}
              title={partner.nameEn}
            >
              {t(`partners.${partner.key}`)}
            </motion.div>
          ))}
        </motion.div>

        {/* Certifications */}
        <motion.div
          className="flex flex-wrap items-center justify-center gap-4"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.6, delay: 0.2 }}
        >
          {certificationsConfig.map((cert) => (
            <div
              key={cert.key}
              className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-full shadow-sm"
            >
              <span>{cert.icon}</span>
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                {t(`certifications.${cert.key}`)}
              </span>
            </div>
          ))}
        </motion.div>
      </div>
    </section>
  )
}
