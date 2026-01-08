"use client"

import { motion } from "framer-motion"

const partners = [
  { name: "أرامكو", nameEn: "Aramco" },
  { name: "STC", nameEn: "STC" },
  { name: "البنك الأهلي", nameEn: "SNB" },
  { name: "سابك", nameEn: "SABIC" },
  { name: "الراجحي", nameEn: "Al Rajhi" },
  { name: "نيوم", nameEn: "NEOM" },
  { name: "أكوا باور", nameEn: "ACWA Power" },
  { name: "معادن", nameEn: "Ma'aden" },
]

const certifications = [
  { name: "ISO 27001", icon: "🔒" },
  { name: "SOC 2", icon: "✓" },
  { name: "NCA Certified", icon: "🛡️" },
  { name: "SAMA Approved", icon: "🏦" },
]

export function TrustStrip() {
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
          موثوق من قبل المنشآت الرائدة في المملكة
        </motion.p>

        {/* Partner Logos */}
        <motion.div
          className="flex flex-wrap items-center justify-center gap-8 md:gap-12 mb-12"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.6 }}
        >
          {partners.map((partner, index) => (
            <motion.div
              key={partner.name}
              className="text-xl md:text-2xl font-bold text-gray-400 dark:text-gray-600 hover:text-gray-600 dark:hover:text-gray-400 transition-colors cursor-default"
              initial={{ opacity: 0 }}
              whileInView={{ opacity: 1 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.1 }}
              title={partner.nameEn}
            >
              {partner.name}
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
          {certifications.map((cert) => (
            <div
              key={cert.name}
              className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-full shadow-sm"
            >
              <span>{cert.icon}</span>
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                {cert.name}
              </span>
            </div>
          ))}
        </motion.div>
      </div>
    </section>
  )
}
