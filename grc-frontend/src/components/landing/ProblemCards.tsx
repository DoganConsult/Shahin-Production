"use client"

import { motion } from "framer-motion"
import { AlertTriangle, Clock, FileX, Users } from "lucide-react"
import { useTranslations } from "next-intl"

const problemsConfig = [
  {
    key: "dataScatter",
    icon: FileX,
    color: "from-red-500 to-orange-500",
  },
  {
    key: "timeWaste",
    icon: Clock,
    color: "from-orange-500 to-amber-500",
  },
  {
    key: "complianceRisks",
    icon: AlertTriangle,
    color: "from-amber-500 to-yellow-500",
  },
  {
    key: "skillsGap",
    icon: Users,
    color: "from-purple-500 to-pink-500",
  },
]

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.15 }
  }
}

const itemVariants = {
  hidden: { opacity: 0, y: 30 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.5 } }
}

export function ProblemCards() {
  const t = useTranslations("landing.problems")

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
          <span className="text-red-600 dark:text-red-400 font-semibold mb-4 block">
            {t("sectionLabel")}
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
            {t("title")}
          </h2>
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
            {t("subtitle")}
          </p>
        </motion.div>

        {/* Problem Cards Grid */}
        <motion.div
          className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6"
          variants={containerVariants}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
        >
          {problemsConfig.map((problem) => (
            <motion.div
              key={problem.key}
              className="group relative bg-gray-50 dark:bg-gray-900 rounded-2xl p-6 border border-gray-200 dark:border-gray-800 hover:border-red-300 dark:hover:border-red-800 transition-all duration-300 hover:shadow-xl"
              variants={itemVariants}
            >
              {/* Icon */}
              <div className={`w-14 h-14 rounded-xl bg-gradient-to-br ${problem.color} flex items-center justify-center mb-5 shadow-lg group-hover:scale-110 transition-transform`}>
                <problem.icon className="w-7 h-7 text-white" />
              </div>

              {/* Content */}
              <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-3">
                {t(`${problem.key}.title`)}
              </h3>
              <p className="text-gray-600 dark:text-gray-400 text-sm leading-relaxed mb-6">
                {t(`${problem.key}.description`)}
              </p>

              {/* Stat */}
              <div className="pt-4 border-t border-gray-200 dark:border-gray-800">
                <div className={`text-3xl font-bold bg-gradient-to-r ${problem.color} bg-clip-text text-transparent`}>
                  {t(`${problem.key}.stat`)}
                </div>
                <div className="text-xs text-gray-500 dark:text-gray-500 mt-1">
                  {t(`${problem.key}.statLabel`)}
                </div>
              </div>
            </motion.div>
          ))}
        </motion.div>

        {/* Bottom CTA */}
        <motion.div
          className="text-center mt-12"
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
          transition={{ delay: 0.5 }}
        >
          <p className="text-lg text-gray-700 dark:text-gray-300">
            {t.rich("cta", {
              appName: (chunks) => <span className="font-bold text-emerald-600">{chunks}</span>
            })}
          </p>
        </motion.div>
      </div>
    </section>
  )
}
