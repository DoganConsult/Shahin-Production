"use client"

import { motion } from "framer-motion"
import { Briefcase, TrendingUp, Clock, DollarSign, Shield, Users, CheckCircle, ArrowLeft } from "lucide-react"
import { Button } from "@/components/ui/button"
import Link from "next/link"

const features = [
  {
    icon: Clock,
    title: "إعداد سريع",
    description: "ابدأ في دقائق مع قوالب جاهزة ومعدة مسبقاً للشركات المتوسطة",
  },
  {
    icon: DollarSign,
    title: "تسعير مرن",
    description: "خطط مرنة تناسب ميزانيتك مع إمكانية الترقية حسب نمو شركتك",
  },
  {
    icon: Shield,
    title: "امتثال مبسط",
    description: "واجهة سهلة الاستخدام تجعل إدارة الامتثال أمراً بسيطاً",
  },
  {
    icon: Users,
    title: "تعاون الفريق",
    description: "أدوات تعاون متقدمة لفريقك مع تتبع المهام والمسؤوليات",
  },
  {
    icon: TrendingUp,
    title: "قابل للنمو",
    description: "ينمو معك من 50 إلى 500 موظف بسلاسة",
  },
  {
    icon: Briefcase,
    title: "دعم متخصص",
    description: "دعم فني باللغة العربية ومساعدة في الإعداد والتدريب",
  },
]

const benefits = [
  "تقليل الوقت المستغرق في الامتثال بنسبة 70%",
  "لوحات تحكم بسيطة وسهلة الفهم",
  "تقارير جاهزة للجهات التنظيمية",
  "تكامل مع أدوات العمل الشائعة",
  "تحديثات تلقائية للأطر التنظيمية",
  "تدريب شامل لفريقك",
]

const stats = [
  { value: "300+", label: "شركة متوسطة" },
  { value: "15 دقيقة", label: "متوسط وقت الإعداد" },
  { value: "4.9/5", label: "تقييم العملاء" },
  { value: "70%", label: "توفير في الوقت" },
]

export default function BusinessPage() {
  return (
    <div className="min-h-screen">
      {/* Hero Section */}
      <section className="relative min-h-[70vh] flex items-center justify-center overflow-hidden bg-gradient-to-br from-blue-900 via-indigo-900 to-purple-900">
        <div className="absolute inset-0 opacity-20">
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_50%_120%,rgba(99,102,241,0.1),transparent)]" />
        </div>

        <div className="container mx-auto px-6 relative z-10 py-20">
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            className="max-w-4xl mx-auto text-center"
          >
            <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-white/10 backdrop-blur-sm text-blue-300 text-sm font-medium mb-8">
              <Briefcase className="w-4 h-4" />
              حلول الشركات المتوسطة
            </div>

            <h1 className="text-4xl md:text-6xl font-bold text-white mb-6">
              إدارة GRC بسيطة وقوية
              <span className="block text-blue-400 mt-2">للشركات المتوسطة</span>
            </h1>

            <p className="text-xl text-gray-300 mb-10 max-w-3xl mx-auto">
              حل عملي وميسور التكلفة لإدارة الحوكمة والمخاطر والامتثال، مصمم خصيصاً
              للشركات المتوسطة النامية في المملكة
            </p>

            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link href="/trial">
                <Button size="xl" className="bg-white text-blue-900 hover:bg-gray-100 group">
                  ابدأ تجربتك المجانية
                  <ArrowLeft className="w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                </Button>
              </Link>
              <Link href="/demo">
                <Button size="xl" variant="outline" className="border-white text-white hover:bg-white/10">
                  شاهد عرض توضيحي
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
                <div className="text-3xl md:text-4xl font-bold text-blue-600 dark:text-blue-400 mb-2">
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
              مميزات مثالية للشركات المتوسطة
            </h2>
            <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
              كل ما تحتاجه لإدارة الامتثال بكفاءة دون تعقيدات غير ضرورية
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
                <div className="w-12 h-12 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-xl flex items-center justify-center mb-4">
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
                الحل الأمثل لشركتك المتوسطة
              </h2>
              <p className="text-lg text-gray-600 dark:text-gray-400 mb-8">
                احصل على إمكانيات المؤسسات الكبيرة بتكلفة وبساطة تناسب شركتك
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
                    <div className="w-6 h-6 bg-blue-100 dark:bg-blue-900/30 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5">
                      <CheckCircle className="w-4 h-4 text-blue-600 dark:text-blue-400" />
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
              className="bg-gradient-to-br from-blue-500 to-indigo-600 rounded-2xl p-8 text-white"
            >
              <h3 className="text-2xl font-bold mb-4">تجربة مجانية لمدة 14 يوم</h3>
              <p className="mb-6 text-blue-50">
                جرب المنصة كاملة بدون بطاقة ائتمان. إذا لم تعجبك، لا توجد التزامات.
              </p>

              <div className="bg-white/10 backdrop-blur-sm rounded-xl p-4 mb-6">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm">الباقة الأساسية</span>
                  <span className="text-2xl font-bold">مجاناً</span>
                </div>
                <p className="text-sm text-blue-100">لأول 14 يوم</p>
              </div>

              <Link href="/trial">
                <Button size="lg" className="bg-white text-blue-900 hover:bg-gray-100 w-full group">
                  ابدأ الآن مجاناً
                  <ArrowLeft className="w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                </Button>
              </Link>
            </motion.div>
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="py-20 bg-gradient-to-br from-blue-50 to-indigo-50 dark:from-gray-900 dark:to-gray-800">
        <div className="container mx-auto px-6">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-center max-w-3xl mx-auto"
          >
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-6">
              هل لديك أسئلة؟
            </h2>
            <p className="text-lg text-gray-600 dark:text-gray-400 mb-8">
              تحدث مع أحد خبرائنا لمعرفة كيف يمكن لشاهين مساعدة شركتك
            </p>
            <Link href="/contact">
              <Button size="lg" variant="gradient">
                تواصل معنا
                <ArrowLeft className="w-5 h-5" />
              </Button>
            </Link>
          </motion.div>
        </div>
      </section>
    </div>
  )
}
