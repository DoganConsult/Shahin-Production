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

const features = [
  {
    icon: Shield,
    title: "إدارة الامتثال",
    description: "متابعة شاملة للامتثال مع جميع الأطر التنظيمية السعودية والدولية",
    gradient: "from-emerald-500 to-teal-600",
  },
  {
    icon: BarChart3,
    title: "إدارة المخاطر",
    description: "تقييم وتحليل المخاطر بأساليب احترافية مع خرائط حرارية تفاعلية",
    gradient: "from-blue-500 to-cyan-600",
  },
  {
    icon: FileCheck,
    title: "إدارة التدقيق",
    description: "تخطيط وتنفيذ عمليات التدقيق الداخلي والخارجي بكفاءة",
    gradient: "from-purple-500 to-pink-600",
  },
  {
    icon: Workflow,
    title: "محرك سير العمل",
    description: "أتمتة العمليات والموافقات مع محرك سير عمل متقدم",
    gradient: "from-orange-500 to-red-600",
  },
  {
    icon: FileText,
    title: "إدارة السياسات",
    description: "إنشاء وتوزيع ومتابعة السياسات والإجراءات التنظيمية",
    gradient: "from-indigo-500 to-purple-600",
  },
  {
    icon: Users,
    title: "إدارة الموردين",
    description: "تقييم ومتابعة مخاطر الأطراف الثالثة والموردين",
    gradient: "from-teal-500 to-green-600",
  },
  {
    icon: Bell,
    title: "التنبيهات الذكية",
    description: "إشعارات وتنبيهات آلية للمواعيد والمهام المستحقة",
    gradient: "from-amber-500 to-orange-600",
  },
  {
    icon: Target,
    title: "خطط العمل",
    description: "إدارة ومتابعة خطط المعالجة والتحسين المستمر",
    gradient: "from-rose-500 to-pink-600",
  },
  {
    icon: Layers,
    title: "الأطر التنظيمية",
    description: "مكتبة شاملة للأطر التنظيمية السعودية والدولية",
    gradient: "from-cyan-500 to-blue-600",
  },
  {
    icon: Lock,
    title: "الأمان المتقدم",
    description: "تشفير متقدم وتحكم في الوصول متعدد المستويات",
    gradient: "from-gray-600 to-gray-800",
  },
  {
    icon: GitBranch,
    title: "التكامل",
    description: "تكامل سهل مع الأنظمة الموجودة وأدوات الأمن السيبراني",
    gradient: "from-violet-500 to-purple-600",
  },
  {
    icon: Gauge,
    title: "لوحات المعلومات",
    description: "لوحات تحكم تفاعلية مع تقارير ورسوم بيانية متقدمة",
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
            المميزات
          </span>
          <h2 className="section-title">
            كل ما تحتاجه في منصة واحدة
          </h2>
          <p className="section-subtitle mx-auto">
            منصة شاملة لإدارة الحوكمة والمخاطر والامتثال مع أدوات متكاملة
            لجميع متطلبات مؤسستك
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
          {features.map((feature, index) => (
            <motion.div
              key={feature.title}
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
                {feature.title}
              </h3>
              <p className="text-gray-600 dark:text-gray-400 text-sm leading-relaxed">
                {feature.description}
              </p>

              {/* Hover Arrow */}
              <div className="mt-4 text-emerald-600 dark:text-emerald-400 opacity-0 group-hover:opacity-100 transition-opacity">
                <span className="text-sm font-medium">اعرف المزيد ←</span>
              </div>
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  )
}
