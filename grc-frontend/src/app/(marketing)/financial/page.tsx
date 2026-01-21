"use client"

import { motion } from "framer-motion"
import { Landmark, Shield, FileCheck, Scale, Lock, BarChart3, CheckCircle, ArrowLeft } from "lucide-react"
import { Button } from "@/components/ui/button"
import Link from "next/link"

const regulations = [
  { name: "لوائح مؤسسة النقد العربي السعودي (SAMA)", icon: Landmark },
  { name: "الضوابط والمعايير الاحترازية", icon: Shield },
  { name: "لوائح مكافحة غسل الأموال", icon: Scale },
  { name: "معايير أمن المعلومات المالية", icon: Lock },
  { name: "Basel III / Basel IV", icon: FileCheck },
  { name: "متطلبات إدارة المخاطر المالية", icon: BarChart3 },
]

const features = [
  {
    icon: Landmark,
    title: "امتثال SAMA",
    description: "دعم كامل لجميع لوائح ومتطلبات مؤسسة النقد العربي السعودي",
  },
  {
    icon: Shield,
    title: "إدارة المخاطر المالية",
    description: "تقييم وإدارة المخاطر المالية والتشغيلية والائتمانية",
  },
  {
    icon: Scale,
    title: "مكافحة غسل الأموال",
    description: "أدوات متقدمة لمراقبة المعاملات والامتثال لمتطلبات AML/CFT",
  },
  {
    icon: Lock,
    title: "حماية البيانات المالية",
    description: "تشفير متقدم وحماية البيانات الحساسة للعملاء",
  },
  {
    icon: FileCheck,
    title: "تقارير تنظيمية",
    description: "تقارير تلقائية جاهزة للتقديم للجهات التنظيمية",
  },
  {
    icon: BarChart3,
    title: "لوحات تحكم تحليلية",
    description: "رؤية شاملة لحالة الامتثال والمخاطر في الوقت الفعلي",
  },
]

const benefits = [
  "تقليل مخاطر الغرامات التنظيمية",
  "أتمتة التقارير التنظيمية الدورية",
  "مراقبة مستمرة للامتثال",
  "تتبع التغييرات التنظيمية تلقائياً",
  "تكامل مع الأنظمة المصرفية الأساسية",
  "دعم متعدد الفروع والوحدات",
]

const stats = [
  { value: "50+", label: "مؤسسة مالية" },
  { value: "100%", label: "امتثال SAMA" },
  { value: "24/7", label: "مراقبة" },
  { value: "99.9%", label: "دقة التقارير" },
]

export default function FinancialPage() {
  return (
    <div className="min-h-screen">
      {/* Hero Section */}
      <section className="relative min-h-[70vh] flex items-center justify-center overflow-hidden bg-gradient-to-br from-emerald-900 via-teal-900 to-cyan-900">
        <div className="absolute inset-0 opacity-20">
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_50%_120%,rgba(20,184,166,0.1),transparent)]" />
        </div>

        <div className="container mx-auto px-6 relative z-10 py-20">
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            className="max-w-4xl mx-auto text-center"
          >
            <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-white/10 backdrop-blur-sm text-emerald-300 text-sm font-medium mb-8">
              <Landmark className="w-4 h-4" />
              حلول القطاع المالي
            </div>

            <h1 className="text-4xl md:text-6xl font-bold text-white mb-6">
              إدارة الامتثال التنظيمي
              <span className="block text-emerald-400 mt-2">للمؤسسات المالية</span>
            </h1>

            <p className="text-xl text-gray-300 mb-10 max-w-3xl mx-auto">
              حل متخصص لإدارة الامتثال التنظيمي والمخاطر للبنوك وشركات التأمين والمؤسسات
              المالية مع دعم كامل للوائح SAMA
            </p>

            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link href="/demo">
                <Button size="xl" className="bg-white text-emerald-900 hover:bg-gray-100 group">
                  احجز عرض توضيحي
                  <ArrowLeft className="w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                </Button>
              </Link>
              <Link href="/contact">
                <Button size="xl" variant="outline" className="border-white text-white hover:bg-white/10">
                  تحدث مع خبير
                </Button>
              </Link>
            </div>
          </motion.div>
        </div>
      </section>

      {/* Stats */}
      <section className="py-16 bg-white dark:bg-gray-900">
        <div className="container mx-auto px-6">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
            {stats.map((stat, index) => (
              <motion.div
                key={stat.label}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className="text-center"
              >
                <div className="text-3xl md:text-4xl font-bold text-emerald-600 dark:text-emerald-400 mb-2">
                  {stat.value}
                </div>
                <div className="text-sm text-gray-600 dark:text-gray-400">{stat.label}</div>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Regulations */}
      <section className="py-24 bg-gray-50 dark:bg-gray-900/50">
        <div className="container mx-auto px-6">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-center mb-16"
          >
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
              الأطر التنظيمية المدعومة
            </h2>
            <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
              دعم شامل لجميع اللوائح والمعايير التنظيمية للقطاع المالي السعودي
            </p>
          </motion.div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
            {regulations.map((regulation, index) => (
              <motion.div
                key={regulation.name}
                initial={{ opacity: 0, scale: 0.9 }}
                whileInView={{ opacity: 1, scale: 1 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className="bg-white dark:bg-gray-800 rounded-xl p-6 shadow-lg hover:shadow-xl transition-all border-2 border-transparent hover:border-emerald-500"
              >
                <regulation.icon className="w-10 h-10 text-emerald-600 dark:text-emerald-400 mb-4" />
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                  {regulation.name}
                </h3>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Features */}
      <section className="py-24 bg-white dark:bg-gray-900">
        <div className="container mx-auto px-6">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-center mb-16"
          >
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
              مميزات متخصصة للقطاع المالي
            </h2>
            <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
              أدوات وإمكانيات مصممة خصيصاً لتلبية احتياجات المؤسسات المالية
            </p>
          </motion.div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
            {features.map((feature, index) => (
              <motion.div
                key={feature.title}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className="bg-gradient-to-br from-gray-50 to-emerald-50 dark:from-gray-800 dark:to-emerald-900/20 rounded-xl p-6 hover:shadow-xl transition-shadow"
              >
                <div className="w-12 h-12 bg-gradient-to-br from-emerald-500 to-teal-600 rounded-xl flex items-center justify-center mb-4">
                  <feature.icon className="w-6 h-6 text-white" />
                </div>
                <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                  {feature.title}
                </h3>
                <p className="text-gray-600 dark:text-gray-400">{feature.description}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Benefits */}
      <section className="py-24 bg-gray-50 dark:bg-gray-900/50">
        <div className="container mx-auto px-6">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <motion.div
              initial={{ opacity: 0, x: -30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
            >
              <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-6">
                لماذا تختار شاهين للقطاع المالي؟
              </h2>
              <p className="text-lg text-gray-600 dark:text-gray-400 mb-8">
                حل موثوق يضمن الامتثال الكامل ويحميك من المخاطر التنظيمية
              </p>

              <div className="space-y-4">
                {benefits.map((benefit, index) => (
                  <motion.div
                    key={benefit}
                    initial={{ opacity: 0, x: -20 }}
                    whileInView={{ opacity: 1, x: 0 }}
                    viewport={{ once: true }}
                    transition={{ delay: index * 0.1 }}
                    className="flex items-start gap-3"
                  >
                    <div className="w-6 h-6 bg-emerald-100 dark:bg-emerald-900/30 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5">
                      <CheckCircle className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
                    </div>
                    <span className="text-gray-700 dark:text-gray-300">{benefit}</span>
                  </motion.div>
                ))}
              </div>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, x: 30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              className="bg-gradient-to-br from-emerald-500 to-teal-600 rounded-2xl p-8 text-white"
            >
              <h3 className="text-2xl font-bold mb-4">موثوق به من قبل المؤسسات المالية</h3>
              <p className="mb-6 text-emerald-50">
                انضم إلى العشرات من البنوك وشركات التأمين والمؤسسات المالية التي تثق في شاهين
              </p>

              <div className="bg-white/10 backdrop-blur-sm rounded-xl p-6 mb-6 space-y-3">
                <div className="flex items-center gap-3">
                  <CheckCircle className="w-5 h-5" />
                  <span>امتثال كامل للوائح SAMA</span>
                </div>
                <div className="flex items-center gap-3">
                  <CheckCircle className="w-5 h-5" />
                  <span>تحديثات تنظيمية مستمرة</span>
                </div>
                <div className="flex items-center gap-3">
                  <CheckCircle className="w-5 h-5" />
                  <span>دعم متخصص للقطاع المالي</span>
                </div>
              </div>

              <Link href="/contact">
                <Button size="lg" className="bg-white text-emerald-900 hover:bg-gray-100 w-full group">
                  تواصل مع فريق القطاع المالي
                  <ArrowLeft className="w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                </Button>
              </Link>
            </motion.div>
          </div>
        </div>
      </section>
    </div>
  )
}
