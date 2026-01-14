"use client"

export const dynamic = 'force-dynamic'

import { motion } from "framer-motion"
import { Check, X, Zap, Shield, Building2, ArrowLeft } from "lucide-react"
import Link from "next/link"
import { Navbar } from "@/components/layout/Navbar"
import { Footer } from "@/components/layout/Footer"
import { Button } from "@/components/ui/button"

const plans = [
  {
    name: "المبتدئ",
    nameEn: "Starter",
    description: "للمنشآت الصغيرة والناشئة",
    price: "2,500",
    period: "شهرياً",
    features: [
      { text: "حتى 50 مستخدم", included: true },
      { text: "3 أطر تنظيمية", included: true },
      { text: "إدارة الامتثال الأساسية", included: true },
      { text: "إدارة المخاطر", included: true },
      { text: "تقارير أساسية", included: true },
      { text: "دعم عبر البريد الإلكتروني", included: true },
      { text: "وكلاء الذكاء الاصطناعي", included: false },
      { text: "التكامل مع الأنظمة", included: false },
      { text: "API Access", included: false },
    ],
    cta: "ابدأ التجربة المجانية",
    popular: false,
    gradient: "from-gray-600 to-gray-700",
  },
  {
    name: "الاحترافي",
    nameEn: "Professional",
    description: "للمنشآت المتوسطة والكبيرة",
    price: "7,500",
    period: "شهرياً",
    features: [
      { text: "حتى 200 مستخدم", included: true },
      { text: "10 أطر تنظيمية", included: true },
      { text: "إدارة الامتثال المتقدمة", included: true },
      { text: "إدارة المخاطر والتدقيق", included: true },
      { text: "تقارير متقدمة وتحليلات", included: true },
      { text: "دعم أولوي 24/5", included: true },
      { text: "3 وكلاء ذكاء اصطناعي", included: true },
      { text: "تكامل محدود", included: true },
      { text: "API Access", included: false },
    ],
    cta: "ابدأ التجربة المجانية",
    popular: true,
    gradient: "from-emerald-500 to-teal-600",
  },
  {
    name: "المؤسسات",
    nameEn: "Enterprise",
    description: "للمؤسسات الكبرى والحكومية",
    price: "مخصص",
    period: "تواصل معنا",
    features: [
      { text: "مستخدمين غير محدودين", included: true },
      { text: "جميع الأطر التنظيمية", included: true },
      { text: "جميع الميزات المتقدمة", included: true },
      { text: "إدارة شاملة للحوكمة", included: true },
      { text: "تقارير وتحليلات مخصصة", included: true },
      { text: "دعم مخصص 24/7", included: true },
      { text: "9 وكلاء ذكاء اصطناعي", included: true },
      { text: "تكامل كامل مع الأنظمة", included: true },
      { text: "API Access كامل", included: true },
    ],
    cta: "تواصل مع المبيعات",
    popular: false,
    gradient: "from-purple-500 to-pink-600",
  },
]

const faqs = [
  {
    q: "هل يمكنني تغيير الباقة لاحقاً؟",
    a: "نعم، يمكنك الترقية أو التخفيض في أي وقت. سيتم احتساب الفرق بشكل تناسبي.",
  },
  {
    q: "ما هي مدة التجربة المجانية؟",
    a: "نقدم تجربة مجانية لمدة 14 يوم مع جميع ميزات الباقة الاحترافية.",
  },
  {
    q: "هل الأسعار تشمل ضريبة القيمة المضافة؟",
    a: "الأسعار المعروضة لا تشمل ضريبة القيمة المضافة (15%) والتي ستُضاف عند الفوترة.",
  },
  {
    q: "هل يمكن الدفع سنوياً؟",
    a: "نعم، نقدم خصم 20% على الاشتراك السنوي مقارنة بالدفع الشهري.",
  },
]

export default function PricingPage() {
  return (
    <main className="min-h-screen bg-white dark:bg-gray-950">
      <Navbar />

      {/* Hero */}
      <section className="pt-32 pb-16 bg-gradient-to-b from-gray-50 to-white dark:from-gray-900 dark:to-gray-950">
        <div className="container mx-auto px-6 text-center">
          <motion.span
            className="text-emerald-600 dark:text-emerald-400 font-semibold mb-4 block"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
          >
            الأسعار
          </motion.span>
          <motion.h1
            className="text-4xl md:text-5xl font-bold text-gray-900 dark:text-white mb-6"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
          >
            باقات تناسب جميع المنشآت
          </motion.h1>
          <motion.p
            className="text-xl text-gray-600 dark:text-gray-400 max-w-2xl mx-auto"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
          >
            ابدأ مجاناً لمدة 14 يوم واختر الباقة المناسبة لحجم منشأتك واحتياجاتك
          </motion.p>
        </div>
      </section>

      {/* Pricing Cards */}
      <section className="py-16">
        <div className="container mx-auto px-6">
          <div className="grid md:grid-cols-3 gap-8 max-w-6xl mx-auto">
            {plans.map((plan, index) => (
              <motion.div
                key={plan.name}
                className={`relative rounded-2xl p-8 ${
                  plan.popular
                    ? "bg-gradient-to-b from-emerald-50 to-white dark:from-emerald-900/20 dark:to-gray-900 border-2 border-emerald-500 shadow-xl scale-105"
                    : "bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800"
                }`}
                initial={{ opacity: 0, y: 30 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
              >
                {/* Popular Badge */}
                {plan.popular && (
                  <div className="absolute -top-4 left-1/2 -translate-x-1/2">
                    <span className="bg-gradient-to-r from-emerald-500 to-teal-600 text-white text-sm font-bold px-4 py-1 rounded-full">
                      الأكثر شيوعاً
                    </span>
                  </div>
                )}

                {/* Plan Icon */}
                <div className={`w-14 h-14 rounded-xl bg-gradient-to-br ${plan.gradient} flex items-center justify-center mb-6`}>
                  {index === 0 && <Zap className="w-7 h-7 text-white" />}
                  {index === 1 && <Shield className="w-7 h-7 text-white" />}
                  {index === 2 && <Building2 className="w-7 h-7 text-white" />}
                </div>

                {/* Plan Name */}
                <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                  {plan.name}
                </h3>
                <p className="text-gray-600 dark:text-gray-400 mb-6">
                  {plan.description}
                </p>

                {/* Price */}
                <div className="mb-8">
                  <span className="text-4xl font-bold text-gray-900 dark:text-white">
                    {plan.price}
                  </span>
                  {plan.price !== "مخصص" && (
                    <span className="text-gray-500 dark:text-gray-400 mr-2">
                      ريال / {plan.period}
                    </span>
                  )}
                  {plan.price === "مخصص" && (
                    <span className="block text-gray-500 dark:text-gray-400 mt-1">
                      {plan.period}
                    </span>
                  )}
                </div>

                {/* Features */}
                <ul className="space-y-4 mb-8">
                  {plan.features.map((feature) => (
                    <li
                      key={feature.text}
                      className={`flex items-center gap-3 ${
                        feature.included
                          ? "text-gray-700 dark:text-gray-300"
                          : "text-gray-400 dark:text-gray-600"
                      }`}
                    >
                      {feature.included ? (
                        <Check className="w-5 h-5 text-emerald-500 flex-shrink-0" />
                      ) : (
                        <X className="w-5 h-5 text-gray-400 flex-shrink-0" />
                      )}
                      <span className="text-sm">{feature.text}</span>
                    </li>
                  ))}
                </ul>

                {/* CTA */}
                <Link href={plan.price === "مخصص" ? "/contact" : "/trial"}>
                  <Button
                    variant={plan.popular ? "gradient" : "outline"}
                    size="lg"
                    className="w-full group"
                  >
                    {plan.cta}
                    <ArrowLeft className="w-4 h-4 group-hover:-translate-x-1 transition-transform" />
                  </Button>
                </Link>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* FAQs */}
      <section className="py-20 bg-gray-50 dark:bg-gray-900/50">
        <div className="container mx-auto px-6">
          <motion.div
            className="text-center mb-12"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            <h2 className="text-3xl font-bold text-gray-900 dark:text-white mb-4">
              الأسئلة الشائعة
            </h2>
          </motion.div>

          <div className="max-w-3xl mx-auto grid gap-6">
            {faqs.map((faq, index) => (
              <motion.div
                key={faq.q}
                className="bg-white dark:bg-gray-800 rounded-xl p-6 border border-gray-200 dark:border-gray-700"
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
              >
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                  {faq.q}
                </h3>
                <p className="text-gray-600 dark:text-gray-400">
                  {faq.a}
                </p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="py-20">
        <div className="container mx-auto px-6 text-center">
          <motion.div
            className="max-w-2xl mx-auto"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            <h2 className="text-3xl font-bold text-gray-900 dark:text-white mb-4">
              هل لديك أسئلة أخرى؟
            </h2>
            <p className="text-gray-600 dark:text-gray-400 mb-8">
              فريقنا جاهز للإجابة على استفساراتك ومساعدتك في اختيار الباقة المناسبة
            </p>
            <Link href="/contact">
              <Button variant="gradient" size="xl">
                تواصل معنا
              </Button>
            </Link>
          </motion.div>
        </div>
      </section>

      <Footer />
    </main>
  )
}
