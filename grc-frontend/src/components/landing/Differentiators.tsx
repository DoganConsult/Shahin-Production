"use client"

import { motion } from "framer-motion"
import { useTranslations } from "next-intl"
import {
  Bot,
  Radar,
  ShieldCheck,
  FileSearch,
  Code2,
  Package,
  ArrowRight
} from "lucide-react"
import Link from "next/link"
import { Button } from "@/components/ui/button"

const diffData = [
  { key: "aiAgents", icon: Bot, gradient: "from-violet-500 to-purple-600" },
  { key: "regIntel", icon: Radar, gradient: "from-blue-500 to-cyan-600" },
  { key: "monitoring", icon: ShieldCheck, gradient: "from-emerald-500 to-teal-600" },
  { key: "evidenceFirst", icon: FileSearch, gradient: "from-orange-500 to-red-600" },
  { key: "complianceAsCode", icon: Code2, gradient: "from-pink-500 to-rose-600" },
  { key: "industryPacks", icon: Package, gradient: "from-amber-500 to-orange-600" },
]

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.1 }
  }
}

const itemVariants = {
  hidden: { opacity: 0, scale: 0.95 },
  visible: { opacity: 1, scale: 1, transition: { duration: 0.4 } }
}

export function Differentiators() {
  const t = useTranslations("landing.differentiators")

  const differentiators = diffData.map((d) => ({
    ...d,
    title: t(`${d.key}.title`),
    description: t(`${d.key}.description`),
    highlight: t(`${d.key}.highlight`),
  }))

  return (
    <section className="py-24 bg-gradient-to-b from-gray-50 to-white dark:from-gray-900 dark:to-gray-950">
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

        {/* Differentiators Grid */}
        <motion.div
          className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
          variants={containerVariants}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
        >
          {differentiators.map((item) => (
            <motion.div
              key={item.key}
              className="group relative bg-white dark:bg-gray-900 rounded-2xl p-8 border border-gray-200 dark:border-gray-800 hover:shadow-2xl transition-all duration-500 overflow-hidden"
              variants={itemVariants}
            >
              {/* Background Gradient on Hover */}
              <div className={`absolute inset-0 bg-gradient-to-br ${item.gradient} opacity-0 group-hover:opacity-5 transition-opacity duration-500`} />

              {/* Highlight Badge */}
              <div className={`absolute top-4 right-4 px-3 py-1 rounded-full text-xs font-bold text-white bg-gradient-to-r ${item.gradient}`}>
                {item.highlight}
              </div>

              {/* Icon */}
              <div className={`w-16 h-16 rounded-2xl bg-gradient-to-br ${item.gradient} flex items-center justify-center mb-6 shadow-lg group-hover:scale-110 group-hover:rotate-3 transition-all duration-300`}>
                <item.icon className="w-8 h-8 text-white" />
              </div>

              {/* Content */}
              <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-3">
                {item.title}
              </h3>
              <p className="text-gray-600 dark:text-gray-400 leading-relaxed">
                {item.description}
              </p>

              {/* Hover Arrow */}
              <div className="mt-6 flex items-center text-emerald-600 dark:text-emerald-400 opacity-0 group-hover:opacity-100 transition-all duration-300 transform translate-x-[-10px] group-hover:translate-x-0">
                <span className="text-sm font-medium">{t("learnMore")}</span>
                <ArrowRight className="w-4 h-4 ml-2" />
              </div>
            </motion.div>
          ))}
        </motion.div>

        {/* Bottom CTA */}
        <motion.div
          className="text-center mt-16"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ delay: 0.3 }}
        >
          <Link href="/trial">
            <Button className="bg-gradient-to-r from-emerald-500 to-teal-600 text-white px-8 py-6 text-lg hover:from-emerald-600 hover:to-teal-700 group">
              {t("cta")}
              <ArrowRight className="w-5 h-5 ml-2 group-hover:translate-x-1 transition-transform" />
            </Button>
          </Link>
        </motion.div>
      </div>
    </section>
  )
}
