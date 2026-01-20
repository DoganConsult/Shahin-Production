"use client"

import { motion } from "framer-motion"
import { useTranslations } from "next-intl"
import { Play, Monitor, Shield, BarChart3, FileCheck } from "lucide-react"

const featureData = [
  { key: "complianceDashboard", icon: Shield },
  { key: "riskHeatmap", icon: BarChart3 },
  { key: "evidenceManagement", icon: FileCheck },
  { key: "interactiveReports", icon: Monitor },
]

export function PlatformPreview() {
  const t = useTranslations("landing.platformPreview")

  const features = featureData.map((f) => ({
    ...f,
    title: t(`${f.key}.title`),
    description: t(`${f.key}.description`),
  }))

  return (
    <section className="py-24 bg-gradient-to-b from-gray-900 to-gray-950 overflow-hidden">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-16"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <span className="text-emerald-400 font-semibold mb-4 block">
            {t("sectionLabel")}
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-white mb-4">
            {t("title")}
          </h2>
          <p className="text-lg text-gray-400 max-w-2xl mx-auto">
            {t("subtitle")}
          </p>
        </motion.div>

        <div className="grid lg:grid-cols-2 gap-12 items-center">
          {/* Preview Window */}
          <motion.div
            className="relative"
            initial={{ opacity: 0, x: -50 }}
            whileInView={{ opacity: 1, x: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            {/* Glow Effect */}
            <div className="absolute -inset-4 bg-gradient-to-r from-emerald-500/20 via-teal-500/20 to-cyan-500/20 rounded-3xl blur-2xl" />

            {/* Browser Window */}
            <div className="relative bg-gray-800 rounded-2xl border border-gray-700 overflow-hidden shadow-2xl">
              {/* Browser Top Bar */}
              <div className="flex items-center gap-2 px-4 py-3 bg-gray-900 border-b border-gray-700">
                <div className="flex gap-1.5">
                  <div className="w-3 h-3 rounded-full bg-red-500" />
                  <div className="w-3 h-3 rounded-full bg-yellow-500" />
                  <div className="w-3 h-3 rounded-full bg-green-500" />
                </div>
                <div className="flex-1 mx-4">
                  <div className="bg-gray-800 rounded-lg px-4 py-1.5 text-sm text-gray-400 text-center">
                    app.shahin-ai.com
                  </div>
                </div>
              </div>

              {/* Screen Content */}
              <div className="aspect-video bg-gradient-to-br from-gray-800 via-gray-850 to-gray-900 flex items-center justify-center relative">
                {/* Placeholder Dashboard Elements */}
                <div className="absolute inset-4 grid grid-cols-4 grid-rows-3 gap-3 opacity-30">
                  <div className="col-span-1 row-span-3 bg-gray-700 rounded-lg" />
                  <div className="col-span-3 bg-gray-700 rounded-lg" />
                  <div className="bg-gray-700 rounded-lg" />
                  <div className="bg-gray-700 rounded-lg" />
                  <div className="bg-gray-700 rounded-lg" />
                  <div className="col-span-2 bg-gray-700 rounded-lg" />
                  <div className="bg-gray-700 rounded-lg" />
                </div>

                {/* Play Button Overlay */}
                <motion.button
                  className="relative z-10 w-20 h-20 rounded-full bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center shadow-lg shadow-emerald-500/30 hover:scale-110 transition-transform"
                  whileHover={{ scale: 1.1 }}
                  whileTap={{ scale: 0.95 }}
                >
                  <Play className="w-8 h-8 text-white ml-1" fill="white" />
                </motion.button>

                {/* Video Coming Soon Text */}
                <div className="absolute bottom-6 left-0 right-0 text-center">
                  <span className="text-gray-500 text-sm">
                    {t("videoComingSoon")}
                  </span>
                </div>
              </div>
            </div>
          </motion.div>

          {/* Features List */}
          <motion.div
            className="space-y-6"
            initial={{ opacity: 0, x: 50 }}
            whileInView={{ opacity: 1, x: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6, delay: 0.2 }}
          >
            {features.map((feature, index) => (
              <motion.div
                key={feature.key}
                className="flex gap-4 p-4 rounded-xl bg-gray-800/50 border border-gray-700 hover:border-emerald-700 transition-colors"
                initial={{ opacity: 0, x: 30 }}
                whileInView={{ opacity: 1, x: 0 }}
                viewport={{ once: true }}
                transition={{ delay: 0.3 + index * 0.1 }}
              >
                <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-emerald-500/20 to-teal-500/20 flex items-center justify-center flex-shrink-0">
                  <feature.icon className="w-6 h-6 text-emerald-400" />
                </div>
                <div>
                  <h3 className="text-lg font-semibold text-white mb-1">
                    {feature.title}
                  </h3>
                  <p className="text-gray-400 text-sm">
                    {feature.description}
                  </p>
                </div>
              </motion.div>
            ))}

            {/* CTA */}
            <motion.div
              className="pt-4"
              initial={{ opacity: 0 }}
              whileInView={{ opacity: 1 }}
              viewport={{ once: true }}
              transition={{ delay: 0.7 }}
            >
              <a
                href="/trial"
                className="inline-flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-emerald-500 to-teal-600 text-white font-semibold rounded-xl hover:from-emerald-600 hover:to-teal-700 transition-all shadow-lg shadow-emerald-500/25"
              >
                {t("cta")}
                <span className="text-lg">→</span>
              </a>
            </motion.div>
          </motion.div>
        </div>
      </div>
    </section>
  )
}
