"use client"

import { motion } from "framer-motion"
import { Building2, Shield, Users, Globe, Zap, Lock, CheckCircle, ArrowLeft } from "lucide-react"
import { Button } from "@/components/ui/button"
import Link from "next/link"

const features = [
  {
    icon: Shield,
    title: "امتثال متعدد الأطر",
    description: "إدارة الامتثال لعشرات الأطر التنظيمية في آن واحد مع تقارير موحدة",
  },
  {
    icon: Users,
    title: "إدارة فرق متعددة",
    description: "دعم هياكل تنظيمية معقدة مع صلاحيات وأدوار متقدمة",
  },
  {
    icon: Globe,
    title: "عمليات عالمية",
    description: "دعم فروع متعددة عبر مناطق جغرافية مختلفة",
  },
  {
    icon: Zap,
    title: "أتمتة متقدمة",
    description: "سير عمل مخصص وأتمتة شاملة للعمليات المتكررة",
  },
  {
    icon: Lock,
    title: "أمان على مستوى المؤسسات",
    description: "تشفير متقدم، SSO، ومراقبة أمنية على مدار الساعة",
  },
  {
    icon: CheckCircle,
    title: "دعم مخصص",
    description: "مدير حساب مخصص ودعم فني أولوية على مدار الساعة",
  },
]

const stats = [
  { value: "99.99%", label: "وقت التشغيل المضمون" },
  { value: "50+", label: "إطار تنظيمي" },
  { value: "24/7", label: "دعم فني متواصل" },
  { value: "ISO 27001", label: "معتمد" },
]

const benefits = [
  "تخفيض تكلفة الامتثال بنسبة 60%",
  "توحيد جميع عمليات GRC في منصة واحدة",
  "تقارير تنفيذية جاهزة لمجلس الإدارة",
  "تكامل سلس مع أنظمة المؤسسات الموجودة",
  "امتثال كامل للمتطلبات المحلية والدولية",
  "قابلية توسع غير محدودة",
]

export default function EnterprisePage() {
  return (
    <div className="min-h-screen">
      {/* Hero Section */}
      <section className="relative min-h-[70vh] flex items-center justify-center overflow-hidden bg-gradient-to-br from-gray-900 via-emerald-900 to-teal-900">
        <div className="absolute inset-0 opacity-20">
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_50%_120%,rgba(16,185,129,0.1),transparent)]" />
        </div>

        <div className="container mx-auto px-6 relative z-10 py-20">
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            className="max-w-4xl mx-auto text-center"
          >
            <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-white/10 backdrop-blur-sm text-emerald-300 text-sm font-medium mb-8">
              <Building2 className="w-4 h-4" />
              حلول المؤسسات الكبيرة
            </div>

            <h1 className="text-4xl md:text-6xl font-bold text-white mb-6">
              منصة GRC متكاملة
              <span className="block text-emerald-400 mt-2">للمؤسسات الكبرى</span>
            </h1>

            <p className="text-xl text-gray-300 mb-10 max-w-3xl mx-auto">
              حل شامل لإدارة الحوكمة والمخاطر والامتثال مصمم خصيصاً لتلبية احتياجات
              المؤسسات الكبيرة والشركات متعددة الجنسيات
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
                  تحدث مع فريق المبيعات
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

      {/* Features */}
      <section className="py-24 bg-gray-50 dark:bg-gray-900/50">
        <div className="container mx-auto px-6">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-center mb-16"
          >
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
              مميزات مصممة للمؤسسات الكبيرة
            </h2>
            <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
              إمكانيات متقدمة لتلبية احتياجات المؤسسات الأكثر تعقيداً
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
                className="bg-white dark:bg-gray-800 rounded-xl p-6 shadow-lg hover:shadow-xl transition-shadow"
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
      <section className="py-24 bg-white dark:bg-gray-900">
        <div className="container mx-auto px-6">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <motion.div
              initial={{ opacity: 0, x: -30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
            >
              <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-6">
                لماذا تختار شاهين لمؤسستك؟
              </h2>
              <p className="text-lg text-gray-600 dark:text-gray-400 mb-8">
                استثمر في حل متكامل يوفر عليك الوقت والتكاليف ويضمن الامتثال الكامل
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
              <h3 className="text-2xl font-bold mb-6">هل أنت جاهز للبدء؟</h3>
              <p className="mb-8 text-emerald-50">
                تواصل مع فريق المؤسسات لدينا للحصول على عرض مخصص وعرض توضيحي شامل للمنصة
              </p>
              <Link href="/contact">
                <Button size="lg" className="bg-white text-emerald-900 hover:bg-gray-100 w-full group">
                  تواصل معنا الآن
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
