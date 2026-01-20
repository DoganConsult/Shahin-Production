"use client"

import { useState } from "react"
import { motion } from "framer-motion"
import {
  Cloud,
  Server,
  Check,
  X,
  ArrowRight,
  Shield,
  Zap,
  Users,
  Building2,
  Lock,
  Headphones
} from "lucide-react"
import { Button } from "@/components/ui/button"
import Link from "next/link"
import { useLocale } from "@/components/providers/locale-provider"

const features = [
  { name: "AI-Powered Agents", nameAr: "وكلاء الذكاء الاصطناعي", saas: true, onprem: true, category: "core" },
  { name: "Workflow Automation", nameAr: "أتمتة سير العمل", saas: true, onprem: true, category: "core" },
  { name: "ERP Integration", nameAr: "تكامل ERP", saas: true, onprem: true, category: "core" },
  { name: "Real-time Dashboards", nameAr: "لوحات تحكم فورية", saas: true, onprem: true, category: "core" },
  { name: "Audit Trail & Compliance", nameAr: "سجل التدقيق والامتثال", saas: true, onprem: true, category: "core" },
  { name: "Multi-tenant Support", nameAr: "دعم المستأجرين المتعددين", saas: true, onprem: false, category: "architecture" },
  { name: "Auto Updates & Patches", nameAr: "التحديثات التلقائية", saas: true, onprem: false, category: "maintenance" },
  { name: "99.9% Uptime SLA", nameAr: "اتفاقية وقت التشغيل 99.9%", saas: true, onprem: false, category: "reliability" },
  { name: "Data Residency Options", nameAr: "خيارات استضافة البيانات", saas: "Saudi Arabia", saasAr: "السعودية", onprem: "Your Infrastructure", onpremAr: "بنيتك التحتية", category: "compliance" },
  { name: "Custom Integrations", nameAr: "التكاملات المخصصة", saas: "API Access", saasAr: "وصول API", onprem: "Full Source", onpremAr: "كود المصدر الكامل", category: "customization" },
  { name: "Support", nameAr: "الدعم", saas: "24/7 Cloud Support", saasAr: "دعم سحابي 24/7", onprem: "Dedicated Engineer", onpremAr: "مهندس مخصص", category: "support" },
  { name: "Initial Setup", nameAr: "الإعداد الأولي", saas: "Same Day", saasAr: "نفس اليوم", onprem: "2-4 Weeks", onpremAr: "2-4 أسابيع", category: "deployment" }
]

const plans = {
  saas: {
    icon: Cloud,
    name: "Cloud (SaaS)",
    nameAr: "السحابة (SaaS)",
    tagline: "Get started instantly",
    taglineAr: "ابدأ فوراً",
    description: "Fully managed cloud solution with automatic updates and scaling",
    descriptionAr: "حل سحابي مُدار بالكامل مع تحديثات وتوسع تلقائي",
    price: "From 2,500",
    priceAr: "من 2,500",
    period: "SAR/month",
    periodAr: "ريال/شهر",
    highlights: [
      "No infrastructure to manage",
      "Automatic backups & updates",
      "Saudi data residency",
      "Scale as you grow"
    ],
    highlightsAr: [
      "لا حاجة لإدارة البنية التحتية",
      "نسخ احتياطي وتحديثات تلقائية",
      "استضافة البيانات في السعودية",
      "التوسع مع نموك"
    ],
    cta: "Start Free Trial",
    ctaAr: "ابدأ النسخة التجريبية المجانية",
    ctaLink: "/trial",
    gradient: "from-emerald-500 to-teal-600",
    popular: true
  },
  onprem: {
    icon: Server,
    name: "On-Premise",
    nameAr: "محلي",
    tagline: "Full control & ownership",
    taglineAr: "تحكم وملكية كاملة",
    description: "Deploy on your infrastructure with complete data sovereignty",
    descriptionAr: "انشر على بنيتك التحتية مع سيادة كاملة على البيانات",
    price: "Contact Sales",
    priceAr: "تواصل مع المبيعات",
    period: "",
    periodAr: "",
    highlights: [
      "Deploy on your servers",
      "Full source code access",
      "Air-gapped deployment option",
      "Dedicated implementation team"
    ],
    highlightsAr: [
      "انشر على خوادمك",
      "وصول كامل لكود المصدر",
      "خيار النشر المعزول",
      "فريق تنفيذ مخصص"
    ],
    cta: "Contact Sales",
    ctaAr: "تواصل مع المبيعات",
    ctaLink: "/contact",
    gradient: "from-violet-500 to-purple-600",
    popular: false
  }
}

const content = {
  sectionLabel: { en: "Flexible Deployment", ar: "نشر مرن" },
  title: { en: "Cloud or On-Premise — Your Choice", ar: "السحابة أو المحلي — خيارك" },
  subtitle: {
    en: "Same powerful platform, deployed how you need it. Choose the option that fits your security and compliance requirements.",
    ar: "نفس المنصة القوية، منشورة بالطريقة التي تحتاجها. اختر الخيار الذي يناسب متطلبات الأمان والامتثال لديك."
  },
  mostPopular: { en: "MOST POPULAR", ar: "الأكثر شيوعاً" },
  featureComparison: { en: "Feature Comparison", ar: "مقارنة الميزات" },
  feature: { en: "Feature", ar: "الميزة" },
  cloud: { en: "Cloud", ar: "السحابة" },
  onPremise: { en: "On-Premise", ar: "المحلي" },
  enterpriseCta: {
    text: { en: "Need a custom enterprise solution?", ar: "هل تحتاج حل مؤسسي مخصص؟" },
    link: { en: "Contact our team", ar: "تواصل مع فريقنا" }
  }
}

export function PricingComparator() {
  const { locale } = useLocale()
  const isArabic = locale === "ar"
  const [selectedPlan, setSelectedPlan] = useState<"saas" | "onprem">("saas")

  return (
    <section className="py-24 bg-gray-50 dark:bg-gray-900">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-12"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <span className="text-emerald-600 dark:text-emerald-400 font-semibold mb-4 block">
            {isArabic ? content.sectionLabel.ar : content.sectionLabel.en}
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
            {isArabic ? content.title.ar : content.title.en}
          </h2>
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
            {isArabic ? content.subtitle.ar : content.subtitle.en}
          </p>
        </motion.div>

        {/* Plan Cards */}
        <motion.div
          className="grid md:grid-cols-2 gap-8 max-w-5xl mx-auto mb-16"
          initial={{ opacity: 0, y: 30 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          {Object.entries(plans).map(([key, plan]) => (
            <div
              key={key}
              className={`relative bg-white dark:bg-gray-800 rounded-2xl border-2 transition-all duration-300 overflow-hidden ${
                selectedPlan === key
                  ? "border-emerald-500 shadow-xl shadow-emerald-500/10"
                  : "border-gray-200 dark:border-gray-700 hover:border-gray-300 dark:hover:border-gray-600"
              }`}
              onClick={() => setSelectedPlan(key as "saas" | "onprem")}
            >
              {/* Popular Badge */}
              {plan.popular && (
                <div className={`absolute top-0 ${isArabic ? "left-0" : "right-0"}`}>
                  <div className={`bg-emerald-500 text-white text-xs font-bold px-4 py-1 ${isArabic ? "rounded-br-lg" : "rounded-bl-lg"}`}>
                    {isArabic ? content.mostPopular.ar : content.mostPopular.en}
                  </div>
                </div>
              )}

              <div className="p-8">
                {/* Header */}
                <div className="flex items-center gap-4 mb-6">
                  <div className={`w-14 h-14 rounded-xl bg-gradient-to-br ${plan.gradient} flex items-center justify-center shadow-lg`}>
                    <plan.icon className="w-7 h-7 text-white" />
                  </div>
                  <div>
                    <h3 className="text-xl font-bold text-gray-900 dark:text-white">
                      {isArabic ? plan.nameAr : plan.name}
                    </h3>
                    <p className="text-sm text-gray-500 dark:text-gray-400">
                      {isArabic ? plan.taglineAr : plan.tagline}
                    </p>
                  </div>
                </div>

                <p className="text-gray-600 dark:text-gray-400 mb-6">
                  {isArabic ? plan.descriptionAr : plan.description}
                </p>

                {/* Price */}
                <div className="mb-6">
                  <span className="text-3xl font-bold text-gray-900 dark:text-white">
                    {isArabic ? plan.priceAr : plan.price}
                  </span>
                  {plan.period && (
                    <span className={`text-gray-500 dark:text-gray-400 ${isArabic ? "mr-2" : "ml-2"}`}>
                      {isArabic ? plan.periodAr : plan.period}
                    </span>
                  )}
                </div>

                {/* Highlights */}
                <ul className="space-y-3 mb-8">
                  {(isArabic ? plan.highlightsAr : plan.highlights).map((highlight, index) => (
                    <li key={index} className="flex items-center gap-3 text-sm">
                      <Check className="w-5 h-5 text-emerald-500 flex-shrink-0" />
                      <span className="text-gray-700 dark:text-gray-300">{highlight}</span>
                    </li>
                  ))}
                </ul>

                {/* CTA */}
                <Link href={plan.ctaLink}>
                  <Button
                    className={`w-full py-6 text-lg font-semibold group ${
                      plan.popular
                        ? "bg-emerald-600 hover:bg-emerald-700 text-white"
                        : "bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 text-gray-900 dark:text-white"
                    }`}
                  >
                    {isArabic ? plan.ctaAr : plan.cta}
                    <ArrowRight className={`w-5 h-5 ${isArabic ? "mr-2 group-hover:-translate-x-1" : "ml-2 group-hover:translate-x-1"} transition-transform`} />
                  </Button>
                </Link>
              </div>
            </div>
          ))}
        </motion.div>

        {/* Feature Comparison Table */}
        <motion.div
          className="max-w-4xl mx-auto"
          initial={{ opacity: 0, y: 30 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <h3 className="text-xl font-bold text-gray-900 dark:text-white text-center mb-8">
            {isArabic ? content.featureComparison.ar : content.featureComparison.en}
          </h3>

          <div className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 overflow-hidden">
            {/* Table Header */}
            <div className="grid grid-cols-3 bg-gray-50 dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700">
              <div className="p-4 font-semibold text-gray-700 dark:text-gray-300">
                {isArabic ? content.feature.ar : content.feature.en}
              </div>
              <div className="p-4 text-center font-semibold text-gray-700 dark:text-gray-300">
                <div className="flex items-center justify-center gap-2">
                  <Cloud className="w-4 h-4 text-emerald-500" />
                  {isArabic ? content.cloud.ar : content.cloud.en}
                </div>
              </div>
              <div className="p-4 text-center font-semibold text-gray-700 dark:text-gray-300">
                <div className="flex items-center justify-center gap-2">
                  <Server className="w-4 h-4 text-violet-500" />
                  {isArabic ? content.onPremise.ar : content.onPremise.en}
                </div>
              </div>
            </div>

            {/* Table Rows */}
            {features.map((feature, index) => (
              <div
                key={feature.name}
                className={`grid grid-cols-3 ${
                  index < features.length - 1 ? "border-b border-gray-100 dark:border-gray-700" : ""
                }`}
              >
                <div className="p-4 text-sm text-gray-700 dark:text-gray-300">
                  {isArabic ? feature.nameAr : feature.name}
                </div>
                <div className="p-4 flex justify-center">
                  {typeof feature.saas === "boolean" ? (
                    feature.saas ? (
                      <Check className="w-5 h-5 text-emerald-500" />
                    ) : (
                      <X className="w-5 h-5 text-gray-300 dark:text-gray-600" />
                    )
                  ) : (
                    <span className="text-sm text-emerald-600 dark:text-emerald-400 font-medium">
                      {isArabic && feature.saasAr ? feature.saasAr : feature.saas}
                    </span>
                  )}
                </div>
                <div className="p-4 flex justify-center">
                  {typeof feature.onprem === "boolean" ? (
                    feature.onprem ? (
                      <Check className="w-5 h-5 text-violet-500" />
                    ) : (
                      <X className="w-5 h-5 text-gray-300 dark:text-gray-600" />
                    )
                  ) : (
                    <span className="text-sm text-violet-600 dark:text-violet-400 font-medium">
                      {isArabic && feature.onpremAr ? feature.onpremAr : feature.onprem}
                    </span>
                  )}
                </div>
              </div>
            ))}
          </div>
        </motion.div>

        {/* Enterprise CTA */}
        <motion.div
          className="text-center mt-12"
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
        >
          <div className="inline-flex items-center gap-3 px-6 py-4 bg-gray-100 dark:bg-gray-800 rounded-xl">
            <Building2 className="w-5 h-5 text-gray-600 dark:text-gray-400" />
            <span className="text-gray-700 dark:text-gray-300">
              {isArabic ? content.enterpriseCta.text.ar : content.enterpriseCta.text.en}
            </span>
            <Link href="/contact" className="text-emerald-600 dark:text-emerald-400 font-semibold hover:underline">
              {isArabic ? content.enterpriseCta.link.ar : content.enterpriseCta.link.en}
            </Link>
          </div>
        </motion.div>
      </div>
    </section>
  )
}
