"use client"

import { motion } from "framer-motion"
import {
  Shield,
  FileCheck,
  BarChart3,
  Users,
  Workflow,
  Lock,
  Bell,
  FileText,
  Target,
  Layers,
  GitBranch,
  Gauge
} from "lucide-react"
import { useTranslations } from 'next-intl'

const featuresConfig = [
  {
    key: "compliance",
    icon: Shield,
    gradient: "from-emerald-500 to-teal-600",
  },
  {
    key: "risks",
    icon: BarChart3,
    gradient: "from-blue-500 to-cyan-600",
  },
  {
    key: "audits",
    icon: FileCheck,
    gradient: "from-purple-500 to-pink-600",
  },
  {
    key: "workflow",
    icon: Workflow,
    gradient: "from-orange-500 to-red-600",
  },
  {
    key: "policies",
    icon: FileText,
    gradient: "from-indigo-500 to-purple-600",
  },
  {
    key: "vendors",
    icon: Users,
    gradient: "from-teal-500 to-green-600",
  },
  {
    key: "notifications",
    icon: Bell,
    gradient: "from-amber-500 to-orange-600",
  },
  {
    key: "actionPlans",
    icon: Target,
    gradient: "from-rose-500 to-pink-600",
  },
  {
    key: "frameworks",
    icon: Layers,
    gradient: "from-cyan-500 to-blue-600",
  },
  {
    key: "security",
    icon: Lock,
    gradient: "from-gray-600 to-gray-800",
  },
  {
    key: "integration",
    icon: GitBranch,
    gradient: "from-violet-500 to-purple-600",
  },
  {
    key: "dashboards",
    icon: Gauge,
    gradient: "from-emerald-600 to-teal-700",
  },
]

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.1 }
  }
}

const itemVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: { opacity: 1, y: 0 }
}

export function Features() {
  const t = useTranslations('landing.features')

  return (
    <section className="py-24 bg-gray-50 dark:bg-gray-900/50">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-16"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <span className="text-emerald-600 dark:text-emerald-400 font-semibold mb-4 block">
            {t('sectionLabel')}
          </span>
          <h2 className="section-title">
            {t('title')}
          </h2>
          <p className="section-subtitle mx-auto">
            {t('subtitle')}
          </p>
        </motion.div>

        {/* Features Grid */}
        <motion.div
          className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6"
          variants={containerVariants}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
        >
          {featuresConfig.map((feature, index) => (
            <motion.div
              key={feature.key}
              className="feature-card group"
              variants={itemVariants}
            >
              {/* Icon */}
              <div className={`w-14 h-14 rounded-xl bg-gradient-to-br ${feature.gradient}
                              flex items-center justify-center mb-5 shadow-lg
                              group-hover:scale-110 transition-transform duration-300`}>
                <feature.icon className="w-7 h-7 text-white" />
              </div>

              {/* Content */}
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                {t(`${feature.key}.title`)}
              </h3>
              <p className="text-gray-600 dark:text-gray-400 text-sm leading-relaxed">
                {t(`${feature.key}.description`)}
              </p>

              {/* Hover Arrow */}
              <div className="mt-4 text-emerald-600 dark:text-emerald-400 opacity-0 group-hover:opacity-100 transition-opacity">
                <span className="text-sm font-medium">{t('learnMore')} ←</span>
              </div>
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  )
}
