"use client"

import { motion, useInView } from "framer-motion"
import { useTranslations } from "next-intl"
import { useRef, useEffect, useState } from "react"
import { Building2, Shield, FileCheck, Bot } from "lucide-react"

const statData = [
  { key: "regulators", icon: Building2, value: 120, suffix: "+" },
  { key: "frameworks_count", icon: Shield, value: 240, suffix: "+" },
  { key: "controls", icon: FileCheck, value: 57000, suffix: "+" },
  { key: "ai_agents", icon: Bot, value: 9, suffix: "" },
]

function AnimatedCounter({ value, suffix }: { value: number; suffix: string }) {
  const [count, setCount] = useState(0)
  const ref = useRef(null)
  const isInView = useInView(ref, { once: true })

  useEffect(() => {
    if (isInView) {
      const duration = 2000
      const steps = 60
      const increment = value / steps
      let current = 0
      const timer = setInterval(() => {
        current += increment
        if (current >= value) {
          setCount(value)
          clearInterval(timer)
        } else {
          setCount(Math.floor(current))
        }
      }, duration / steps)
      return () => clearInterval(timer)
    }
  }, [isInView, value])

  return (
    <span ref={ref}>
      {count.toLocaleString()}
      {suffix}
    </span>
  )
}

export function Stats() {
  const t = useTranslations("landing.statsSec")

  const stats = statData.map((s) => ({
    ...s,
    label: t(`${s.key}.label`),
    description: t(`${s.key}.description`),
  }))

  return (
    <section className="py-20 bg-gradient-to-r from-emerald-600 via-teal-600 to-cyan-600">
      <div className="container mx-auto px-6">
        <motion.div
          className="grid grid-cols-2 md:grid-cols-4 gap-8"
          initial={{ opacity: 0, y: 30 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.6 }}
        >
          {stats.map((stat, index) => (
            <motion.div
              key={stat.key}
              className="text-center"
              initial={{ opacity: 0, scale: 0.9 }}
              whileInView={{ opacity: 1, scale: 1 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.1 }}
            >
              {/* Icon */}
              <div className="w-14 h-14 rounded-2xl bg-white/20 backdrop-blur flex items-center justify-center mx-auto mb-4">
                <stat.icon className="w-7 h-7 text-white" />
              </div>

              {/* Value */}
              <div className="text-4xl md:text-5xl font-bold text-white mb-2">
                <AnimatedCounter value={stat.value} suffix={stat.suffix} />
              </div>

              {/* Label */}
              <div className="text-lg font-semibold text-white/90 mb-1">
                {stat.label}
              </div>

              {/* Description */}
              <div className="text-sm text-white/70">
                {stat.description}
              </div>
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  )
}
