"use client"

import { motion } from "framer-motion"
import { useTranslations } from "next-intl"

const regulators = [
  { name: "NCA", fullName: "National Cybersecurity Authority" },
  { name: "SAMA", fullName: "Saudi Arabian Monetary Authority" },
  { name: "CMA", fullName: "Capital Market Authority" },
  { name: "CITC", fullName: "Communications & IT Commission" },
  { name: "SDAIA", fullName: "Saudi Data & AI Authority" },
  { name: "PDPL", fullName: "Personal Data Protection Law" },
]

const frameworks = [
  "NCA ECC-1:2018",
  "NCA CSCC-1:2019",
  "NCA DCC-1:2022",
  "SAMA Cybersecurity Framework",
  "PDPL Compliance",
  "ISO 27001:2022",
  "ISO 27701:2019",
  "NIST CSF 2.0",
  "PCI DSS 4.0",
  "SOC 2 Type II",
]

export function Regulators() {
  const t = useTranslations("landing.regulators")

  return (
    <section id="regulators" className="py-24 bg-white dark:bg-gray-900">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-16"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <span className="text-emerald-600 dark:text-emerald-400 font-semibold mb-4 block">
            {t("sectionLabel")}
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
            {t("title")}
          </h2>
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
            {t("subtitle")}
          </p>
        </motion.div>

        {/* Regulators Grid */}
        <motion.div
          className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-8 mb-16"
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
          transition={{ staggerChildren: 0.1 }}
        >
          {regulators.map((reg, index) => (
            <motion.div
              key={reg.name}
              className="flex flex-col items-center p-6 rounded-2xl bg-gray-50 dark:bg-gray-800/50
                         border border-gray-100 dark:border-gray-700/50
                         hover:border-emerald-500/30 hover:shadow-lg transition-all duration-300"
              initial={{ opacity: 0, scale: 0.9 }}
              whileInView={{ opacity: 1, scale: 1 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.1 }}
            >
              <div className="w-16 h-16 rounded-xl bg-white dark:bg-gray-700 flex items-center justify-center mb-4 shadow-sm">
                <span className="text-2xl font-bold text-emerald-600 dark:text-emerald-400">
                  {reg.name.charAt(0)}
                </span>
              </div>
              <span className="text-sm font-semibold text-gray-900 dark:text-white mb-1">
                {reg.name}
              </span>
              <span className="text-xs text-gray-500 dark:text-gray-400 text-center">
                {reg.fullName}
              </span>
            </motion.div>
          ))}
        </motion.div>

        {/* Frameworks Tags */}
        <motion.div
          className="flex flex-wrap justify-center gap-3"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          {frameworks.map((framework, index) => (
            <motion.span
              key={framework}
              className="px-4 py-2 rounded-full bg-emerald-50 dark:bg-emerald-900/20
                         text-emerald-700 dark:text-emerald-400 text-sm font-medium
                         border border-emerald-200 dark:border-emerald-800
                         hover:bg-emerald-100 dark:hover:bg-emerald-900/40 transition-colors cursor-default"
              initial={{ opacity: 0, scale: 0.9 }}
              whileInView={{ opacity: 1, scale: 1 }}
              viewport={{ once: true }}
              transition={{ delay: 0.3 + index * 0.05 }}
            >
              {framework}
            </motion.span>
          ))}
        </motion.div>
      </div>
    </section>
  )
}
