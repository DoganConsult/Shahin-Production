"use client"

import { motion } from "framer-motion"
import { AlertTriangle, Clock, FileX, Users } from "lucide-react"

const problems = [
  {
    icon: FileX,
    title: "تشتت البيانات",
    description: "معلومات الامتثال مبعثرة في ملفات Excel ورسائل البريد الإلكتروني والمجلدات المختلفة",
    stat: "73%",
    statLabel: "من المؤسسات تعاني من تشتت البيانات",
    color: "from-red-500 to-orange-500",
  },
  {
    icon: Clock,
    title: "ضياع الوقت",
    description: "ساعات طويلة تُهدر في جمع الأدلة يدوياً والبحث عن الوثائق للتدقيق",
    stat: "40%",
    statLabel: "من وقت الفريق يضيع في العمل اليدوي",
    color: "from-orange-500 to-amber-500",
  },
  {
    icon: AlertTriangle,
    title: "مخاطر عدم الامتثال",
    description: "غرامات مالية كبيرة وأضرار بالسمعة نتيجة عدم متابعة المتطلبات التنظيمية",
    stat: "5M+",
    statLabel: "ريال متوسط غرامات عدم الامتثال",
    color: "from-amber-500 to-yellow-500",
  },
  {
    icon: Users,
    title: "نقص الكفاءات",
    description: "صعوبة إيجاد خبراء الامتثال المؤهلين وارتفاع تكلفة التدريب والاستقطاب",
    stat: "68%",
    statLabel: "من المؤسسات تواجه فجوة في المهارات",
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
            التحديات
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
            هل تواجه هذه التحديات؟
          </h2>
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
            معظم المؤسسات السعودية تعاني من نفس التحديات في إدارة الامتثال والمخاطر
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
          {problems.map((problem) => (
            <motion.div
              key={problem.title}
              className="group relative bg-gray-50 dark:bg-gray-900 rounded-2xl p-6 border border-gray-200 dark:border-gray-800 hover:border-red-300 dark:hover:border-red-800 transition-all duration-300 hover:shadow-xl"
              variants={itemVariants}
            >
              {/* Icon */}
              <div className={`w-14 h-14 rounded-xl bg-gradient-to-br ${problem.color} flex items-center justify-center mb-5 shadow-lg group-hover:scale-110 transition-transform`}>
                <problem.icon className="w-7 h-7 text-white" />
              </div>

              {/* Content */}
              <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-3">
                {problem.title}
              </h3>
              <p className="text-gray-600 dark:text-gray-400 text-sm leading-relaxed mb-6">
                {problem.description}
              </p>

              {/* Stat */}
              <div className="pt-4 border-t border-gray-200 dark:border-gray-800">
                <div className={`text-3xl font-bold bg-gradient-to-r ${problem.color} bg-clip-text text-transparent`}>
                  {problem.stat}
                </div>
                <div className="text-xs text-gray-500 dark:text-gray-500 mt-1">
                  {problem.statLabel}
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
            منصة <span className="font-bold text-emerald-600">شاهين</span> تحل كل هذه التحديات في منصة واحدة متكاملة
          </p>
        </motion.div>
      </div>
    </section>
  )
}
