"use client"

import { motion } from "framer-motion"
import { ClipboardCheck, Settings, Upload, Eye } from "lucide-react"
import { useTranslations } from "next-intl"

const stepsConfig = [
  {
    number: 1,
    key: "assess",
    icon: ClipboardCheck,
  },
  {
    number: 2,
    key: "design",
    icon: Settings,
  },
  {
    number: 3,
    key: "implement",
    icon: Upload,
  },
  {
    number: 4,
    key: "monitor",
    icon: Eye,
  },
]

const statsConfig = [
  { key: "implementation" },
  { key: "timeSaving" },
  { key: "accuracy" },
  { key: "support" },
]

export function HowItWorks() {
  const t = useTranslations("landing.howItWorks")

  return (
    <section className="py-24 bg-white dark:bg-gray-950">
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

        {/* Steps */}
        <div className="relative">
          {/* Connection Line */}
          <div className="hidden lg:block absolute top-1/2 left-0 right-0 h-1 bg-gradient-to-r from-emerald-500 via-teal-500 to-cyan-500 transform -translate-y-1/2 rounded-full" />

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
            {stepsConfig.map((step, index) => (
              <motion.div
                key={step.number}
                className="relative"
                initial={{ opacity: 0, y: 30 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.15 }}
              >
                {/* Card */}
                <div className="bg-gray-50 dark:bg-gray-900 rounded-2xl p-6 border border-gray-200 dark:border-gray-800 hover:shadow-xl transition-shadow relative z-10">
                  {/* Number Badge */}
                  <div className="w-14 h-14 rounded-full bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center text-2xl font-bold text-white shadow-lg mb-6 mx-auto lg:mx-0">
                    {step.number}
                  </div>

                  {/* Icon */}
                  <div className="w-12 h-12 rounded-xl bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center mb-4">
                    <step.icon className="w-6 h-6 text-emerald-600 dark:text-emerald-400" />
                  </div>

                  {/* Content */}
                  <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-3">
                    {t(`${step.key}.title`)}
                  </h3>
                  <p className="text-gray-600 dark:text-gray-400 text-sm leading-relaxed mb-4">
                    {t(`${step.key}.description`)}
                  </p>

                  {/* Details List */}
                  <ul className="space-y-2">
                    {[1, 2, 3].map((detailNum) => (
                      <li
                        key={detailNum}
                        className="flex items-center text-sm text-gray-500 dark:text-gray-500"
                      >
                        <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 ml-2" />
                        {t(`${step.key}.detail${detailNum}`)}
                      </li>
                    ))}
                  </ul>
                </div>

                {/* Arrow for desktop */}
                {index < stepsConfig.length - 1 && (
                  <div className="hidden lg:block absolute top-1/2 -left-4 transform -translate-y-1/2 z-20">
                    <div className="w-8 h-8 rounded-full bg-white dark:bg-gray-950 border-4 border-emerald-500 flex items-center justify-center">
                      <span className="text-emerald-500 text-lg">←</span>
                    </div>
                  </div>
                )}
              </motion.div>
            ))}
          </div>
        </div>

        {/* Bottom Stats */}
        <motion.div
          className="mt-16 grid grid-cols-2 md:grid-cols-4 gap-6"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ delay: 0.5 }}
        >
          {statsConfig.map((stat) => (
            <div
              key={stat.key}
              className="text-center p-4 rounded-xl bg-emerald-50 dark:bg-emerald-900/20 border border-emerald-100 dark:border-emerald-800"
            >
              <div className="text-2xl md:text-3xl font-bold text-emerald-600 dark:text-emerald-400">
                {t(`stats.${stat.key}.value`)}
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                {t(`stats.${stat.key}.label`)}
              </div>
            </div>
          ))}
        </motion.div>
      </div>
    </section>
  )
}
